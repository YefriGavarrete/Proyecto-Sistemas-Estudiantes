namespace Proyecto_Evaluacion_Estudiantes.Models
{
    /*
       Clase base de la que heredan todos los ViewModels de página.
       Provee los datos del perfil del sidebar (nombre, título, código, periodo, rol)
       para que el layout los lea desde el modelo en lugar de ViewData.*/
    public class LayoutViewModel
    {
        public string NombreUsuario { get; set; } = string.Empty;

        /// <summary>Título o rol visible en el sidebar ("Lic.", "Ing.", "Administrador", etc.).</summary>
        public string TituloUsuario { get; set; } = string.Empty;

        /// <summary>Código de identificación del usuario (ej. DOC-001, ADM-001).</summary>
        public string CodigoUsuario { get; set; } = string.Empty;

        /// <summary>Nombre del sistema mostrado en el sidebar.</summary>
        public string Sistema { get; set; } = "EduPath AI";

        /// <summary>Periodo académico activo.</summary>
        public string Periodo { get; set; } = "2026-1";

        /// <summary>Indica si el usuario autenticado es administrador.</summary>
        public bool EsAdmin { get; set; } = false;

        /// <summary>Identificador del ítem activo del menú lateral.</summary>
        public string ActiveMenu { get; set; } = string.Empty;
    }
}
