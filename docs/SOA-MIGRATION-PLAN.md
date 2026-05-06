# Plan de Migración a SOA — Sistema UMLIoT

> **Fuente del análisis**: `docs/CLAUDE.md` y `docs/README.md` (estado actual del ensamblado).
> **Metodología**: principios SOA canónicos de Thomas Erl + capability-based decomposition + contract-first design.
> **Patrón de migración**: Strangler Fig (extracción incremental sin reescritura).

---

## 1. Análisis del estado actual

### 1.1 Tipo de ensamblado
Monolito procedimental empaquetado como **un único proyecto .NET 8** (`UMLIoT.csproj`) con un solo punto de entrada (`Program.cs`) y persistencia en memoria. La separación es lógica (carpetas `Core/` y `Patterns/`), no física: todo compila a un binario y comparte el mismo proceso, hilo y heap.

### 1.2 Capacidades de negocio identificadas
A partir del menú interactivo (`Program.cs`) y de la fachada `IoTFacade`:

| # | Capacidad | Operaciones | Localización actual |
|---|---|---|---|
| C1 | Gestión de identidad y acceso | `registerUser`, `login`, `logout`, `isUserLoggedIn` | `Core/Users/` (`AuthService`, `UserRepository`, `User`) |
| C2 | Inventario de dispositivos | `registerDevice`, `removeDevice`, `getAllDevice`, asignación de ID e IP | `Core/Controllers/ControladorIOT`, `Patterns/Factory/Devices/*` |
| C3 | Control / actuación de dispositivos | `turnOnDevice`, `turnOffDevice`, `activateAlarm`, `startRecording` | `Patterns/Command/*` ejecutados por `ControladorIOT` |
| C4 | Monitoreo / estado de dispositivos | `getDeviceStatus`, máquina de estados (`Online/Offline/Error`) | `Patterns/State/*`, `Device.handleStatus` |
| C5 | Notificación y auditoría de eventos | `notifyObservers`, observadores móvil/seguridad/log | `Patterns/Observer/*` |

### 1.3 Reglas de negocio y datos críticos
- **Asignación de identidad de dispositivo**: el `id` y la `ipAddress` los asigna `ControladorIOT.addDevice` (no el cliente ni el factory). Patrón: `192.168.1.{nextId}`.
- **Login obligatorio para registrar dispositivos**, pero **no** para operarlos. Esta inconsistencia debe resolverse explícitamente durante la migración.
- **Sesión global por proceso**: `AuthService.currentUser` es un único campo — no hay multi-sesión.
- **Persistencia**: `List<>` en memoria → estado se pierde al cerrar el proceso.
- **Convención no estándar**: métodos en camelCase. En contratos SOA usaremos PascalCase (estándar WCF/REST .NET); el adaptador interno puede mantener camelCase.

### 1.4 Acoplamientos críticos
- `IoTFacade` es **la única costura limpia** del sistema → es el punto natural de partida para extraer servicios.
- `ControladorIOT` mezcla 3 responsabilidades (inventario, ejecución de comandos, notificación de eventos) y se va a partir.
- Los observadores (`MobileNotifier`, `SecuritySystem`, `EventLogger`) están acoplados al mismo proceso → candidatos claros a desacoplarse vía mensajería asíncrona.

---

## 2. Principios SOA aplicables

| Principio | Aplicación concreta en UMLIoT |
|---|---|
| **Standardized Service Contract** | Contratos en OpenAPI 3.0 (REST) y AsyncAPI (eventos) **antes** de tocar código. |
| **Service Loose Coupling** | Cada servicio dueño de su esquema de datos; comunicación solo por contrato. Los observadores migran a publish/subscribe. |
| **Service Abstraction** | Internals (factories, comandos, máquina de estado) quedan ocultos detrás del contrato. |
| **Service Reusability** | `IdentityService` y `NotificationService` se diseñan agnósticos al dominio IoT. |
| **Service Autonomy** | Cada servicio con su propio runtime, ciclo de despliegue y store. Mata la lista en memoria compartida. |
| **Service Statelessness** | El estado de sesión sale del proceso (token JWT). El estado del dispositivo va a la BD del DeviceService. |
| **Service Composability** | `IoTOrchestrator` (ex-`IoTFacade`) compone Identity + Device + Control + Notification. |
| **Service Discoverability** | Catálogo de servicios + service registry (Consul / Eureka / API Gateway). |

---

## 3. Mapa de capacidades → Catálogo de servicios candidatos

Granularidad propuesta siguiendo la clasificación de Erl (entity / task / utility / process services):

### S1 — IdentityService *(entity service)*
- **Responsabilidad**: ciclo de vida de usuarios y emisión/validación de credenciales.
- **Operaciones contractuales**:
  - `POST /users` — registro
  - `POST /sessions` — login → devuelve JWT
  - `DELETE /sessions/{id}` — logout
  - `GET /sessions/current` — validación
