using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Proyecto_Evaluacion_Estudiantes.Models
{
    // ─────────────────────────────────────────────────────────────
    // PASO 1 — Pantalla de selección (Curso + Parcial)
    // ─────────────────────────────────────────────────────────────

    public class SubirNotasSeleccionViewModel : LayoutViewModel
    {
        /// <summary>Cursos donde el docente tiene AsignacionDocente activa.</summary>
        public List<SelectListItem> CursosDisponibles { get; set; } = new();

        /// <summary>Parciales 1–4 (opciones fijas).</summary>
        public List<SelectListItem> ParcialesItems { get; set; } = new()
        {
            new SelectListItem { Value = "1", Text = "Primer Parcial"   },
            new SelectListItem { Value = "2", Text = "Segundo Parcial"  },
            new SelectListItem { Value = "3", Text = "Tercer Parcial"   },
            new SelectListItem { Value = "4", Text = "Cuarto Parcial"   },
        };

        [Required(ErrorMessage = "Debes seleccionar un curso.")]
        public int? CursoId { get; set; }

        [Required(ErrorMessage = "Debes seleccionar un parcial.")]
        [Range(1, 4, ErrorMessage = "El parcial debe ser 1, 2, 3 o 4.")]
        public int? Parcial { get; set; }
    }

    // ─────────────────────────────────────────────────────────────
    // PASO 2 — Grilla de calificaciones
    // ─────────────────────────────────────────────────────────────

    /// <summary>Describe una columna de la grilla (una asignatura del docente en ese curso).</summary>
    public class AsignaturaColumna
    {
        public int    Id     { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Codigo { get; set; } = string.Empty;
    }

    /// <summary>Una fila de la grilla: un estudiante con sus notas actuales por asignatura.</summary>
    public class FilaEstudianteGrilla
    {
        public int    EstudianteId   { get; set; }
        public string Codigo         { get; set; } = string.Empty;
        public string NombreCompleto { get; set; } = string.Empty;

        /// <summary>
        /// Mapa AsignaturaId → nota existente en NotasParciales para el parcial seleccionado.
        /// Null significa que todavía no hay nota para esa celda.
        /// </summary>
        public Dictionary<int, decimal?> NotasPorAsignatura { get; set; } = new();
    }

    /// <summary>
    /// Una entrada de nota enviada desde el form de la grilla.
    /// Cada celda (Estudiante × Asignatura) genera un ítem de esta clase en el POST.
    /// </summary>
    public class NotaEntrada
    {
        public int      EstudianteId { get; set; }
        public int      AsignaturaId { get; set; }

        /// <summary>
        /// Null o cadena vacía = celda sin nota; se ignora en el guardado.
        /// Valor numérico válido = insertar o actualizar en NotasParciales.
        /// </summary>
        public decimal? Nota         { get; set; }
    }

    /// <summary>ViewModel principal del Paso 2.</summary>
    public class SubirNotasGrillaViewModel : LayoutViewModel
    {
        public int    CursoId      { get; set; }
        public string CursoNombre  { get; set; } = string.Empty;
        public int    Parcial      { get; set; }
        public string ParcialNombre { get; set; } = string.Empty;

        /// <summary>Columnas = asignaturas que el docente tiene en este curso.</summary>
        public List<AsignaturaColumna>    Asignaturas { get; set; } = new();

        /// <summary>Filas = estudiantes activos del curso.</summary>
        public List<FilaEstudianteGrilla> Filas       { get; set; } = new();

        // ── Helpers ──────────────────────────────────────────────

        public static string NombreParcial(int p) => p switch
        {
            1 => "Primer Parcial",
            2 => "Segundo Parcial",
            3 => "Tercer Parcial",
            4 => "Cuarto Parcial",
            _ => $"Parcial {p}"
        };

        public int TotalCeldas       => Asignaturas.Count * Filas.Count;
        public int CeldasConNota     => Filas.Sum(f => f.NotasPorAsignatura.Count(kvp => kvp.Value.HasValue));
        public int CeldasSinNota     => TotalCeldas - CeldasConNota;
    }
}
