using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Proyecto_Evaluacion_Estudiantes.Models
{
    [Table("Cursos")]
    public class Curso
    {
        [Key]
        public int Id { get; set; }


        [StringLength(200)]
        [Display(Name = "Nombre del Curso")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El codigo del curso es obligatorio.")]
        [StringLength(30)]
        [Display(Name = "Codigo")]
        public string Codigo { get; set; } = string.Empty;

        [Required(ErrorMessage = "El periodo es obligatorio.")]
        [StringLength(20)]
        [Display(Name = "Periodo")]
        public string Periodo { get; set; } = string.Empty;




        [Display(Name = "Grado")]
        public int? GradoId { get; set; }

        [ForeignKey(nameof(GradoId))]
        public Grado? Grado { get; set; }

        [StringLength(5)]
        [Display(Name = "Seccion")]
        public string? Seccion { get; set; }

        // maestro guia del curso
        [Display(Name = "Docente Tutor")]
        public int? DocenteTutorId { get; set; }

        [ForeignKey(nameof(DocenteTutorId))]
        public Docente? DocenteTutor { get; set; }

        public bool Activo { get; set; } = true;


        public ICollection<Estudiante>        Estudiantes        { get; set; } = new List<Estudiante>();
        public ICollection<AsignacionDocente> AsignacionDocentes { get; set; } = new List<AsignacionDocente>();


        [NotMapped]
        public string NombreCompleto =>
            Grado != null
                ? $"{Grado.Nombre} — Sec. {Seccion ?? "?"} ({Periodo})"
                : (!string.IsNullOrEmpty(Nombre) ? Nombre : $"Curso #{Id}");
    }
}
