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
        public async Task<IActionResult> Index()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("NombreDocente")))
                return RedirectToAction(nameof(IniciarSesion));

            // Si es admin y llega aquí por error, redirigir a su panel
            if (HttpContext.Session.GetString("Rol") == "Admin")
                return RedirectToAction("Index", "Administradores");

            // ── Obtener DocenteId de sesión ───────────────────────
            int.TryParse(HttpContext.Session.GetString("DocenteId"), out int docenteId);

            // ── Cargar todos los estudiantes activos del docente ──
            var estudiantes = docenteId > 0
                ? await _context.Estudiantes
                    .Include(e => e.Curso)
                    .Where(e => e.Curso!.DocenteId == docenteId && e.Activo)
                    .OrderByDescending(e => e.FechaRegistro)
                    .ToListAsync()
                : new List<Estudiante>();

            // ── Calcular métricas ─────────────────────────────────
            int total      = estudiantes.Count;
            int aprobados  = estudiantes.Count(e => e.Estado == "Aprobado");
            int reprobados = estudiantes.Count(e => e.Estado == "Reprobado");
            int sinNotas   = estudiantes.Count(e => e.Estado == null || e.Estado == "Sin Notas");

            // En Riesgo: promedio calculado entre 0 y 64.99, o marcado manualmente
            int enRiesgo   = estudiantes.Count(e =>
                e.EnRiesgoIA == true ||
                (e.Promedio.HasValue && e.Promedio.Value > 0 && e.Promedio.Value < 65));

            decimal promedioGeneral = total > 0 && estudiantes.Any(e => e.Promedio.HasValue)
                ? Math.Round(estudiantes.Where(e => e.Promedio.HasValue)
                                        .Average(e => e.Promedio!.Value), 2)
                : 0;

            decimal pctAprobados = total > 0
                ? Math.Round((decimal)aprobados / total * 100, 0)
                : 0;

            // ── Listas para tablas del dashboard ─────────────────
            // Estudiantes recientes (últimos 8)
            var recientes = estudiantes
                .Take(8)
                .Select(e => new
                {
                    Nombre   = e.NombreCompleto,
                    Promedio = e.Promedio ?? 0m,
                    Estado   = e.Estado ?? "Sin Notas"
                })
                .ToList<dynamic>();

            // Estudiantes en riesgo (promedio < 65 con al menos una nota)
            var riesgoLista = estudiantes
                .Where(e => e.Promedio.HasValue && e.Promedio.Value > 0 && e.Promedio.Value < 65)
                .OrderBy(e => e.Promedio)
                .Take(10)
                .Select(e => new
                {
                    Nombre   = e.NombreCompleto,
                    Correo   = e.Correo,
                    Nota1    = e.Nota1.HasValue ? e.Nota1.Value.ToString("F1") : "—",
                    Nota2    = e.Nota2.HasValue ? e.Nota2.Value.ToString("F1") : "—",
                    Promedio = e.Promedio ?? 0m
                })
                .ToList<dynamic>();

            ViewData["EstudiantesRecientes"] = recientes;
            ViewData["EstudiantesRiesgo"]    = riesgoLista;

            var vm = new DocenteDashboardViewModel
            {
                // ── Datos del perfil (sidebar + layout) ──
                NombreUsuario  = HttpContext.Session.GetString("NombreDocente") ?? "Docente",
                TituloUsuario  = HttpContext.Session.GetString("TituloDocente") ?? "Docente",
                CodigoUsuario  = HttpContext.Session.GetString("CodigoDocente") ?? "---",
                Sistema        = "EduPath AI",
                Periodo        = "2026-1",
                EsAdmin        = false,
                ActiveMenu     = "Dashboard",

                // ── Estadísticas calculadas desde la BD ──
                TotalEstudiantes    = total,
                PromedioGeneral     = promedioGeneral,
                PorcentajeAprobados = pctAprobados,
                EstudiantesEnRiesgo = enRiesgo,
                TotalInscritos      = total,
                TotalAprobados      = aprobados,
                TotalReprobados     = reprobados,
                TotalEnRiesgoIA     = enRiesgo
            };

            // ViewData solo para campos que el _Layout todavía lee cuando el modelo
            // no hereda de LayoutViewModel (vistas con modelos de entidad).
            ViewData["ActiveMenu"] = "Dashboard";
            ViewData["EsAdmin"]    = false;

            return View(vm);
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
                HttpContext.Session.SetString("DocenteId",     docente.Id.ToString());
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