- **Datos propios**: tabla `Users` (id, name, email, passwordHash).
- **Reemplaza**: `AuthService`, `UserRepository`, `User`.

### S2 — DeviceRegistryService *(entity service)*
- **Responsabilidad**: inventario CRUD de dispositivos y asignación de identidad/red.
- **Operaciones**:
  - `POST /devices` — alta (`{type, name, config}`)
  - `DELETE /devices/{id}`
  - `GET /devices` / `GET /devices/{id}`
  - `PATCH /devices/{id}/network` — asignación IP
- **Datos propios**: tabla `Devices` (id, type, name, ipAddress, ownerUserId, currentStatus).
- **Reemplaza**: parte de inventario de `ControladorIOT` + jerarquía `DeviceCreator`.
- **Regla migrada**: la asignación `192.168.1.{n}` se vuelve un `IIpAllocator` interno del servicio con bloqueo transaccional (resuelve la race condition latente del `nextDeviceId` actual).

### S3 — DeviceControlService *(task service)*
- **Responsabilidad**: ejecutar comandos de actuación. Reemplaza el patrón Command como servicio.
- **Operaciones**:
  - `POST /devices/{id}/commands/turn-on`
  - `POST /devices/{id}/commands/turn-off`
  - `POST /devices/{id}/commands/trigger-alarm`
  - `POST /devices/{id}/commands/start-recording`
- **Dependencias**: consulta `DeviceRegistryService` para resolver capacidades (`ISwitchable`, `IAlarm`, `IMonitorable`).
- **Publica eventos**: `DeviceTurnedOn`, `AlarmTriggered`, `RecordingStarted` al bus.
- **Reemplaza**: `Patterns/Command/*` + dispatch en `IoTFacade`.

### S4 — DeviceStatusService *(entity service ligero)*
- **Responsabilidad**: máquina de estados y consulta de estado actual.
- **Operaciones**:
  - `GET /devices/{id}/status`
  - `POST /devices/{id}/status/transitions` — connect, disconnect, fault
- **Datos propios**: histórico de transiciones (event sourcing recomendado).
- **Suscrito a**: eventos de `DeviceControlService` para auto-transicionar.
- **Reemplaza**: `Patterns/State/*` + `Device.handleStatus`.
- **Decisión**: si la lógica de estados sigue siendo trivial podría fusionarse con S2; se separa porque va a crecer (anticipar evolución de granularidad).

### S5 — NotificationService *(utility service)*
- **Responsabilidad**: ruteo de eventos a canales (móvil, seguridad, log de auditoría).
- **Operaciones**:
  - Suscriptor en bus de eventos.
  - `GET /audit/events` — consulta del log.
- **Reemplaza**: `MobileNotifier`, `SecuritySystem`, `EventLogger`.
- **Cambio de paradigma**: pasa de Observer in-process a **pub/sub asíncrono** sobre un broker (RabbitMQ / Azure Service Bus / Kafka).

### S6 — IoTOrchestrator *(process service / fachada externa)*
- **Responsabilidad**: composición de operaciones de negocio cross-servicio. Sucesor directo de `IoTFacade` y del menú de `Program.cs`.
- **Operaciones representativas**: "Registrar dispositivo" = autenticar (S1) + crear en S2 + suscribir en S5.
- **No es opcional**: sin él, los clientes tendrían que orquestar 3-4 llamadas y replicar reglas de negocio.

---

## 4. Diseño contract-first (muestra)

Antes de escribir una línea de implementación, los contratos se publican en `/contracts/`. Ejemplo `IdentityService`:

```yaml
# contracts/identity-service.openapi.yaml
openapi: 3.0.3
info: { title: IdentityService, version: 1.0.0 }
paths:
  /users:
    post:
      requestBody:
        content:
          application/json:
            schema: { $ref: '#/components/schemas/UserRegistration' }
      responses:
        '201': { description: Created, content: { application/json: { schema: { $ref: '#/components/schemas/User' } } } }
        '409': { description: Email already registered }
  /sessions:
    post:
      requestBody:
        content:
          application/json:
            schema: { $ref: '#/components/schemas/Credentials' }
      responses:
        '200': { content: { application/json: { schema: { $ref: '#/components/schemas/Token' } } } }
        '401': { description: Invalid credentials }
components:
  schemas:
    UserRegistration:
      type: object
      required: [name, email, password]
      properties:
        name: { type: string }
        email: { type: string, format: email }
        password: { type: string, minLength: 8 }
    User:
      type: object
      properties:
        id: { type: string, format: uuid }
        name: { type: string }
        email: { type: string }
    Credentials:
      type: object
      required: [email, password]
      properties:
        email: { type: string }
        password: { type: string }
    Token:
      type: object
      properties:
        accessToken: { type: string }
        expiresIn: { type: integer }
```

