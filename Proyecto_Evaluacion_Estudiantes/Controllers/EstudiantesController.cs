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

        void LlenarLayoutVm(LayoutViewModel vm, string activeMenu = "Informacion")
        {
            vm.NombreUsuario = HttpContext.Session.GetString("NombreDocente") ?? "Docente";
            vm.TituloUsuario = HttpContext.Session.GetString("TituloDocente") ?? "Docente";
            vm.CodigoUsuario = HttpContext.Session.GetString("CodigoDocente") ?? "---";
            vm.Sistema       = "EduPath AI";
            vm.Periodo       = "2026-1";
            vm.EsAdmin       = HttpContext.Session.GetString("Rol") == "Admin";
            vm.ActiveMenu    = activeMenu;
        }



        /// Devuelve todos los Cursos donde el docente logueado tiene
        /// al menos una AsignacionDocente activa.
        private async Task<List<Curso>> ObtenerCursosAsignadosAsync()
        {
            int docenteId = ObtenerDocenteId();
            if (docenteId == 0) return new List<Curso>();

            // Cursos distintos donde el docente aparece en AsignacionDocente
            var cursoIds = await _context.AsignacionDocentes
                .Where(a => a.DocenteId == docenteId && a.Activo)
                .Select(a => a.CursoId)
                .Distinct()
                .ToListAsync();

            return await _context.Cursos
                .Include(c => c.Grado)
                .Where(c => cursoIds.Contains(c.Id) && c.Activo)
                .OrderBy(c => c.GradoId)
                .ThenBy(c => c.Seccion)
                .ToListAsync();
        }

        /// Verifica que el docente logueado tenga al menos una AsignacionDocente
        /// para el CursoId dado. Usado como gate de seguridad en todas las acciones.
        private async Task<bool> DocenteTieneAccesoACursoAsync(int docenteId, int cursoId)
        {
            return await _context.AsignacionDocentes
                .AnyAsync(a => a.DocenteId == docenteId
                            && a.CursoId   == cursoId
                            && a.Activo);
        }

        // ── GET: /Estudiantes/Index 
        public async Task<IActionResult> Index()
        {
            if (!VerificarDocente())
                return RedirectToAction("IniciarSesion", "Home");

            int docenteId = ObtenerDocenteId();

            // Cursos del docente vía AsignacionDocente
            var cursoIds = await _context.AsignacionDocentes
                .Where(a => a.DocenteId == docenteId && a.Activo)
                .Select(a => a.CursoId)
                .Distinct()
                .ToListAsync();

            var estudiantes = await _context.Estudiantes
                .Include(e => e.Curso)
                    .ThenInclude(c => c!.Grado)
                .Where(e => cursoIds.Contains(e.CursoId) && e.Activo)
                .OrderBy(e => e.Curso!.GradoId)
                .ThenBy(e => e.Apellido)
                .ThenBy(e => e.Nombre)
                .ToListAsync();

            var vm = new EstudianteIndexViewModel { Estudiantes = estudiantes };
            LlenarLayoutVm(vm, "Informacion");

            return View(vm);
        }

        // ── Helper: construye RegistroEstudianteViewModel con dropdown de cursos ──
        private async Task<RegistroEstudianteViewModel> CrearVmRegistroAsync(
            RegistroEstudianteViewModel? desde = null)
        {
            var cursos = await ObtenerCursosAsignadosAsync();

            var vm = desde ?? new RegistroEstudianteViewModel
            {
                FechaNacimiento = new DateTime(2000, 1, 1),
                Activo          = true
            };

            vm.CursosDisponibles = cursos.Select(c => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
            {
                Value    = c.Id.ToString(),
                Text     = c.NombreCompleto,   // "Primer Grado — Sec. A (2026)"
                Selected = c.Id == vm.CursoId
            }).ToList();

            LlenarLayoutVm(vm, "Informacion");
            return vm;
        }

        // ── GET: /Estudiantes/Registro ────────────────────────────
        [HttpGet]
        public async Task<IActionResult> Registro()
        {
            if (!VerificarDocente())
                return RedirectToAction("IniciarSesion", "Home");

            var vm = await CrearVmRegistroAsync();

            if (!vm.CursosDisponibles.Any())
            {
                TempData["ErrorMessage"] =
                    "No tienes cursos asignados. Pide al administrador que te asigne a un curso en Gestión de Cursos.";
                return RedirectToAction(nameof(Index));
            }

            return View(vm);
        }

        // ── POST: /Estudiantes/Registro ───────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Registro(RegistroEstudianteViewModel vm)
        {
            if (!VerificarDocente())
                return RedirectToAction("IniciarSesion", "Home");

            // Reconstruir dropdown antes de cualquier retorno
            var cursos = await ObtenerCursosAsignadosAsync();
            vm.CursosDisponibles = cursos.Select(c => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
            {
                Value    = c.Id.ToString(),
                Text     = c.NombreCompleto,
                Selected = c.Id == vm.CursoId
            }).ToList();
            LlenarLayoutVm(vm, "Informacion");

            ModelState.Remove("CursosDisponibles");

            if (!ModelState.IsValid)
                return View(vm);

            // ── Validación de seguridad: el docente debe tener AsignacionDocente en ese curso ──
            int docenteId = ObtenerDocenteId();
            if (!vm.CursoId.HasValue || !await DocenteTieneAccesoACursoAsync(docenteId, vm.CursoId.Value))
            {
                ModelState.AddModelError("CursoId",
                    "No tienes asignación en el curso seleccionado. Contacta al administrador.");
                return View(vm);
            }

            // Verificar correo duplicado en el mismo curso
            bool correoExiste = await _context.Estudiantes
                .AnyAsync(e => e.Correo == vm.Correo.Trim() && e.CursoId == vm.CursoId.Value);
            if (correoExiste)
            {
                ModelState.AddModelError("Correo", "Este correo ya está registrado en el curso seleccionado.");
                return View(vm);
            }

            var nuevo = new Estudiante
            {
                Nombre          = vm.Nombre.Trim(),
                Apellido        = vm.Apellido.Trim(),
                FechaNacimiento = vm.FechaNacimiento,
                Identidad       = vm.Identidad?.Trim(),
                Correo          = vm.Correo.Trim(),
                Telefono        = vm.Telefono?.Trim(),
                Genero          = vm.Genero,
                Seccion         = vm.Seccion?.Trim(),
                Observaciones   = vm.Observaciones?.Trim(),
                Activo          = vm.Activo,
                CursoId         = vm.CursoId.Value,
                FechaRegistro   = DateTime.Now
            };

            _context.Estudiantes.Add(nuevo);
            await _context.SaveChangesAsync();

            // Código automático después de obtener el Id
            nuevo.Codigo = $"EST-{DateTime.Now.Year}-{nuevo.Id:D4}";
            await _context.SaveChangesAsync();

            _logger.LogInformation("Estudiante registrado: {Nombre} {Apellido} (Id={Id})",
                nuevo.Nombre, nuevo.Apellido, nuevo.Id);

            TempData["MensajeExito"] =
                $"Estudiante {nuevo.Nombre} {nuevo.Apellido} registrado correctamente. Código: {nuevo.Codigo}";

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

            var cursoIds = await _context.AsignacionDocentes
                .Where(a => a.DocenteId == docenteId && a.Activo)
                .Select(a => a.CursoId)
                .Distinct()
                .ToListAsync();

            var estudiantes = await _context.Estudiantes
                .Include(e => e.Curso)
                    .ThenInclude(c => c!.Grado)
                .Where(e => cursoIds.Contains(e.CursoId) && e.Activo)
                .OrderBy(e => e.Curso!.GradoId)
                .ThenBy(e => e.Apellido)
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

            // Verificar que el docente tiene AsignacionDocente en el curso del estudiante
            int docenteId = ObtenerDocenteId();
            bool tieneAcceso = await DocenteTieneAccesoACursoAsync(docenteId, estudiante.CursoId);
            if (!tieneAcceso)
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
            bool tieneAccesoGet = await DocenteTieneAccesoACursoAsync(docenteId, estudiante.CursoId);
            if (!tieneAccesoGet)
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
            bool tieneAccesoPost = await DocenteTieneAccesoACursoAsync(docenteId, estudiante.CursoId);
            if (!tieneAccesoPost)
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

        static string NombreParcial(int p) => p switch
        {
            1 => "Primer Parcial",
            2 => "Segundo Parcial",
            3 => "Tercer Parcial",
            4 => "Cuarto Parcial",
            _ => $"Parcial {p}"
        };

        // ── GET: /Estudiantes/SubirNotas (Paso 1 — selección) ─────
        [HttpGet]
        public async Task<IActionResult> SubirNotas()
        {
            if (!VerificarDocente())
                return RedirectToAction("IniciarSesion", "Home");

            var cursos = await ObtenerCursosAsignadosAsync();

            var vm = new SubirNotasSeleccionViewModel
            {
                CursosDisponibles = cursos.Select(c => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text  = c.NombreCompleto
                }).ToList()
            };
            LlenarLayoutVm(vm, "Notas");

            if (!vm.CursosDisponibles.Any())
            {
                TempData["ErrorMessage"] =
                    "No tienes cursos asignados. Pide al administrador que te configure en Gestión de Cursos.";
                return RedirectToAction(nameof(Index));
            }

            return View(vm);
        }

        // ── POST: /Estudiantes/SubirNotas (Paso 1 — valida y redirige) ──
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubirNotas(SubirNotasSeleccionViewModel vm)
        {
            if (!VerificarDocente())
                return RedirectToAction("IniciarSesion", "Home");

            // Reconstruir dropdown antes de cualquier retorno
            var cursos = await ObtenerCursosAsignadosAsync();
            vm.CursosDisponibles = cursos.Select(c => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
            {
                Value    = c.Id.ToString(),
                Text     = c.NombreCompleto,
                Selected = c.Id == vm.CursoId
            }).ToList();
            LlenarLayoutVm(vm, "Notas");
            ModelState.Remove("CursosDisponibles");
            ModelState.Remove("ParcialesItems");

            if (!ModelState.IsValid)
                return View(vm);

            // Validación de seguridad: el docente debe tener AsignacionDocente en ese curso
            int docenteId = ObtenerDocenteId();
            if (!vm.CursoId.HasValue || !await DocenteTieneAccesoACursoAsync(docenteId, vm.CursoId.Value))
            {
                ModelState.AddModelError("CursoId",
                    "No tienes asignación en el curso seleccionado.");
                return View(vm);
            }

            return RedirectToAction(nameof(SubirNotasGrilla),
                new { cursoId = vm.CursoId.Value, parcial = vm.Parcial!.Value });
        }



        // ── GET: /Estudiantes/SubirNotasGrilla?cursoId=X&parcial=Y (Paso 2) ──
        [HttpGet]
        public async Task<IActionResult> SubirNotasGrilla(int cursoId, int parcial)
        {
            if (!VerificarDocente())
                return RedirectToAction("IniciarSesion", "Home");

            if (parcial < 1 || parcial > 4)
                return RedirectToAction(nameof(SubirNotas));

            int docenteId = ObtenerDocenteId();

            // Seguridad: verificar acceso al curso
            if (!await DocenteTieneAccesoACursoAsync(docenteId, cursoId))
            {
                TempData["ErrorMessage"] = "No tienes asignación en el curso seleccionado.";
                return RedirectToAction(nameof(SubirNotas));
            }

            // Columnas: asignaturas del docente en este curso
            var asignaturas = await _context.AsignacionDocentes
                .Include(a => a.Asignatura)
                .Where(a => a.DocenteId == docenteId && a.CursoId == cursoId && a.Activo)
                .OrderBy(a => a.Asignatura!.Nombre)
                .Select(a => new AsignaturaColumna
                {
                    Id     = a.AsignaturaId,
                    Nombre = a.Asignatura!.Nombre,
                    Codigo = a.Asignatura!.Codigo
                })
                .ToListAsync();

            if (!asignaturas.Any())
            {
                TempData["ErrorMessage"] = "No tienes asignaturas asignadas en ese curso.";
                return RedirectToAction(nameof(SubirNotas));
            }

            // Filas: estudiantes activos del curso
            var estudiantes = await _context.Estudiantes
                .Where(e => e.CursoId == cursoId && e.Activo)
                .OrderBy(e => e.Apellido)
                .ThenBy(e => e.Nombre)
                .ToListAsync();

            // Notas existentes en NotasParciales para este curso + parcial
            var estudianteIds = estudiantes.Select(e => e.Id).ToList();
            var asignaturaIds = asignaturas.Select(a => a.Id).ToList();

            var notasExistentes = await _context.NotasParciales
                .Where(n => estudianteIds.Contains(n.EstudianteId)
                         && asignaturaIds.Contains(n.AsignaturaId)
                         && n.Parcial == (byte)parcial)
                .ToListAsync();

            // Indexar notas: (EstudianteId, AsignaturaId) → Nota
            var notasIndex = notasExistentes
                .ToDictionary(n => (n.EstudianteId, n.AsignaturaId), n => (decimal?)n.Nota);

            // Construir filas de la grilla
            var filas = estudiantes.Select(e => new FilaEstudianteGrilla
            {
                EstudianteId   = e.Id,
                Codigo         = e.Codigo,
                NombreCompleto = e.NombreCompleto,
                NotasPorAsignatura = asignaturas.ToDictionary(
                    a => a.Id,
                    a => notasIndex.TryGetValue((e.Id, a.Id), out var n) ? n : (decimal?)null)
            }).ToList();

            // Nombre del curso
            var curso = await _context.Cursos
                .Include(c => c.Grado)
                .FirstOrDefaultAsync(c => c.Id == cursoId);

            var vm = new SubirNotasGrillaViewModel
            {
                CursoId      = cursoId,
                CursoNombre  = curso?.NombreCompleto ?? $"Curso #{cursoId}",
                Parcial      = parcial,
                ParcialNombre = NombreParcial(parcial),
                Asignaturas  = asignaturas,
                Filas        = filas
            };
            LlenarLayoutVm(vm, "Notas");

            return View(vm);
        }

        // ── POST: /Estudiantes/GuardarGrilla ─────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GuardarGrilla(
            int cursoId, int parcial, List<NotaEntrada> entradas)
        {
            if (!VerificarDocente())
                return RedirectToAction("IniciarSesion", "Home");

            if (parcial < 1 || parcial > 4)
                return RedirectToAction(nameof(SubirNotas));

            int docenteId = ObtenerDocenteId();

            // Seguridad: el docente debe tener acceso al curso
            if (!await DocenteTieneAccesoACursoAsync(docenteId, cursoId))
            {
                TempData["ErrorMessage"] = "Sin permiso para este curso.";
                return RedirectToAction(nameof(SubirNotas));
            }

            // Asignaturas que el docente realmente tiene en ese curso
            // (whitelist para validar cada entrada del POST)
            var asignaturasPermitidas = (await _context.AsignacionDocentes
                .Where(a => a.DocenteId == docenteId && a.CursoId == cursoId && a.Activo)
                .Select(a => a.AsignaturaId)
                .ToListAsync()).ToHashSet();

            // Estudiantes válidos del curso (whitelist)
            var estudiantesDelCurso = (await _context.Estudiantes
                .Where(e => e.CursoId == cursoId && e.Activo)
                .Select(e => e.Id)
                .ToListAsync()).ToHashSet();

            int guardadas = 0;
            int omitidas  = 0;

            foreach (var entrada in entradas)
            {
                // Ignorar celdas vacías
                if (!entrada.Nota.HasValue) { omitidas++; continue; }

                // Validación de rango
                if (entrada.Nota.Value < 0 || entrada.Nota.Value > 100) { omitidas++; continue; }

                // Validación de seguridad: estudiante pertenece al curso
                if (!estudiantesDelCurso.Contains(entrada.EstudianteId)) { omitidas++; continue; }

                // Validación de seguridad: asignatura asignada al docente en ese curso
                if (!asignaturasPermitidas.Contains(entrada.AsignaturaId)) { omitidas++; continue; }

                // Buscar registro existente o crear uno nuevo
                var registro = await _context.NotasParciales
                    .FirstOrDefaultAsync(n => n.EstudianteId == entrada.EstudianteId
                                           && n.AsignaturaId == entrada.AsignaturaId
                                           && n.Parcial == (byte)parcial);

                if (registro == null)
                {
                    _context.NotasParciales.Add(new NotaParcial
                    {
                        EstudianteId  = entrada.EstudianteId,
                        AsignaturaId  = entrada.AsignaturaId,
                        Parcial       = (byte)parcial,
                        Nota          = entrada.Nota.Value,
                        FechaRegistro = DateTime.UtcNow
                    });
                }
                else
                {
                    registro.Nota          = entrada.Nota.Value;
                    registro.FechaRegistro = DateTime.UtcNow;
                }

                guardadas++;
            }

            await _context.SaveChangesAsync();

            // ── Recalcular Nota{N} del estudiante ─────────────────
            // Para cada estudiante del curso, promedia todas sus notas
            // en NotasParciales de este parcial y escribe el resultado
            // en Estudiante.Nota1/2/3/4 → SQL Server recalcula Promedio y Estado.

            var idsEstudiantesAfectados = entradas
                .Where(e => e.Nota.HasValue && estudiantesDelCurso.Contains(e.EstudianteId))
                .Select(e => e.EstudianteId)
                .Distinct()
                .ToList();

            foreach (var estId in idsEstudiantesAfectados)
            {
                var notasDeParcial = await _context.NotasParciales
                    .Where(n => n.EstudianteId == estId && n.Parcial == (byte)parcial)
                    .Select(n => n.Nota)
                    .ToListAsync();

                if (!notasDeParcial.Any()) continue;

                decimal promedioParcial = Math.Round(notasDeParcial.Average(), 2);

                var estudiante = await _context.Estudiantes
                    .FirstOrDefaultAsync(e => e.Id == estId);

                if (estudiante == null) continue;

                switch (parcial)
                {
                    case 1: estudiante.Nota1 = promedioParcial; estudiante.FechaNota1 = DateTime.Now; break;
                    case 2: estudiante.Nota2 = promedioParcial; estudiante.FechaNota2 = DateTime.Now; break;
                    case 3: estudiante.Nota3 = promedioParcial; estudiante.FechaNota3 = DateTime.Now; break;
                    case 4: estudiante.Nota4 = promedioParcial; estudiante.FechaNota4 = DateTime.Now; break;
                }
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "GuardarGrilla — Docente {D}, Curso {C}, Parcial {P}: {G} guardadas, {O} omitidas.",
                docenteId, cursoId, parcial, guardadas, omitidas);

            TempData["MensajeExito"] =
                $"{guardadas} nota(s) guardada(s) correctamente para el {NombreParcial(parcial)}.";

            // Regresar a la misma grilla para que el docente vea los cambios
            return RedirectToAction(nameof(SubirNotasGrilla), new { cursoId, parcial });
        }

        // ── GET: /Estudiantes/Prediccion ──────────────────────────
        [HttpGet]
        public async Task<IActionResult> Prediccion()
        {
            if (!VerificarDocente())
                return RedirectToAction("IniciarSesion", "Home");

            int docenteId = ObtenerDocenteId();

            var cursoIds = docenteId > 0
                ? await _context.AsignacionDocentes
                    .Where(a => a.DocenteId == docenteId && a.Activo)
                    .Select(a => a.CursoId)
                    .Distinct()
                    .ToListAsync()
                : new List<int>();

            var todos = cursoIds.Any()
                ? await _context.Estudiantes
                    .Include(e => e.Curso)
                        .ThenInclude(c => c!.Grado)
                    .Where(e => cursoIds.Contains(e.CursoId) && e.Activo)
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
