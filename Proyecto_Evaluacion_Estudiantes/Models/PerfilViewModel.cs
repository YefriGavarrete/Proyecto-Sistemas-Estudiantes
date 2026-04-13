namespace Proyecto_Evaluacion_Estudiantes.Models
{
    /// <summary>
    /// ViewModel de la vista /Home/Perfil.
    /// Muestra los datos del usuario autenticado en modo solo lectura.
    /// </summary>
    public class PerfilViewModel : LayoutViewModel
    {
        // ── Datos comunes a ambos roles ───────────────────────────
        public string NombreCompleto { get; set; } = string.Empty;
        public string Usuario        { get; set; } = string.Empty;
        public string Rol            { get; set; } = string.Empty;   // "Docente" | "Administrador"
        public string Codigo         { get; set; } = string.Empty;   // "DOC-001" | "ADM-001"

        // ── Datos exclusivos de Docente (null para admins) ────────
        /// <summary>Título académico: "Lic.", "Ing.", "Dr.", etc. Solo docentes.</summary>
        public string? Titulo        { get; set; }

        /// <summary>Correo electrónico del docente.</summary>
        public string? Correo        { get; set; }

        /// <summary>Fecha en que fue creada la cuenta.</summary>
        public DateTime? FechaCreacion { get; set; }

        /// <summary>Última vez que el usuario inició sesión.</summary>
        public DateTime? UltimoAcceso  { get; set; }

        public string Iniciales
        {
            get
            {
                var partes = NombreCompleto.Split(' ',
                    StringSplitOptions.RemoveEmptyEntries);
                return partes.Length >= 2
                    ? $"{partes[0][0]}{partes[1][0]}".ToUpper()
                    : NombreCompleto.Length >= 2
                        ? NombreCompleto[..2].ToUpper()
                        : NombreCompleto.ToUpper();
            }
        }
        public string RolBadgeColor => Rol == "Administrador"
            ? "#7c3aed"   // morado
            : "#0096c7";  // me aplicar para el docente de EduPath
    }
}
