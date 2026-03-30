using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Proyecto_Evaluacion_Estudiantes.Models
{
    [Table("Estudiantes")]
    public class Estudiante
    {
        [Key]
        public int Id { get; set; }

        [StringLength(20)]
        [Display(Name = "Código")]
        public string Codigo { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(100, ErrorMessage = "Máximo 100 caracteres.")]
        [Display(Name = "Nombre")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El apellido es obligatorio.")]
        [StringLength(100, ErrorMessage = "Máximo 100 caracteres.")]
        [Display(Name = "Apellido")]
        public string Apellido { get; set; } = string.Empty;

        [Required(ErrorMessage = "La fecha de nacimiento es obligatoria.")]
        [DataType(DataType.Date)]
        [Display(Name = "Fecha de Nacimiento")]
        public DateTime FechaNacimiento { get; set; }

        [StringLength(20)]
        [Display(Name = "Identidad / DUI")]
        public string? Identidad { get; set; }

        [Required(ErrorMessage = "El correo es obligatorio.")]
        [EmailAddress(ErrorMessage = "Formato de correo inválido.")]
        [StringLength(150)]
        [Display(Name = "Correo Electrónico")]
        public string Correo { get; set; } = string.Empty;

        [Phone]
        [StringLength(20)]
        [Display(Name = "Teléfono")]
        public string? Telefono { get; set; }

        [StringLength(15)]
        [Display(Name = "Género")]
        public string? Genero { get; set; }

        [StringLength(20)]
        [Display(Name = "Sección")]
        public string? Seccion { get; set; }

        [StringLength(500)]
        [Display(Name = "Observaciones")]
        public string? Observaciones { get; set; }

        [Display(Name = "Activo")]
        public bool Activo { get; set; } = true;

        // ── Notas por Parcial ────────────────────────────────────
        [Range(0, 100, ErrorMessage = "La nota debe estar entre 0 y 100.")]
        [Display(Name = "Parcial 1")]
        [Column(TypeName = "decimal(5,2)")]
        public decimal? Nota1 { get; set; }

        [Display(Name = "Fecha Parcial 1")]
        public DateTime? FechaNota1 { get; set; }

        [Range(0, 100, ErrorMessage = "La nota debe estar entre 0 y 100.")]
        [Display(Name = "Parcial 2")]
        [Column(TypeName = "decimal(5,2)")]
        public decimal? Nota2 { get; set; }

        [Display(Name = "Fecha Parcial 2")]
        public DateTime? FechaNota2 { get; set; }

        [Range(0, 100, ErrorMessage = "La nota debe estar entre 0 y 100.")]
        [Display(Name = "Parcial 3")]
        [Column(TypeName = "decimal(5,2)")]
        public decimal? Nota3 { get; set; }

        [Display(Name = "Fecha Parcial 3")]
        public DateTime? FechaNota3 { get; set; }

        [Range(0, 100, ErrorMessage = "La nota debe estar entre 0 y 100.")]
        [Display(Name = "Parcial 4")]
        [Column(TypeName = "decimal(5,2)")]
        public decimal? Nota4 { get; set; }

        [Display(Name = "Fecha Parcial 4")]
        public DateTime? FechaNota4 { get; set; }





        // ── Columnas calculadas por SQL Server ───────────────────
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        [Column(TypeName = "decimal(5,2)")]
        [Display(Name = "Promedio")]
        public decimal? Promedio { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        [StringLength(20)]
        [Display(Name = "Estado")]
        public string? Estado { get; set; }

        // ── Predicción IA ────────────────────────────────────────
        [Display(Name = "En Riesgo (IA)")]
        public bool? EnRiesgoIA { get; set; }





        // ── Relación con Curso ───────────────────────────────────
        [Required(ErrorMessage = "Debe seleccionar un curso.")]
        [Display(Name = "Curso")]
        public int CursoId { get; set; }

        [ForeignKey(nameof(CursoId))]
        public Curso? Curso { get; set; }

        [Display(Name = "Fecha de Registro")]
        public DateTime FechaRegistro { get; set; } = DateTime.Now;




        // ── Propiedades calculadas (solo en memoria) ─────────────
        [NotMapped]
        public string NombreCompleto => $"{Nombre} {Apellido}";

        /// <summary>Edad calculada en tiempo real desde FechaNacimiento.</summary>
        [NotMapped]
        public int Edad
        {
            get
            {
                var hoy  = DateTime.Today;
                int edad = hoy.Year - FechaNacimiento.Year;
                if (FechaNacimiento.Date > hoy.AddYears(-edad)) edad--;
                return edad;
            }
        }

        [NotMapped]
        public string EstadoBadge => Estado switch
        {
            "Aprobado"  => "bg-lightgreen",
            "Reprobado" => "bg-lightred",
            _           => "bg-secondary"
        };

        /// <summary>Promedio calculado en memoria (antes de SaveChanges).</summary>
        [NotMapped]
        public decimal PromedioCalculado
        {
            get
            {
                var notas = new[] { Nota1, Nota2, Nota3, Nota4 }
                    .Where(n => n.HasValue)
                    .Select(n => n!.Value)
                    .ToList();
                return notas.Count == 0 ? 0 : Math.Round(notas.Average(), 2);
            }
        }

        [NotMapped]
        public string EstadoCalculado =>
            PromedioCalculado > 0
                ? (PromedioCalculado >= 60 ? "Aprobado" : "Reprobado")
                : "Sin Notas";

        [NotMapped]
        public bool EnRiesgoCalculado => PromedioCalculado > 0 && PromedioCalculado < 65;

        public decimal? NotaParcial(int parcial) => parcial switch
        {
            1 => Nota1,
            2 => Nota2,
            3 => Nota3,
            4 => Nota4,
            _ => null
        };

        public DateTime? FechaParcial(int parcial) => parcial switch
        {
            1 => FechaNota1,
            2 => FechaNota2,
            3 => FechaNota3,
            4 => FechaNota4,
            _ => null
        };
    }
}
