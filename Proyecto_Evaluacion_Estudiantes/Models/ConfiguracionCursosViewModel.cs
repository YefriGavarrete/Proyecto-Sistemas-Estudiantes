using System.ComponentModel.DataAnnotations;

namespace Proyecto_Evaluacion_Estudiantes.Models
{
    public class ConfiguracionCursosViewModel : LayoutViewModel
    {
        public List<Curso>   Cursos   { get; set; } = new();
        public List<Docente> Docentes { get; set; } = new();

        public int? EditandoId { get; set; }

        [Required(ErrorMessage = "El nombre del curso es obligatorio.")]
        [StringLength(200, ErrorMessage = "Máximo 200 caracteres.")]
        [Display(Name = "Nombre del Curso")]
        public string FormNombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El código es obligatorio.")]
        [StringLength(30, ErrorMessage = "Máximo 30 caracteres.")]
        [Display(Name = "Código")]
        public string FormCodigo { get; set; } = string.Empty;

        [Required(ErrorMessage = "El período es obligatorio.")]
        [StringLength(20, ErrorMessage = "Máximo 20 caracteres.")]
        [Display(Name = "Período")]
        public string FormPeriodo { get; set; } = string.Empty;

        [Range(1, int.MaxValue, ErrorMessage = "Seleccione un docente.")]
        [Display(Name = "Docente Asignado")]
        public int FormDocenteId { get; set; }

        [Display(Name = "Activo")]
        public bool FormActivo { get; set; } = true;
    }
}
