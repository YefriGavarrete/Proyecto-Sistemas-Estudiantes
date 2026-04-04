using System.ComponentModel.DataAnnotations;

namespace Proyecto_Evaluacion_Estudiantes.Models
{
    public class ConfiguracionGradosViewModel : LayoutViewModel
    {
        public List<Grado> Grados { get; set; } = new();

        public int? EditandoId { get; set; }
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(100, ErrorMessage = "Máximo 100 caracteres.")]
        [Display(Name = "Nombre")]
        public string FormNombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El código es obligatorio.")]
        [StringLength(10, ErrorMessage = "Máximo 10 caracteres.")]
        [Display(Name = "Código")]
        public string FormCodigo { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nivel es obligatorio.")]
        [Display(Name = "Nivel")]
        public string FormNivel { get; set; } = "Primaria";

        [StringLength(300)]
        [Display(Name = "Descripción")]
        public string? FormDescripcion { get; set; }

        [Display(Name = "Activo")]
        public bool FormActivo { get; set; } = true;
    }
}
