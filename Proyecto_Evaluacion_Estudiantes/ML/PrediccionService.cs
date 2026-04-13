using Microsoft.ML;
using Microsoft.ML.Trainers.FastTree;
using Proyecto_Evaluacion_Estudiantes.Models;

namespace Proyecto_Evaluacion_Estudiantes.ML
{

    /// Servicio Singleton que entrena el modelo FastForest una vez al arrancar
    /// y expone Predecir() para estimar el riesgo académico de un estudiante.

    //Esto fue lo implemente por la falta de información sobre el dataset , por lo cual decidi generar datos sintéticos con diferentes patrones de notas y riesgos.
    //Para que al momento de hacer la predicción, el modelo pueda reconocer tendencias como riesgo alto con notas bajas,
    //tendencia descendente, incertidumbre por falta de datos, tendencia ascendente y asi sucesivamente hasta encontrar malas notas con bajo riesgo, para que no solo se base en el promedio anual
    public class PrediccionService
    {
        private readonly MLContext _mlContext;
        private readonly PredictionEngine<RiesgoModelInput, RiesgoModelOutput> _engine;

        public PrediccionService()
        {
            _mlContext = new MLContext(seed: 42);
            var modelo = EntrenarModelo();
            _engine = _mlContext.Model.CreatePredictionEngine<RiesgoModelInput, RiesgoModelOutput>(modelo);
        }

        /// Devuelve la probabilidad de riesgo en [0.0, 1.0].
        public float Predecir(RiesgoModelInput input)
        {
            var resultado = _engine.Predict(input);
            return Math.Clamp(resultado.Score, 0f, 1f);
        }

        private ITransformer EntrenarModelo()
        {
            var datos = _mlContext.Data.LoadFromEnumerable(GenerarDatosSinteticos());

            var pipeline = _mlContext.Transforms
                .Concatenate("Features",
                    nameof(RiesgoModelInput.Nota1),
                    nameof(RiesgoModelInput.Nota2),
                    nameof(RiesgoModelInput.Nota3),
                    nameof(RiesgoModelInput.Nota4),
                    nameof(RiesgoModelInput.PromedioActual),
                    nameof(RiesgoModelInput.NotasRegistradas))
                .Append(_mlContext.Regression.Trainers.FastForest(
                    new FastForestRegressionTrainer.Options
                    {
                        LabelColumnName        = "Label",
                        FeatureColumnName      = "Features",
                        NumberOfTrees          = 120,
                        NumberOfLeaves         = 20,
                        MinimumExampleCountPerLeaf = 3
                    }));

            return pipeline.Fit(datos);
        }

