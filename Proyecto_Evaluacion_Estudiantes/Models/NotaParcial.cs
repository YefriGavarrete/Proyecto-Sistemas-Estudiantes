using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Proyecto_Evaluacion_Estudiantes.Models
{
    /// <summary>
    /// Almacena la nota de un estudiante para una asignatura específica y un parcial.
    /// La combinación (EstudianteId, AsignaturaId, Parcial) es única.
    ///
    /// Flujo de escritura:
    ///   1. El docente ingresa notas en la grilla de RegistroNotas.
    ///   2. El sistema guarda/actualiza registros en esta tabla.
    ///   3. Tras guardar, recalcula el promedio de todas las asignaturas
    ///      de ese Parcial y escribe el resultado en Estudiante.Nota{Parcial}.
    ///   4. SQL Server recomputa automáticamente Promedio y Estado
    ///      (columnas PERSISTED en la tabla Estudiantes).
    /// </summary>
    [Table("NotasParciales")]
    public class NotaParcial
    {
        [Key]
        public int Id { get; set; }

        // ── Relación con Estudiante ──────────────────────────────
        [Required]
        public int EstudianteId { get; set; }

        [ForeignKey(nameof(EstudianteId))]
        public Estudiante? Estudiante { get; set; }

        // ── Relación con Asignatura ──────────────────────────────
        [Required]
        public int AsignaturaId { get; set; }

        [ForeignKey(nameof(AsignaturaId))]
        public Asignatura? Asignatura { get; set; }

        // ── Datos de la nota ─────────────────────────────────────

        /// <summary>Número de parcial: 1, 2, 3 o 4.</summary>
        [Required]
        [Range(1, 4, ErrorMessage = "El parcial debe ser 1, 2, 3 o 4.")]
        public byte Parcial { get; set; }

        /// <summary>Nota del estudiante en esta asignatura para este parcial (0–100).</summary>
        [Required]
        [Range(0, 100, ErrorMessage = "La nota debe estar entre 0 y 100.")]
        [Column(TypeName = "decimal(5,2)")]
        public decimal Nota { get; set; }

        /// <summary>Fecha/hora UTC en que se registró o actualizó la nota.</summary>
        public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;
    }
}
