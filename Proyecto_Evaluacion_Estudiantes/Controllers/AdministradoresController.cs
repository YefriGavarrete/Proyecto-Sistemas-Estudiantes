using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Proyecto_Evaluacion_Estudiantes.Data;
using Proyecto_Evaluacion_Estudiantes.Models;

namespace Proyecto_Evaluacion_Estudiantes.Controllers
{
    public class AdministradoresController : Controller
    {
        private readonly ILogger<AdministradoresController> _logger;
        private readonly ApplicationDbContext _context;

        public AdministradoresController(
            ILogger<AdministradoresController> logger,
            ApplicationDbContext context)
        {
            _logger  = logger;
            _context = context;
        }
        private bool VerificarAdmin() =>
            HttpContext.Session.GetString("Rol") == "Admin";

        private void CargarViewData(string activeMenu = "Administrador")
        {
            ViewData["NombreDocente"] = HttpContext.Session.GetString("NombreDocente");
            ViewData["TituloDocente"] = HttpContext.Session.GetString("TituloDocente") ?? "Administrador";
            ViewData["CodigoDocente"] = HttpContext.Session.GetString("CodigoDocente") ?? "---";
            ViewData["CursoActual"]   = "EduPath AI";
            ViewData["Periodo"]       = "2026-1";
            ViewData["ActiveMenu"]    = activeMenu;
            ViewData["EsAdmin"]       = true;
        }

        public async Task<IActionResult> Index()
        {
            if (!VerificarAdmin())
                return RedirectToAction("IniciarSesion", "Home");

            CargarViewData("Administrador");

            ViewData["TotalAdmins"]   = await _context.Administradores.CountAsync(a => a.Activo);
            ViewData["TotalDocentes"] = await _context.Docentes.CountAsync(d => d.Activo);

            return View();
        }


        [HttpGet]
        public IActionResult ConfiguracionUsuarios()
        {
            if (!VerificarAdmin())
                return RedirectToAction("IniciarSesion", "Home");

            CargarViewData("Administrador");

            if (TempData["MensajeExito"] != null)
                ViewData["MensajeExito"] = TempData["MensajeExito"]!.ToString();

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfiguracionUsuarios(
            string  Titulo,
            string  Nombre,
            string  Apellido,
            int     Edad,
            string  Identidad,
            string  UsuarioLogin,
            string? NuevaContrasena)
        {
            if (!VerificarAdmin())
                return RedirectToAction("IniciarSesion", "Home");

            if (string.IsNullOrWhiteSpace(Nombre)    ||
                string.IsNullOrWhiteSpace(Apellido)  ||
                string.IsNullOrWhiteSpace(Identidad) ||
                string.IsNullOrWhiteSpace(UsuarioLogin))
            {
                CargarViewData("Administrador");
                ViewData["Error"]        = "Todos los campos son obligatorios.";
                ViewData["FormTitulo"]   = Titulo;
                ViewData["FormNombre"]   = Nombre;
                ViewData["FormApellido"] = Apellido;
                ViewData["FormEdad"]     = Edad;
                ViewData["FormIdentidad"]= Identidad;
                ViewData["FormUsuario"]  = UsuarioLogin;
                return View();
            }

            // Buscar o crear el docente por usuario
            var docente = await _context.Docentes
                .FirstOrDefaultAsync(d => d.Usuario == UsuarioLogin);

            if (docente == null)
            {
                docente = new Docente { Usuario = UsuarioLogin };
                _context.Docentes.Add(docente);
            }

            docente.Titulo         = Titulo;
            docente.NombreCompleto = $"{Nombre} {Apellido}";

            if (!string.IsNullOrWhiteSpace(NuevaContrasena))
                docente.Contrasena = BCrypt.Net.BCrypt.HashPassword(NuevaContrasena, 11);
            else if (string.IsNullOrEmpty(docente.Contrasena))
                docente.Contrasena = BCrypt.Net.BCrypt.HashPassword("Cambiar123!", 11);

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Catedrático guardado: {Titulo} {Nombre} {Apellido}",
                Titulo, Nombre, Apellido);

            TempData["MensajeExito"] =
                $"Catedrático {Titulo} {Nombre} {Apellido} guardado correctamente.";
            return RedirectToAction(nameof(ConfiguracionUsuarios));
        }

        // ── Configuración de Administradores ─────────────────────
        [HttpGet]
        public IActionResult ConfiguracionAdministradores()
        {
            if (!VerificarAdmin())
                return RedirectToAction("IniciarSesion", "Home");

            CargarViewData("Administrador");

            if (TempData["MensajeExito"] != null)
                ViewData["MensajeExito"] = TempData["MensajeExito"]!.ToString();

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfiguracionAdministradores(
            string  Nombre,
            string  Apellido,
            string  NombreUsuario,
            string  Contrasena)
        {
            if (!VerificarAdmin())
                return RedirectToAction("IniciarSesion", "Home");

            if (string.IsNullOrWhiteSpace(Nombre)        ||
                string.IsNullOrWhiteSpace(Apellido)      ||
                string.IsNullOrWhiteSpace(NombreUsuario) ||
                string.IsNullOrWhiteSpace(Contrasena))
            {
                CargarViewData("Administrador");
                ViewData["Error"]         = "Todos los campos son obligatorios.";
                ViewData["FormNombre"]    = Nombre;
                ViewData["FormApellido"]  = Apellido;
                ViewData["FormUsuario"]   = NombreUsuario;
                return View();
            }

            // Verificar que el nombre de usuario tenga mínimo 8 caracteres y 2 dígitos
            int digitCount = NombreUsuario.Count(char.IsDigit);
            if (NombreUsuario.Length < 8 || digitCount < 2)
            {
                CargarViewData("Administrador");
                ViewData["Error"]        = "El nombre de usuario debe tener mínimo 8 caracteres e incluir al menos 2 dígitos.";
                ViewData["FormNombre"]   = Nombre;
                ViewData["FormApellido"] = Apellido;
                ViewData["FormUsuario"]  = NombreUsuario;
                return View();
            }

            // Verificar unicidad del usuario
            bool existe = await _context.Administradores
                .AnyAsync(a => a.NombreUsuario == NombreUsuario);
            if (existe)
            {
                CargarViewData("Administrador");
                ViewData["Error"]        = $"El usuario «{NombreUsuario}» ya está en uso.";
                ViewData["FormNombre"]   = Nombre;
                ViewData["FormApellido"] = Apellido;
                ViewData["FormUsuario"]  = NombreUsuario;
                return View();
            }

            var admin = new Administrador
            {
                Nombre        = Nombre.Trim(),
                Apellido      = Apellido.Trim(),
                NombreUsuario = NombreUsuario.Trim(),
                Contrasena    = BCrypt.Net.BCrypt.HashPassword(Contrasena, 11),
                Activo        = true
            };

            _context.Administradores.Add(admin);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Nuevo administrador creado: {NombreUsuario}", NombreUsuario);

            TempData["MensajeExito"] =
                $"Administrador {Nombre} {Apellido} creado correctamente.";
            return RedirectToAction(nameof(ConfiguracionAdministradores));
        }
    }
}
