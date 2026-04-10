using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Proyecto_Evaluacion_Estudiantes.Data;
using Proyecto_Evaluacion_Estudiantes.Models;

namespace Proyecto_Evaluacion_Estudiantes.Controllers
{
    public class ReportesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReportesController(ApplicationDbContext context)
        {
            _context = context;
        }

        private bool VerificarDocente()
        {
            var nombre = HttpContext.Session.GetString("NombreDocente");
            var rol = HttpContext.Session.GetString("Rol");
            return !string.IsNullOrEmpty(nombre) && rol == "Docente";
        }

        private int ObtenerDocenteId()
        {
            int.TryParse(HttpContext.Session.GetString("DocenteId"), out int id);
            return id;
        }

        private void LlenarLayoutVm(LayoutViewModel vm, string activeMenu = "Reportes")
        {
            vm.NombreUsuario = HttpContext.Session.GetString("NombreDocente") ?? "Docente";
            vm.TituloUsuario = HttpContext.Session.GetString("TituloDocente") ?? "Docente";
            vm.CodigoUsuario = HttpContext.Session.GetString("CodigoDocente") ?? "---";
            vm.Sistema = "EduPath AI";
            vm.Periodo = "2026-1";
            vm.EsAdmin = HttpContext.Session.GetString("Rol") == "Admin";
            vm.ActiveMenu = activeMenu;
        }

        // ── GET: /Reportes/General ───────────────────────────────
        [HttpGet]
        public async Task<IActionResult> General()
        {
            if (!VerificarDocente())
                return RedirectToAction("IniciarSesion", "Home");

            int docenteId = ObtenerDocenteId();

            var cursoIdsG = docenteId > 0
                ? await _context.AsignacionDocentes
                    .Where(a => a.DocenteId == docenteId && a.Activo)
                    .Select(a => a.CursoId).Distinct().ToListAsync()
                : new List<int>();

            var estudiantes = cursoIdsG.Any()
                ? await _context.Estudiantes
                    .Include(e => e.Curso)
                    .Where(e => cursoIdsG.Contains(e.CursoId) && e.Activo)
                    .OrderBy(e => e.Apellido).ThenBy(e => e.Nombre)
                    .AsNoTracking()
                    .ToListAsync()
                : new List<Estudiante>();

            int total = estudiantes.Count;
            int aprobados = estudiantes.Count(e => e.Estado == "Aprobado");
            int reprobados = estudiantes.Count(e => e.Estado == "Reprobado");
            int sinNotas = estudiantes.Count(e => e.Promedio == null);
            decimal promedio = total > 0 && estudiantes.Any(e => e.Promedio.HasValue)

                ? Math.Round(estudiantes.Where(e => e.Promedio.HasValue).Average(e => e.Promedio!.Value), 2)
                : 0;

            // Distribución de notas (rangos)
            var conPromedio = estudiantes.Where(e => e.Promedio.HasValue).ToList();
            int rango0_59 = conPromedio.Count(e => e.Promedio!.Value < 60);
            int rango60_69 = conPromedio.Count(e => e.Promedio!.Value >= 60 && e.Promedio.Value < 70);
            int rango70_79 = conPromedio.Count(e => e.Promedio!.Value >= 70 && e.Promedio.Value < 80);
            int rango80_89 = conPromedio.Count(e => e.Promedio!.Value >= 80 && e.Promedio.Value < 90);
            int rango90_100 = conPromedio.Count(e => e.Promedio!.Value >= 90);

            // Promedios por parcial
            decimal prom1 = estudiantes.Any(e => e.Nota1.HasValue) ? Math.Round(estudiantes.Where(e => e.Nota1.HasValue).Average(e => e.Nota1!.Value), 2) : 0;
            decimal prom2 = estudiantes.Any(e => e.Nota2.HasValue) ? Math.Round(estudiantes.Where(e => e.Nota2.HasValue).Average(e => e.Nota2!.Value), 2) : 0;
            decimal prom3 = estudiantes.Any(e => e.Nota3.HasValue) ? Math.Round(estudiantes.Where(e => e.Nota3.HasValue).Average(e => e.Nota3!.Value), 2) : 0;
            decimal prom4 = estudiantes.Any(e => e.Nota4.HasValue) ? Math.Round(estudiantes.Where(e => e.Nota4.HasValue).Average(e => e.Nota4!.Value), 2) : 0;

            var vm = new ReporteGeneralViewModel
            {
                TotalEstudiantes = total,
                Aprobados = aprobados,
                Reprobados = reprobados,
                SinNotas = sinNotas,
                PromedioGeneral = promedio,
                PctAprobados = total > 0 ? Math.Round(aprobados * 100m / total, 1) : 0,
                Rango0_59 = rango0_59,
                Rango60_69 = rango60_69,
                Rango70_79 = rango70_79,
                Rango80_89 = rango80_89,
                Rango90_100 = rango90_100,
                PromedioParcial1 = prom1,
                PromedioParcial2 = prom2,
                PromedioParcial3 = prom3,
                PromedioParcial4 = prom4,
                Estudiantes = estudiantes
            };
            LlenarLayoutVm(vm, "Reportes");

            return View(vm);
        }

        // ── GET: /Reportes/Riesgo ────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> Riesgo()
        {
            if (!VerificarDocente())
                return RedirectToAction("IniciarSesion", "Home");

            int docenteId = ObtenerDocenteId();

            var cursoIdsR = docenteId > 0
                ? await _context.AsignacionDocentes
                    .Where(a => a.DocenteId == docenteId && a.Activo)
                    .Select(a => a.CursoId).Distinct().ToListAsync()
                : new List<int>();

            var enRiesgo = cursoIdsR.Any()
                ? await _context.Estudiantes
                    .Include(e => e.Curso)
                    .Where(e => cursoIdsR.Contains(e.CursoId) && e.Activo
                             && (e.Promedio < 65 || e.EnRiesgoIA == true))
                    .OrderBy(e => e.Promedio)
                    .AsNoTracking()
                    .ToListAsync()
                : new List<Estudiante>();

            var vm = new ReporteRiesgoViewModel
            {
                Estudiantes = enRiesgo
            };
            LlenarLayoutVm(vm, "Reportes");
            return View(vm);
        }
    }
}

            