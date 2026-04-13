using Proyecto_Evaluacion_Estudiantes.Helpers;

namespace Proyecto_Evaluacion_Estudiantes.Models
{
    /*
       Clase base de la que heredan todos los ViewModels de página.
       Provee los datos del perfil del sidebar (nombre, título, código, periodo, rol)
       para que el layout los lea desde el modelo en lugar de ViewData.*/
    public class LayoutViewModel
    {
        public string NombreUsuario { get; set; } = string.Empty;
        public string TituloUsuario { get; set; } = string.Empty;

        public string CodigoUsuario { get; set; } = string.Empty;

        public string Sistema { get; set; } = "EduPath AI";
        public string Periodo { get; set; } = PeriodoAcademico.ObtenerPeriodoDescriptivo();

        /// Indica si el usuario autenticado es administrador.
        public bool EsAdmin { get; set; } = false;

        public string ActiveMenu { get; set; } = string.Empty;
    }
}
