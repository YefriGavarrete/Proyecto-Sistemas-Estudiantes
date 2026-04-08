namespace Proyecto_Evaluacion_Estudiantes.Models
{
    public class ReporteGeneralViewModel : LayoutViewModel
    {
        public int TotalEstudiantes  { get; set; }
        public int Aprobados { get; set; }
        public int Reprobados { get; set; }
        public int SinNotas { get; set; }
        public decimal PromedioGeneral { get; set; }
        public decimal PctAprobados { get; set; }

        // Distribución de notas (rangos)
        public int Rango0_59   { get; set; }
        public int Rango60_69  { get; set; }
        public int Rango70_79  { get; set; }
        public int Rango80_89  { get; set; }
        public int Rango90_100 { get; set; }

        // 
        public decimal PromedioParcial1 { get; set; }
        public decimal PromedioParcial2 { get; set; }
        public decimal PromedioParcial3 { get; set; }
        public decimal PromedioParcial4 { get; set; }

        public List<Estudiante> Estudiantes { get; set; } = new();
    }

    public class ReporteRiesgoViewModel : LayoutViewModel
    {
        public List<Estudiante> Estudiantes { get; set; } = new();
    }

    public class PrediccionViewModel : LayoutViewModel
    {
        public List<Estudiante> EstudiantesRiesgo  { get; set; } = new();
        public List<Estudiante> EstudiantesAtencion { get; set; } = new();
        public int TotalAnalizados { get; set; }
    }
}
