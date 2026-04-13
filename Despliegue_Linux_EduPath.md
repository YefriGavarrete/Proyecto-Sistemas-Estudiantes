# Guía de Despliegue — EduPath AI en Linux (Opción A: SQL Server en Linux)

> **Requisito de proyecto #6 — Preparar el proyecto para hosting Linux**  
> Entorno objetivo: Ubuntu Server 22.04 LTS | .NET 8 | SQL Server 2022 for Linux | Nginx | systemd

---

## Orden de implementación

### FASE 1 — En tu máquina local (Windows, Visual Studio)

---

#### Paso 1 — Verificar que `appsettings.Production.json` está listo ✅

El archivo ya fue creado en el proyecto. Contiene:

- `TrustServerCertificate=False` → correcto para producción con certificado válido
- `User Id` y `Password` como placeholder → las credenciales reales van en variable de entorno, **nunca** en este archivo
- Nivel de log `Warning` en lugar de `Information` → reduce escritura a disco en producción

**No subas contraseñas reales a este archivo. Git no debe contener secretos.**

---

#### Paso 2 — Agregar `.gitignore` para secretos (si usas Git)

Asegúrate de que tu `.gitignore` incluya estas líneas para no subir credenciales por accidente:

```
appsettings.Production.local.json
*.user
wwwroot/uploads/
```

---

#### Paso 3 — Publicar la aplicación (Publish para Linux)

Abre una terminal en la carpeta del proyecto y ejecuta:

```bash
dotnet publish Proyecto_Evaluacion_Estudiantes/Proyecto_Evaluacion_Estudiantes.csproj \
  -c Release \
  -r linux-x64 \
  --self-contained false \
  -o ./publish
```

Esto genera la carpeta `./publish` con todos los archivos listos para copiar al servidor.

> **`--self-contained false`** → el servidor debe tener .NET 8 instalado (se instala en el Paso 5).  
> Si prefieres no instalar .NET en el servidor, cambia a `--self-contained true` (el ejecutable sube de ~10 MB a ~80 MB pero no depende del runtime del servidor).

---

#### Paso 4 — Copiar archivos al servidor Linux

Desde tu máquina Windows, usa **SCP** o **WinSCP** para subir la carpeta `publish`:

```bash
# Con SCP (desde PowerShell o WSL):
scp -r ./publish usuario@IP_SERVIDOR:/var/www/edupath
```

O bien usa WinSCP con interfaz gráfica apuntando a `/var/www/edupath`.

---

### FASE 2 — En el servidor Linux (Ubuntu 22.04)

Conéctate por SSH:

```bash
ssh usuario@IP_SERVIDOR
```

---

#### Paso 5 — Instalar .NET 8 Runtime en Ubuntu

```bash
# Agregar el repositorio de Microsoft
wget https://packages.microsoft.com/config/ubuntu/22.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
rm packages-microsoft-prod.deb

# Instalar el runtime (no el SDK completo, solo runtime para producción)
sudo apt-get update
sudo apt-get install -y aspnetcore-runtime-8.0

# Verificar instalación
dotnet --version   # debe mostrar 8.x.x
```

---

#### Paso 6 — Instalar SQL Server 2022 for Linux

```bash
# Importar clave de Microsoft y repositorio
curl -fsSL https://packages.microsoft.com/keys/microsoft.asc | sudo gpg --dearmor -o /usr/share/keyrings/microsoft-prod.gpg
curl -fsSL https://packages.microsoft.com/config/ubuntu/22.04/mssql-server-2022.list | sudo tee /etc/apt/sources.list.d/mssql-server-2022.list

sudo apt-get update
sudo apt-get install -y mssql-server

# Configurar SQL Server (aquí defines la contraseña del SA)
sudo /opt/mssql/bin/mssql-conf setup
# Selecciona: Developer edition (gratis) → escribe contraseña SA → confirma

# Verificar que el servicio está activo
sudo systemctl status mssql-server
```

---

#### Paso 7 — Crear la base de datos y el usuario de aplicación