AsyncAPI para los eventos:

```yaml
# contracts/device-events.asyncapi.yaml
asyncapi: 2.6.0
channels:
  device.lifecycle.added:
    publish: { message: { $ref: '#/components/messages/DeviceAdded' } }
  device.control.turned-on:
    publish: { message: { $ref: '#/components/messages/DeviceTurnedOn' } }
  device.control.alarm-triggered:
    publish: { message: { $ref: '#/components/messages/AlarmTriggered' } }
```

**Regla**: los contratos son artefactos versionados en el repo y nadie los modifica sin bump de versión semántica.

---

## 5. Arquitectura objetivo

```
                ┌──────────────────────────┐
                │   Cliente (consola/web)  │
                └────────────┬─────────────┘
                             │ HTTPS
                ┌────────────▼─────────────┐
                │  API Gateway / Ingress   │  (auth, rate-limit, routing)
                └────────────┬─────────────┘
                             │
                ┌────────────▼─────────────┐
                │     IoTOrchestrator      │  (S6 - process service)
                └─┬───┬──────────┬─────┬───┘
       sync REST  │   │          │     │  sync REST
                  │   │          │     │
        ┌─────────▼─┐ │ ┌────────▼─┐ ┌─▼──────────┐
        │ Identity  │ │ │ Device   │ │ Device     │
        │ Service   │ │ │ Registry │ │ Control    │
        │   (S1)    │ │ │  (S2)    │ │  (S3)      │
        └─────┬─────┘ │ └────┬─────┘ └─────┬──────┘
              │       │      │             │
              ▼       │      ▼             ▼ publish events
           [Users DB] │ [Devices DB]   ┌───────────────┐
                      │                │  Event Bus    │  (RabbitMQ/Kafka)
                      │                └────┬─────┬────┘
                      │                     │     │ subscribe
                      │              ┌──────▼┐  ┌─▼─────────────┐
                      │              │Status │  │ Notification  │
                      └─────────────►│  (S4) │  │   (S5)        │
                                     └───┬───┘  └──────┬────────┘
                                         ▼             ▼
                                    [Status DB]   [Audit Log]
```

**Decisiones de comunicación**:
- **Sync (REST/JSON)** para flujos donde el cliente espera resultado: orquestador ↔ S1/S2/S3.
- **Async (pub/sub)** para eventos de dominio: S3 y S2 → bus → S4 y S5. Esto desacopla los antiguos observadores.
- **Service registry**: Consul, o el propio API Gateway (Kong / YARP / Ocelot en stack .NET) con health checks.

---

## 6. Plan de migración por fases (Strangler Fig)

Premisa: **no reescribir, estrangular**. El monolito sigue funcionando; cada servicio extraído lo va vaciando.

### Fase 0 — Preparación (1-2 semanas)
- [ ] Mover persistencia `List<>` a una BD relacional **dentro del monolito** (SQLite / SQL Server). Sin esto, la extracción es imposible: cada servicio necesita su propio store.
- [ ] Introducir tests de caracterización sobre `IoTFacade` que cubran los 11 casos del menú. Son la red de seguridad de toda la migración.
- [ ] Publicar el repositorio de contratos `/contracts/` con los 6 contratos OpenAPI/AsyncAPI base.
- [ ] Levantar infraestructura mínima: API Gateway, broker de mensajes, registry, observabilidad (OpenTelemetry + Jaeger + Prometheus).

### Fase 1 — Extraer IdentityService (S1)
Razón de ir primero: es el menos acoplado (solo `IoTFacade` lo consume) y es prerequisito para auth distribuida.
- Implementar S1 como nuevo proyecto .NET (Web API) detrás del gateway.
- Reemplazar dentro del monolito `AuthService` por un cliente HTTP que llame a S1 (anti-corruption layer). El `IoTFacade` no se entera.
- Migrar usuarios existentes (script de ETL).
- Switch a JWT: `IoTFacade.isUserLoggedIn` se vuelve validación de token.
- **Done criteria**: monolito sin código de auth, S1 corriendo, tests verdes.

### Fase 2 — Extraer NotificationService (S5)
Segundo en orden porque es **append-only y sin retorno crítico** → tolera fallos durante la migración.
- Cambiar `DeviceEventManager.notifyObservers` para que **además** de invocar observers locales, publique al bus.
- Implementar S5 como suscriptor.
- Una vez verificado, eliminar los observers in-process.
- **Done criteria**: ningún `IDeviceObserver` registrado en `Program.cs`.

