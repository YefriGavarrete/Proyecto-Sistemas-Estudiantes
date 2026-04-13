using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Proyecto_Evaluacion_Estudiantes.Data;
using Proyecto_Evaluacion_Estudiantes.Helpers;
using Proyecto_Evaluacion_Estudiantes.ML;
using Proyecto_Evaluacion_Estudiantes.Models;

namespace Proyecto_Evaluacion_Estudiantes.Controllers
{
    public class PrediccionController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly PrediccionService    _prediccion;

        public PrediccionController(ApplicationDbContext context, PrediccionService prediccion)
        {
            _context    = context;
            _prediccion = prediccion;
        }
        private bool VerificarDocente()
        {
            var nombre = HttpContext.Session.GetString("NombreDocente");
            var rol    = HttpContext.Session.GetString("Rol");
            return !string.IsNullOrEmpty(nombre) && rol == "Docente";
        }

        private int ObtenerDocenteId()
        {
            int.TryParse(HttpContext.Session.GetString("DocenteId"), out int id);
            return id;
        }

        private void LlenarLayoutVm(LayoutViewModel vm, string activeMenu = "Prediccion")
        {
            vm.NombreUsuario = HttpContext.Session.GetString("NombreDocente") ?? "Docente";
            vm.TituloUsuario = HttpContext.Session.GetString("TituloDocente") ?? "Docente";
            vm.CodigoUsuario = HttpContext.Session.GetString("CodigoDocente") ?? "---";
            vm.Sistema       = "EduPath AI";
            vm.Periodo       = PeriodoAcademico.ObtenerPeriodoDescriptivo();
            vm.EsAdmin       = false;
            vm.ActiveMenu    = activeMenu;
        }

        public async Task<IActionResult> Buscar(string? q)
        {
            if (!VerificarDocente())
                return RedirectToAction("IniciarSesion", "Home");

            int docenteId = ObtenerDocenteId();

            // Cursos del docente
            var cursoIds = await _context.AsignacionDocentes
                .Where(a => a.DocenteId == docenteId && a.Activo)
                .Select(a => a.CursoId)
                .Distinct()
                .ToListAsync();

            List<Estudiante> resultados = new();

            if (!string.IsNullOrWhiteSpace(q))
            {
                var termino = q.Trim().ToLower();
                resultados = await _context.Estudiantes
                    .Include(e => e.Curso)
                        .ThenInclude(c => c!.Grado)
                    .Where(e => cursoIds.Contains(e.CursoId)
                             && e.Activo
                             && (e.Nombre.ToLower().Contains(termino)
                              || e.Apellido.ToLower().Contains(termino)
                              || e.Codigo.ToLower().Contains(termino)
                              || (e.Identidad != null && e.Identidad.ToLower().Contains(termino))))
                    .OrderBy(e => e.Apellido)
                    .ThenBy(e => e.Nombre)
                    .ToListAsync();
            }

            var vm = new EstudianteIndexViewModel { Estudiantes = resultados };
            LlenarLayoutVm(vm, "Prediccion");
            ViewData["q"] = q ?? "";
            return View(vm);
        }

        public async Task<IActionResult> Resultado(int estudianteId)
        {
            if (!VerificarDocente())
                return RedirectToAction("IniciarSesion", "Home");

            int docenteId = ObtenerDocenteId();

            // Verificar que el estudiante pertenece al docente
            var cursoIds = await _context.AsignacionDocentes
                .Where(a => a.DocenteId == docenteId && a.Activo)
                .Select(a => a.CursoId)
                .Distinct()
                .ToListAsync();

            var estudiante = await _context.Estudiantes
                .Include(e => e.Curso)
                    .ThenInclude(c => c!.Grado)
                .FirstOrDefaultAsync(e => e.Id == estudianteId
                                       && cursoIds.Contains(e.CursoId)
                                       && e.Activo);

            if (estudiante is null)
                return RedirectToAction(nameof(Buscar));

            // Construir entrada al modelo
            float n1 = estudiante.Nota1.HasValue ? (float)estudiante.Nota1.Value : -1f;
            float n2 = estudiante.Nota2.HasValue ? (float)estudiante.Nota2.Value : -1f;
            float n3 = estudiante.Nota3.HasValue ? (float)estudiante.Nota3.Value : -1f;
            float n4 = estudiante.Nota4.HasValue ? (float)estudiante.Nota4.Value : -1f;

            var notasConValor = new[] { n1, n2, n3, n4 }.Where(n => n >= 0).ToArray();
            float promedio        = notasConValor.Length > 0 ? notasConValor.Average() : 0f;
            float notasRegistradas = notasConValor.Length;

            var input = new RiesgoModelInput
            {
                Nota1             = n1,
                Nota2             = n2,
                Nota3             = n3,
                Nota4             = n4,
                PromedioActual    = promedio,
                NotasRegistradas  = notasRegistradas
            };

            float prob = _prediccion.Predecir(input);

            // Clasificar nivel de riesgo
            string nivel, color, badge, mensaje;
            if (prob < 0.40f)
            {
                nivel   = "Bajo";
                color   = "text-success";
                badge   = "bg-success";
                mensaje = "El estudiante muestra un rendimiento estable. Se recomienda mantener el seguimiento regular.";
            }
            else if (prob <= 0.65f)
            {
                nivel   = "Medio";
                color   = "text-warning";
                badge   = "bg-warning";
                mensaje = "El estudiante presenta señales de alerta. Se recomienda atención especial en los próximos parciales.";
            }
            else
            {
                nivel   = "Alto";
                color   = "text-danger";
                badge   = "bg-danger";
                mensaje = "El estudiante tiene alta probabilidad de reprobar. Se recomienda intervención inmediata.";
            }

            var vm = new PrediccionRiesgoViewModel
            {
                EstudianteInfo = estudiante,
                Probabilidad   = prob,
                NivelRiesgo    = nivel,
                ColorRiesgo    = color,
                BadgeRiesgo    = badge,
                MensajeRiesgo  = mensaje,
                PromedioActual = promedio,
                DatosGrafico   = new float?[]
                {
                    estudiante.Nota1.HasValue ? (float?)( float)estudiante.Nota1.Value : null,
                    estudiante.Nota2.HasValue ? (float?)(float)estudiante.Nota2.Value : null,
                    estudiante.Nota3.HasValue ? (float?)(float)estudiante.Nota3.Value : null,
                    estudiante.Nota4.HasValue ? (float?)(float)estudiante.Nota4.Value : null,
                }
            };
            LlenarLayoutVm(vm, "Prediccion");

            return View(vm);
        }
    }
}
