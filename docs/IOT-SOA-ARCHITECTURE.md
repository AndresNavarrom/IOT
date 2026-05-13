# Documento de Arquitectura SOA — Sistema UMLIoT
### Metodología de Arquitectura Orientada a Servicios (SOA)
**Asignatura:** Arquitectura de Software  
**Proyecto:** Sistema de Gestión de Dispositivos IoT (UMLIoT)  
**Tecnología base:** .NET 8 / C#  
**Metodología:** Principios SOA canónicos de Thomas Erl + Capability-Based Decomposition + Contract-First Design  
**Patrón de migración:** Strangler Fig

---

## Tabla de Contenidos

1. [Identificación de Capacidades Organizacionales](#1-identificación-de-capacidades-organizacionales)
2. [Identificación de Servicios Candidatos por Capacidad](#2-identificación-de-servicios-candidatos-por-capacidad)
3. [Diseño de la Arquitectura Jerárquica de Servicios](#3-diseño-de-la-arquitectura-jerárquica-de-servicios-soa)
4. [Dependencias y Relaciones entre Servicios](#4-dependencias-y-relaciones-entre-servicios)
5. [Definición de Contratos de Servicio (Contract First)](#5-definición-de-contratos-de-servicio-contract-first)
6. [Catálogo de Servicios](#6-catálogo-de-servicios)
7. [Vista Final de la Arquitectura SOA](#7-vista-final-de-la-arquitectura-soa)

---

## 1. Identificación de Capacidades Organizacionales

### 1.1 Contexto del sistema

El sistema UMLIoT es una aplicación de gestión de dispositivos de Internet de las Cosas (IoT), desarrollada originalmente como un monolito procedimental en .NET 8. El sistema permite registrar usuarios, autenticar sesiones, gestionar un inventario de dispositivos heterogéneos (cámaras, luces inteligentes y alarmas), controlar su estado operativo y notificar eventos relevantes a distintos canales de salida. La interfaz de usuario se provee a través de un menú interactivo de consola que delega todas las operaciones a una fachada central (`IoTFacade`).

El análisis de capacidades organizacionales parte del mapa extraído del estado actual del ensamblado: el menú de comandos de `Program.cs`, la fachada `IoTFacade`, el controlador `ControladorIOT` y los patrones de dominio (`Core/` y `Patterns/`).

### 1.2 Mapa de Capacidades del Negocio

A continuación se presentan las cinco capacidades organizacionales identificadas, derivadas directamente de las responsabilidades funcionales del sistema:

---

#### C1 — Gestión de Identidad y Acceso

**Propósito:** Administrar el ciclo de vida de los usuarios del sistema, incluyendo el registro de nuevas cuentas, la autenticación de credenciales, el mantenimiento de sesiones activas y el cierre de sesión.

**Localización actual en el monolito:** `Core/Users/` — clases `AuthService`, `UserRepository`, `User`.

**Aporte de valor al negocio:**  
Esta capacidad es la puerta de entrada al sistema. Garantiza que únicamente usuarios autorizados puedan realizar operaciones sensibles (como el registro de nuevos dispositivos), protegiendo la integridad del inventario y la trazabilidad de acciones. Sin esta capacidad, el sistema carecería de control de acceso y no podría atribuir responsabilidades a actores concretos.

**Operaciones actuales:**
- `registerUser` — alta de usuario con nombre, correo y contraseña.
- `login` — autenticación por credenciales; establece sesión activa.
- `logout` — cierre de sesión.
- `isUserLoggedIn` — verificación del estado de sesión.

---

#### C2 — Inventario de Dispositivos

**Propósito:** Gestionar el ciclo de vida de los dispositivos IoT registrados en el sistema: alta, consulta y baja de dispositivos, así como la asignación automática de identificador único y dirección IP de red.

**Localización actual en el monolito:** `Core/Controllers/ControladorIOT` (fracción de inventario) + `Patterns/Factory/Devices/` (`DeviceCreator`, `AlarmCreator`, `CameraCreator`, `SmartlightCreator`).

**Aporte de valor al negocio:**  
Mantener un inventario fiable y actualizado de todos los dispositivos es fundamental para cualquier sistema IoT. Esta capacidad garantiza que el sistema conozca qué dispositivos existen, qué tipo son, a qué dirección IP responden y a qué usuario pertenecen. Es la fuente de verdad sobre los activos gestionados.

**Operaciones actuales:**
- `registerDevice` — creación de un dispositivo con asignación de ID e IP.
- `removeDevice` — eliminación del inventario.
- `getAllDevices` — consulta del listado completo.

---

#### C3 — Control y Actuación de Dispositivos

**Propósito:** Ejecutar comandos de actuación sobre los dispositivos registrados: encendido, apagado, disparo de alarma y arranque de grabación. Encapsula las operaciones de control como objetos de comando (patrón Command).

**Localización actual en el monolito:** `Patterns/Command/` (`ICommand`, `TurnOnCommand`, `TurnOffCommand`, `TriggerAlarmCommand`, `StartRecordingCommand`) + despacho en `IoTFacade`.

**Aporte de valor al negocio:**  
Esta capacidad representa la inteligencia operativa del sistema IoT. Permite que los usuarios interactúen con los dispositivos físicos de manera uniforme y desacoplada, sin importar el tipo concreto de dispositivo. Cada comando es una unidad de trabajo autocontenida, lo que facilita la auditoría, la repetición y la extensión futura.

**Operaciones actuales:**
- `turnOnDevice` — encendido del dispositivo.
- `turnOffDevice` — apagado del dispositivo.
- `activateAlarm` — disparo del sistema de alerta.
- `startRecording` — inicio de grabación en cámaras.

---

#### C4 — Monitoreo y Estado de Dispositivos

**Propósito:** Representar y gestionar el estado operativo de cada dispositivo a través de una máquina de estados finita (`Online`, `Offline`, `Error`), así como exponer el estado actual para consulta.

**Localización actual en el monolito:** `Patterns/State/` (`DeviceStatus`, `OnlineStatus`, `OfflineStatus`, `ErrorStatus`) + método `Device.handleStatus`.

**Aporte de valor al negocio:**  
En un sistema IoT, conocer el estado en tiempo real de cada dispositivo es crítico para la toma de decisiones operativas y la detección de fallos. Esta capacidad garantiza que las transiciones de estado sean consistentes (no se puede pasar directamente de `Offline` a `Error` sin pasar por `Online`), y provee una fuente de verdad sobre la disponibilidad de cada activo.

**Operaciones actuales:**
- `getDeviceStatus` — consulta del estado actual.
- Transiciones implícitas: `connect()` → `Online`; `disconnect()` → `Offline`; fallo → `Error`.

---

#### C5 — Notificación y Auditoría de Eventos

**Propósito:** Propagar eventos de dominio relevantes (registro de dispositivos, cambios de estado, comandos ejecutados) a múltiples canales de salida: notificaciones móviles, sistema de seguridad y log de auditoría.

**Localización actual en el monolito:** `Patterns/Observer/` (`DeviceEventManager`, `IDeviceObserver`, `IDeviceSubject`, `MobileNotifier`, `SecuritySystem`, `EventLogger`).

**Aporte de valor al negocio:**  
La trazabilidad de eventos es un requisito tanto operativo como regulatorio en sistemas IoT. Esta capacidad desacopla la generación de eventos de su consumo, permitiendo que distintos subsistemas reaccionen de forma independiente a los cambios en el sistema sin crear dependencias directas entre ellos. Constituye también la base de la auditoría de seguridad del sistema.

**Operaciones actuales:**
- `notifyObservers(device, eventType)` — propagación de evento a todos los suscriptores registrados.
- Consumidores: `MobileNotifier`, `SecuritySystem`, `EventLogger`.

---

### 1.3 Resumen del Mapa de Capacidades

| ID | Capacidad | Propósito Central | Valor al Negocio |
|----|-----------|-------------------|------------------|
| C1 | Gestión de identidad y acceso | Control de usuarios y sesiones | Seguridad y trazabilidad de acciones |
| C2 | Inventario de dispositivos | CRUD de activos IoT | Fuente de verdad sobre activos gestionados |
| C3 | Control y actuación de dispositivos | Ejecución de comandos sobre dispositivos | Operación remota de activos físicos |
| C4 | Monitoreo y estado de dispositivos | Máquina de estados y consultas de estado | Disponibilidad y detección de fallos |
| C5 | Notificación y auditoría de eventos | Propagación de eventos a canales | Trazabilidad, seguridad y reactividad |

---

## 2. Identificación de Servicios Candidatos por Capacidad

La identificación de servicios candidatos sigue la clasificación taxonómica de Thomas Erl, que distingue cuatro tipos de servicios según su granularidad y propósito:

- **Entity Service (Servicio de Entidad):** Encapsula datos y lógica de negocio asociados a una entidad de dominio concreta.
- **Task Service (Servicio de Tarea):** Encapsula una operación de negocio específica, generalmente de grano medio.
- **Utility Service (Servicio de Utilidad):** Provee funcionalidad transversal o de soporte, reutilizable por múltiples dominios.
- **Process Service (Servicio de Proceso):** Orquesta la colaboración entre múltiples servicios para completar un proceso de negocio end-to-end.

### 2.1 Servicios candidatos por capacidad

---

#### C1 → S1: IdentityService *(Entity Service)*

**Descripción:** Encapsula toda la lógica relativa a la gestión de usuarios y la autenticación. Es responsable de registrar nuevos usuarios, validar credenciales, emitir tokens de sesión (JWT) y verificar la validez de dichos tokens.

**Justificación de pertenencia a C1:**  
Toda la lógica de identidad y acceso recae exclusivamente en este servicio. Al aislarlo como entidad independiente, se garantiza que ningún otro servicio gestione directamente credenciales o sesiones, eliminando la duplicación de lógica de autenticación que en el monolito residía simultáneamente en `AuthService` y `IoTFacade.isUserLoggedIn`.

**Alta cohesión:** Todas las operaciones del servicio giran en torno a la entidad `Usuario` y su ciclo de vida de autenticación.  
**Bajo acoplamiento:** El servicio es agnóstico al dominio IoT. Puede ser consumido por cualquier sistema que requiera autenticación, sin conocer nada sobre dispositivos.

---

#### C2 → S2: DeviceRegistryService *(Entity Service)*

**Descripción:** Gestiona el inventario de dispositivos IoT: creación, consulta, eliminación y asignación atómica de identificador único y dirección IP de red (`192.168.1.{n}`).

**Justificación de pertenencia a C2:**  
Concentra toda la responsabilidad sobre el ciclo de vida de los activos IoT. La asignación de ID e IP, que en el monolito se realizaba de forma no atómica en `ControladorIOT`, se convierte en una transacción garantizada por el motor de persistencia propio del servicio.

**Alta cohesión:** Todas las operaciones operan sobre la entidad `Dispositivo`.  
**Bajo acoplamiento:** No ejecuta comandos ni conoce el estado operativo del dispositivo. Delega esas responsabilidades a S3 y S4 respectivamente.

---

#### C3 → S3: DeviceControlService *(Task Service)*

**Descripción:** Ejecuta comandos de actuación sobre dispositivos. Recibe solicitudes de control (encender, apagar, activar alarma, iniciar grabación), valida las capacidades del dispositivo mediante consulta a S2, ejecuta el comando correspondiente y publica eventos de dominio al bus de mensajes.

**Justificación de pertenencia a C3:**  
Encapsula exclusivamente la lógica de ejecución de comandos, reemplazando el patrón Command in-process por una API de comandos orientada a servicios. No gestiona inventario ni estado persistente propio; su dominio es la actuación.

**Alta cohesión:** Todas las operaciones son comandos de actuación sobre dispositivos.  
**Bajo acoplamiento:** No conoce cómo se notifican los eventos ni cómo se persiste el estado. Publica al bus y delega.

---

#### C4 → S4: DeviceStatusService *(Entity Service)*

**Descripción:** Gestiona y persiste la máquina de estados de cada dispositivo (`Online`, `Offline`, `Error`). Suscribe eventos del bus publicados por S3 para auto-transicionar el estado, y expone el estado actual para consulta.

**Justificación de pertenencia a C4:**  
Materializa la capacidad de monitoreo del sistema. Al separar el estado del dispositivo del inventario (S2) y del control (S3), se permite que cada dimensión evolucione de forma independiente, aplicando event sourcing para mantener un historial completo de transiciones.

**Alta cohesión:** Solo gestiona transiciones de estado y consulta de estado actual.  
**Bajo acoplamiento:** Consume eventos del bus; no tiene dependencia directa con S3.

---

#### C5 → S5: NotificationService *(Utility Service)*

**Descripción:** Suscribe eventos del bus de mensajes y los enruta a los canales de salida correspondientes: notificaciones push a móviles, alertas al sistema de seguridad y registros en el log de auditoría. Expone también una API de consulta del historial de auditoría.

**Justificación de pertenencia a C5:**  
Encapsula completamente la lógica de propagación de eventos, reemplazando el patrón Observer in-process por un mecanismo de pub/sub asíncrono. Al ser un servicio de utilidad, puede ser consumido por cualquier dominio que necesite notificaciones, sin acoplarse a la lógica IoT.

**Alta cohesión:** Todas las responsabilidades son de enrutamiento y registro de eventos.  
**Bajo acoplamiento:** No conoce la fuente de los eventos; solo los consume desde el bus.

---

#### C1–C5 → S6: IoTOrchestrator *(Process Service)*

**Descripción:** Orquesta procesos de negocio que cruzan múltiples capacidades. Es el sucesor directo de `IoTFacade` y expone las operaciones compuestas que el cliente (interfaz de consola, API externa) necesita. No posee lógica de negocio propia ni estado persistente.

**Justificación de diseño:**  
Sin un orquestador, los clientes deberían conocer y coordinar múltiples servicios, replicando reglas de negocio. Al centralizar la orquestación, se protege a los clientes de la complejidad interna y se garantiza la consistencia de los flujos de negocio end-to-end.

**Alta cohesión:** Solo contiene lógica de composición y orquestación.  
**Bajo acoplamiento:** No accede directamente a las bases de datos de ningún servicio; todo se comunica a través de contratos.

---

### 2.2 Tabla resumen: Capacidades → Servicios candidatos

| Capacidad | Servicio Candidato | Tipo (Erl) | Justificación de cohesión |
|-----------|-------------------|------------|--------------------------|
| C1 | S1 — IdentityService | Entity | Único responsable de usuarios y credenciales |
| C2 | S2 — DeviceRegistryService | Entity | Único responsable del inventario de activos |
| C3 | S3 — DeviceControlService | Task | Único responsable de ejecución de comandos |
| C4 | S4 — DeviceStatusService | Entity | Único responsable de la máquina de estados |
| C5 | S5 — NotificationService | Utility | Único responsable del enrutamiento de eventos |
| C1–C5 | S6 — IoTOrchestrator | Process | Composición de flujos end-to-end |

---

## 3. Diseño de la Arquitectura Jerárquica de Servicios (SOA)

La arquitectura jerárquica organiza los servicios en tres niveles de granularidad progresivamente mayor, siguiendo el modelo de capas de Erl. Cada nivel abstrae y agrupa las capacidades del nivel inferior.

### 3.1 Descripción de los niveles jerárquicos

---

#### Nivel 1 — Servicios de Entidad (Entity Services): Grano Fino

Representan las entidades de dominio fundamentales del sistema. Son altamente reutilizables, no dependen de otros servicios de negocio y poseen su propio almacén de datos.

| Servicio | Entidad de Dominio | Responsabilidad Principal |
|----------|--------------------|--------------------------|
| S1 — IdentityService | Usuario / Sesión | CRUD de usuarios + emisión y validación de JWT |
| S2 — DeviceRegistryService | Dispositivo | CRUD de dispositivos + asignación de identidad de red |
| S4 — DeviceStatusService | Estado del dispositivo | Persistencia y transición de estado operativo |

**Función del nivel:** Proveer acceso estructurado a los datos de dominio a través de contratos bien definidos. Ningún otro servicio accede directamente a las tablas de estas entidades; todo pasa por el contrato del servicio propietario.

---

#### Nivel 2 — Servicios de Tarea y Utilidad (Task & Utility Services): Grano Medio

Encapsulan operaciones de negocio concretas o funcionalidad transversal. Pueden consumir servicios de Nivel 1 para ejecutar su lógica.

| Servicio | Tipo | Responsabilidad Principal |
|----------|------|--------------------------|
| S3 — DeviceControlService | Task | Ejecución de comandos de actuación sobre dispositivos |
| S5 — NotificationService | Utility | Enrutamiento de eventos a canales de salida |

**Función del nivel:** Ejecutar operaciones de negocio que requieren acceder a los datos de las entidades del Nivel 1 o coordinar efectos secundarios (publicar eventos, enviar notificaciones). Estos servicios no poseen lógica de orquestación; su alcance es una sola tarea o función de soporte.

---

#### Nivel 3 — Servicios de Proceso (Process Services): Grano Grueso / End-to-End

Orquestan la colaboración entre múltiples servicios de los niveles anteriores para completar un flujo de negocio completo. Son el punto de entrada para los clientes externos.

| Servicio | Tipo | Responsabilidad Principal |
|----------|------|--------------------------|
| S6 — IoTOrchestrator | Process | Composición de operaciones cross-servicio: registro de dispositivos, control remoto, consultas de estado |

**Función del nivel:** Proveer operaciones de negocio de alto nivel que combinan múltiples llamadas a servicios de Nivel 1 y 2 en un único flujo coherente. El orquestador es el único componente que conoce la secuencia y las dependencias entre servicios.

---

### 3.2 Diagrama de jerarquía de servicios

```
╔══════════════════════════════════════════════════════════════════════╗
║  NIVEL 3 — PROCESO (End-to-End)                                      ║
║  ┌────────────────────────────────────────────────────────────────┐  ║
║  │                   S6 — IoTOrchestrator                         │  ║
║  │  (Orquesta flujos: registrar dispositivo, controlar, auditar)  │  ║
║  └────────────────────────────────────────────────────────────────┘  ║
╠══════════════════════════════════════════════════════════════════════╣
║  NIVEL 2 — TAREA / UTILIDAD (Grano Medio)                            ║
║  ┌────────────────────────────┐  ┌────────────────────────────────┐  ║
║  │  S3 — DeviceControlService │  │   S5 — NotificationService     │  ║
║  │  (Comandos de actuación)   │  │   (Enrutamiento de eventos)    │  ║
║  └────────────────────────────┘  └────────────────────────────────┘  ║
╠══════════════════════════════════════════════════════════════════════╣
║  NIVEL 1 — ENTIDAD (Grano Fino)                                       ║
║  ┌───────────────────┐  ┌──────────────────────┐  ┌──────────────┐   ║
║  │  S1 — Identity    │  │  S2 — DeviceRegistry │  │ S4 — Status  │   ║
║  │  Service          │  │  Service             │  │ Service      │   ║
║  │  (Usuarios/JWT)   │  │  (Inventario IoT)    │  │ (Máq.estado) │   ║
║  └───────────────────┘  └──────────────────────┘  └──────────────┘   ║
╚══════════════════════════════════════════════════════════════════════╝
```

---

## 4. Dependencias y Relaciones entre Servicios

### 4.1 Dependencias funcionales

Las dependencias entre servicios se clasifican en dos categorías según el modo de comunicación: **sincrónicas** (REST/HTTP, donde el caller espera respuesta) y **asincrónicas** (pub/sub sobre bus de eventos, donde el publisher no espera respuesta del subscriber).

#### Dependencias sincrónicas (REST/HTTP)

| Servicio Consumidor | Servicio Proveedor | Operación Consumida | Justificación |
|--------------------|--------------------|--------------------|-|
| S6 — IoTOrchestrator | S1 — IdentityService | `POST /sessions` (login), `GET /sessions/current` (validación de token) | El orquestador valida que el usuario esté autenticado antes de registrar dispositivos |
| S6 — IoTOrchestrator | S2 — DeviceRegistryService | `POST /devices`, `GET /devices`, `DELETE /devices/{id}` | El orquestador gestiona el inventario de activos a través de S2 |
| S6 — IoTOrchestrator | S3 — DeviceControlService | `POST /devices/{id}/commands/*` | El orquestador despacha comandos de usuario al servicio de control |
| S6 — IoTOrchestrator | S4 — DeviceStatusService | `GET /devices/{id}/status` | El orquestador consulta el estado actual de un dispositivo para mostrarlo al usuario |
| S3 — DeviceControlService | S2 — DeviceRegistryService | `GET /devices/{id}` | S3 debe verificar que el dispositivo existe y que implementa la capacidad requerida (`ISwitchable`, `IAlarm`, `IMonitorable`) antes de ejecutar el comando |

#### Dependencias asincrónicas (Bus de Eventos / Pub-Sub)

| Servicio Publisher | Canal del Bus | Servicio Subscriber | Evento | Acción del Subscriber |
|-------------------|--------------|--------------------|---------|-----------------------|
| S3 — DeviceControlService | `device.control.turned-on` | S4 — DeviceStatusService | `DeviceTurnedOn` | Transicionar estado a `Online` |
| S3 — DeviceControlService | `device.control.turned-off` | S4 — DeviceStatusService | `DeviceTurnedOff` | Transicionar estado a `Offline` |
| S3 — DeviceControlService | `device.control.alarm-triggered` | S5 — NotificationService | `AlarmTriggered` | Alertar al sistema de seguridad + log de auditoría |
| S3 — DeviceControlService | `device.control.recording-started` | S5 — NotificationService | `RecordingStarted` | Notificar a canal móvil + log de auditoría |
| S2 — DeviceRegistryService | `device.lifecycle.added` | S5 — NotificationService | `DeviceAdded` | Registrar en log de auditoría |
| S2 — DeviceRegistryService | `device.lifecycle.removed` | S5 — NotificationService | `DeviceRemoved` | Registrar en log de auditoría |

### 4.2 Dependencias de datos

Cada servicio es propietario exclusivo de su esquema de datos (patrón **Database-per-Service**). Ningún servicio accede directamente a la base de datos de otro; toda consulta de datos cruzados se realiza a través del contrato del servicio propietario.

| Servicio | Datos Propios | Tipo de Almacén Recomendado |
|----------|--------------|----------------------------|
| S1 — IdentityService | Tabla `Users` (id, name, email, passwordHash), Tabla `Sessions` (tokenId, userId, expiresAt) | Base de datos relacional (PostgreSQL / SQL Server) |
| S2 — DeviceRegistryService | Tabla `Devices` (id, type, name, ipAddress, ownerUserId, createdAt) | Base de datos relacional |
| S3 — DeviceControlService | Sin estado persistente propio | Sin almacén propio |
| S4 — DeviceStatusService | Tabla `StatusEvents` (deviceId, previousStatus, newStatus, timestamp) — modelo event-sourced | Base de datos de eventos / time-series |
| S5 — NotificationService | Tabla `AuditLog` (eventId, eventType, payload, receivedAt) | Base de datos documental o relacional |
| S6 — IoTOrchestrator | Sin estado persistente propio | Sin almacén propio |

### 4.3 Eliminación de duplicación de lógica y datos

El principal vector de duplicación en el monolito original reside en tres puntos críticos que la arquitectura SOA resuelve explícitamente:

1. **Lógica de autenticación duplicada:** En el monolito, `IoTFacade.isUserLoggedIn()` replica la verificación de sesión que también realiza `AuthService`. En la arquitectura SOA, esta lógica reside únicamente en S1. El gateway de API valida el JWT en el perímetro; ningún servicio interno repite esta verificación.

2. **Asignación de ID e IP no atómica:** En `ControladorIOT.addDevice`, la asignación de `nextDeviceId` y la construcción de `ipAddress` son operaciones no atómicas (race condition latente en escenarios multi-hilo). En S2, esta lógica se encapsula en un componente `IpAllocator` interno que opera dentro de una transacción de base de datos con restricción `UNIQUE` sobre `ipAddress`, eliminando la duplicación y el riesgo.

3. **Lógica de notificación en múltiples capas:** Los observadores `MobileNotifier`, `SecuritySystem` y `EventLogger` eran invocados directamente desde `ControladorIOT`, mezclando responsabilidades. En la arquitectura SOA, S3 y S2 solo publican eventos al bus; toda la lógica de enrutamiento y registro reside exclusivamente en S5.

### 4.4 Justificación de decisiones de acoplamiento

| Decisión | Justificación |
|----------|---------------|
| S3 depende sincrónicamente de S2 | S3 debe verificar la existencia y capacidades del dispositivo antes de ejecutar un comando. Esta dependencia es inevitable y se acepta; se mitiga con un timeout y circuit breaker en el cliente HTTP de S3. |
| S4 y S5 dependen del bus (asíncrono) | Al desacoplar los consumidores del bus del publisher (S3), se elimina la dependencia directa entre el sistema de control y el sistema de notificación/estado, replicando la intención original del patrón Observer pero en un paradigma distribuido. |
| S6 depende de S1, S2, S3, S4 | El orquestador es el único componente que asume la dependencia transitiva de todos los servicios. Este acoplamiento es deliberado y aceptado: S6 es la capa de composición y es el único que conoce los flujos de negocio completos. |

---

## 5. Definición de Contratos de Servicio (Contract First)

Todos los contratos se publican en el repositorio antes de iniciar cualquier implementación. Se utiliza **OpenAPI 3.0** para contratos REST/HTTP y **AsyncAPI 2.6** para contratos de eventos asíncronos.

---

### Contrato S1 — IdentityService

**Nombre:** IdentityService  
**Versión:** 1.0.0  
**Protocolo:** REST / HTTP + JSON  
**Emisor de token:** JWT firmado (HS256 / RS256)

| Operación | Método | Ruta | Entrada | Salida Exitosa | Errores |
|-----------|--------|------|---------|----------------|---------|
| Registrar usuario | POST | `/users` | `UserRegistration` {name, email, password} | `201 Created` → `User` {id, name, email} | `409 Conflict` (email duplicado) |
| Iniciar sesión | POST | `/sessions` | `Credentials` {email, password} | `200 OK` → `Token` {accessToken, expiresIn} | `401 Unauthorized` |
| Cerrar sesión | DELETE | `/sessions/{sessionId}` | `sessionId` (path param) | `204 No Content` | `404 Not Found` |
| Validar sesión | GET | `/sessions/current` | `Authorization: Bearer {token}` (header) | `200 OK` → `SessionInfo` {userId, email, expiresAt} | `401 Unauthorized` |

**Esquemas de datos:**
```
UserRegistration: { name: string, email: string(email), password: string(min:8) }
User:             { id: uuid, name: string, email: string }
Credentials:      { email: string, password: string }
Token:            { accessToken: string, expiresIn: integer }
SessionInfo:      { userId: uuid, email: string, expiresAt: datetime }
```

---

### Contrato S2 — DeviceRegistryService

**Nombre:** DeviceRegistryService  
**Versión:** 1.0.0  
**Protocolo:** REST / HTTP + JSON

| Operación | Método | Ruta | Entrada | Salida Exitosa | Errores |
|-----------|--------|------|---------|----------------|---------|
| Registrar dispositivo | POST | `/devices` | `DeviceRegistration` {type, name, config} | `201 Created` → `Device` {id, type, name, ipAddress, ownerUserId} | `400 Bad Request`, `401 Unauthorized` |
| Consultar todos los dispositivos | GET | `/devices` | — (token en header) | `200 OK` → `Device[]` | `401 Unauthorized` |
| Consultar un dispositivo | GET | `/devices/{id}` | `id` (path param) | `200 OK` → `Device` | `404 Not Found` |
| Eliminar dispositivo | DELETE | `/devices/{id}` | `id` (path param) | `204 No Content` | `404 Not Found`, `401 Unauthorized` |
| Asignar dirección de red | PATCH | `/devices/{id}/network` | `NetworkAssignment` {ipAddress} | `200 OK` → `Device` actualizado | `409 Conflict` (IP en uso) |

**Esquemas de datos:**
```
DeviceRegistration: { type: enum[CAMERA,SMARTLIGHT,ALARM], name: string, config: object }
Device:             { id: uuid, type: string, name: string, ipAddress: string, ownerUserId: uuid, createdAt: datetime }
NetworkAssignment:  { ipAddress: string(pattern: 192.168.1.\d+) }
```

**Notas del contrato:** El campo `config` acepta propiedades extendidas según el tipo de dispositivo:
- `CAMERA`: `{}` (sin configuración adicional en registro)
- `SMARTLIGHT`: `{ color: string, schedule: string }`
- `ALARM`: `{}` (sin configuración adicional en registro)

---

### Contrato S3 — DeviceControlService

**Nombre:** DeviceControlService  
**Versión:** 1.0.0  
**Protocolo:** REST / HTTP + JSON (entrada) + AsyncAPI / Bus de eventos (salida)

| Operación | Método | Ruta | Entrada | Salida Exitosa | Errores |
|-----------|--------|------|---------|----------------|---------|
| Encender dispositivo | POST | `/devices/{id}/commands/turn-on` | `id` (path param) | `202 Accepted` → `CommandResult` {commandId, status} | `404 Not Found`, `409 Conflict` (ya encendido) |
| Apagar dispositivo | POST | `/devices/{id}/commands/turn-off` | `id` (path param) | `202 Accepted` → `CommandResult` | `404 Not Found` |
| Activar alarma | POST | `/devices/{id}/commands/trigger-alarm` | `id` (path param) | `202 Accepted` → `CommandResult` | `404 Not Found`, `422 Unprocessable Entity` (no es alarma) |
| Iniciar grabación | POST | `/devices/{id}/commands/start-recording` | `id` (path param) | `202 Accepted` → `CommandResult` | `404 Not Found`, `422 Unprocessable Entity` (no es cámara) |

**Esquemas de datos:**
```
CommandResult: { commandId: uuid, deviceId: uuid, commandType: string, status: enum[ACCEPTED,REJECTED], timestamp: datetime }
```

**Eventos publicados al bus:**
```
DeviceTurnedOn:      { deviceId, commandId, timestamp }
DeviceTurnedOff:     { deviceId, commandId, timestamp }
AlarmTriggered:      { deviceId, commandId, timestamp }
RecordingStarted:    { deviceId, commandId, timestamp }
```

---

### Contrato S4 — DeviceStatusService

**Nombre:** DeviceStatusService  
**Versión:** 1.0.0  
**Protocolo:** REST / HTTP + JSON (consultas) + AsyncAPI (suscripción a eventos)

| Operación | Método | Ruta | Entrada | Salida Exitosa | Errores |
|-----------|--------|------|---------|----------------|---------|
| Consultar estado actual | GET | `/devices/{id}/status` | `id` (path param) | `200 OK` → `StatusInfo` {deviceId, currentStatus, since} | `404 Not Found` |
| Registrar transición de estado | POST | `/devices/{id}/status/transitions` | `StatusTransition` {event: connect/disconnect/fault} | `201 Created` → `StatusEvent` {id, previousStatus, newStatus, timestamp} | `404 Not Found`, `409 Conflict` (transición inválida) |

**Esquemas de datos:**
```
StatusInfo:       { deviceId: uuid, currentStatus: enum[ONLINE,OFFLINE,ERROR], since: datetime }
StatusTransition: { event: enum[connect,disconnect,fault] }
StatusEvent:      { id: uuid, deviceId: uuid, previousStatus: string, newStatus: string, timestamp: datetime }
```

**Suscripción a eventos del bus:**
- Canal `device.control.turned-on` → ejecuta transición `connect`
- Canal `device.control.turned-off` → ejecuta transición `disconnect`

---

### Contrato S5 — NotificationService

**Nombre:** NotificationService  
**Versión:** 1.0.0  
**Protocolo:** AsyncAPI (suscripción) + REST / HTTP + JSON (consulta de auditoría)

| Operación | Método | Ruta | Entrada | Salida Exitosa | Errores |
|-----------|--------|------|---------|----------------|---------|
| Consultar log de auditoría | GET | `/audit/events` | `?from=datetime&to=datetime&deviceId=uuid` | `200 OK` → `AuditEvent[]` | `400 Bad Request` |
| Consultar evento de auditoría específico | GET | `/audit/events/{eventId}` | `eventId` (path param) | `200 OK` → `AuditEvent` | `404 Not Found` |

**Esquemas de datos:**
```
AuditEvent: { eventId: uuid, eventType: string, deviceId: uuid, payload: object, receivedAt: datetime, channel: enum[MOBILE,SECURITY,LOG] }
```

**Suscripciones al bus:**
- `device.lifecycle.added` → log de auditoría
- `device.lifecycle.removed` → log de auditoría
- `device.control.alarm-triggered` → sistema de seguridad + log de auditoría
- `device.control.recording-started` → notificación móvil + log de auditoría
- `device.control.turned-on` → log de auditoría
- `device.control.turned-off` → log de auditoría

---

### Contrato S6 — IoTOrchestrator

**Nombre:** IoTOrchestrator  
**Versión:** 1.0.0  
**Protocolo:** REST / HTTP + JSON

| Operación | Método | Ruta | Entrada | Salida Exitosa | Errores |
|-----------|--------|------|---------|----------------|---------|
| Registrar usuario (flujo completo) | POST | `/iot/users` | `UserRegistration` | `201 Created` → `User` | `409 Conflict` |
| Iniciar sesión | POST | `/iot/sessions` | `Credentials` | `200 OK` → `Token` | `401 Unauthorized` |
| Cerrar sesión | DELETE | `/iot/sessions/current` | (token en header) | `204 No Content` | `401 Unauthorized` |
| Registrar dispositivo (flujo completo) | POST | `/iot/devices` | `DeviceRegistration` | `201 Created` → `Device` | `401 Unauthorized`, `400 Bad Request` |
| Listar dispositivos | GET | `/iot/devices` | — | `200 OK` → `Device[]` | `401 Unauthorized` |
| Encender dispositivo | POST | `/iot/devices/{id}/turn-on` | `id` (path param) | `202 Accepted` → `CommandResult` | `404 Not Found` |
| Apagar dispositivo | POST | `/iot/devices/{id}/turn-off` | `id` (path param) | `202 Accepted` → `CommandResult` | `404 Not Found` |
| Activar alarma | POST | `/iot/devices/{id}/trigger-alarm` | `id` (path param) | `202 Accepted` → `CommandResult` | `404 Not Found`, `422` |
| Iniciar grabación | POST | `/iot/devices/{id}/start-recording` | `id` (path param) | `202 Accepted` → `CommandResult` | `404 Not Found`, `422` |
| Eliminar dispositivo | DELETE | `/iot/devices/{id}` | `id` (path param) | `204 No Content` | `404 Not Found` |
| Consultar estado de dispositivo | GET | `/iot/devices/{id}/status` | `id` (path param) | `200 OK` → `StatusInfo` | `404 Not Found` |

---

## 6. Catálogo de Servicios

El catálogo de servicios constituye el inventario oficial y versionado de todos los servicios que conforman la arquitectura SOA del sistema UMLIoT. Sirve como referencia formal para equipos de desarrollo, integración y gobernanza.

### 6.1 Catálogo completo

| ID | Nombre del Servicio | Capacidad Asociada | Tipo (Erl) | Protocolo | Operaciones Principales | Entrada | Salida | SLA | Dueño |
|----|--------------------|--------------------|------------|-----------|------------------------|---------|--------|-----|-------|
| S1 | IdentityService | C1 — Gestión de Identidad y Acceso | Entity Service | REST/HTTP + JWT | RegisterUser, Login, Logout, ValidateSession | UserRegistration, Credentials | User, Token, SessionInfo | 99.9%, p95 < 200ms | Equipo Identity |
| S2 | DeviceRegistryService | C2 — Inventario de Dispositivos | Entity Service | REST/HTTP | RegisterDevice, GetDevice, GetAllDevices, RemoveDevice, AssignNetwork | DeviceRegistration, id | Device, Device[] | 99.5%, p95 < 300ms | Equipo IoT |
| S3 | DeviceControlService | C3 — Control y Actuación | Task Service | REST/HTTP + Pub/Sub | TurnOn, TurnOff, TriggerAlarm, StartRecording | id (path param) | CommandResult + Eventos | 99.0%, p95 < 500ms | Equipo IoT |
| S4 | DeviceStatusService | C4 — Monitoreo y Estado | Entity Service | REST/HTTP + Sub | GetStatus, RegisterTransition | id, StatusTransition | StatusInfo, StatusEvent | 99.5%, p95 < 200ms | Equipo IoT |
| S5 | NotificationService | C5 — Notificación y Auditoría | Utility Service | Sub + REST/HTTP | GetAuditEvents, GetAuditEvent | eventId, filtros | AuditEvent[] | 99.0%, best-effort | Equipo Plataforma |
| S6 | IoTOrchestrator | C1–C5 (Composición) | Process Service | REST/HTTP | RegisterUser, Login, RegisterDevice, ControlDevice, GetStatus, etc. | Varios (ver §5) | Varios (ver §5) | 99.0%, p95 < 800ms | Equipo IoT |

### 6.2 Catálogo de eventos del bus de mensajes

| Canal | Tipo de Evento | Publisher | Subscribers | Esquema del Mensaje |
|-------|---------------|-----------|-------------|---------------------|
| `device.lifecycle.added` | `DeviceAdded` | S2 | S5 | `{deviceId, type, name, ownerUserId, timestamp}` |
| `device.lifecycle.removed` | `DeviceRemoved` | S2 | S5 | `{deviceId, timestamp}` |
| `device.control.turned-on` | `DeviceTurnedOn` | S3 | S4, S5 | `{deviceId, commandId, timestamp}` |
| `device.control.turned-off` | `DeviceTurnedOff` | S3 | S4, S5 | `{deviceId, commandId, timestamp}` |
| `device.control.alarm-triggered` | `AlarmTriggered` | S3 | S5 | `{deviceId, commandId, timestamp}` |
| `device.control.recording-started` | `RecordingStarted` | S3 | S5 | `{deviceId, commandId, timestamp}` |

### 6.3 Catálogo de datos propios por servicio

| Servicio | Entidad de Datos | Almacén | Patrón de Persistencia |
|----------|-----------------|---------|----------------------|
| S1 — IdentityService | Users, Sessions | RDBMS | CRUD convencional |
| S2 — DeviceRegistryService | Devices | RDBMS | CRUD + transacción de asignación de red |
| S4 — DeviceStatusService | StatusEvents | Event Store / RDBMS | Event Sourcing; estado actual derivado |
| S5 — NotificationService | AuditLog | RDBMS / Documental | Append-only; inmutable |

---

## 7. Vista Final de la Arquitectura SOA

### 7.1 Organización por capas

La arquitectura SOA final del sistema UMLIoT se organiza en cuatro capas horizontales, más una capa transversal de infraestructura:

| Capa | Componentes | Función |
|------|-------------|---------|
| **Capa de Presentación / Cliente** | Interfaz de consola, cliente web (futuro) | Punto de interacción del usuario final. Se comunica únicamente con S6 a través del API Gateway. |
| **Capa de Acceso / API Gateway** | Gateway (Kong / YARP / Ocelot) | Valida tokens JWT, aplica rate-limiting, enruta solicitudes a S6 y registra trazas de acceso. |
| **Capa de Orquestación** | S6 — IoTOrchestrator | Compone los flujos de negocio end-to-end consumiendo S1, S2, S3 y S4 mediante REST. |
| **Capa de Servicios de Dominio** | S1, S2, S3, S4, S5 | Servicios autónomos con responsabilidades únicas. S3 y S2 publican al bus; S4 y S5 suscriben. |
| **Capa de Infraestructura** | Bus de eventos, bases de datos, service registry, observabilidad | Provee la plataforma de comunicación asíncrona, persistencia, descubrimiento y monitoreo. |

### 7.2 Diagrama de arquitectura final

```
╔══════════════════════════════════════════════════════════════════════════════╗
║  CLIENTE (Consola / Web)                                                     ║
║  ┌──────────────────────────────────────────────────────────────────────┐   ║
║  │  Interfaz de usuario (menú CLI o API cliente)                        │   ║
║  └──────────────────────────┬───────────────────────────────────────────┘   ║
╚═══════════════════════════╦═╩════════════════════════════════════════════════╝
                             ║ HTTPS
╔════════════════════════════╩═════════════════════════════════════════════════╗
║  API GATEWAY (Kong / YARP / Ocelot)                                          ║
║  Validación JWT · Rate-limiting · Routing · Trazas de acceso                ║
╚════════════════════════════╦═════════════════════════════════════════════════╝
                             ║ HTTP interno
╔════════════════════════════╩═════════════════════════════════════════════════╗
║  CAPA DE ORQUESTACIÓN                                                        ║
║  ┌──────────────────────────────────────────────────────────────────────┐   ║
║  │                    S6 — IoTOrchestrator                              │   ║
║  │  POST /iot/devices · POST /iot/sessions · POST /iot/devices/{id}/... │   ║
║  └──────┬──────────────────┬────────────────────┬──────────────────┬────┘   ║
╚═════════╪══════════════════╪════════════════════╪══════════════════╪════════╝
          ║ REST             ║ REST               ║ REST             ║ REST
╔═════════╩═════╗  ╔═════════╩══════════╗  ╔══════╩═══════╗  ╔══════╩══════════╗
║ S1 Identity   ║  ║ S2 DeviceRegistry  ║  ║ S3 Device    ║  ║ S4 DeviceStatus ║
║ Service       ║  ║ Service            ║  ║ Control      ║  ║ Service         ║
║               ║  ║                   ║  ║ Service      ║  ║                 ║
║ POST /users   ║  ║ POST /devices      ║  ║ POST /cmd/*  ║  ║ GET /status     ║
║ POST /sessions║  ║ GET /devices       ║  ║              ║  ║ POST /transitions║
║ DEL /sessions ║  ║ DEL /devices/{id}  ║  ║  ↓ REST      ║  ║                 ║
║ GET /sessions ║  ║                   ║  ║  S2 (verify) ║  ║                 ║
║               ║  ║                   ║  ║              ║  ║                 ║
║ [Users DB]    ║  ║ [Devices DB]      ║  ║  ↓ PUBLICA   ║  ║ [Status DB]    ║
╚═══════════════╝  ╚═══════════════════╝  ╚══════╦═══════╝  ╚═════════╦═══════╝
                          ║ PUBLICA                ║ EVENTOS             ║ SUSCRIBE
                          ╚════════════════════════╩═════════════╗       ║
                                                                  ║       ║
╔═════════════════════════════════════════════════╩═══════════════╩═══════╝═════╗
║  BUS DE EVENTOS (RabbitMQ / Azure Service Bus / Kafka)                        ║
║  Canales: device.lifecycle.* · device.control.*                               ║
╚═════════════════════════════════╦════════════════════════════════════════════╝
                                  ║ SUSCRIBE
                   ╔══════════════╩═════════════╗
                   ║  S5 — NotificationService   ║
                   ║                            ║
                   ║  MobileNotifier            ║
                   ║  SecuritySystem            ║
                   ║  EventLogger               ║
                   ║                            ║
                   ║  GET /audit/events         ║
                   ║  [Audit Log DB]            ║
                   ╚════════════════════════════╝

╔══════════════════════════════════════════════════════════════════════════════╗
║  INFRAESTRUCTURA TRANSVERSAL                                                 ║
║  Service Registry (Consul)  ·  OpenTelemetry + Jaeger  ·  Prometheus        ║
║  mTLS entre servicios  ·  OAuth2/OIDC via S1  ·  Docker / Kubernetes        ║
╚══════════════════════════════════════════════════════════════════════════════╝
```

### 7.3 Interacción entre servicios: flujos principales

#### Flujo 1 — Registro de dispositivo (operación compuesta)

```
Cliente → S6: POST /iot/devices {type, name, config}
  S6 → S1: GET /sessions/current  [validar token]
  S1 → S6: 200 OK SessionInfo
  S6 → S2: POST /devices {type, name, config, ownerUserId}
  S2 → S2: asignar ID + IP (transacción atómica)
  S2 → Bus: publicar DeviceAdded
  S2 → S6: 201 Created Device
  Bus → S5: consume DeviceAdded → AuditLog
  S6 → Cliente: 201 Created Device
```

#### Flujo 2 — Encendido de dispositivo (operación de control)

```
Cliente → S6: POST /iot/devices/{id}/turn-on
  S6 → S3: POST /devices/{id}/commands/turn-on
  S3 → S2: GET /devices/{id}  [verificar capacidad ISwitchable]
  S2 → S3: 200 OK Device
  S3 → Bus: publicar DeviceTurnedOn
  S3 → S6: 202 Accepted CommandResult
  Bus → S4: consume DeviceTurnedOn → transición a Online
  Bus → S5: consume DeviceTurnedOn → AuditLog
  S6 → Cliente: 202 Accepted CommandResult
```

#### Flujo 3 — Consulta de estado (operación de lectura)

```
Cliente → S6: GET /iot/devices/{id}/status
  S6 → S4: GET /devices/{id}/status
  S4 → S6: 200 OK StatusInfo {ONLINE, since: ...}
  S6 → Cliente: 200 OK StatusInfo
```

### 7.4 Principios SOA aplicados en la arquitectura final

| Principio SOA (Thomas Erl) | Materialización en UMLIoT |
|---------------------------|--------------------------|
| **Standardized Service Contract** | Todos los contratos definidos en OpenAPI 3.0 / AsyncAPI 2.6 antes de implementar. Versionados en `/contracts/` del repositorio. |
| **Service Loose Coupling** | Cada servicio se comunica solo a través de su contrato público. Los observadores migran a pub/sub asíncrono eliminando dependencias directas. |
| **Service Abstraction** | Los internals (factories, máquina de estados, patrón Command) quedan ocultos detrás de los contratos REST. Ningún consumidor accede a las clases internas. |
| **Service Reusability** | S1 (IdentityService) y S5 (NotificationService) son agnósticos al dominio IoT y pueden ser reutilizados por otros sistemas. |
| **Service Autonomy** | Cada servicio tiene su propio runtime, base de datos, ciclo de despliegue y equipo responsable. Elimina la lista en memoria compartida del monolito. |
| **Service Statelessness** | El estado de sesión sale del proceso (JWT en S1). El estado del dispositivo persiste en S4 con event sourcing, no en memoria. |
| **Service Composability** | S6 (IoTOrchestrator) compone S1 + S2 + S3 + S4 + S5 para operaciones de negocio end-to-end, replicando el rol de `IoTFacade` en SOA. |
| **Service Discoverability** | Catálogo formal de servicios (§6) + service registry (Consul) + documentación OpenAPI accesible en el gateway. |

### 7.5 Aspectos transversales (cross-cutting concerns)

| Aspecto | Decisión Arquitectónica |
|---------|------------------------|
| **Seguridad** | OAuth2/OIDC + JWT emitido por S1. El API Gateway valida tokens en el perímetro. Comunicación interna por mTLS o red privada. |
| **Observabilidad** | OpenTelemetry en todos los servicios; trazas correlacionadas por `traceparent` en headers HTTP. Logs estructurados (Serilog → ELK). Métricas en Prometheus + Grafana. |
| **Resiliencia** | Polly en clientes HTTP del orquestador: retry con backoff exponencial, circuit breaker, timeout. Bus de eventos con Dead Letter Queue y outbox pattern. |
| **Versionado de contratos** | Semantic Versioning. Cambios backward-compatible dentro del mismo major. Breaking changes → nueva ruta `/v2/`. Contratos son artefactos inmutables una vez publicados. |
| **Despliegue** | Cada servicio con su `Dockerfile`, pipeline CI/CD independiente y health endpoint (`/health`). Docker Compose para desarrollo local; Kubernetes para producción. |
| **Consistencia de datos** | Patrón Database-per-Service. Saga pattern (coreografía vía bus de eventos) para transacciones distribuidas. Se acepta consistencia eventual entre S2 (registro) y S4 (estado). |
| **Escalabilidad** | S3 y S5 son stateless y horizontalmente escalables. S2 escala con réplicas de lectura. El bus de eventos absorbe picos de carga en notificaciones. |

---

*Documento generado conforme a la metodología SOA de Thomas Erl. Todos los servicios, contratos y flujos se derivan exclusivamente de las capacidades organizacionales identificadas en el sistema UMLIoT.*