### Fase 3 — Extraer DeviceRegistryService (S2)
- Implementar S2 con su propia BD.
- Sustituir `ControladorIOT` (parte de inventario) por cliente HTTP a S2.
- Crítico: la asignación atómica de id/IP migra dentro de S2 con transacción (resuelve la race condition que hoy existe).
- **Done criteria**: `ControladorIOT` ya no contiene `List<IDevice>` ni `nextDeviceId`.

### Fase 4 — Extraer DeviceControlService (S3) y DeviceStatusService (S4)
Se extraen juntos porque comparten el bus (S3 publica, S4 consume).
- Migrar `Patterns/Command/*` a endpoints HTTP en S3.
- Migrar `Patterns/State/*` a S4 con event sourcing.
- **Done criteria**: la carpeta `Patterns/` desaparece del monolito.

### Fase 5 — Convertir el monolito en IoTOrchestrator (S6)
- Lo que queda de `IoTFacade` y `Program.cs` se reempaqueta como S6 (Web API + cliente CLI separados).
- Eliminar el ejecutable monolítico.
- **Done criteria**: `UMLIoT.csproj` ya no existe; en su lugar, 6 proyectos independientes con su propio pipeline.

---

## 7. Aspectos transversales (cross-cutting)

| Aspecto | Decisión |
|---|---|
| **Seguridad** | OAuth2 / OIDC + JWT emitido por S1; gateway valida tokens antes de rutear. Comunicación interna por mTLS o red privada. |
| **Observabilidad** | OpenTelemetry en todos los servicios → traces correlacionados (`traceparent` atraviesa headers). Logs estructurados (Serilog → ELK). |
| **Resiliencia** | Polly en clientes HTTP del orquestador: retry exponencial, circuit breaker, timeout. Bus de eventos con DLQ y outbox pattern para evitar pérdida. |
| **Versionado de contratos** | SemVer en cada contrato; backward-compatible obligatorio dentro de major. Breaking change → ruta `/v2/`. |
| **Despliegue** | Cada servicio: su Dockerfile, su pipeline CI/CD, su health endpoint. Orquestación con Docker Compose (dev) / Kubernetes (prod). |
| **Datos** | Patrón Database-per-Service. Saga pattern para transacciones que cruzan servicios (alta de dispositivo = S2 + S5 + auditoría). |

---

## 8. Riesgos y mitigaciones

| Riesgo | Probabilidad | Impacto | Mitigación |
|---|---|---|---|
| Race condition al asignar IDs/IPs en S2 (hoy oculta porque hay un solo proceso) | Alta | Alto | Secuencia/identity en BD + restricción `UNIQUE`; tests de carga. |
| Pérdida de eventos al desacoplar Observers a bus | Media | Medio | Outbox pattern en publishers, idempotencia en consumidores, DLQ. |
| Latencia agregada (antes 1 llamada in-proc, ahora N HTTP) | Alta | Medio | Orquestador con paralelismo donde aplique; cache en gateway para `GET /devices`. |
| Inconsistencia eventual entre S2 (registro) y S4 (estado) | Media | Bajo | Aceptar eventual consistency; convergencia por eventos; UI muestra "estado actualizando". |
| Equipo sin experiencia operando microservicios | Alta | Alto | Fase 0 incluye spike de infra; un servicio extraído en cada fase antes de seguir. |
| Reintroducir un "monolito distribuido" si el orquestador conoce demasiados detalles | Media | Alto | Code review explícito contra los 8 principios de Erl en cada PR de S6. |

---

## 9. Catálogo final de servicios

| ID | Servicio | Tipo (Erl) | Dueño | Datos propios | SLA tentativo |
|---|---|---|---|---|---|
| S1 | IdentityService | Entity | Equipo Identity | Users, Sessions | 99.9%, p95 < 200ms |
| S2 | DeviceRegistryService | Entity | Equipo IoT | Devices | 99.5%, p95 < 300ms |
| S3 | DeviceControlService | Task | Equipo IoT | (sin estado propio) | 99.0%, p95 < 500ms |
| S4 | DeviceStatusService | Entity | Equipo IoT | Status events (event-sourced) | 99.5%, p95 < 200ms |
| S5 | NotificationService | Utility | Equipo Plataforma | Audit log | 99.0% best-effort |
| S6 | IoTOrchestrator | Process | Equipo IoT | (sin estado propio) | 99.0%, p95 < 800ms |

---

## 10. Próximos pasos sugeridos

1. **Validar este catálogo y orden de fases con la metodología enseñada en clase** (las diapositivas de SOA). Si el template del catálogo o la nomenclatura difieren, ajustar este documento.
2. Generar los 6 contratos OpenAPI/AsyncAPI completos en `/contracts/`.
3. Crear la propuesta de estructura de solución multi-proyecto (`IoT.Identity`, `IoT.DeviceRegistry`, etc.) lista para arrancar la Fase 1.
4. Escribir los tests de caracterización sobre `IoTFacade` que blindan la Fase 0.
