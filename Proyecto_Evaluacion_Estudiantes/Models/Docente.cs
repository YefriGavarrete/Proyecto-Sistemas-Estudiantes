using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Proyecto_Evaluacion_Estudiantes.Models
{

    /// La contraseña siempre se almacena como hash BCrypt, NUNCA en texto plano.
    [Table("Docentes")]
    public class Docente
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre completo es obligatorio.")]
        [StringLength(150, ErrorMessage = "Máximo 150 caracteres.")]
        [Display(Name = "Nombre Completo")]
        public string NombreCompleto { get; set; } = string.Empty;


        [Required]
        [StringLength(20)]
        [Display(Name = "Título")]
        public string Titulo { get; set; } = "Lic.";

        [Required(ErrorMessage = "El nombre de usuario es obligatorio.")]
        [StringLength(80)]
        [Display(Name = "Usuario")]
        public string Usuario { get; set; } = string.Empty;

        // Hash BCrypt de la contraseña.
        [Required]
        [StringLength(256)]
        public string Contrasena { get; set; } = string.Empty;

        [Required(ErrorMessage = "El correo es obligatorio.")]


        [RegularExpression(@"^[^@\s]+@[^@\s]+\.[a-zA-Z]{2,10}$",
            ErrorMessage = "Ingrese un correo electrónico válido (ej: usuario@dominio.com).")]
        [StringLength(150)]
        [Display(Name = "Correo Electrónico")]
        public string Correo { get; set; } = string.Empty;

        [Display(Name = "Activo")]
        public bool Activo { get; set; } = true;

        [Display(Name = "Fecha de Creación")]
        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        [Display(Name = "Último Acceso")]
        public DateTime? UltimoAcceso { get; set; }


        public ICollection<Curso> Cursos { get; set; } = new List<Curso>();

        [NotMapped]
        public string NombreConTitulo => $"{Titulo} {NombreCompleto}";
    }
}
