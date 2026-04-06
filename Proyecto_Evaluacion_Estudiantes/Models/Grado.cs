using System.ComponentModel.DataAnnotations;

namespace Proyecto_Evaluacion_Estudiantes.Models
{
    public class Grado
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(100, ErrorMessage = "Máximo 100 caracteres.")]
        [Display(Name = "Nombre")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El código es obligatorio.")]
        [StringLength(10, ErrorMessage = "Máximo 10 caracteres.")]
        [Display(Name = "Código")]
        public string Codigo { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nivel es obligatorio.")]
        [StringLength(50)]
        [Display(Name = "Nivel")]
        public string Nivel { get; set; } = "Primaria";

        [Display(Name = "Orden")]
        public int Orden { get; set; }

        [StringLength(300)]
        [Display(Name = "Descripción")]
        public string? Descripcion { get; set; }

        [Display(Name = "Activo")]
        public bool Activo { get; set; } = true;
    }
}
