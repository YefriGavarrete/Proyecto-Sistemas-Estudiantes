using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Proyecto_Evaluacion_Estudiantes.Data;
using Proyecto_Evaluacion_Estudiantes.Models;

namespace Proyecto_Evaluacion_Estudiantes.Controllers
{
    public class EstudiantesController : Controller
    {
        private readonly ILogger<EstudiantesController> _logger;
        private readonly ApplicationDbContext _context;

        public EstudiantesController(
            ILogger<EstudiantesController> logger,
            ApplicationDbContext context)
        {
            _logger  = logger;
            _context = context;
        }

        bool VerificarDocente() =>
            !string.IsNullOrEmpty(HttpContext.Session.GetString("NombreDocente"));

        int ObtenerDocenteId()
        {
            int.TryParse(HttpContext.Session.GetString("DocenteId"), out int id);
            return id;
        }

        private void CargarViewData(string activeMenu = "Informacion")
        {
            ViewData["NombreDocente"] = HttpContext.Session.GetString("NombreDocente");
            ViewData["TituloDocente"] = HttpContext.Session.GetString("TituloDocente") ?? "Docente";
            ViewData["CodigoDocente"] = HttpContext.Session.GetString("CodigoDocente") ?? "---";
            ViewData["CursoActual"]   = "EduPath AI";
            ViewData["Periodo"]       = "2026-1";
            ViewData["ActiveMenu"]    = activeMenu;
            ViewData["EsAdmin"]       = HttpContext.Session.GetString("Rol") == "Admin";
        }

        private void LlenarLayoutVm(LayoutViewModel vm, string activeMenu = "Informacion")
        {
            vm.NombreUsuario = HttpContext.Session.GetString("NombreDocente") ?? "Docente";
            vm.TituloUsuario = HttpContext.Session.GetString("TituloDocente") ?? "Docente";
            vm.CodigoUsuario = HttpContext.Session.GetString("CodigoDocente") ?? "---";
            vm.Sistema       = "EduPath AI";
            vm.Periodo       = "2026-1";
            vm.EsAdmin       = HttpContext.Session.GetString("Rol") == "Admin";
            vm.ActiveMenu    = activeMenu;
        }

        /// <summary>Obtiene el primer CursoId activo del docente logueado.</summary>
        private async Task<int?> ObtenerCursoActivoAsync()
        {
            int docenteId = ObtenerDocenteId();
            if (docenteId == 0) return null;

            var curso = await _context.Cursos
                .Where(c => c.DocenteTutorId == docenteId && c.Activo)
                .FirstOrDefaultAsync();

            return curso?.Id;
        }

        // ── GET: /Estudiantes/Index ───────────────────────────────
        public async Task<IActionResult> Index()
        {
            if (!VerificarDocente())
                return RedirectToAction("IniciarSesion", "Home");

            int docenteId = ObtenerDocenteId();

            var estudiantes = await _context.Estudiantes
                .Include(e => e.Curso)
                .Where(e => e.Curso!.DocenteTutorId == docenteId && e.Activo)
                .OrderBy(e => e.Apellido)
                .ThenBy(e => e.Nombre)
                .ToListAsync();

            var vm = new EstudianteIndexViewModel { Estudiantes = estudiantes };
            LlenarLayoutVm(vm, "Informacion");

            return View(vm);
        }

        // ── GET: /Estudiantes/Registro ────────────────────────────
        [HttpGet]
        public async Task<IActionResult> Registro()
        {
            if (!VerificarDocente())
                return RedirectToAction("IniciarSesion", "Home");

            CargarViewData("Informacion");

            // Pasar el CursoId por defecto para el campo hidden
            int? cursoId = await ObtenerCursoActivoAsync();
            ViewData["CursoId"] = cursoId;

            return View(new Estudiante
            {
                CursoId          = cursoId ?? 0,
                FechaNacimiento  = new DateTime(2000, 1, 1),
                Activo           = true
            });
        }

