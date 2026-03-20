# Instrucciones para configurar la Base de Datos

## 1. Instalar paquetes NuGet

> ⚠️ **El proyecto usa .NET 8.0.** NuGet sin versión instala la última (v10.x),
> incompatible con net8.0. **Siempre especifica la versión 8.x.**

### Método A — Ya configurado en el .csproj (recomendado)
Los paquetes ya están en `Proyecto_Evaluacion_Estudiantes.csproj`.
Solo restaura desde Visual Studio: clic derecho en la solución → **Restaurar paquetes NuGet**.
O desde la Consola del Administrador de Paquetes:

```powershell
dotnet restore
```

### Método B — Instalar manualmente con versión exacta

```powershell
# ✅ Versión 8.x — compatible con net8.0
Install-Package Microsoft.EntityFrameworkCore.SqlServer -Version 8.0.10
Install-Package Microsoft.EntityFrameworkCore.Tools     -Version 8.0.10
Install-Package BCrypt.Net-Next                         -Version 4.0.3
```

## 2. Configurar la cadena de conexión

Edita `appsettings.json` y reemplaza `TU_SERVIDOR` con el nombre de tu instancia:

```json
"DefaultConnection": "Server=.\\SQLEXPRESS;Database=EvaluacionEstudiantesDB;Trusted_Connection=True;TrustServerCertificate=True;"
```

Nombres de servidor comunes:
- `localhost` o `.` — SQL Server predeterminado
- `.\\SQLEXPRESS` — SQL Server Express

---


## 4. Hashear la contraseña del docente administrador

Después de ejecutar el script, el docente `admin` tiene una contraseña placeholder.
Usa este fragmento en un controller o en la consola de depuración para generar
el hash real de "admin123":

```csharp
// En cualquier parte del código C# (instala BCrypt.Net-Next)
string hashReal = BCrypt.Net.BCrypt.HashPassword("admin123", workFactor: 11);
// hashReal → algo como: $2a$11$xxx...

// Luego actualiza la BD:
UPDATE Docentes
SET Contrasena = '<hash_generado>'
WHERE Usuario = 'admin';
```

En `HomeController.cs`, la validación ya usa BCrypt correctamente
una vez que reemplaces la línea de credencial temporal:

```csharp
// Cambiar esta línea temporal:
bool credencialesValidas = Usuario.Trim().ToLower() == "admin" && Contrasena == "admin123";

// Por esta (con BD real):
var docente = await _context.Docentes
    .FirstOrDefaultAsync(d => d.Usuario == Usuario.Trim() && d.Activo);

bool credencialesValidas = docente != null
    && BCrypt.Net.BCrypt.Verify(Contrasena, docente.Contrasena);
```

---

## 5. Por qué los cambios no se ven en el navegador

Cuando modificas vistas `.cshtml` o archivos `.css`, debes:

1. **Detener la aplicación** en Visual Studio (botón Stop o Shift+F5).
2. **Limpiar la solución**: `Compilar → Limpiar solución`.
3. **Reconstruir**: `Compilar → Recompilar solución`.
4. **Iniciar de nuevo**: F5 o botón de Play.
5. En el navegador: **Ctrl + Shift + R** (recarga forzada sin caché).

