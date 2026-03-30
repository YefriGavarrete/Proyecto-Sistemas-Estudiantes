namespace Proyecto_Evaluacion_Estudiantes.Models
{
    /// <summary>
    /// ViewModel para la lista de estudiantes (Estudiantes/Index).
    /// Hereda LayoutViewModel para el sidebar.
    /// </summary>
    public class EstudianteIndexViewModel : LayoutViewModel
    {
        public List<Estudiante> Estudiantes { get; set; } = new();

        // Totales para el resumen rápido
        public int TotalEstudiantes  => Estudiantes.Count;
        public int TotalAprobados    => Estudiantes.Count(e => e.Estado == "Aprobado");
        public int TotalReprobados   => Estudiantes.Count(e => e.Estado == "Reprobado");
        public int TotalSinNotas     => Estudiantes.Count(e => e.Estado == "Sin Notas" || e.Estado == null);
    }
}
