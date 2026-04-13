using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Proyecto_Evaluacion_Estudiantes.Data;
using Proyecto_Evaluacion_Estudiantes.Helpers;
using Proyecto_Evaluacion_Estudiantes.Models;

namespace Proyecto_Evaluacion_Estudiantes.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly IMemoryCache _cache;

        public HomeController(
            ILogger<HomeController> logger,
            ApplicationDbContext context,
            IMemoryCache cache)
        {
            _logger  = logger;
            _context = context;
            _cache   = cache;
        }

        // ── GET: /Home/Index ──────────────────────────────────────
        public async Task<IActionResult> Index()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("NombreDocente")))
                return RedirectToAction(nameof(IniciarSesion));

            if (HttpContext.Session.GetString("Rol") == "Admin")
                return RedirectToAction("Index", "Administradores");

            int.TryParse(HttpContext.Session.GetString("DocenteId"), out int docenteId);

            var cursoIdsDash = docenteId > 0
                ? await _context.AsignacionDocentes
                    .Where(a => a.DocenteId == docenteId && a.Activo)
                    .Select(a => a.CursoId)
                    .Distinct()
                    .ToListAsync()
                : new List<int>();

            var estudiantes = cursoIdsDash.Any()
                ? await _context.Estudiantes
                    .Include(e => e.Curso)
                    .Where(e => cursoIdsDash.Contains(e.CursoId) && e.Activo)
                    .OrderByDescending(e => e.FechaRegistro)
                    .ToListAsync()
                : new List<Estudiante>();

            int total      = estudiantes.Count;
            int aprobados  = estudiantes.Count(e => e.Estado == "Aprobado");
            int reprobados = estudiantes.Count(e => e.Estado == "Reprobado");
            int enRiesgo   = estudiantes.Count(e =>
                e.EnRiesgoIA == true ||
                (e.Promedio.HasValue && e.Promedio.Value > 0 && e.Promedio.Value < 65));

            decimal promedioGeneral = total > 0 && estudiantes.Any(e => e.Promedio.HasValue)
                ? Math.Round(estudiantes.Where(e => e.Promedio.HasValue).Average(e => e.Promedio!.Value), 2)
                : 0;

            decimal pctAprobados = total > 0
                ? Math.Round((decimal)aprobados / total * 100, 0)
                : 0;

            var recientes = estudiantes
                .Take(8)
                .Select(e => new
                {
                    Nombre   = e.NombreCompleto,
                    Promedio = e.Promedio ?? 0m,
                    Estado   = e.Estado ?? "Sin Notas"
                })
                .ToList<dynamic>();

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
                NombreUsuario       = HttpContext.Session.GetString("NombreDocente") ?? "Docente",
                TituloUsuario       = HttpContext.Session.GetString("TituloDocente") ?? "Docente",
                CodigoUsuario       = HttpContext.Session.GetString("CodigoDocente") ?? "---",
                Sistema             = "EduPath AI",
                Periodo             = PeriodoAcademico.ObtenerPeriodoDescriptivo(),
                EsAdmin             = false,
                ActiveMenu          = "Dashboard",
                TotalEstudiantes    = total,
                PromedioGeneral     = promedioGeneral,
                PorcentajeAprobados = pctAprobados,
                EstudiantesEnRiesgo = enRiesgo,
                TotalInscritos      = total,
                TotalAprobados      = aprobados,
                TotalReprobados     = reprobados,
                TotalEnRiesgoIA     = enRiesgo
            };

            ViewData["ActiveMenu"] = "Dashboard";
            ViewData["EsAdmin"]    = false;

            return View(vm);
        }

        // ── GET: /Home/IniciarSesion ──────────────────────────────
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

            // (expiración automática, sin memory leak) me sirve para limitar a 5 intentos por minuto por IP y mitigar ataques de fuerza
            var ip         = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var cacheKey   = $"login_attempts_{ip}";
            int attempts   = _cache.GetOrCreate(cacheKey, e =>
            {
                e.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1);
                return 0;
            });

            if (attempts >= 5)
            {
                ViewData["Error"] = "Demasiados intentos. Espere un minuto.";
                return View();
            }

            _cache.Set(cacheKey, attempts + 1, TimeSpan.FromMinutes(1));

            // ── 1. Buscar en Administradores ──────────────────────
            var admin = await _context.Administradores
                .FirstOrDefaultAsync(a => a.NombreUsuario == Usuario.Trim() && a.Activo);

            if (admin != null && BCrypt.Net.BCrypt.Verify(Contrasena, admin.Contrasena))
            {
                HttpContext.Session.SetString("NombreDocente", admin.NombreCompleto);
                HttpContext.Session.SetString("TituloDocente", "Administrador");
                HttpContext.Session.SetString("CodigoDocente", $"ADM-{admin.Id:D3}");
                HttpContext.Session.SetString("Rol",     "Admin");
                HttpContext.Session.SetString("AdminId", admin.Id.ToString());

                _cache.Remove(cacheKey);
                _logger.LogInformation("Admin autenticado: {Usuario}", Usuario);
                return RedirectToAction("Index", "Administradores");
            }

            // ── 2. Buscar en Docentes ─────────────────────────────
            var docente = await _context.Docentes
                .FirstOrDefaultAsync(d => d.Usuario == Usuario.Trim() && d.Activo);

            if (docente != null && BCrypt.Net.BCrypt.Verify(Contrasena, docente.Contrasena))
            {
                docente.UltimoAcceso = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                HttpContext.Session.SetString("NombreDocente", docente.NombreCompleto);
                HttpContext.Session.SetString("TituloDocente", docente.Titulo);
                HttpContext.Session.SetString("CodigoDocente", $"DOC-{docente.Id:D3}");
                HttpContext.Session.SetString("DocenteId", docente.Id.ToString());
                HttpContext.Session.SetString("Rol", "Docente");

                _cache.Remove(cacheKey);
                _logger.LogInformation("Docente autenticado: {Usuario}", Usuario);
                return RedirectToAction(nameof(Index));
            }

            _logger.LogWarning("Login fallido: {Usuario}", Usuario);
            ViewData["Error"] = "Usuario o contraseña incorrectos.";
            return View();
        }

        public IActionResult CerrarSesion()
        {
            HttpContext.Session.Clear();
            return RedirectToAction(nameof(IniciarSesion));
        }

        // ── GET: /Home/Perfil
        [HttpGet]
        public async Task<IActionResult> Perfil()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("NombreDocente")))
                return RedirectToAction(nameof(IniciarSesion));

            string rol = HttpContext.Session.GetString("Rol") ?? "Docente";
            var vm = new PerfilViewModel();

            if (rol == "Admin")
            {
                int.TryParse(HttpContext.Session.GetString("AdminId"), out int adminId);
                var admin = await _context.Administradores.FirstOrDefaultAsync(a => a.Id == adminId);
                if (admin == null) return RedirectToAction(nameof(IniciarSesion));

                vm.NombreCompleto = admin.NombreCompleto;
                vm.Usuario        = admin.NombreUsuario;
                vm.Rol            = "Administrador";
                vm.Codigo         = $"ADM-{admin.Id:D3}";
            }
            else
            {
                int.TryParse(HttpContext.Session.GetString("DocenteId"), out int docenteId);
                var docente = await _context.Docentes.FirstOrDefaultAsync(d => d.Id == docenteId);
                if (docente == null) return RedirectToAction(nameof(IniciarSesion));

                vm.NombreCompleto = docente.NombreCompleto;
                vm.Usuario        = docente.Usuario;
                vm.Rol            = "Docente";
                vm.Codigo         = $"DOC-{docente.Id:D3}";
                vm.Titulo         = docente.Titulo;
                vm.Correo         = docente.Correo;
                vm.FechaCreacion  = docente.FechaCreacion;
                vm.UltimoAcceso   = docente.UltimoAcceso;
            }



            vm.NombreUsuario = HttpContext.Session.GetString("NombreDocente") ?? vm.NombreCompleto;
            vm.TituloUsuario = HttpContext.Session.GetString("TituloDocente") ?? vm.Rol;
            vm.CodigoUsuario = HttpContext.Session.GetString("CodigoDocente") ?? vm.Codigo;
            vm.EsAdmin       = rol == "Admin";
            vm.ActiveMenu    = "Configuracion";

            return View(vm);
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
