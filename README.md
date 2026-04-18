# EduPath AI - Sistema de Evaluacion Estudiantil

Aplicacion web desarrollada en ASP.NET Core MVC (.NET 8) con Entity Framework Core 8 y SQL Server. Permite a docentes gestionar estudiantes, registrar notas parciales y visualizar el rendimiento academico con predicciones mediante ML.NET.

## Funcionalidades

- Autenticacion de usuarios con contrasenas cifradas mediante BCrypt. Soporta dos roles: Administrador y Docente.
- Gestion de cursos y estudiantes con operaciones CRUD completas. Cada estudiante recibe un codigo unico generado automaticamente con el formato EST-{Anio}-{Id}.
- Registro de notas parciales (Nota1 a Nota4) con actualizacion en tiempo real mediante AJAX, sin recargar la pagina.
- Calculo automatico de promedio y estado academico (Aprobado, Reprobado, Sin Notas) mediante columnas computadas PERSISTED en SQL Server.
- Subida de foto de perfil por estudiante, almacenada en base64 en la base de datos.
- Dashboard con resumen estadistico: total de estudiantes, aprobados, reprobados y promedio general.
- Prediccion de riesgo academico usando ML.NET (FastTree) entrenado en tiempo de ejecucion con el historial de notas del curso.
- Control de acceso por rol: los docentes solo acceden a sus propios cursos y estudiantes.

## Requisitos

- .NET 8 SDK
- SQL Server 2019 o superior (local) o base de datos MSSQL en hosting

## Configuracion local

1. Clonar el repositorio:
   ```bash
   git clone https://github.com/tu-usuario/Proyecto_Evaluacion_Estudiantes.git
   ```
2. Ejecutar `Proyecto_Evaluacion_Estudiantes/Scripts_SQL/SQLBaseDeDatosEvaluacion.sql` en SQL Server para crear la base de datos y tablas.
3. La cadena de conexion en `appsettings.json` apunta a `localhost\MSSQLSERVER02`. Si tu instancia tiene un nombre diferente, editala antes de ejecutar:
   ```json
   "DefaultConnection": "Server=localhost\\TU_INSTANCIA;Database=EvaluacionEstudiantes;Trusted_Connection=True;TrustServerCertificate=True;"
   ```
4. Ejecutar con `dotnet run` desde la carpeta `Proyecto_Evaluacion_Estudiantes/`.

> El archivo `appsettings.Production.json` no esta incluido en el repositorio. Si quieres ejecutar el proyecto en modo produccion, debes crearlo manualmente en la raiz del proyecto con tu propia cadena de conexion:
> ```json
> {
>   "ConnectionStrings": {
>     "DefaultConnection": "Server=TU_SERVIDOR;Database=TU_BD;User Id=TU_USUARIO;Password=TU_PASSWORD;Encrypt=False;MultipleActiveResultSets=True;TrustServerCertificate=True;"
>   }
> }
> ```

## Despliegue en MonsterASP.NET

La aplicacion esta desplegada en hosting gratuito de MonsterASP.NET con base de datos MSSQL incluida.

1. Publicar con `dotnet publish -c Release -o ./publish` (sin flag de runtime, el hosting es Windows).
2. Subir el contenido de `./publish` al servidor via FTP.
3. La cadena de conexion de produccion se configura en `appsettings.Production.json`, el cual esta excluido del repositorio mediante `.gitignore` para no exponer credenciales.
4. La base de datos remota se inicializa ejecutando `Scripts_SQL/InsertDatosIniciales.sql` 
URL de produccion: http://edupath-ai.runasp.net/

## Seguridad

El archivo `appsettings.Production.json` contiene las credenciales de la base de datos de produccion y esta listado en `.gitignore`. No se debe subir al repositorio. Las contrasenas de usuarios se almacenan siempre como hash BCrypt con work factor 11.

## Tecnologias utilizadas

- ASP.NET Core MVC 8, Entity Framework Core 8
- SQL Server con columnas PERSISTED y restricciones de unicidad
- BCrypt.Net-Next para cifrado de contrasenas
- ML.NET 3.0 (FastTree) para prediccion de riesgo academico
- jQuery, AJAX, DataTables, Bootstrap 5

## Autor

Yefri Gavarrete - UCENM, Programacion para Internet II, 2026
