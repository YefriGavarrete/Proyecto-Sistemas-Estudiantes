# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands

```bash
# Restore dependencies
dotnet restore

# Build the project
dotnet build

# Run the application
dotnet run --project Proyecto_Evaluacion_Estudiantes/Proyecto_Evaluacion_Estudiantes.csproj

# Run with hot reload (views/CSS changes visible without full rebuild)
dotnet watch run --project Proyecto_Evaluacion_Estudiantes/Proyecto_Evaluacion_Estudiantes.csproj
```

If CSS or Razor view changes don't appear in the browser after rebuild: stop the app, run `Compilar → Limpiar solución`, rebuild, and hard-refresh with `Ctrl+Shift+R`.

## Database Setup

1. Edit `appsettings.json` to match your SQL Server instance name:
   ```json
   "DefaultConnection": "Server=localhost\\MSSQLSERVER02;Database=EvaluacionEstudiantes;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
   ```
   Common values: `localhost`, `.\\SQLEXPRESS`, `.\\MSSQLSERVER02`

2. Run `Scripts_SQL/CrearTablas_EduPath.sql` in SQL Server Management Studio (safe to re-run — uses IF NOT EXISTS checks).

3. If updating an existing database, run `Scripts_SQL/AlterTablaEstudiantes.sql` instead.

4. Seed initial users directly in the DB with BCrypt-hashed passwords (workFactor: 11).

> NuGet packages target **net8.0** — always use version 8.x. EF Core v10 is incompatible.

## Architecture

**ASP.NET Core MVC (.NET 8.0)** with session-based authentication and SQL Server via EF Core (no migrations — schema managed by SQL scripts).

### Authentication & Authorization

- Login at `Home/IniciarSesion` checks **Administradores** first, then **Docentes** — both use `BCrypt.Net.BCrypt.Verify()`.
- Session variables set on login: `NombreDocente`, `TituloDocente`, `CodigoDocente`, `Rol`, `DocenteId`.
- Every controller action calls `VerificarDocente()` or `VerificarAdmin()` (private helper methods in each controller) to validate session; redirect to login if missing.
- Data isolation: docentes only see students belonging to their own active `Curso` (filtered by `CursoId.DocenteId`).

### ViewModel Pattern

All ViewModels inherit `LayoutViewModel`, which carries session-derived display values (`NombreUsuario`, `TituloUsuario`, `CodigoUsuario`, `EsAdmin`, `ActiveMenu`). Controllers call `LlenarLayoutVm()` to populate it before passing to views. The `_Layout.cshtml` reads from the ViewModel (with ViewData as fallback) to render the sidebar and role-based menu.

### Computed Database Columns

`Estudiante` has two **SQL Server PERSISTED computed columns** defined in `ApplicationDbContext.cs` (not in C# model logic):

- `Promedio` — average of whichever `Nota1`–`Nota4` are non-null
- `Estado` — `"Aprobado"` if Promedio ≥ 60, `"Reprobado"` if < 60, `"Sin Notas"` otherwise

These are read-only from EF Core. The C# model also has in-memory computed properties (`PromedioCalculado`, `EstadoCalculado`) that replicate this logic for cases where the DB column is not populated. Do not write to `Promedio` or `Estado` directly.

### Grade Entry (AJAX)

`Estudiantes/GuardarNota` is a POST endpoint that accepts JSON, updates a single parcial grade (`Nota1`–`Nota4` and its timestamp), and returns the updated `Promedio` and `Estado` as JSON. The `RegistroNotas` view calls this endpoint via jQuery AJAX per student row.

### Role-Based Navigation

The sidebar in `_Layout.cshtml` renders different menus based on `EsAdmin`:
- **Docente**: Dashboard, student management, AI prediction, reports
- **Admin**: User management (docentes), admin account management

### Key Relationships

- `Docente` → `Curso` (one-to-many, Restrict delete)
- `Curso` → `Estudiante` (one-to-many, Cascade delete)
- A student's `Correo + CursoId` combination is unique (a student can appear in multiple courses but not twice in the same one)

### Student Code Generation

New students receive an auto-generated code: `EST-{Year}-{Id:D4}` (set in `EstudiantesController.Registro` after the initial save, then saved again).

### Default Passwords

New docentes created by admins get `"Cambiar123!"` as default password (BCrypt-hashed before storage).
