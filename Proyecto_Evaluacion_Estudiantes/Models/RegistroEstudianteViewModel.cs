using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Proyecto_Evaluacion_Estudiantes.Models
{

    /// Incluye el dropdown de Cursos filtrado por AsignacionDocente del docente en sesion.
    public class RegistroEstudianteViewModel : LayoutViewModel
    {

        public List<SelectListItem> CursosDisponibles { get; set; } = new();

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(100, ErrorMessage = "Maximo 100 caracteres.")]
        [Display(Name = "Nombre")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El apellido es obligatorio.")]
        [StringLength(100, ErrorMessage = "Maximo 100 caracteres.")]
        [Display(Name = "Apellido")]
        public string Apellido { get; set; } = string.Empty;

        [Required(ErrorMessage = "La fecha de nacimiento es obligatoria.")]
        [DataType(DataType.Date)]
        [Display(Name = "Fecha de Nacimiento")]
        public DateTime FechaNacimiento { get; set; } = new DateTime(2000, 1, 1);

        [StringLength(20)]
        [Display(Name = "Identidad")]
        public string? Identidad { get; set; }

        [Required(ErrorMessage = "El correo es obligatorio.")]
        [EmailAddress(ErrorMessage = "Formato de correo invalido.")]
        [StringLength(150)]
        [Display(Name = "Correo Electronico")]
        public string Correo { get; set; } = string.Empty;

        [StringLength(20)]
        [Display(Name = "Telefono")]
        public string? Telefono { get; set; }

        [StringLength(15)]
        [Display(Name = "Genero")]
        public string? Genero { get; set; }

        [StringLength(20)]
        [Display(Name = "Seccion")]
        public string? Seccion { get; set; }

        [StringLength(500)]
        [Display(Name = "Observaciones")]
        public string? Observaciones { get; set; }

        [Display(Name = "Activo")]
        public bool Activo { get; set; } = true;



        [Required(ErrorMessage = "Debe seleccionar un curso.")]
        [Display(Name = "Curso / Grado")]
        public int? CursoId { get; set; }
    }
}
