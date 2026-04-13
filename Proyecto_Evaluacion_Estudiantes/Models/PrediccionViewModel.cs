namespace Proyecto_Evaluacion_Estudiantes.Models
{
    public class PrediccionRiesgoViewModel : LayoutViewModel
    {
        public Estudiante EstudianteInfo { get; set; } = null!;

        // Resultado del modelo ML
        public float Probabilidad { get; set; }


        public string NivelRiesgo { get; set; } = "";

        public string ColorRiesgo { get; set; } = "";

        public string BadgeRiesgo { get; set; } = "";
        public string MensajeRiesgo { get; set; } = "";
        public string[] EtiquetasGrafico { get; set; }
            = ["Parcial 1", "Parcial 2", "Parcial 3", "Parcial 4"];
        public float?[] DatosGrafico { get; set; } = new float?[4];


        public float PromedioActual { get; set; }
    }
}
