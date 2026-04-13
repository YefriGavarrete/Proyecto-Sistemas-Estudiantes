using Microsoft.ML.Data;

namespace Proyecto_Evaluacion_Estudiantes.Models
{
    /// <summary>
    /// Resultado del modelo ML.NET.
    /// Score está en [0, 1]: probabilidad estimada de riesgo académico.
    /// </summary>
    public class RiesgoModelOutput
    {
        [ColumnName("Score")]
        public float Score { get; set; }
    }
}
