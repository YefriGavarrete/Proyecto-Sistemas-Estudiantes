using System.ComponentModel.DataAnnotations;

namespace Proyecto_Evaluacion_Estudiantes.Models
{
    public class Asignatura
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(150, ErrorMessage = "Máximo 150 caracteres.")]
        [Display(Name = "Asignatura")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El código es obligatorio.")]
        [StringLength(20, ErrorMessage = "Máximo 20 caracteres.")]
        [Display(Name = "Código")]
        public string Codigo { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        [Display(Name = "Nivel de Aplicación")]
        public string NivelAplicacion { get; set; } = "Todos";

        [Display(Name = "Activo")]
        public bool Activo { get; set; } = true;

        // Es calculada en memoria (EF la ignora) 
        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public bool SoloTercerCiclo => NivelAplicacion == "TercerCiclo";
    }
}
