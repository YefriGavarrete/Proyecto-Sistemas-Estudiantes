namespace Proyecto_Evaluacion_Estudiantes.Models
{

    /// ViewModel del Panel del Administrador (Administradores/Index).
    /// Hereda de LayoutViewModel para los datos del perfil del sidebar
    /// y agrega las estadísticas propias del panel de administración.
    public class AdminIndexViewModel : LayoutViewModel
    {
        public int TotalAdmins   { get; set; }
        public int TotalDocentes { get; set; }

        public string NombreAdmin
        {
            get => NombreUsuario;
            set => NombreUsuario = value;
        }
    }
}