        // ── POST: /Estudiantes/Registro ───────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Registro(
            [Bind("Nombre,Apellido,FechaNacimiento,Identidad,Correo,Telefono,Genero,Seccion,Observaciones,Activo,CursoId")]
            Estudiante modelo)
        {
            if (!VerificarDocente())
                return RedirectToAction("IniciarSesion", "Home");

            CargarViewData("Informacion");

            // Excluir campos calculados de la validación del modelo
            ModelState.Remove("Codigo");
            ModelState.Remove("Promedio");
            ModelState.Remove("Estado");
            ModelState.Remove("Curso");

            if (!ModelState.IsValid)
                return View(modelo);

            // Validar que se tenga un curso asignado
            if (modelo.CursoId == 0)
            {
                int? cursoId = await ObtenerCursoActivoAsync();
                if (cursoId == null)
                {
                    TempData["ErrorMessage"] = "No tienes un curso activo. Crea uno primero en Configuración.";
                    return View(modelo);
                }
                modelo.CursoId = cursoId.Value;
            }

            // Verificar correo duplicado en el mismo curso
            bool correoExiste = await _context.Estudiantes
                .AnyAsync(e => e.Correo == modelo.Correo.Trim() && e.CursoId == modelo.CursoId);
            if (correoExiste)
            {
                ModelState.AddModelError("Correo", "Este correo ya está registrado en el curso.");
                return View(modelo);
            }

            modelo.Nombre    = modelo.Nombre.Trim();
            modelo.Apellido  = modelo.Apellido.Trim();
            modelo.Correo    = modelo.Correo.Trim();
            modelo.FechaRegistro = DateTime.Now;

            _context.Estudiantes.Add(modelo);
            await _context.SaveChangesAsync();

            // Generar código auto una vez que ya tenemos el Id
            modelo.Codigo = $"EST-{DateTime.Now.Year}-{modelo.Id:D4}";
            await _context.SaveChangesAsync();

            _logger.LogInformation("Estudiante registrado: {Nombre} {Apellido} (Id={Id})",
                modelo.Nombre, modelo.Apellido, modelo.Id);

            TempData["MensajeExito"] =
                $"Estudiante {modelo.Nombre} {modelo.Apellido} registrado correctamente. Código: {modelo.Codigo}";

            return RedirectToAction(nameof(Index));
        }

        // ── GET: /Estudiantes/RegistroNotas?parcial=1 ────────────
        [HttpGet]
        public async Task<IActionResult> RegistroNotas(int parcial = 1)
        {
            if (!VerificarDocente())
                return RedirectToAction("IniciarSesion", "Home");

            if (parcial < 1 || parcial > 4) parcial = 1;

            int docenteId = ObtenerDocenteId();

            var estudiantes = await _context.Estudiantes
                .Include(e => e.Curso)
                .Where(e => e.Curso!.DocenteTutorId == docenteId && e.Activo)
                .OrderBy(e => e.Apellido)
                .ThenBy(e => e.Nombre)
                .ToListAsync();

            var vm = new EstudianteNotasViewModel
            {
                ParcialSeleccionado = parcial,
                Estudiantes         = estudiantes
            };
            LlenarLayoutVm(vm, "Informacion");

            return View(vm);
        }

        // ── POST AJAX: /Estudiantes/GuardarNota ───────────────────
        // Guarda o modifica la nota de un parcial específico para un estudiante.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GuardarNota(int estudianteId, int parcial, decimal nota)
        {
            if (!VerificarDocente())
                return Json(new { ok = false, msg = "Sesión expirada." });

            if (parcial < 1 || parcial > 4)
                return Json(new { ok = false, msg = "Parcial inválido." });

            if (nota < 0 || nota > 100)
                return Json(new { ok = false, msg = "La nota debe estar entre 0 y 100." });

            var estudiante = await _context.Estudiantes
                .Include(e => e.Curso)
                .FirstOrDefaultAsync(e => e.Id == estudianteId);

            if (estudiante == null)
                return Json(new { ok = false, msg = "Estudiante no encontrado." });

            // Verificar que el estudiante pertenece al docente logueado
            int docenteId = ObtenerDocenteId();
            if (estudiante.Curso?.DocenteTutorId != docenteId)
                return Json(new { ok = false, msg = "Sin permiso para este estudiante." });

            // Asignar la nota al parcial correspondiente
            switch (parcial)
            {
                case 1: estudiante.Nota1 = nota; estudiante.FechaNota1 = DateTime.Now; break;
                case 2: estudiante.Nota2 = nota; estudiante.FechaNota2 = DateTime.Now; break;
                case 3: estudiante.Nota3 = nota; estudiante.FechaNota3 = DateTime.Now; break;
                case 4: estudiante.Nota4 = nota; estudiante.FechaNota4 = DateTime.Now; break;
            }

            await _context.SaveChangesAsync();

            // Recargar para obtener el Promedio y Estado calculados por SQL
            await _context.Entry(estudiante).ReloadAsync();

            _logger.LogInformation(
                "Nota Parcial {P} guardada — Estudiante {Id}: {Nota}",
                parcial, estudianteId, nota);

            return Json(new
            {
                ok       = true,
                msg      = "Nota guardada correctamente.",
                promedio = estudiante.Promedio?.ToString("F2") ?? "—",
                estado   = estudiante.Estado ?? "Sin Notas",
                fecha    = DateTime.Now.ToString("dd/MM/yyyy HH:mm")
            });
        }

