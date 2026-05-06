# IOT (Sistema de Gestión de Internet de las Cosas)

## Descripción General
Este proyecto es un sistema de gestión avanzada de dispositivos de Internet de las Cosas (IoT) desarrollado en C# (.NET 8). El sistema permite registrar usuarios, gestionar dispositivos (cámaras, luces inteligentes, alarmas) y controlar su estado a nivel de red y comportamiento a través de una interfaz de consola interactiva.

El proyecto está diseñado con un fuerte enfoque en buenas prácticas de Ingeniería de Software (principios SOLID) y Arquitectura, destacando una clara separación de responsabilidades dentro de su estructura orientada al dominio y la aplicación de múltiples patrones de diseño.

## Arquitectura y Patrones de Diseño
Para garantizar la escalabilidad, bajo acoplamiento y fácil mantenimiento, el sistema hace un uso intensivo de los siguientes patrones de diseño (GoF):

- **Facade (`IoTFacade`)**: Proporciona una interfaz unificada y simplificada que oculta la complejidad del subsistema subyacente interactuando con las diversas funcionalidades de dispositivos y usuarios de manera centralizada.
- **Observer (`DeviceEventManager` y Observadores)**: Sistema de notificaciones reactivo que permite que varios componentes (`MobileNotifier`, `SecuritySystem`, `EventLogger`) escuchen y reaccionen automáticamente a los eventos generados por los cambios en los dispositivos.
- **Factory Method (`DeviceCreator`, `UserCreator`)**: Centraliza y estandariza la instanciación de entidades. Existen creadores concretos (`AlarmCreator`, `CameraCreator`, `SmartlightCreator`) lo cual facilita la extensión del código para añadir nuevos dispositivos sin impactar el código existente.
- **Command (`ICommand`, `TurnOnCommand`, etc.)**: Encapsula todas las peticiones y operaciones sobre los dispositivos (ej. `TurnOffCommand`, `StartRecordingCommand`, `TriggerAlarmCommand`) como objetos. Esto desacopla el objeto que invoca la operación de aquél que tiene el conocimiento para realizarla.
- **State (`DeviceStatus`, `OnlineStatus`, `OfflineStatus`, `ErrorStatus`)**: Representa y maneja el comportamiento interno del dispositivo según su estado de conexión, aislando las lógicas propias de cada estado y facilitando transiciones fluidas.

## Core / Dominio (Dispositivos y Usuarios)

### Interfaces de Dispositivos (`Core.Devices`)
Sigue el principio de Segregación de Interfaces al dividir las responsabilidades según la capacidad:
- `IDevice`: Contrato base de todo dispositivo IoT.
- `ISwitchable`: Dispositivos cuyo estado energético pueda ser alterado.
- `IMonitorable`: Dispositivos con capacidades avanzadas de monitoreo de red (ej. IP y estado).
- `IAlarm`: Capacidad específica de disparo y sistema de alertas.

### Tipos de Dispositivos
- **Cámara (`Camera`)**: Implementa monitoreo con IP generada de forma autónoma y funcionalidad de inicio/detención de grabación.
- **Luz Inteligente (`Smartlight`)**: Permite la personalización visual (configuración de color) y la programación de encendido mediante un `schedule`.
- **Alarma (`Alarm`)**: Sistema disuasorio y de notificación crítica diseñado para ser atado al sistema de seguridad.

### Gestión de Usuarios (`Core.Users`)
Módulo independiente que maneja los registros y sesiones:
- `User`: Entidad de dominio de usuario.
- `AuthService` y `UserRepository`: Aislamiento de la lógica de negocio de autenticación y la persistencia en memoria, promoviendo mayor mantenibilidad.

## Características Funcionales de la Aplicación
- **Autenticación en Consola**: Sistema de registro y login. Las operaciones sobre dispositivos requieren una sesión activa.
- **Registro Flexible**: Parámetros dinámicos según el tipo de dispositivo registrado (ej. las luces inteligentes solicitan parámetros adicionales como `color` y `schedule`).
- **Control Unificado**: Comandos unificados en consola para la administración del estado, encendido, apagado, disparo de alarmas y captura de grabaciones.

## Cómo ejecutar

1. Asegúrate de tener instalado el SDK de **.NET 8**.
2. Clona el repositorio a través de Git:
   ```bash
   git clone https://github.com/AndresNavarrom/IOT.git
   ```
3. Abre una terminal (como PowerShell o Bash) en la raíz del proyecto.
4. Ejecuta el comando de construcción y arranque:
   ```bash
   dotnet run
   ```
5. Sigue las instrucciones del menú interactivo en la consola.

## Menú de Comandos en Consola

Al ejecutar la aplicación, dispondrás de un menú completo (del 0 al 11) que se ejecuta en ciclo (loop iterativo) para que puedas manipular el sistema en un mismo entorno de prueba:

1. **Registrar usuario**
2. **Login**
3. **Registrar dispositivo** (Requiere login)
4. **Ver dispositivos**
5. **Turn on device** (Encender mediante Command)
6. **Turn off device** (Apagar mediante Command)
7. **Activate alarm**
8. **Start recording** (Específico para cámaras)
9. **Remove device**
10. **Get device status**
11. **Logout**
0. **Salir**