Instala la herramienta de línea de comandos `sqlcmd`:

```bash
sudo apt-get install -y mssql-tools18 unixodbc-dev
echo 'export PATH="$PATH:/opt/mssql-tools18/bin"' >> ~/.bashrc
source ~/.bashrc
```

Conéctate y crea el usuario dedicado (nunca uses SA en producción):

```bash
sqlcmd -S localhost -U SA -P 'TU_CONTRASENA_SA' -Q "
CREATE DATABASE EvaluacionEstudiantes;
GO
USE EvaluacionEstudiantes;
GO
CREATE LOGIN edupath_user WITH PASSWORD = 'EduPath@2026Prod!';
GO
CREATE USER edupath_user FOR LOGIN edupath_user;
GO
ALTER ROLE db_owner ADD MEMBER edupath_user;
GO
"
```

Luego ejecuta el script de creación de tablas:

```bash
sqlcmd -S localhost -U SA -P 'TU_CONTRASENA_SA' \
  -d EvaluacionEstudiantes \
  -i /var/www/edupath/Scripts_SQL/CrearTablas_EduPath.sql
```

> Copia el archivo `.sql` al servidor junto con los archivos publicados, o ejecútalo desde tu máquina local apuntando a la IP del servidor.

---

#### Paso 8 — Configurar la variable de entorno de conexión (LO MÁS IMPORTANTE)

**ASP.NET Core lee automáticamente variables de entorno** con el prefijo `CONNECTIONSTRINGS__` y las usa como si fueran `ConnectionStrings:DefaultConnection` en `appsettings.json`. Esto significa que **las credenciales nunca tocan el código**.

La variable de entorno sobreescribe `appsettings.Production.json` en tiempo de ejecución:

```bash
# Editar el archivo de variables de entorno del sistema
sudo nano /etc/environment
```

Agrega esta línea al final (todo en una sola línea):

```
CONNECTIONSTRINGS__DEFAULTCONNECTION="Server=localhost;Database=EvaluacionEstudiantes;User Id=edupath_user;Password=EduPath@2026Prod!;TrustServerCertificate=True;MultipleActiveResultSets=true"
```

