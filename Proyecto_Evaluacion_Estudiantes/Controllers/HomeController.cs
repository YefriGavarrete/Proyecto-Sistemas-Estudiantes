using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Proyecto_Evaluacion_Estudiantes.Data;
using Proyecto_Evaluacion_Estudiantes.Models;

namespace Proyecto_Evaluacion_Estudiantes.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger  = logger;
            _context = context;
        }

        private void CargarViewDataSesion(string activeMenu = "Dashboard")
        {
            ViewData["NombreDocente"] = HttpContext.Session.GetString("NombreDocente");
            ViewData["TituloDocente"] = HttpContext.Session.GetString("TituloDocente") ?? "Docente";
            ViewData["CodigoDocente"] = HttpContext.Session.GetString("CodigoDocente") ?? "---";
            ViewData["CursoActual"]   = "EduPath AI";
            ViewData["Periodo"]       = "2026-1";
            ViewData["ActiveMenu"]    = activeMenu;
            ViewData["EsAdmin"]       = HttpContext.Session.GetString("Rol") == "Admin";
        }

        // GET: /Home/Index
        public IActionResult Index()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("NombreDocente")))
                return RedirectToAction(nameof(IniciarSesion));

            // Si es admin y llega aquí por error, redirigir a su panel
            if (HttpContext.Session.GetString("Rol") == "Admin")
                return RedirectToAction("Index", "Administradores");

            CargarViewDataSesion("Dashboard");

            ViewData["TotalEstudiantes"]    = 0;
            ViewData["PromedioGeneral"]     = "0.00";
            ViewData["PorcentajeAprobados"] = "0";
            ViewData["EstudiantesEnRiesgo"] = 0;
            ViewData["TotalInscritos"]      = 0;
            ViewData["TotalAprobados"]      = 0;
            ViewData["TotalReprobados"]     = 0;
            ViewData["TotalEnRiesgoIA"]     = 0;

            return View();
        }

        [HttpGet]
        public IActionResult IniciarSesion()
        {
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("NombreDocente")))
            {
                return HttpContext.Session.GetString("Rol") == "Admin"
                    ? RedirectToAction("Index", "Administradores")
                    : RedirectToAction(nameof(Index));
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> IniciarSesion(string Usuario, string Contrasena)
        {
            if (string.IsNullOrWhiteSpace(Usuario) || string.IsNullOrWhiteSpace(Contrasena))
            {
                ViewData["Error"] = "El usuario y la contraseña son obligatorios.";
                return View();
            }
            var admin = await _context.Administradores
                .FirstOrDefaultAsync(a => a.NombreUsuario == Usuario.Trim() && a.Activo);

            if (admin != null && BCrypt.Net.BCrypt.Verify(Contrasena, admin.Contrasena))
            {
                HttpContext.Session.SetString("NombreDocente", admin.NombreCompleto);
                HttpContext.Session.SetString("TituloDocente", "Administrador");
                HttpContext.Session.SetString("CodigoDocente", $"ADM-{admin.Id:D3}");
                HttpContext.Session.SetString("Rol", "Admin");

                _logger.LogInformation("Admin autenticado: {Usuario}", Usuario);
                return RedirectToAction("Index", "Administradores");
            }

            // ── 2. Buscar en Docentes ─────────────────────────────
            var docente = await _context.Docentes
                .FirstOrDefaultAsync(d => d.Usuario == Usuario.Trim() && d.Activo);

            if (docente != null && BCrypt.Net.BCrypt.Verify(Contrasena, docente.Contrasena))
            {
                // Actualizar último acceso
                docente.UltimoAcceso = DateTime.Now;
                await _context.SaveChangesAsync();

                HttpContext.Session.SetString("NombreDocente", docente.NombreCompleto);
                HttpContext.Session.SetString("TituloDocente", docente.Titulo);
                HttpContext.Session.SetString("CodigoDocente", $"DOC-{docente.Id:D3}");
                HttpContext.Session.SetString("Rol", "Docente");

                _logger.LogInformation("Docente autenticado: {Usuario}", Usuario);
                return RedirectToAction(nameof(Index));
            }

            _logger.LogWarning("Login fallido: {Usuario}", Usuario);
            ViewData["Error"] = "Usuario o contraseña incorrectos.";
            return View();
        }

        public IActionResult Perfil()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("NombreDocente")))
                return RedirectToAction(nameof(IniciarSesion));

            CargarViewDataSesion("Configuracion");
            return View();
        }
        public IActionResult CerrarSesion()
        {
            HttpContext.Session.Clear();
            return RedirectToAction(nameof(IniciarSesion));
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }
    }
}
