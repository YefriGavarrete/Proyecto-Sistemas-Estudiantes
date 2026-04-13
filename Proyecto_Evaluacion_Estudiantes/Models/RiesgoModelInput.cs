using Microsoft.ML.Data;

namespace Proyecto_Evaluacion_Estudiantes.Models
{
    /// <summary>
    /// Datos de entrada al modelo ML.NET.
    /// Usa -1f como centinela para parciales sin nota (0 es nota válida).
    /// </summary>
    public class RiesgoModelInput
    {
        [LoadColumn(0)] public float Nota1 { get; set; }
        [LoadColumn(1)] public float Nota2 { get; set; }
        [LoadColumn(2)] public float Nota3 { get; set; }
        [LoadColumn(3)] public float Nota4 { get; set; }
        [LoadColumn(4)] public float PromedioActual { get; set; }
        [LoadColumn(5)] public float NotasRegistradas { get; set; }

        /// <summary>Etiqueta de entrenamiento: 0.0 = sin riesgo, 1.0 = riesgo máximo.</summary>
        [LoadColumn(6)]
        [ColumnName("Label")]
        public float Label { get; set; }
    }
}