> **Nota sobre `TrustServerCertificate`:**  
> - En este paso usamos `True` porque SQL Server for Linux instala por defecto un certificado autofirmado.  
> - Si en el futuro instalas un certificado SSL válido para SQL Server (ej. Let's Encrypt o un certificado de CA), cambia a `False` en esta variable. Para la entrega del proyecto, `True` es correcto y aceptable.

Guarda y recarga:

```bash
source /etc/environment

# Verificar que la variable quedó configurada
echo $CONNECTIONSTRINGS__DEFAULTCONNECTION
```

---

#### Paso 9 — Crear el servicio systemd para la aplicación

Systemd mantiene la aplicación ejecutándose como servicio del sistema (se reinicia si falla, arranca con el servidor):

```bash
sudo nano /etc/systemd/system/edupath.service
```

Contenido del archivo:

```ini
[Unit]
Description=EduPath AI - Sistema de Evaluacion Estudiantil
After=network.target mssql-server.service

[Service]
WorkingDirectory=/var/www/edupath
ExecStart=/usr/bin/dotnet /var/www/edupath/Proyecto_Evaluacion_Estudiantes.dll
Restart=always
RestartSec=10
SyslogIdentifier=edupath
User=www-data
EnvironmentFile=/etc/environment
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=ASPNETCORE_URLS=http://localhost:5000

[Install]
WantedBy=multi-user.target
```

Activa y arranca el servicio:

```bash
sudo systemctl daemon-reload
sudo systemctl enable edupath
sudo systemctl start edupath

# Verificar que está corriendo
sudo systemctl status edupath

# Ver logs si algo falla
sudo journalctl -u edupath -f
```

---

#### Paso 10 — Instalar y configurar Nginx como reverse proxy

```bash
sudo apt-get install -y nginx
```

Crea el archivo de configuración del sitio:

```bash
sudo nano /etc/nginx/sites-available/edupath
```

Contenido:

```nginx
server {
    listen 80;
    server_name TU_DOMINIO_O_IP;

    location / {
        proxy_pass         http://localhost:5000;
        proxy_http_version 1.1;
        proxy_set_header   Upgrade $http_upgrade;
        proxy_set_header   Connection keep-alive;
        proxy_set_header   Host $host;
        proxy_cache_bypass $http_upgrade;
        proxy_set_header   X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header   X-Forwarded-Proto $scheme;

        # Límite de upload consistente con el de la app (2 MB)
        client_max_body_size 2M;
    }
}
```

Habilita el sitio y reinicia Nginx:

```bash
sudo ln -s /etc/nginx/sites-available/edupath /etc/nginx/sites-enabled/
sudo nginx -t          # verificar sintaxis
sudo systemctl restart nginx
sudo systemctl enable nginx
```

---

#### Paso 11 — (Opcional pero recomendado) SSL con Let's Encrypt

Si tienes un dominio registrado apuntando al servidor:

```bash
sudo apt-get install -y certbot python3-certbot-nginx
sudo certbot --nginx -d tudominio.com
```

Certbot modifica automáticamente el archivo de Nginx para usar HTTPS. Una vez hecho esto:

- Cambia `TrustServerCertificate` en la variable de entorno a `True` (sigue igual, aplica solo a la conexión app→SQL Server, no al certificado web)
- El sitio ya estará disponible en `https://tudominio.com`

---

### FASE 3 — Verificación final (en el servidor)

```bash
# 1. Verificar servicios activos
sudo systemctl status mssql-server   # debe estar Active (running)
sudo systemctl status edupath        # debe estar Active (running)
sudo systemctl status nginx          # debe estar Active (running)

# 2. Probar la aplicación localmente en el servidor
curl -I http://localhost:5000        # debe devolver HTTP/1.1 302 (redirige a login)

# 3. Probar desde el navegador
# Abrir: http://IP_DEL_SERVIDOR
# Debe mostrar la pantalla de login de EduPath AI

# 4. Verificar logs de la aplicación
sudo journalctl -u edupath --since "5 minutes ago"
```

---

## Resumen: ¿dónde se hace cada paso?

| Paso | Dónde se ejecuta |
|------|-----------------|
| 1 — Verificar appsettings.Production.json | Máquina local (Visual Studio / carpeta del proyecto) |
| 2 — .gitignore | Máquina local |
| 3 — `dotnet publish` | Máquina local (terminal / PowerShell) |
| 4 — Copiar archivos al servidor | Máquina local → Servidor (SCP / WinSCP) |
| 5 — Instalar .NET 8 Runtime | Servidor Linux (SSH) |
| 6 — Instalar SQL Server for Linux | Servidor Linux (SSH) |
| 7 — Crear BD y usuario | Servidor Linux (SSH con sqlcmd) |
| 8 — Variable de entorno de conexión | Servidor Linux (SSH, `/etc/environment`) |
| 9 — Servicio systemd | Servidor Linux (SSH) |
| 10 — Nginx reverse proxy | Servidor Linux (SSH) |
| 11 — SSL Let's Encrypt (opcional) | Servidor Linux (SSH) |
| Verificación final | Servidor Linux + navegador del cliente |

---

## Diferencias clave entre Desarrollo y Producción

| Aspecto | Desarrollo (Windows) | Producción (Linux) |
|---------|---------------------|-------------------|
| Connection string | `appsettings.json` | Variable de entorno del sistema |
| `TrustServerCertificate` | `True` (autofirmado local) | `True` con SQL Server Linux autofirmado; `False` si se instala certificado válido de CA |
| `ASPNETCORE_ENVIRONMENT` | `Development` | `Production` |
| Servidor web | Kestrel directo (IIS Express) | Nginx → Kestrel |
| Gestión de proceso | Visual Studio | systemd |
| Logs | Consola VS | `journalctl -u edupath` |

---

*Guía generada para EduPath AI — Proyecto de Evaluación Estudiantil — Abril 2026*
