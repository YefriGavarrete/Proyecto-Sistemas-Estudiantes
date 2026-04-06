using System.ComponentModel.DataAnnotations;

namespace Proyecto_Evaluacion_Estudiantes.Models
{
    public class ConfiguracionAsignaturasViewModel : LayoutViewModel
    {
        public List<Asignatura> Asignaturas { get; set; } = new();

        // Id de la asignatura que se está editando (null = modo crear)
        public int? EditandoId { get; set; }
        [Required(ErrorMessage = "El nombre de la asignatura es obligatorio.")]
        [StringLength(150, ErrorMessage = "Máximo 150 caracteres.")]
        [Display(Name = "Asignatura")]
        public string FormNombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El código es obligatorio.")]
        [StringLength(20, ErrorMessage = "Máximo 20 caracteres.")]
        [Display(Name = "Código")]
        public string FormCodigo { get; set; } = string.Empty;

        [Required(ErrorMessage = "Seleccione el nivel de aplicación.")]
        [Display(Name = "Nivel de Aplicación")]
        public string FormNivelAplicacion { get; set; } = "Todos";

        [Display(Name = "Activo")]
        public bool FormActivo { get; set; } = true;
    }
}
