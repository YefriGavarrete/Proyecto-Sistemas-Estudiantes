using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Proyecto_Evaluacion_Estudiantes.Models
{
    /// <summary>
    /// ViewModel de la vista /Home/Perfil.
    /// Muestra los atributos del usuario autenticado (Docente o Administrador)
    /// en modo solo lectura, más el formulario de subida de foto.
    /// </summary>
    public class PerfilViewModel : LayoutViewModel
    {
        // ── Datos comunes a ambos roles ───────────────────────────
        public string NombreCompleto { get; set; } = string.Empty;
        public string Usuario        { get; set; } = string.Empty;
        public string Rol            { get; set; } = string.Empty;   // "Docente" | "Administrador"
        public string Codigo         { get; set; } = string.Empty;   // "DOC-001" | "ADM-001"
        public bool   TieneFoto      { get; set; }


        public string? FotoUrl       { get; set; }

        // ── Datos exclusivos de Docente (null para admins) ────────
        /// <summary>Título académico: "Lic.", "Ing.", "Dr.", etc. Solo docentes.</summary>
        public string? Titulo        { get; set; }

        /// <summary>Correo electrónico del docente. Los admins no lo tienen actualmente.</summary>
        public string? Correo        { get; set; }
            
        /// <summary>Fecha en que fue creada la cuenta.</summary>
        public DateTime? FechaCreacion { get; set; }

        /// <summary>Última vez que el usuario inició sesión.</summary>
        public DateTime? UltimoAcceso  { get; set; }

        /// <summary>
        /// Archivo de imagen seleccionado por el usuario.
        /// Solo se usa en el POST de subida de foto; se ignora en el GET.
        /// Extensiones aceptadas: .jpg, .jpeg, .png, .webp
        /// Tamaño máximo: 2 MB (validado en el controlador).
        /// </summary>
        [Display(Name = "Foto de perfil")]
        public IFormFile? FotoFile { get; set; }

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
