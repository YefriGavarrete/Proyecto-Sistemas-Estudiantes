using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Proyecto_Evaluacion_Estudiantes.Models
{


    public class SubirNotasSeleccionViewModel : LayoutViewModel
    {
        //Me muestra los Cursos donde el docente tiene AsignacionDocente activa.
        public List<SelectListItem> CursosDisponibles { get; set; } = new();


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

    public class AsignaturaColumna
    {
        public int    Id     { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Codigo { get; set; } = string.Empty;
    }

    // Una fila de la grilla: un estudiante con sus notas actuales por asignatura
    public class FilaEstudianteGrilla
    {
        public int    EstudianteId   { get; set; }
        public string Codigo         { get; set; } = string.Empty;
        public string NombreCompleto { get; set; } = string.Empty;
        public Dictionary<int, decimal?> NotasPorAsignatura { get; set; } = new();
    }



    // Cada celda (Estudiante × Asignatura) genera un ítem de esta clase en el POST.
    public class NotaEntrada
    {
        public int      EstudianteId { get; set; }
        public int      AsignaturaId { get; set; }
        // Null o cadena vacía = celda sin nota; se ignora en el guardado.
        public decimal? Nota         { get; set; }
    }

    // Siempre heredo la ViewModel principal del layout para que tenga acceso a los datos comunes (nombre del docente, etc).
    public class SubirNotasGrillaViewModel : LayoutViewModel
    {
        public int    CursoId      { get; set; }
        public string CursoNombre  { get; set; } = string.Empty;
        public int    Parcial      { get; set; }
        public string ParcialNombre { get; set; } = string.Empty;


        public List<AsignaturaColumna>    Asignaturas { get; set; } = new();


        public List<FilaEstudianteGrilla> Filas       { get; set; } = new();

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
