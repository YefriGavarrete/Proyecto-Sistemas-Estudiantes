namespace Proyecto_Evaluacion_Estudiantes.Models
{

    //Muestra solo los datos del usuario autenticado en modo solo lectura.

    public class PerfilViewModel : LayoutViewModel
    {

        public string NombreCompleto { get; set; } = string.Empty;
        public string Usuario        { get; set; } = string.Empty;
        public string Rol            { get; set; } = string.Empty;   
        public string Codigo         { get; set; } = string.Empty;   


        public string? Titulo        { get; set; }
        public string? Correo        { get; set; }
        public DateTime? FechaCreacion { get; set; }

        //Última vez que el usuario inició sesión.
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
