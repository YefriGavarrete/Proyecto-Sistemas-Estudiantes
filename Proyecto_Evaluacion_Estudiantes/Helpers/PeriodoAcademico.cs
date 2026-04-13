namespace Proyecto_Evaluacion_Estudiantes.Helpers
{
    /// <summary>
    /// Calcula el período académico vigente según el calendario hondureño.
    ///
    /// El año académico va de febrero a noviembre y se divide en 4 parciales:
    ///   Parcial 1 : 01/Feb  –  15/Abr   (~10.5 semanas)
    ///   Parcial 2 : 16/Abr  –  30/Jun   (~10.8 semanas)
    ///   Parcial 3 : 01/Jul  –  15/Sep   (~11   semanas)
    ///   Parcial 4 : 16/Sep  –  30/Nov   (~10.8 semanas)

    public static class PeriodoAcademico
    {
        /// <summary>
        /// Devuelve el período académico actual en formato "YYYY-N"
        /// (p. ej. "2026-1", "2026-3") basándose en <paramref name="fecha"/>
        /// o en <see cref="DateTime.Now"/> si no se indica fecha.
        /// </summary>
        public static string ObtenerPeriodoActual(DateTime? fecha = null)
        {
            var hoy = fecha ?? DateTime.Now;
            int anio = hoy.Year;
            int mes  = hoy.Month;
            int dia  = hoy.Day;

            // Enero pertenece al inicio del año que está por comenzar.
            if (mes == 1)
                return $"{anio}-1";

            // Diciembre cierra el año académico que acaba de terminar.
            if (mes == 12)
                return $"{anio}-4";

            // ── Parcial 1 : 01/Feb – 15/Abr ─────────────────────────────
            if (mes < 4 || (mes == 4 && dia <= 15))
                return $"{anio}-1";

            // ── Parcial 2 : 16/Abr – 30/Jun ─────────────────────────────
            if (mes < 7)
                return $"{anio}-2";

            // ── Parcial 3 : 01/Jul – 15/Sep ─────────────────────────────
            if (mes < 9 || (mes == 9 && dia <= 15))
                return $"{anio}-3";

            // ── Parcial 4 : 16/Sep – 30/Nov ─────────────────────────────
            return $"{anio}-4";
        }

        public static string ObtenerPeriodoDescriptivo(DateTime? fecha = null)
        {
            string codigo = ObtenerPeriodoActual(fecha);
            // codigo tiene formato "YYYY-N"
            var partes = codigo.Split('-');
            if (partes.Length == 2 && int.TryParse(partes[1], out int num))
            {
                string romano = num switch { 1 => "I", 2 => "II", 3 => "III", 4 => "IV", _ => num.ToString() };
                return $"{partes[0]} — {romano} Parcial";
            }
            return codigo;
        }

    }
}
