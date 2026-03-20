using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Proyecto_Evaluacion_Estudiantes.Models
{
    [Table("Cursos")]
    public class Curso
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre del curso es obligatorio.")]
        [StringLength(200, ErrorMessage = "Máximo 200 caracteres.")]
        [Display(Name = "Nombre del Curso")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El código del curso es obligatorio.")]
        [StringLength(30)]
        [Display(Name = "Código")]
        public string Codigo { get; set; } = string.Empty;

        [Required(ErrorMessage = "El período es obligatorio.")]
        [StringLength(20)]
        [Display(Name = "Período")]
        public string Periodo { get; set; } = string.Empty;

        [Required]
        public int DocenteId { get; set; }

        [ForeignKey(nameof(DocenteId))]
        public Docente? Docente { get; set; }

        public bool Activo { get; set; } = true;

        // Relación 1:N con Estudiantes
        public ICollection<Estudiante> Estudiantes { get; set; } = new List<Estudiante>();
    }
}
