namespace Proyecto_Evaluacion_Estudiantes.Models
{
    public class PrediccionRiesgoViewModel : LayoutViewModel
    {
        // ── Datos del estudiante ─────────────────────────────────────────────
        public Estudiante EstudianteInfo { get; set; } = null!;

        // ── Resultado del modelo ML ──────────────────────────────────────────
        /// <summary>Probabilidad de riesgo en [0.0, 1.0].</summary>
        public float Probabilidad { get; set; }

        /// <summary>"Bajo", "Medio" o "Alto".</summary>
        public string NivelRiesgo { get; set; } = "";

        /// <summary>Clase Bootstrap: text-success / text-warning / text-danger.</summary>
        public string ColorRiesgo { get; set; } = "";

        /// <summary>Clase Bootstrap para el badge de fondo.</summary>
        public string BadgeRiesgo { get; set; } = "";

        /// <summary>Texto explicativo según el nivel de riesgo.</summary>
        public string MensajeRiesgo { get; set; } = "";

        // ── Datos del gráfico Chart.js ───────────────────────────────────────
        public string[] EtiquetasGrafico { get; set; }
            = ["Parcial 1", "Parcial 2", "Parcial 3", "Parcial 4"];

        /// <summary>Nota de cada parcial para el gráfico; null = sin nota (punto no dibujado).</summary>
        public float?[] DatosGrafico { get; set; } = new float?[4];

        /// <summary>Promedio de los parciales con nota registrada.</summary>
        public float PromedioActual { get; set; }
    }
}
