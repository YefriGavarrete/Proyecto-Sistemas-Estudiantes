using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Proyecto_Evaluacion_Estudiantes.Models
{
    [Table("Administradores")]
    public class Administrador
    {
        [Key]
        [Column("Id_Administradores")]
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Nombre { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string Apellido { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string NombreUsuario { get; set; } = string.Empty;

        /// <summary>Hash BCrypt de la contraseña.</summary>
        [Required]
        [StringLength(256)]
        public string Contrasena { get; set; } = string.Empty;

        public bool Activo { get; set; } = true;

        [NotMapped]
        public string NombreCompleto => $"{Nombre} {Apellido}";
    }
}