        // Datos sintéticos de entrenamiento.
        // SN = -1f → sin nota registrada en ese parcial.
        private static List<RiesgoModelInput> GenerarDatosSinteticos()
        {
            const float SN = -1f;
            return new List<RiesgoModelInput>
            {
                // Riesgo muy alto nutriniendo el model de con las notas bajas
                new() { Nota1=20,  Nota2=15,  Nota3=25,  Nota4=18,  PromedioActual=19.5f,  NotasRegistradas=4, Label=1.00f },
                new() { Nota1=35,  Nota2=40,  Nota3=30,  Nota4=38,  PromedioActual=35.75f, NotasRegistradas=4, Label=0.95f },
                new() { Nota1=45,  Nota2=42,  Nota3=48,  Nota4=40,  PromedioActual=43.75f, NotasRegistradas=4, Label=0.92f },
                new() { Nota1=30,  Nota2=25,  Nota3=35,  Nota4=28,  PromedioActual=29.5f,  NotasRegistradas=4, Label=1.00f },
                new() { Nota1=50,  Nota2=48,  Nota3=52,  Nota4=46,  PromedioActual=49.0f,  NotasRegistradas=4, Label=0.88f },
                new() { Nota1=55,  Nota2=53,  Nota3=58,  Nota4=54,  PromedioActual=55.0f,  NotasRegistradas=4, Label=0.78f },
                new() { Nota1=10,  Nota2=5,   Nota3=SN,  Nota4=SN,  PromedioActual=7.5f,   NotasRegistradas=2, Label=1.00f },
                new() { Nota1=40,  Nota2=35,  Nota3=SN,  Nota4=SN,  PromedioActual=37.5f,  NotasRegistradas=2, Label=0.95f },
                new() { Nota1=50,  Nota2=45,  Nota3=SN,  Nota4=SN,  PromedioActual=47.5f,  NotasRegistradas=2, Label=0.90f },
                new() { Nota1=55,  Nota2=53,  Nota3=SN,  Nota4=SN,  PromedioActual=54.0f,  NotasRegistradas=2, Label=0.82f },
                new() { Nota1=30,  Nota2=SN,  Nota3=SN,  Nota4=SN,  PromedioActual=30.0f,  NotasRegistradas=1, Label=0.92f },
                new() { Nota1=50,  Nota2=SN,  Nota3=SN,  Nota4=SN,  PromedioActual=50.0f,  NotasRegistradas=1, Label=0.78f },
                new() { Nota1=55,  Nota2=SN,  Nota3=SN,  Nota4=SN,  PromedioActual=55.0f,  NotasRegistradas=1, Label=0.72f },
                new() { Nota1=45,  Nota2=SN,  Nota3=SN,  Nota4=SN,  PromedioActual=45.0f,  NotasRegistradas=1, Label=0.87f },

                // Tendencia descendente pero no medio
                new() { Nota1=80,  Nota2=60,  Nota3=45,  Nota4=30,  PromedioActual=53.75f, NotasRegistradas=4, Label=0.85f },
                new() { Nota1=70,  Nota2=55,  Nota3=42,  Nota4=35,  PromedioActual=50.5f,  NotasRegistradas=4, Label=0.88f },
                new() { Nota1=75,  Nota2=60,  Nota3=50,  Nota4=SN,  PromedioActual=61.67fss, NotasRegistradas=3, Label=0.75f },
                new() { Nota1=65,  Nota2=55,  Nota3=45,  Nota4=SN,  PromedioActual=55.0f,  NotasRegistradas=3, Label=0.82f },
                new() { Nota1=60,  Nota2=55,  Nota3=48,  Nota4=42,  PromedioActual=51.25f, NotasRegistradas=4, Label=0.87f },
                new() { Nota1=90,  Nota2=75,  Nota3=55,  Nota4=40,  PromedioActual=65.0f,  NotasRegistradas=4, Label=0.68f },
                new() { Nota1=85,  Nota2=70,  Nota3=52,  Nota4=SN,  PromedioActual=69.0f,  NotasRegistradas=3, Label=0.60f },

                // riesgo medio
                new() { Nota1=62,  Nota2=58,  Nota3=65,  Nota4=60,  PromedioActual=61.25f, NotasRegistradas=4, Label=0.55f },
                new() { Nota1=60,  Nota2=60,  Nota3=60,  Nota4=60,  PromedioActual=60.0f,  NotasRegistradas=4, Label=0.52f },
                new() { Nota1=65,  Nota2=62,  Nota3=58,  Nota4=63,  PromedioActual=62.0f,  NotasRegistradas=4, Label=0.47f },
                new() { Nota1=58,  Nota2=62,  Nota3=60,  Nota4=59,  PromedioActual=59.75f, NotasRegistradas=4, Label=0.57f },
                new() { Nota1=63,  Nota2=SN,  Nota3=SN,  Nota4=SN,  PromedioActual=63.0f,  NotasRegistradas=1, Label=0.50f },
                new() { Nota1=62,  Nota2=65,  Nota3=SN,  Nota4=SN,  PromedioActual=63.5f,  NotasRegistradas=2, Label=0.46f },
                new() { Nota1=60,  Nota2=58,  Nota3=62,  Nota4=SN,  PromedioActual=60.0f,  NotasRegistradas=3, Label=0.54f },
                new() { Nota1=55,  Nota2=60,  Nota3=63,  Nota4=58,  PromedioActual=59.0f,  NotasRegistradas=4, Label=0.59f },
                new() { Nota1=56,  Nota2=59,  Nota3=SN,  Nota4=SN,  PromedioActual=57.5f,  NotasRegistradas=2, Label=0.65f },
                new() { Nota1=55,  Nota2=58,  Nota3=60,  Nota4=SN,  PromedioActual=57.67f, NotasRegistradas=3, Label=0.62f },
                new() { Nota1=59,  Nota2=59,  Nota3=59,  Nota4=59,  PromedioActual=59.0f,  NotasRegistradas=4, Label=0.60f },
                new() { Nota1=61,  Nota2=61,  Nota3=61,  Nota4=61,  PromedioActual=61.0f,  NotasRegistradas=4, Label=0.47f },

                // incertidumbre media 
                new() { Nota1=SN,  Nota2=SN,  Nota3=SN,  Nota4=SN,  PromedioActual=0f,     NotasRegistradas=0, Label=0.50f },
                new() { Nota1=SN,  Nota2=SN,  Nota3=SN,  Nota4=SN,  PromedioActual=0f,     NotasRegistradas=0, Label=0.55f },
                new() { Nota1=SN,  Nota2=SN,  Nota3=SN,  Nota4=SN,  PromedioActual=0f,     NotasRegistradas=0, Label=0.48f },

                // TENDENCIA ASCENDENTE desde riesgo 
                new() { Nota1=45,  Nota2=55,  Nota3=62,  Nota4=SN,  PromedioActual=54.0f,  NotasRegistradas=3, Label=0.55f },
                new() { Nota1=50,  Nota2=58,  Nota3=63,  Nota4=68,  PromedioActual=59.75f, NotasRegistradas=4, Label=0.40f },
                new() { Nota1=40,  Nota2=50,  Nota3=60,  Nota4=70,  PromedioActual=55.0f,  NotasRegistradas=4, Label=0.45f },
                new() { Nota1=30,  Nota2=45,  Nota3=60,  Nota4=75,  PromedioActual=52.5f,  NotasRegistradas=4, Label=0.48f },
                new() { Nota1=20,  Nota2=40,  Nota3=65,  Nota4=80,  PromedioActual=51.25f, NotasRegistradas=4, Label=0.46f },

                // ── BAJO RIESGO 
                new() { Nota1=70,  Nota2=72,  Nota3=68,  Nota4=71,  PromedioActual=70.25f, NotasRegistradas=4, Label=0.15f },
                new() { Nota1=65,  Nota2=68,  Nota3=70,  Nota4=67,  PromedioActual=67.5f,  NotasRegistradas=4, Label=0.22f },
                new() { Nota1=75,  Nota2=78,  Nota3=72,  Nota4=76,  PromedioActual=75.25f, NotasRegistradas=4, Label=0.10f },
                new() { Nota1=68,  Nota2=SN,  Nota3=SN,  Nota4=SN,  PromedioActual=68.0f,  NotasRegistradas=1, Label=0.25f },
                new() { Nota1=70,  Nota2=73,  Nota3=SN,  Nota4=SN,  PromedioActual=71.5f,  NotasRegistradas=2, Label=0.18f },
                new() { Nota1=65,  Nota2=70,  Nota3=68,  Nota4=SN,  PromedioActual=67.67f, NotasRegistradas=3, Label=0.22f },
                new() { Nota1=72,  Nota2=75,  Nota3=70,  Nota4=SN,  PromedioActual=72.33f, NotasRegistradas=3, Label=0.15f },
                new() { Nota1=76,  Nota2=78,  Nota3=SN,  Nota4=SN,  PromedioActual=77.0f,  NotasRegistradas=2, Label=0.12f },
                new() { Nota1=65,  Nota2=65,  Nota3=65,  Nota4=65,  PromedioActual=65.0f,  NotasRegistradas=4, Label=0.35f },
                new() { Nota1=75,  Nota2=SN,  Nota3=SN,  Nota4=SN,  PromedioActual=75.0f,  NotasRegistradas=1, Label=0.15f },


                new() { Nota1=50,  Nota2=65,  Nota3=75,  Nota4=82,  PromedioActual=68.0f,  NotasRegistradas=4, Label=0.20f },
                new() { Nota1=55,  Nota2=65,  Nota3=72,  Nota4=SN,  PromedioActual=64.0f,  NotasRegistradas=3, Label=0.30f },
                new() { Nota1=60,  Nota2=68,  Nota3=75,  Nota4=80,  PromedioActual=70.75f, NotasRegistradas=4, Label=0.15f },
                new() { Nota1=62,  Nota2=70,  Nota3=SN,  Nota4=SN,  PromedioActual=66.0f,  NotasRegistradas=2, Label=0.28f },
                new() { Nota1=60,  Nota2=65,  Nota3=70,  Nota4=75,  PromedioActual=67.5f,  NotasRegistradas=4, Label=0.22f },
                new() { Nota1=70,  Nota2=65,  Nota3=60,  Nota4=55,  PromedioActual=62.5f,  NotasRegistradas=4, Label=0.46f },
                new() { Nota1=80,  Nota2=75,  Nota3=68,  Nota4=60,  PromedioActual=70.75f, NotasRegistradas=4, Label=0.30f },
                new() { Nota1=90,  Nota2=80,  Nota3=70,  Nota4=55,  PromedioActual=73.75f, NotasRegistradas=4, Label=0.38f },
                new() { Nota1=58,  Nota2=60,  Nota3=55,  Nota4=62,  PromedioActual=58.75f, NotasRegistradas=4, Label=0.60f },

                // MUY BAJO RIESGO — promedio >= 80 
                new() { Nota1=85,  Nota2=88,  Nota3=90,  Nota4=87,  PromedioActual=87.5f,  NotasRegistradas=4, Label=0.02f },
                new() { Nota1=90,  Nota2=92,  Nota3=88,  Nota4=91,  PromedioActual=90.25f, NotasRegistradas=4, Label=0.01f },
                new() { Nota1=95,  Nota2=98,  Nota3=97,  Nota4=96,  PromedioActual=96.5f,  NotasRegistradas=4, Label=0.00f },
                new() { Nota1=80,  Nota2=82,  Nota3=85,  Nota4=83,  PromedioActual=82.5f,  NotasRegistradas=4, Label=0.05f },
                new() { Nota1=88,  Nota2=SN,  Nota3=SN,  Nota4=SN,  PromedioActual=88.0f,  NotasRegistradas=1, Label=0.08f },
                new() { Nota1=90,  Nota2=92,  Nota3=SN,  Nota4=SN,  PromedioActual=91.0f,  NotasRegistradas=2, Label=0.04f },
                new() { Nota1=85,  Nota2=88,  Nota3=90,  Nota4=SN,  PromedioActual=87.67f, NotasRegistradas=3, Label=0.03f },
                new() { Nota1=82,  Nota2=85,  Nota3=SN,  Nota4=SN,  PromedioActual=83.5f,  NotasRegistradas=2, Label=0.07f },
                new() { Nota1=100, Nota2=100, Nota3=100, Nota4=100, PromedioActual=100.0f, NotasRegistradas=4, Label=0.00f },
                new() { Nota1=78,  Nota2=80,  Nota3=82,  Nota4=79,  PromedioActual=79.75f, NotasRegistradas=4, Label=0.08f },
                new() { Nota1=85,  Nota2=SN,  Nota3=SN,  Nota4=SN,  PromedioActual=85.0f,  NotasRegistradas=1, Label=0.08f },
                new() { Nota1=70,  Nota2=72,  Nota3=68,  Nota4=SN,  PromedioActual=70.0f,  NotasRegistradas=3, Label=0.18f },
                new() { Nota1=80,  Nota2=78,  Nota3=82,  Nota4=SN,  PromedioActual=80.0f,  NotasRegistradas=3, Label=0.06f },
            };
        }
    }
}
