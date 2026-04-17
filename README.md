# EduPath AI - Sistema de Evaluacion Estudiantil

Aplicacion web desarrollada en ASP.NET Core MVC (.NET 8) con Entity Framework Core 8 y SQL Server. Permite a docentes gestionar estudiantes, registrar notas parciales y visualizar el rendimiento academico mediante reportes y predicciones con ML.NET.

## Funcionalidades

- Autenticacion de usuarios con contrasenas cifradas mediante BCrypt. Soporta dos roles: Administrador y Docente.
- Gestion de cursos y estudiantes con operaciones CRUD completas. Cada estudiante recibe un codigo unico generado automaticamente con el formato EST-{Anio}-{Id}.
- Registro de notas parciales (Nota1 a Nota4) con actualizacion en tiempo real mediante AJAX, sin necesidad de recargar la pagina.
- Calculo automatico de promedio y estado academico (Aprobado, Reprobado, Sin Notas) mediante columnas computadas PERSISTED en SQL Server.
- Subida de foto de perfil por estudiante, almacenada en base64 en la base de datos.
- Dashboard con resumen estadistico: total de estudiantes, aprobados, reprobados y promedio general.
- Prediccion de rendimiento academico usando ML.NET (regresion lineal) basada en el historial de notas.
- Exportacion de reportes a PDF y Excel por curso.
- Control de acceso por rol: los docentes solo acceden a sus propios cursos y estudiantes.

## Requisitos

- .NET 8 SDK
- SQL Server 2019 o superior
- Node.js (opcional, para herramientas de front-end)

## Configuracion

1. Clonar el repositorio.
2. Crear la base de datos ejecutando los scripts ubicados en la carpeta `Scripts/`: primero `CrearTablas_EduPath.sql`, luego `AlterTablaEstudiantes.sql`.
3. Configurar la cadena de conexion en `appsettings.json` o mediante la variable de entorno `CONNECTIONSTRINGS__DEFAULTCONNECTION`.
4. Ejecutar la aplicacion con `dotnet run` o publicarla con `dotnet publish`.

## Despliegue en Linux

La aplicacion esta preparada para ejecutarse en Ubuntu Server con Nginx como reverse proxy y systemd como gestor del servicio. La cadena de conexion se configura via variable de entorno en `/etc/environment` para no exponer credenciales en el codigo fuente.

## Tecnologias utilizadas

- ASP.NET Core MVC 8, Entity Framework Core 8
- SQL Server, columnas PERSISTED y restricciones de unicidad
- BCrypt.Net para cifrado de contrasenas
- ML.NET para prediccion de rendimiento
- jQuery, AJAX, DataTables, Bootstrap 5
- iTextSharp y ClosedXML para exportacion de reportes

## Autor

Yefri Gavarrete - UCENM, Programacion para Internet II, 2026
