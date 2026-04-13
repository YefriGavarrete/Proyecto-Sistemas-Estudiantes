using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;

namespace Proyecto_Evaluacion_Estudiantes.Models
{
    [Table("Estudiantes")]
    public class Estudiante
    {
            
        public int Id { get; set; }

        [StringLength(20)]
        [Display(Name = "Código")]
        public string Codigo { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(100, ErrorMessage = "Máximo 100 caracteres.")]
        [Display(Name = "Nombre")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El apellido es obligatorio.")]
        [StringLength(100, ErrorMessage = "Máximo 100 caracteres.")]
        [Display(Name = "Apellido")]
        public string Apellido { get; set; } = string.Empty;

        [Required(ErrorMessage = "La fecha de nacimiento es obligatoria.")]
        [DataType(DataType.Date)]
        [Display(Name = "Fecha de Nacimiento")]
        public DateTime FechaNacimiento { get; set; }

        [StringLength(20)]
        [Display(Name = "Identidad")]
        public string? Identidad { get; set; }

        [Required(ErrorMessage = "El correo es obligatorio.")]
        [EmailAddress(ErrorMessage = "Formato de correo inválido.")]
        [StringLength(150)]
        [Display(Name = "Correo Electrónico")]
        public string Correo { get; set; } = string.Empty;

        [Phone]
        [StringLength(20)]
        [Display(Name = "Teléfono")]
        public string? Telefono { get; set; }

        [StringLength(15)]
        [Display(Name = "Género")]
        public string? Genero { get; set; }

        [StringLength(20)]
        [Display(Name = "Sección")]
        public string? Seccion { get; set; }

        [StringLength(500)]
        [Display(Name = "Observaciones")]
        public string? Observaciones { get; set; }

        [Display(Name = "Activo")]
        public bool Activo { get; set; } = true;

        // Notas de Parcales estan separadas para permitir fechas individuales y manejo de valores nulos

        [Range(0, 100, ErrorMessage = "La nota debe estar entre 0 y 100.")]
        [Display(Name = "Parcial 1")]
        [Column(TypeName = "decimal(5,2)")]
        public decimal? Nota1 { get; set; }

        [Display(Name = "Fecha Parcial 1")]
        public DateTime? FechaNota1 { get; set; }

        [Range(0, 100, ErrorMessage = "La nota debe estar entre 0 y 100.")]
        [Display(Name = "Parcial 2")]
        [Column(TypeName = "decimal(5,2)")]
        public decimal? Nota2 { get; set; }

        [Display(Name = "Fecha Parcial 2")]
        public DateTime? FechaNota2 { get; set; }

        [Range(0, 100, ErrorMessage = "La nota debe estar entre 0 y 100.")]
        [Display(Name = "Parcial 3")]
        [Column(TypeName = "decimal(5,2)")]
        public decimal? Nota3 { get; set; }

        [Display(Name = "Fecha Parcial 3")]
        public DateTime? FechaNota3 { get; set; }

        [Range(0, 100, ErrorMessage = "La nota debe estar entre 0 y 100.")]
        [Display(Name = "Parcial 4")]
        [Column(TypeName = "decimal(5,2)")]
        public decimal? Nota4 { get; set; }

        [Display(Name = "Fecha Parcial 4")]
        public DateTime? FechaNota4 { get; set; }






        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        [Column(TypeName = "decimal(5,2)")]
        [Display(Name = "Promedio")]
        public decimal? Promedio { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        [StringLength(20)]
        [Display(Name = "Estado")]
        public string? Estado { get; set; }


        [Display(Name = "En Riesgo (IA)")]
        public bool? EnRiesgoIA { get; set; }



        [Required(ErrorMessage = "Debe seleccionar un curso.")]
        [Display(Name = "Curso")]
        public int CursoId { get; set; }

        [ForeignKey(nameof(CursoId))]
        public Curso? Curso { get; set; }


        public ICollection<NotaParcial> NotasParciales { get; set; }
            = new List<NotaParcial>();

        [Display(Name = "Fecha de Registro")]
        public DateTime FechaRegistro { get; set; } = DateTime.Now;





        [NotMapped]
        public string NombreCompleto => $"{Nombre} {Apellido}";


        [NotMapped]
        public int Edad
        {
            get
            {
                var hoy  = DateTime.Today;
                int edad = hoy.Year - FechaNacimiento.Year;
                if (FechaNacimiento.Date > hoy.AddYears(-edad)) edad--;
                return edad;
            }
        }

        [NotMapped]
        // Devuelve un determinado CSS para el badge según el estado del estudiante
        public string EstadoBadge => Estado switch
        {
            "Aprobado"  => "bg-lightgreen",
            "Reprobado" => "bg-lightred",
            _           => "bg-secondary"
        };

        [NotMapped]

        //Por eso utilizo NotMapped, para que no se intente mapear a la base de datos
        public decimal PromedioCalculado
        {
            get
            {
                var notas = new[] { Nota1, Nota2, Nota3, Nota4 }
                    .Where(n => n.HasValue)
                    .Select(n => n!.Value)
                    .ToList();
                return notas.Count == 0 ? 0 : Math.Round(notas.Average(), 2);
            }
        }

        [NotMapped]
        public string EstadoCalculado =>
            PromedioCalculado > 0
                ? (PromedioCalculado >= 60 ? "Aprobado" : "Reprobado")
                : "Sin Notas";

        [NotMapped]
        public bool EnRiesgoCalculado => PromedioCalculado > 0 && PromedioCalculado < 65;

        public decimal? NotaParcial(int parcial) => parcial switch
        {
            1 => Nota1,
            2 => Nota2,
            3 => Nota3,
            4 => Nota4,
            _ => null
        };

        public DateTime? FechaParcial(int parcial) => parcial switch
        {
            1 => FechaNota1,
            2 => FechaNota2,
            3 => FechaNota3,
            4 => FechaNota4,
            _ => null
        };
    }
}
