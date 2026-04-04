# Sistema de Agendamiento de Citas Médicas

Este proyecto es una aplicación web completa para la gestión de citas médicas, diseñada para facilitar la interacción entre pacientes y doctores. Permite a los pacientes buscar médicos por especialidad, ver disponibilidades y agendar citas, mientras que los médicos pueden gestionar sus horarios y consultar reportes de sus consultas.

## 1. Guía de Ejecución

Sigue estos pasos para levantar el proyecto completo en tu entorno local utilizando Docker.

### Prerrequisitos

- [Git](https://git-scm.com/)
- [Docker](https://www.docker.com/products/docker-desktop/)
- [Docker Compose](https://docs.docker.com/compose/install/)

### Pasos para la Instalación

1.  **Clonar el Repositorio**
    Abre tu terminal y clona el repositorio del proyecto en tu máquina local.

    ```bash
    git clone <URL-del-repositorio>
    cd <nombre-del-directorio-del-proyecto>
    ```

2.  **Configurar Variables de Entorno (Opcional)**
    El proyecto está preconfigurado para un entorno de desarrollo. La base de datos y las credenciales de la API se gestionan a través de Docker Compose y los archivos de configuración del backend.

    -   **Backend**: La cadena de conexión a la base de datos y la configuración de JWT se encuentran en `backend/Scheduling.Api/appsettings.json`.
    -   **Email**: Para el envío de correos, puedes usar el servicio simulado (`MockEmailService`) o configurar un proveedor real. Por defecto, `Email:UseMock` está en `false`. Para usar el servicio real, configura tus credenciales SMTP en `appsettings.json` como se indica en `EMAIL_SETUP.md`.

3.  **Levantar los Contenedores con Docker Compose**
    Desde la raíz del proyecto (donde se encuentra el archivo `docker-compose.yml`), ejecuta el siguiente comando. Este comando construirá las imágenes de la aplicación de frontend y backend, y levantará los servicios, incluyendo la base de datos SQL Server.

    ```bash
    docker-compose up --build
    ```

    -   `--build`: Fuerza la reconstrucción de las imágenes si has realizado cambios en el código.
    -   El script `entrypoint.sh` del backend se encargará de esperar a que la base de datos esté lista y de ejecutar un script de inicialización (`script.sql`) para crear las tablas y datos necesarios.

4.  **Acceder a la Aplicación**
    Una vez que los contenedores estén en ejecución, podrás acceder a la aplicación:

    -   **Frontend (Aplicación de Pacientes/Médicos)**: Abre tu navegador y ve a `http://localhost:5173`.
    -   **Backend (API)**: La API estará disponible en `http://localhost:5001`.

### Cuentas de Prueba

-   **Paciente**: Puedes registrar un nuevo usuario desde la interfaz de login.
-   **Médico**: El sistema requiere que un médico esté pre-registrado en la base de datos y asociado a un `User`. El script de inicialización (`script.sql`) debería crear un usuario de prueba que sea médico.

---

## 2. Información Detallada del Proyecto

### Arquitectura General

El proyecto sigue una arquitectura de microservicios contenerizados, desacoplando el frontend, el backend y la base de datos.

-   **Frontend**: Una Single Page Application (SPA) desarrollada con **React**. Se encarga de toda la interfaz de usuario y la experiencia de navegación. Se comunica con el backend a través de una API REST.
-   **Backend**: Una API RESTful construida con **ASP.NET Core**. Maneja toda la lógica de negocio, la autenticación de usuarios, la gestión de datos y la comunicación con servicios externos (email, PDF).
-   **Base de Datos**: **Microsoft SQL Server**, ejecutándose en su propio contenedor de Docker. La persistencia de datos se gestiona con Entity Framework Core.
-   **Contenerización**: **Docker** y **Docker Compose** orquestan todos los servicios, asegurando un entorno de desarrollo y despliegue consistente y aislado.

### Tecnologías Utilizadas

| Componente | Tecnología | Descripción |
| :--- | :--- | :--- |
| **Backend** | C#, ASP.NET Core | Framework para construir la API RESTful. |
| | Entity Framework Core | ORM para la interacción con la base de datos SQL Server. |
| | JWT (JSON Web Tokens) | Para la autenticación y autorización segura de los endpoints. |
| | QuestPDF | Librería para la generación dinámica de documentos PDF. |
| | MailKit | Para el envío de correos electrónicos de confirmación. |
| | BCrypt.Net | Para el hashing seguro de las contraseñas de los usuarios. |
| **Frontend** | React | Librería para construir la interfaz de usuario interactiva. |
| | React Router | Para la gestión de rutas y navegación en la SPA. |
| | JavaScript (ES6+) | Lenguaje principal para la lógica del frontend. |
| | CSS (Inline Styles) | Para el diseño y la presentación visual de los componentes. |
| **Base de Datos** | Microsoft SQL Server | Sistema de gestión de bases de datos relacional. |
| **DevOps** | Docker, Docker Compose | Para la contenerización y orquestación de los servicios. |

### Funcionalidades Clave

#### Módulo de Autenticación
-   Registro de nuevos usuarios (pacientes).
-   Inicio de sesión con credenciales y obtención de un token JWT.
-   Protección de rutas y endpoints mediante el token JWT.
-   Actualización de perfil de usuario (email, contraseña, fecha de nacimiento).

#### Panel del Paciente
-   **Gestión de Citas**:
    -   Búsqueda de médicos por especialidad.
    -   Visualización de la disponibilidad de los médicos en un calendario.
    -   Agendamiento de una nueva cita en un horario disponible.
    -   Reprogramación de una cita existente seleccionando un nuevo horario.
    -   Cancelación de citas pendientes.
-   **Historial de Citas**:
    -   Visualización de todas las citas (pasadas y futuras).
    -   Filtros por estado (Pendiente, Confirmada, Cancelada), fecha y especialidad.
    -   Descarga de un comprobante de la cita en formato PDF.

#### Panel del Médico (Pestaña "Ajustes")
-   **Gestión de Disponibilidad**:
    -   Registro de bloques de tiempo disponibles (fecha, hora de inicio, hora de fin).
    -   Definición de la duración de cada consulta.
    -   Eliminación de disponibilidades registradas.
-   **Reportes de Citas**:
    -   Visualización de una grilla con todas las citas asignadas.
    -   Filtros por rango de fechas, nombre del paciente y estado de la cita.
    -   Descarga de un reporte completo de citas en formato PDF.

#### Servicios Adicionales
-   **Notificaciones por Email**: Envío automático de un correo de confirmación al paciente al agendar o reprogramar una cita, adjuntando un PDF con los detalles.
-   **Generación de PDFs**: Creación dinámica de PDFs tanto para la confirmación de citas individuales como para los reportes de los médicos.

### Estructura de la Base de Datos

El modelo de datos se gestiona con Entity Framework Core y se compone de las siguientes entidades principales:

-   `User`: Almacena la información de los usuarios registrados (pacientes y médicos), incluyendo credenciales.
-   `Medico`: Contiene información específica de los médicos, como su especialidad y datos de contacto. Está vinculado a un `User`.
-   `Especialidad`: Define las especialidades médicas disponibles.
-   `Cita`: Representa una cita agendada. Vincula a un `Paciente` (User) y un `Medico`. Contiene fecha, hora y estado.
-   `DisponibilidadMedica`: Almacena los bloques de tiempo en los que un médico está disponible para atender citas.

### Endpoints de la API

La API expone varios endpoints para gestionar la lógica de la aplicación. Todos los endpoints, excepto `auth/login` y `auth/register`, requieren un token JWT válido.

#### Autenticación (`/auth`)
-   `POST /register`: Registra un nuevo usuario.
-   `POST /login`: Autentica a un usuario y devuelve un token JWT.
-   `GET /me`: Obtiene la información del perfil del usuario autenticado.
-   `PUT /me`: Actualiza la información del perfil del usuario autenticado.

#### Citas y Relacionados (`/citas`)
-   `GET /`: Obtiene las citas del paciente autenticado.
-   `POST /`: Crea una nueva cita.
-   `PUT /{id}`: Reprograma una cita existente.
-   `PUT /{id}/cancelar`: Cancela una cita.
-   `GET /{id}/pdf`: Descarga el comprobante de una cita en PDF.
-   `GET /especialidades`: Obtiene la lista de todas las especialidades.
-   `GET /medicos-por-especialidad`: Obtiene los médicos de una especialidad específica.
-   `GET /disponibilidad`: Obtiene los horarios disponibles de un médico.
-   `GET /citas-ocupadas`: Obtiene los horarios ya ocupados de un médico.

#### Endpoints para Médicos (`/citas/medico`)
-   `GET /me`: Obtiene la información del perfil del médico autenticado.
-   `GET /disponibilidades`: Obtiene las disponibilidades registradas por el médico.
-   `POST /disponibilidades`: Crea un nuevo bloque de disponibilidad.
-   `DELETE /disponibilidades/{id}`: Elimina un bloque de disponibilidad.
-   `GET /reporte`: Obtiene un reporte de citas con filtros.
-   `GET /reporte/pdf`: Descarga el reporte de citas en formato PDF.

---