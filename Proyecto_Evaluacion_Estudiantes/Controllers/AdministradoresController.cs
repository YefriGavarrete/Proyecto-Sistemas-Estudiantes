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

            var vm = new AdminIndexViewModel
            {
                NombreAdmin   = HttpContext.Session.GetString("NombreDocente") ?? "Administrador",
                TituloUsuario = "Administrador",
                CodigoUsuario = HttpContext.Session.GetString("CodigoDocente") ?? "---",
                Sistema       = "EduPath AI",
                Periodo       = "2026-1",
                EsAdmin       = true,
                ActiveMenu    = "Administrador",

                TotalAdmins   = await _context.Administradores.CountAsync(a => a.Activo),
                TotalDocentes = await _context.Docentes.CountAsync(d => d.Activo)
            };


            ViewData["NombreDocente"] = vm.NombreUsuario;
            ViewData["TituloDocente"] = vm.TituloUsuario;
            ViewData["CodigoDocente"] = vm.CodigoUsuario;
            ViewData["CursoActual"]   = vm.Sistema;
            ViewData["Periodo"]       = vm.Periodo;
            ViewData["EsAdmin"]       = true;
            ViewData["ActiveMenu"]    = "Administrador";

            return View(vm);
        }

        [HttpGet]
        public IActionResult ConfiguracionUsuarios()
        {
            if (!VerificarAdmin())
                return RedirectToAction("IniciarSesion", "Home");

            CargarViewData("Administrador");

            var modelo = new Docente { Titulo = "Lic.", Activo = true };

            if (TempData["MensajeExito"] is string msg)
                TempData["MensajeExito"] = msg; // Re-pass para la vista via TempData

            return View(modelo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfiguracionUsuarios(
            [Bind("NombreCompleto,Titulo,Usuario,Correo,Activo")] Docente modelo,
            string? ContrasenaInput)
        {
            if (!VerificarAdmin())
                return RedirectToAction("IniciarSesion", "Home");

            CargarViewData("Administrador");

            ModelState.Remove("Contrasena");
            ModelState.Remove("FechaCreacion");
            ModelState.Remove("Cursos");

            if (!ModelState.IsValid)
                return View(modelo);

            // Buscar docente existente o crear uno nuevo
            var docente = await _context.Docentes
                .FirstOrDefaultAsync(d => d.Usuario == modelo.Usuario);

            if (docente == null)
            {
                docente = new Docente
                {
                    Usuario       = modelo.Usuario,
                    FechaCreacion = DateTime.Now
                };
                _context.Docentes.Add(docente);
            }

            docente.NombreCompleto = modelo.NombreCompleto.Trim();
            docente.Titulo         = modelo.Titulo.Trim();
            docente.Correo         = modelo.Correo.Trim();
            docente.Activo         = modelo.Activo;

            if (!string.IsNullOrWhiteSpace(ContrasenaInput))
                docente.Contrasena = BCrypt.Net.BCrypt.HashPassword(ContrasenaInput, 11);
            else if (string.IsNullOrEmpty(docente.Contrasena))
                docente.Contrasena = BCrypt.Net.BCrypt.HashPassword("Cambiar123!", 11);

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Catedrático guardado: {Titulo} {Nombre}", modelo.Titulo, modelo.NombreCompleto);

            TempData["MensajeExito"] =
                $"Catedrático {modelo.Titulo} {modelo.NombreCompleto} guardado correctamente.";

            return RedirectToAction(nameof(ConfiguracionUsuarios));
        }

        // ── GET: /Administradores/ConfiguracionAdministradores
        [HttpGet]
        public IActionResult ConfiguracionAdministradores()
        {
            if (!VerificarAdmin())
                return RedirectToAction("IniciarSesion", "Home");

            CargarViewData("Administrador");

            var modelo = new Administrador { Activo = true };

            if (TempData["MensajeExito"] is string msg)
                TempData["MensajeExito"] = msg;

            return View(modelo);
        }

        // ── POST: /Administradores/ConfiguracionAdministradores
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfiguracionAdministradores(
            [Bind("Nombre,Apellido,NombreUsuario,Activo")] Administrador modelo,
            string? ContrasenaInput)
        {
            if (!VerificarAdmin())
                return RedirectToAction("IniciarSesion", "Home");

            CargarViewData("Administrador");

            // Contrasena no viene del form — se recibe como ContrasenaInput por separado
            ModelState.Remove("Contrasena");

            // Validaciones de negocio que el modelo no puede expresar con atributos
            if (string.IsNullOrWhiteSpace(ContrasenaInput) || ContrasenaInput.Length < 8)
                ModelState.AddModelError("ContrasenaInput", "La contraseña debe tener mínimo 8 caracteres.");

            if (!string.IsNullOrWhiteSpace(modelo.NombreUsuario))
            {
                int digitos = modelo.NombreUsuario.Count(char.IsDigit);
                if (digitos < 2)
                    ModelState.AddModelError("NombreUsuario", "El usuario debe incluir al menos 2 dígitos.");
            }

            if (!ModelState.IsValid)
                return View(modelo);

            // Verificar unicidad del usuario
            bool existe = await _context.Administradores
                .AnyAsync(a => a.NombreUsuario == modelo.NombreUsuario.Trim());

            if (existe)
            {
                TempData["ErrorMessage"] =
                    $"El usuario «{modelo.NombreUsuario}» ya está en uso. Genera otro diferente.";
                return View(modelo);
            }

            var admin = new Administrador
            {
                Nombre        = modelo.Nombre.Trim(),
                Apellido      = modelo.Apellido.Trim(),
                NombreUsuario = modelo.NombreUsuario.Trim(),
                Contrasena    = BCrypt.Net.BCrypt.HashPassword(ContrasenaInput!, 11),
                Activo        = true
            };

            _context.Administradores.Add(admin);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Admin creado: {Usuario}", modelo.NombreUsuario);

            TempData["MensajeExito"] =
                $"Administrador {modelo.Nombre} {modelo.Apellido} creado correctamente.";

            return RedirectToAction(nameof(ConfiguracionAdministradores));
        }


        private async Task<ConfiguracionCursosViewModel> CrearVmCursos()
        {
            return new ConfiguracionCursosViewModel
            {
                NombreUsuario = HttpContext.Session.GetString("NombreDocente") ?? "Administrador",
                TituloUsuario = "Administrador",
                CodigoUsuario = HttpContext.Session.GetString("CodigoDocente") ?? "---",
                Sistema       = "EduPath AI",
                Periodo       = "2026-1",
                EsAdmin       = true,
                ActiveMenu    = "Administrador",
                Cursos        = await _context.Cursos
                                    .Include(c => c.DocenteTutor)
                                    .OrderBy(c => c.Nombre)
                                    .ToListAsync(),
                Docentes      = await _context.Docentes
                                    .Where(d => d.Activo)
                                    .OrderBy(d => d.NombreCompleto)
                                    .ToListAsync(),
                FormActivo    = true
            };
        }

        private void LimpiarModelStateCursos()
        {
            foreach (var key in new[] {
                "Cursos", "Docentes",
                "NombreUsuario", "TituloUsuario", "CodigoUsuario",
                "Sistema", "Periodo", "ActiveMenu", "EsAdmin"
            })
                ModelState.Remove(key);
        }

        // ── GET: /Administradores/ConfiguracionCursos ──────────
        [HttpGet]
        public async Task<IActionResult> ConfiguracionCursos(int? editarId)
        {
            if (!VerificarAdmin())
                return RedirectToAction("IniciarSesion", "Home");

            CargarViewData("Administrador");

            var vm = await CrearVmCursos();

            if (editarId.HasValue)
            {
                var curso = vm.Cursos.FirstOrDefault(c => c.Id == editarId.Value);
                if (curso != null)
                {
                    vm.EditandoId    = curso.Id;
                    vm.FormNombre    = curso.Nombre;
                    vm.FormCodigo    = curso.Codigo;
                    vm.FormPeriodo   = curso.Periodo;
                    vm.FormDocenteId = curso.DocenteTutorId ?? 0;
                    vm.FormActivo    = curso.Activo;
                }
            }

            return View(vm);
        }

        // ── POST: /Administradores/ConfiguracionCursos (Crear) ─
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfiguracionCursos(ConfiguracionCursosViewModel vm)
        {
            if (!VerificarAdmin())
                return RedirectToAction("IniciarSesion", "Home");

            CargarViewData("Administrador");
            LimpiarModelStateCursos();

            // Recargar listas para devolver la vista si hay error
            vm.Cursos   = await _context.Cursos.Include(c => c.DocenteTutor).OrderBy(c => c.Nombre).ToListAsync();
            vm.Docentes = await _context.Docentes.Where(d => d.Activo).OrderBy(d => d.NombreCompleto).ToListAsync();
            vm.EsAdmin  = true;
            vm.ActiveMenu = "Administrador";

            if (!ModelState.IsValid)
                return View(vm);

            // Código único
            bool codigoExiste = await _context.Cursos
                .AnyAsync(c => c.Codigo == vm.FormCodigo.Trim().ToUpper());
            if (codigoExiste)
            {
                ModelState.AddModelError("FormCodigo", "Ya existe un curso con ese código.");
                return View(vm);
            }

            var nuevo = new Curso
            {
                Nombre         = vm.FormNombre.Trim(),
                Codigo         = vm.FormCodigo.Trim().ToUpper(),
                Periodo        = vm.FormPeriodo.Trim(),
                DocenteTutorId = vm.FormDocenteId > 0 ? vm.FormDocenteId : null,
                Activo         = vm.FormActivo
            };

            _context.Cursos.Add(nuevo);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Curso creado: {Nombre}", nuevo.Nombre);
            TempData["MensajeExito"] = $"Curso '{nuevo.Nombre}' creado correctamente.";
            return RedirectToAction(nameof(ConfiguracionCursos));
        }

        // ── POST: /Administradores/EditarCurso ─────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarCurso(ConfiguracionCursosViewModel vm)
        {
            if (!VerificarAdmin())
                return RedirectToAction("IniciarSesion", "Home");

            CargarViewData("Administrador");
            LimpiarModelStateCursos();

            if (!vm.EditandoId.HasValue)
                return RedirectToAction(nameof(ConfiguracionCursos));

            var curso = await _context.Cursos.FindAsync(vm.EditandoId.Value);
            if (curso == null)
            {
                TempData["ErrorMessage"] = "Curso no encontrado.";
                return RedirectToAction(nameof(ConfiguracionCursos));
            }

            vm.Cursos   = await _context.Cursos.Include(c => c.DocenteTutor).OrderBy(c => c.Nombre).ToListAsync();
            vm.Docentes = await _context.Docentes.Where(d => d.Activo).OrderBy(d => d.NombreCompleto).ToListAsync();
            vm.EsAdmin  = true;
            vm.ActiveMenu = "Administrador";

            if (!ModelState.IsValid)
                return View("ConfiguracionCursos", vm);

            // Código único (excluyendo el propio registro)
            bool codigoExiste = await _context.Cursos
                .AnyAsync(c => c.Codigo == vm.FormCodigo.Trim().ToUpper() && c.Id != vm.EditandoId.Value);
            if (codigoExiste)
            {
                ModelState.AddModelError("FormCodigo", "Ya existe un curso con ese código.");
                return View("ConfiguracionCursos", vm);
            }

            curso.Nombre         = vm.FormNombre.Trim();
            curso.Codigo         = vm.FormCodigo.Trim().ToUpper();
            curso.Periodo        = vm.FormPeriodo.Trim();
            curso.DocenteTutorId = vm.FormDocenteId > 0 ? vm.FormDocenteId : null;
            curso.Activo         = vm.FormActivo;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Curso actualizado: {Nombre}", curso.Nombre);
            TempData["MensajeExito"] = $"Curso '{curso.Nombre}' actualizado correctamente.";
            return RedirectToAction(nameof(ConfiguracionCursos));
        }

        // ── POST: /Administradores/EliminarCurso ───────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarCurso(int id)
        {
            if (!VerificarAdmin())
                return RedirectToAction("IniciarSesion", "Home");

            var curso = await _context.Cursos
                .Include(c => c.Estudiantes)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (curso == null)
            {
                TempData["ErrorMessage"] = "Curso no encontrado.";
                return RedirectToAction(nameof(ConfiguracionCursos));
            }

            if (curso.Estudiantes.Any())
            {
                TempData["ErrorMessage"] =
                    $"No se puede eliminar '{curso.Nombre}' porque tiene {curso.Estudiantes.Count} estudiante(s) registrado(s).";
                return RedirectToAction(nameof(ConfiguracionCursos));
            }

            _context.Cursos.Remove(curso);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Curso eliminado: {Nombre}", curso.Nombre);
            TempData["MensajeExito"] = $"Curso '{curso.Nombre}' eliminado correctamente.";
            return RedirectToAction(nameof(ConfiguracionCursos));
        }




        private async Task<ConfiguracionGradosViewModel> CrearVmGrados()
        {
            return new ConfiguracionGradosViewModel
            {
                NombreUsuario = HttpContext.Session.GetString("NombreDocente") ?? "Administrador",
                TituloUsuario = "Administrador",
                CodigoUsuario = HttpContext.Session.GetString("CodigoDocente") ?? "---",
                Sistema       = "EduPath AI",
                Periodo       = "2026-1",
                EsAdmin       = true,
                ActiveMenu    = "Academica",
                Grados        = await _context.Grados.OrderBy(g => g.Orden).ToListAsync()
            };
        }

        private void LimpiarModelStateGrados()
        {
            foreach (var key in new[] {
                "Grados", "NombreUsuario", "TituloUsuario",
                "CodigoUsuario", "Sistema", "Periodo", "ActiveMenu", "EsAdmin"
            })
                ModelState.Remove(key);
        }

        // ── GET: /Administradores/ConfiguracionGrados ──────────
        [HttpGet]
        public async Task<IActionResult> ConfiguracionGrados(int? editarId)
        {
            if (!VerificarAdmin())
                return RedirectToAction("IniciarSesion", "Home");

            CargarViewData("Academica");
            var vm = await CrearVmGrados();

            if (editarId.HasValue)
            {
                var grado = vm.Grados.FirstOrDefault(g => g.Id == editarId.Value);
                if (grado != null)
                {
                    vm.EditandoId      = grado.Id;
                    vm.FormNombre      = grado.Nombre;
                    vm.FormCodigo      = grado.Codigo;
                    vm.FormNivel       = grado.Nivel;
                    vm.FormDescripcion = grado.Descripcion;
                    vm.FormActivo      = grado.Activo;
                }
            }

            return View(vm);
        }

        // ── POST: /Administradores/EditarGrado ─────────────────
        // Los grados son fijos (1°-9°), solo se permite editar, no crear ni eliminar.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarGrado(ConfiguracionGradosViewModel vm)
        {
            if (!VerificarAdmin())
                return RedirectToAction("IniciarSesion", "Home");

            CargarViewData("Academica");
            LimpiarModelStateGrados();

            if (!vm.EditandoId.HasValue)
                return RedirectToAction(nameof(ConfiguracionGrados));

            var grado = await _context.Grados.FindAsync(vm.EditandoId.Value);
            if (grado == null)
            {
                TempData["ErrorMessage"] = "Grado no encontrado.";
                return RedirectToAction(nameof(ConfiguracionGrados));
            }

            vm.Grados = await _context.Grados.OrderBy(g => g.Orden).ToListAsync();
            vm.EsAdmin  = true;
            vm.ActiveMenu = "Academica";

            if (!ModelState.IsValid)
                return View("ConfiguracionGrados", vm);

            grado.Nombre      = vm.FormNombre.Trim();
            grado.Codigo      = vm.FormCodigo.Trim();
            grado.Nivel       = vm.FormNivel;
            grado.Descripcion = vm.FormDescripcion?.Trim();
            grado.Activo      = vm.FormActivo;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Grado actualizado: {Nombre}", grado.Nombre);
            TempData["MensajeExito"] = $"Grado '{grado.Nombre}' actualizado correctamente.";
            return RedirectToAction(nameof(ConfiguracionGrados));
        }


        //  CONFIGURACIÓN ACADÉMICA — ASIGNATURAS
        private async Task<ConfiguracionAsignaturasViewModel> CrearVmAsignaturas()
        {
            return new ConfiguracionAsignaturasViewModel
            {
                NombreUsuario = HttpContext.Session.GetString("NombreDocente") ?? "Administrador",
                TituloUsuario = "Administrador",
                CodigoUsuario = HttpContext.Session.GetString("CodigoDocente") ?? "---",
                Sistema       = "EduPath AI",
                Periodo       = "2026-1",
                EsAdmin       = true,
                ActiveMenu    = "Academica",
                Asignaturas   = await _context.Asignaturas
                                    .OrderBy(a => a.NivelAplicacion)
                                    .ThenBy(a => a.Nombre)
                                    .ToListAsync()
            };
        }

        private void LimpiarModelStateAsignaturas()
        {
            foreach (var key in new[] {
                "Asignaturas", "NombreUsuario", "TituloUsuario",
                "CodigoUsuario", "Sistema", "Periodo", "ActiveMenu", "EsAdmin"
            })
                ModelState.Remove(key);
        }

        // ── GET: /Administradores/ConfiguracionAsignaturas ─────
        [HttpGet]
        public async Task<IActionResult> ConfiguracionAsignaturas(int? editarId)
        {
            if (!VerificarAdmin())
                return RedirectToAction("IniciarSesion", "Home");

            CargarViewData("Academica");
            var vm = await CrearVmAsignaturas();

            if (editarId.HasValue)
            {
                var a = vm.Asignaturas.FirstOrDefault(x => x.Id == editarId.Value);
                if (a != null)
                {
                    vm.EditandoId          = a.Id;
                    vm.FormNombre          = a.Nombre;
                    vm.FormCodigo          = a.Codigo;
                    vm.FormNivelAplicacion = a.NivelAplicacion;
                    vm.FormActivo          = a.Activo;
                }
            }

            return View(vm);
        }

        // ── POST: /Administradores/ConfiguracionAsignaturas (Crear) ──
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfiguracionAsignaturas(ConfiguracionAsignaturasViewModel vm)
        {
            if (!VerificarAdmin())
                return RedirectToAction("IniciarSesion", "Home");

            CargarViewData("Academica");
            LimpiarModelStateAsignaturas();

            vm.Asignaturas = await _context.Asignaturas
                .OrderBy(a => a.NivelAplicacion).ThenBy(a => a.Nombre).ToListAsync();
            vm.EsAdmin    = true;
            vm.ActiveMenu = "Academica";

            if (!ModelState.IsValid)
                return View(vm);

            // Código único
            bool codigoExiste = await _context.Asignaturas
                .AnyAsync(a => a.Codigo == vm.FormCodigo.Trim().ToUpper());
            if (codigoExiste)
            {
                ModelState.AddModelError("FormCodigo", "Ya existe una asignatura con ese código.");
                return View(vm);
            }

            var nueva = new Asignatura
            {
                Nombre          = vm.FormNombre.Trim(),
                Codigo          = vm.FormCodigo.Trim().ToUpper(),
                NivelAplicacion = vm.FormNivelAplicacion,
                Activo          = vm.FormActivo
            };

            _context.Asignaturas.Add(nueva);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Asignatura creada: {Nombre}", nueva.Nombre);
            TempData["MensajeExito"] = $"Asignatura '{nueva.Nombre}' registrada correctamente.";
            return RedirectToAction(nameof(ConfiguracionAsignaturas));
        }

        // ── POST: /Administradores/EditarAsignatura ────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarAsignatura(ConfiguracionAsignaturasViewModel vm)
        {
            if (!VerificarAdmin())
                return RedirectToAction("IniciarSesion", "Home");

            CargarViewData("Academica");
            LimpiarModelStateAsignaturas();

            if (!vm.EditandoId.HasValue)
                return RedirectToAction(nameof(ConfiguracionAsignaturas));

            var asignatura = await _context.Asignaturas.FindAsync(vm.EditandoId.Value);
            if (asignatura == null)
            {
                TempData["ErrorMessage"] = "Asignatura no encontrada.";
                return RedirectToAction(nameof(ConfiguracionAsignaturas));
            }

            vm.Asignaturas = await _context.Asignaturas
                .OrderBy(a => a.NivelAplicacion).ThenBy(a => a.Nombre).ToListAsync();
            vm.EsAdmin    = true;
            vm.ActiveMenu = "Academica";

            if (!ModelState.IsValid)
                return View("ConfiguracionAsignaturas", vm);

            // Código único (excluyendo el propio)
            bool codigoExiste = await _context.Asignaturas
                .AnyAsync(a => a.Codigo == vm.FormCodigo.Trim().ToUpper() && a.Id != vm.EditandoId.Value);
            if (codigoExiste)
            {
                ModelState.AddModelError("FormCodigo", "Ya existe una asignatura con ese código.");
                return View("ConfiguracionAsignaturas", vm);
            }

            asignatura.Nombre          = vm.FormNombre.Trim();
            asignatura.Codigo          = vm.FormCodigo.Trim().ToUpper();
            asignatura.NivelAplicacion = vm.FormNivelAplicacion;
            asignatura.Activo          = vm.FormActivo;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Asignatura actualizada: {Nombre}", asignatura.Nombre);
            TempData["MensajeExito"] = $"Asignatura '{asignatura.Nombre}' actualizada correctamente.";
            return RedirectToAction(nameof(ConfiguracionAsignaturas));
        }

        // ── POST: /Administradores/EliminarAsignatura ──────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarAsignatura(int id)
        {
            if (!VerificarAdmin())
                return RedirectToAction("IniciarSesion", "Home");

            var asignatura = await _context.Asignaturas.FindAsync(id);
            if (asignatura == null)
            {
                TempData["ErrorMessage"] = "Asignatura no encontrada.";
                return RedirectToAction(nameof(ConfiguracionAsignaturas));
            }

            _context.Asignaturas.Remove(asignatura);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Asignatura eliminada: {Nombre}", asignatura.Nombre);
            TempData["MensajeExito"] = $"Asignatura '{asignatura.Nombre}' eliminada correctamente.";
            return RedirectToAction(nameof(ConfiguracionAsignaturas));
        }

        // ════════════════════════════════════════════════════════════
        //  GESTIÓN DE CURSOS
        // ════════════════════════════════════════════════════════════

        private async Task<GestionCursosViewModel> CrearVmGestionCursos()
        {
            var grados   = await _context.Grados
                               .Where(g => g.Activo)
                               .OrderBy(g => g.Orden)
                               .ToListAsync();

            var docentes = await _context.Docentes
                               .Where(d => d.Activo)
                               .OrderBy(d => d.NombreCompleto)
                               .ToListAsync();

            return new GestionCursosViewModel
            {
                NombreUsuario    = HttpContext.Session.GetString("NombreDocente") ?? "Administrador",
                TituloUsuario    = "Administrador",
                CodigoUsuario    = HttpContext.Session.GetString("CodigoDocente") ?? "---",
                Sistema          = "EduPath AI",
                Periodo          = "2026-1",
                EsAdmin          = true,
                ActiveMenu       = "Cursos",
                Cursos           = await _context.Cursos
                                       .Include(c => c.Grado)
                                       .Include(c => c.DocenteTutor)
                                       .Where(c => c.GradoId != null)
                                       .OrderBy(c => c.GradoId)
                                       .ThenBy(c => c.Seccion)
                                       .ToListAsync(),
                GradosItems      = grados.Select(g => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Value = g.Id.ToString(),
                    Text  = $"{g.Nombre} ({g.Nivel})"
                }).ToList(),
                DocentesTutorItems = docentes.Select(d => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Value = d.Id.ToString(),
                    Text  = $"{d.Titulo} {d.NombreCompleto}"
                }).ToList(),
                FormActivo = true
            };
        }

        private void LimpiarModelStateGestionCursos()
        {
            foreach (var key in new[] {
                "Cursos", "GradosItems", "DocentesTutorItems",
                "NombreUsuario", "TituloUsuario", "CodigoUsuario",
                "Sistema", "Periodo", "ActiveMenu", "EsAdmin"
            })
                ModelState.Remove(key);
        }

        // ── GET: /Administradores/GestionCursos ────────────────
        [HttpGet]
        public async Task<IActionResult> GestionCursos(int? editarId)
        {
            if (!VerificarAdmin())
                return RedirectToAction("IniciarSesion", "Home");

            CargarViewData("Cursos");
            var vm = await CrearVmGestionCursos();

            if (editarId.HasValue)
            {
                var curso = vm.Cursos.FirstOrDefault(c => c.Id == editarId.Value);
                if (curso != null)
                {
                    vm.EditandoId          = curso.Id;
                    vm.FormGradoId         = curso.GradoId;
                    vm.FormSeccion         = curso.Seccion ?? string.Empty;
                    vm.FormPeriodo         = curso.Periodo;
                    vm.FormDocenteTutorId  = curso.DocenteTutorId;
                    vm.FormActivo          = curso.Activo;
                }
            }

            return View(vm);
        }

        // ── POST: /Administradores/GestionCursos (Crear) ───────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GestionCursos(GestionCursosViewModel vm)
        {
            if (!VerificarAdmin())
                return RedirectToAction("IniciarSesion", "Home");

            CargarViewData("Cursos");
            LimpiarModelStateGestionCursos();

            var grados = await _context.Grados.Where(g => g.Activo).OrderBy(g => g.Orden).ToListAsync();
            var docentes = await _context.Docentes.Where(d => d.Activo).OrderBy(d => d.NombreCompleto).ToListAsync();

            vm.GradosItems = grados.Select(g => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
            {
                Value = g.Id.ToString(),
                Text  = $"{g.Nombre} ({g.Nivel})"
            }).ToList();
            vm.DocentesTutorItems = docentes.Select(d => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
            {
                Value = d.Id.ToString(),
                Text  = $"{d.Titulo} {d.NombreCompleto}"
            }).ToList();
            vm.Cursos    = await _context.Cursos
                               .Include(c => c.Grado).Include(c => c.DocenteTutor)
                               .Where(c => c.GradoId != null)
                               .OrderBy(c => c.GradoId).ThenBy(c => c.Seccion)
                               .ToListAsync();
            vm.EsAdmin   = true;
            vm.ActiveMenu = "Cursos";

            if (!ModelState.IsValid)
                return View(vm);

            // Verificar que no exista ya el mismo grado+seccion+periodo
            bool duplicado = await _context.Cursos.AnyAsync(c =>
                c.GradoId == vm.FormGradoId &&
                c.Seccion == vm.FormSeccion.Trim().ToUpper() &&
                c.Periodo == vm.FormPeriodo.Trim());

            if (duplicado)
            {
                ModelState.AddModelError("FormSeccion", "Ya existe un curso con ese Grado, Sección y Período.");
                return View(vm);
            }

            var grado  = grados.FirstOrDefault(g => g.Id == vm.FormGradoId);
            var codigo = $"{grado?.Codigo ?? "CRS"}-{vm.FormSeccion.Trim().ToUpper()}-{vm.FormPeriodo.Trim()}";

            // Si el código generado ya existe, agregar sufijo numérico
            if (await _context.Cursos.AnyAsync(c => c.Codigo == codigo))
                codigo = $"{codigo}-{DateTime.Now.Ticks % 1000}";

            var nuevo = new Curso
            {
                GradoId        = vm.FormGradoId,
                Seccion        = vm.FormSeccion.Trim().ToUpper(),
                Periodo        = vm.FormPeriodo.Trim(),
                DocenteTutorId = vm.FormDocenteTutorId,
                Codigo         = codigo,
                Nombre         = string.Empty,
                Activo         = vm.FormActivo
            };

            _context.Cursos.Add(nuevo);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Curso (gestion) creado: GradoId={Grado} Sec={Sec}", nuevo.GradoId, nuevo.Seccion);
            TempData["MensajeExito"] = "Curso registrado correctamente.";
            return RedirectToAction(nameof(GestionCursos));
        }

        // ── POST: /Administradores/EditarGestionCurso ──────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarGestionCurso(GestionCursosViewModel vm)
        {
            if (!VerificarAdmin())
                return RedirectToAction("IniciarSesion", "Home");

            CargarViewData("Cursos");
            LimpiarModelStateGestionCursos();

            if (!vm.EditandoId.HasValue)
                return RedirectToAction(nameof(GestionCursos));

            var curso = await _context.Cursos.FindAsync(vm.EditandoId.Value);
            if (curso == null)
            {
                TempData["ErrorMessage"] = "Curso no encontrado.";
                return RedirectToAction(nameof(GestionCursos));
            }

            var grados   = await _context.Grados.Where(g => g.Activo).OrderBy(g => g.Orden).ToListAsync();
            var docentes = await _context.Docentes.Where(d => d.Activo).OrderBy(d => d.NombreCompleto).ToListAsync();

            vm.GradosItems = grados.Select(g => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
            {
                Value = g.Id.ToString(), Text = $"{g.Nombre} ({g.Nivel})"
            }).ToList();
            vm.DocentesTutorItems = docentes.Select(d => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
            {
                Value = d.Id.ToString(), Text = $"{d.Titulo} {d.NombreCompleto}"
            }).ToList();
            vm.Cursos    = await _context.Cursos
                               .Include(c => c.Grado).Include(c => c.DocenteTutor)
                               .Where(c => c.GradoId != null)
                               .OrderBy(c => c.GradoId).ThenBy(c => c.Seccion)
                               .ToListAsync();
            vm.EsAdmin    = true;
            vm.ActiveMenu = "Cursos";

            if (!ModelState.IsValid)
                return View("GestionCursos", vm);

            // Duplicado (excluyendo el propio)
            bool duplicado = await _context.Cursos.AnyAsync(c =>
                c.GradoId == vm.FormGradoId &&
                c.Seccion == vm.FormSeccion.Trim().ToUpper() &&
                c.Periodo == vm.FormPeriodo.Trim() &&
                c.Id != vm.EditandoId.Value);

            if (duplicado)
            {
                ModelState.AddModelError("FormSeccion", "Ya existe un curso con ese Grado, Sección y Período.");
                return View("GestionCursos", vm);
            }

            curso.GradoId        = vm.FormGradoId;
            curso.Seccion        = vm.FormSeccion.Trim().ToUpper();
            curso.Periodo        = vm.FormPeriodo.Trim();
            curso.DocenteTutorId = vm.FormDocenteTutorId;
            curso.Activo         = vm.FormActivo;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Curso (gestion) actualizado Id={Id}", curso.Id);
            TempData["MensajeExito"] = "Curso actualizado correctamente.";
            return RedirectToAction(nameof(GestionCursos));
        }

        // ── POST: /Administradores/EliminarGestionCurso ────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarGestionCurso(int id)
        {
            if (!VerificarAdmin())
                return RedirectToAction("IniciarSesion", "Home");

            var curso = await _context.Cursos
                .Include(c => c.Estudiantes)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (curso == null)
            {
                TempData["ErrorMessage"] = "Curso no encontrado.";
                return RedirectToAction(nameof(GestionCursos));
            }

            if (curso.Estudiantes.Any())
            {
                TempData["ErrorMessage"] =
                    $"No se puede eliminar el curso porque tiene {curso.Estudiantes.Count} estudiante(s) registrado(s).";
                return RedirectToAction(nameof(GestionCursos));
            }

            _context.Cursos.Remove(curso);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Curso (gestion) eliminado Id={Id}", id);
            TempData["MensajeExito"] = "Curso eliminado correctamente.";
            return RedirectToAction(nameof(GestionCursos));
        }

        //  Donde asigno un docente
        [HttpGet]
        public async Task<IActionResult> AsignacionDocentes(int? cursoId)
        {
            if (!VerificarAdmin())
                return RedirectToAction("IniciarSesion", "Home");

            CargarViewData("Cursos");

            // Lista de cursos con grado (solo los del nuevo flujo)
            var cursos = await _context.Cursos
                .Include(c => c.Grado)
                .Where(c => c.GradoId != null && c.Activo)
                .OrderBy(c => c.GradoId)
                .ThenBy(c => c.Seccion)
                .ToListAsync();

            var docentes = await _context.Docentes
                .Where(d => d.Activo)
                .OrderBy(d => d.NombreCompleto)
                .ToListAsync();

            var vm = new AsignacionDocentesViewModel
            {
                NombreUsuario = HttpContext.Session.GetString("NombreDocente") ?? "Administrador",
                TituloUsuario = "Administrador",
                CodigoUsuario = HttpContext.Session.GetString("CodigoDocente") ?? "---",
                Sistema       = "EduPath AI",
                Periodo       = "2026-1",
                EsAdmin       = true,
                ActiveMenu    = "Cursos",
                CursosItems   = cursos.Select(c => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Value    = c.Id.ToString(),
                    Text     = c.NombreCompleto,
                    Selected = c.Id == cursoId
                }).ToList(),
                DocentesItems = docentes.Select(d => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Value = d.Id.ToString(),
                    Text  = $"{d.Titulo} {d.NombreCompleto}"
                }).ToList()
            };

            // Si se seleccionó un curso, cargar sus asignaturas y asignaciones actuales
            if (cursoId.HasValue)
            {
                var curso = cursos.FirstOrDefault(c => c.Id == cursoId.Value);
                if (curso != null)
                {
                    vm.CursoId     = curso.Id;
                    vm.CursoNombre = curso.NombreCompleto;
                    vm.GradoNivel  = curso.Grado?.Nivel ?? "Primaria";

                    // Filtrar asignaturas según nivel del grado
                    // Nivel puede ser "Tercer Ciclo" (con espacio) o "TercerCiclo"
                    string nivelGrado = curso.Grado?.Nivel ?? string.Empty;
                    bool esTercerCiclo = nivelGrado.Replace(" ", "").Equals("TercerCiclo", StringComparison.OrdinalIgnoreCase);

                    var asignaturas = await _context.Asignaturas
                        .Where(a => a.Activo &&
                               (esTercerCiclo || a.NivelAplicacion == "Todos"))
                        .OrderBy(a => a.Nombre)
                        .ToListAsync();

                    // Asignaciones existentes para este curso
                    var asignaciones = await _context.AsignacionDocentes
                        .Where(x => x.CursoId == cursoId.Value)
                        .ToListAsync();

                    vm.Filas = asignaturas.Select(a =>
                    {
                        var asig = asignaciones.FirstOrDefault(x => x.AsignaturaId == a.Id);
                        return new FilaAsignacionDocente
                        {
                            AsignaturaId     = a.Id,
                            AsignaturaNombre = a.Nombre,
                            AsignaturaCodigo = a.Codigo,
                            AsignacionId     = asig?.Id,
                            DocenteId        = asig?.DocenteId ?? 0,
                            Activo           = asig?.Activo ?? true
                        };
                    }).ToList();
                }
            }

            return View(vm);
        }

        // ── POST: /Administradores/GuardarAsignaciones ─────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GuardarAsignaciones(int cursoId,
            List<int> asignaturaIds, List<int> docenteIds)
        {
            if (!VerificarAdmin())
                return RedirectToAction("IniciarSesion", "Home");

            for (int i = 0; i < asignaturaIds.Count; i++)
            {
                int asigId    = asignaturaIds[i];
                int docenteId = docenteIds.Count > i ? docenteIds[i] : 0;

                var existente = await _context.AsignacionDocentes
                    .FirstOrDefaultAsync(x => x.CursoId == cursoId && x.AsignaturaId == asigId);

                if (docenteId <= 0)
                {
                    // Sin docente: eliminar asignacion existente si la hay
                    if (existente != null)
                        _context.AsignacionDocentes.Remove(existente);
                    continue;
                }

                if (existente == null)
                {
                    _context.AsignacionDocentes.Add(new AsignacionDocente
                    {
                        CursoId      = cursoId,
                        AsignaturaId = asigId,
                        DocenteId    = docenteId,
                        Activo       = true
                    });
                }
                else
                {
                    existente.DocenteId = docenteId;
                    existente.Activo    = true;
                }
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Asignaciones guardadas para CursoId={Id}", cursoId);
            TempData["MensajeExito"] = "Asignaciones guardadas correctamente.";
            return RedirectToAction(nameof(AsignacionDocentes), new { cursoId });
        }
    }
}
