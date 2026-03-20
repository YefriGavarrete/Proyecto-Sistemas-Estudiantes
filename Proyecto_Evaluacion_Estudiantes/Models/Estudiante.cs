using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Proyecto_Evaluacion_Estudiantes.Models
{
    [Table("Estudiantes")]
    public class Estudiante
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(150, ErrorMessage = "Máximo 150 caracteres.")]
        [Display(Name = "Nombre Completo")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "La edad es obligatoria.")]
        [Range(15, 99, ErrorMessage = "La edad debe estar entre 15 y 99.")]
        [Display(Name = "Edad")]
        public int Edad { get; set; }

        [Required(ErrorMessage = "El correo es obligatorio.")]
        [EmailAddress(ErrorMessage = "Formato de correo inválido.")]
        [StringLength(150)]
        [Display(Name = "Correo Electrónico")]
        public string Correo { get; set; } = string.Empty;

        [Range(0, 100, ErrorMessage = "La nota debe estar entre 0 y 100.")]
        [Display(Name = "Nota 1")]
        [Column(TypeName = "decimal(5,2)")]
        public decimal? Nota1 { get; set; }

        [Range(0, 100, ErrorMessage = "La nota debe estar entre 0 y 100.")]
        [Display(Name = "Nota 2")]
        [Column(TypeName = "decimal(5,2)")]
        public decimal? Nota2 { get; set; }

        [Range(0, 100, ErrorMessage = "La nota debe estar entre 0 y 100.")]
        [Display(Name = "Nota 3")]
        [Column(TypeName = "decimal(5,2)")]
        public decimal? Nota3 { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        [Column(TypeName = "decimal(5,2)")]
        [Display(Name = "Promedio")]
        public decimal? Promedio { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        [StringLength(20)]
        [Display(Name = "Estado")]
        public string? Estado { get; set; }

        // ── Predicción IA ───────────────────────────────────────
        /// <summary>
        /// true = En Riesgo | false = Sin Riesgo | null = No calculado todavía
        /// </summary>
        [Display(Name = "En Riesgo (IA)")]
        public bool? EnRiesgoIA { get; set; }

        // ── Relación con Curso ──────────────────────────────────
        [Required]
        [Display(Name = "Curso")]
        public int CursoId { get; set; }

        [ForeignKey(nameof(CursoId))]
        public Curso? Curso { get; set; }

        [Display(Name = "Fecha de Registro")]
        public DateTime FechaRegistro { get; set; } = DateTime.Now;

        // ── Propiedades auxiliares (solo en memoria, no en BD) ──
        [NotMapped]
        public string EstadoBadge => Estado switch
        {
            "Aprobado"  => "bg-lightgreen",
            "Reprobado" => "bg-lightred",
            _           => "bg-secondary"
        };

        /// <summary>
        /// Calcula el promedio en memoria cuando las notas aún no fueron guardadas.
        /// Útil para vistas de previsualización antes de llamar a SaveChanges().
        /// </summary>
        [NotMapped]
        public decimal PromedioCalculado
        {
            get
            {
                var notas = new[] { Nota1, Nota2, Nota3 }
                    .Where(n => n.HasValue)
                    .Select(n => n!.Value)
                    .ToList();

                return notas.Count == 0 ? 0 : Math.Round(notas.Average(), 2);
            }
        }

        [NotMapped]
        public string EstadoCalculado =>
            PromedioCalculado >= 60 ? "Aprobado" : "Reprobado";

        /// <summary>
        /// Regla simple de riesgo: promedio menor a 65 → estudiante en riesgo.
        /// Esta lógica puede reemplazarse con el modelo ML.NET cuando esté listo.
        /// </summary>
        [NotMapped]
        public bool EnRiesgoCalculado => PromedioCalculado < 65 && PromedioCalculado > 0;
    }
}
