namespace Proyecto_Evaluacion_Estudiantes.Models
{
    /// ViewModel que me carga/edición de notas
    /// El docente selecciona el parcial activo y ve la lista de sus estudiantes.
    public class EstudianteNotasViewModel : LayoutViewModel
    {
        //Parcial seleccionado en el combobox (1, 2, 3 o 4).
        public int ParcialSeleccionado { get; set; } = 1;

        public List<Estudiante> Estudiantes { get; set; } = new();


        public int NotasSubidas   => Estudiantes.Count(e => e.NotaParcial(ParcialSeleccionado) != null);
        public int NotasPendientes => Estudiantes.Count(e => e.NotaParcial(ParcialSeleccionado) == null);

        public string NombreParcial => $"Parcial {ParcialSeleccionado}";
    }
}
