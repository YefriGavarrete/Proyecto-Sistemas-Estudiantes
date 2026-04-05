using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Proyecto_Evaluacion_Estudiantes.Models
{
    // ViewModel para el listado + formulario de Gestion de Cursos
    public class GestionCursosViewModel : LayoutViewModel
    {
        public List<Curso>   Cursos   { get; set; } = new();

        public List<SelectListItem> GradosItems         { get; set; } = new();
        public List<SelectListItem> DocentesTutorItems  { get; set; } = new();

        public int? EditandoId { get; set; }

        [Required(ErrorMessage = "Seleccione un grado.")]
        [Display(Name = "Grado")]
        public int? FormGradoId { get; set; }

        [Required(ErrorMessage = "Ingrese la seccion.")]
        [StringLength(5, ErrorMessage = "Maximo 5 caracteres.")]
        [Display(Name = "Seccion")]
        public string FormSeccion { get; set; } = string.Empty;

        [Required(ErrorMessage = "El periodo es obligatorio.")]
        [StringLength(20, ErrorMessage = "Maximo 20 caracteres.")]
        [Display(Name = "Periodo")]
        public string FormPeriodo { get; set; } = string.Empty;

        [Display(Name = "Docente Tutor")]
        public int? FormDocenteTutorId { get; set; }

        [Display(Name = "Activo")]
        public bool FormActivo { get; set; } = true;
    }

    // Fila de asignatura con el docente asignado para la pantalla de Asignacion
    public class FilaAsignacionDocente
    {
        public int      AsignaturaId    { get; set; }
        public string   AsignaturaNombre { get; set; } = string.Empty;
        public string   AsignaturaCodigo { get; set; } = string.Empty;

        // Id de la AsignacionDocente existente (null si no hay asignacion aun)
        public int?     AsignacionId    { get; set; }

        // Docente actualmente asignado (0 = sin asignar)
        public int      DocenteId       { get; set; }

        public bool     Activo          { get; set; } = true;
    }

    // ViewModel para la pantalla de Asignacion de Docentes
    public class AsignacionDocentesViewModel : LayoutViewModel
    {
        public int      CursoId         { get; set; }
        public string   CursoNombre     { get; set; } = string.Empty;
        public string   GradoNivel      { get; set; } = string.Empty;

        public List<SelectListItem> CursosItems    { get; set; } = new();

        public List<SelectListItem> DocentesItems  { get; set; } = new();

        public List<FilaAsignacionDocente> Filas   { get; set; } = new();
    }
}