        // ── GET: /Estudiantes/Editar/{id} ─────────────────────────
        [HttpGet]
        public async Task<IActionResult> Editar(int id)
        {
            if (!VerificarDocente())
                return RedirectToAction("IniciarSesion", "Home");

            CargarViewData("Informacion");

            var estudiante = await _context.Estudiantes
                .Include(e => e.Curso)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (estudiante == null)
            {
                TempData["ErrorMessage"] = "Estudiante no encontrado.";
                return RedirectToAction(nameof(Index));
            }

            int docenteId = ObtenerDocenteId();
            if (estudiante.Curso?.DocenteTutorId != docenteId)
            {
                TempData["ErrorMessage"] = "No tienes permiso para editar este estudiante.";
                return RedirectToAction(nameof(Index));
            }

            return View(estudiante);
        }

        // ── POST: /Estudiantes/Editar ─────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(
            [Bind("Id,Nombre,Apellido,FechaNacimiento,Identidad,Correo,Telefono,Genero,Seccion,Observaciones,Activo,CursoId")]
            Estudiante modelo)
        {
            if (!VerificarDocente())
                return RedirectToAction("IniciarSesion", "Home");

            CargarViewData("Informacion");

            ModelState.Remove("Codigo");
            ModelState.Remove("Promedio");
            ModelState.Remove("Estado");
            ModelState.Remove("Curso");

            if (!ModelState.IsValid)
                return View(modelo);

            var estudiante = await _context.Estudiantes
                .Include(e => e.Curso)
                .FirstOrDefaultAsync(e => e.Id == modelo.Id);

            if (estudiante == null)
            {
                TempData["ErrorMessage"] = "Estudiante no encontrado.";
                return RedirectToAction(nameof(Index));
            }

            int docenteId = ObtenerDocenteId();
            if (estudiante.Curso?.DocenteTutorId != docenteId)
            {
                TempData["ErrorMessage"] = "Sin permiso.";
                return RedirectToAction(nameof(Index));
            }

            estudiante.Nombre          = modelo.Nombre.Trim();
            estudiante.Apellido        = modelo.Apellido.Trim();
            estudiante.FechaNacimiento = modelo.FechaNacimiento;
            estudiante.Identidad       = modelo.Identidad?.Trim();
            estudiante.Correo          = modelo.Correo.Trim();
            estudiante.Telefono        = modelo.Telefono?.Trim();
            estudiante.Genero          = modelo.Genero;
            estudiante.Seccion         = modelo.Seccion?.Trim();
            estudiante.Observaciones   = modelo.Observaciones?.Trim();
            estudiante.Activo          = modelo.Activo;

            await _context.SaveChangesAsync();

            TempData["MensajeExito"] = $"Datos de {estudiante.NombreCompleto} actualizados.";
            return RedirectToAction(nameof(Index));
        }

        // ── GET: /Estudiantes/Prediccion ──────────────────────────
        [HttpGet]
        public async Task<IActionResult> Prediccion()
        {
            if (!VerificarDocente())
                return RedirectToAction("IniciarSesion", "Home");

            int docenteId = ObtenerDocenteId();

            var todos = docenteId > 0
                ? await _context.Estudiantes
                    .Include(e => e.Curso)
                    .Where(e => e.Curso!.DocenteTutorId == docenteId && e.Activo)
                    .OrderBy(e => e.Promedio)
                    .ToListAsync()
                : new List<Estudiante>();

            // Riesgo alto: promedio < 60 o ya marcados EnRiesgoIA
            var enRiesgo = todos
                .Where(e => e.Promedio.HasValue && e.Promedio.Value < 60
                         || e.EnRiesgoIA == true)
                .ToList();

            // Atención: promedio entre 60 y 70 (zona límite)
            var atencion = todos
                .Where(e => e.Promedio.HasValue
                         && e.Promedio.Value >= 60
                         && e.Promedio.Value < 70
                         && e.EnRiesgoIA != true)
                .ToList();

            var vm = new Proyecto_Evaluacion_Estudiantes.Models.PrediccionViewModel
            {
                EstudiantesRiesgo   = enRiesgo,
                EstudiantesAtencion = atencion,
                TotalAnalizados     = todos.Count
            };
            LlenarLayoutVm(vm, "Informacion");

            return View(vm);
        }
    }
}
