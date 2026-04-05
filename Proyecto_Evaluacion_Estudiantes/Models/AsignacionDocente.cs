using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Proyecto_Evaluacion_Estudiantes.Models
{
    [Table("AsignacionDocente")]
    public class AsignacionDocente
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int CursoId { get; set; }

        [ForeignKey(nameof(CursoId))]
        public Curso? Curso { get; set; }

        [Required]
        public int AsignaturaId { get; set; }

        [ForeignKey(nameof(AsignaturaId))]
        public Asignatura? Asignatura { get; set; }

        [Required]
        public int DocenteId { get; set; }

        [ForeignKey(nameof(DocenteId))]
        public Docente? Docente { get; set; }

        public bool Activo { get; set; } = true;
    }
}
