namespace Proyecto_Evaluacion_Estudiantes.Models
{
    /*ViewModel del Dashboard del Docente (Home/Index).
    Hereda de LayoutViewModel para proveer los datos del perfil del sidebar
    y agrega las estadísticas académicas del grupo.*/

    public class DocenteDashboardViewModel : LayoutViewModel
    {
        public int     TotalEstudiantes    { get; set; }
        public decimal PromedioGeneral     { get; set; }
        public decimal PorcentajeAprobados { get; set; }
        public int     EstudiantesEnRiesgo { get; set; }


        public int TotalInscritos  { get; set; }
        public int TotalAprobados  { get; set; }
        public int TotalReprobados { get; set; }
        public int TotalEnRiesgoIA { get; set; }
    }
}
