using Microsoft.EntityFrameworkCore;
using Proyecto_Evaluacion_Estudiantes.Models;

namespace Proyecto_Evaluacion_Estudiantes.Data
{

    //Entity Framework Core.
    //Gestiona las tablas: Administradores, Docentes, Cursos, Estudiantes.
    
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }


        public DbSet<Administrador> Administradores { get; set; }
        public DbSet<Docente>       Docentes        { get; set; }
        public DbSet<Curso>         Cursos          { get; set; }
        public DbSet<Estudiante>    Estudiantes     { get; set; }
        public DbSet<Grado>              Grados              { get; set; }
        public DbSet<Asignatura>         Asignaturas         { get; set; }
        public DbSet<AsignacionDocente>  AsignacionDocentes  { get; set; }
        public DbSet<NotaParcial>        NotasParciales      { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);


            modelBuilder.Entity<Administrador>(entity =>
            {
                entity.HasIndex(a => a.NombreUsuario).IsUnique();
                entity.Property(a => a.Activo).HasDefaultValue(true);
            });


            modelBuilder.Entity<Docente>(entity =>
            {
                entity.HasIndex(d => d.Usuario).IsUnique();

                entity.Property(d => d.Titulo)
                      .HasDefaultValue("Lic.");

                entity.Property(d => d.Activo)
                      .HasDefaultValue(true);

                entity.Property(d => d.FechaCreacion)
                      .HasDefaultValueSql("GETDATE()");
            });


            modelBuilder.Entity<Curso>(entity =>
            {
                entity.HasIndex(c => c.Codigo).IsUnique();
            });


            modelBuilder.Entity<Grado>(entity =>
            {
                entity.Property(g => g.Activo).HasDefaultValue(true);
                entity.Property(g => g.Nivel).HasDefaultValue("Primaria");
            });


            modelBuilder.Entity<Asignatura>(entity =>
            {
                entity.HasIndex(a => a.Codigo).IsUnique();
                entity.Property(a => a.Activo).HasDefaultValue(true);
                entity.Property(a => a.NivelAplicacion).HasDefaultValue("Todos");
            });


            modelBuilder.Entity<Curso>(entity =>
            {
                entity.HasOne(c => c.Grado)
                      .WithMany()
                      .HasForeignKey(c => c.GradoId)
                      .OnDelete(DeleteBehavior.Restrict);

                // DocenteTutorId es la FK principal: el docente "dueño" del curso
                entity.HasOne(c => c.DocenteTutor)
                      .WithMany(d => d.Cursos)
                      .HasForeignKey(c => c.DocenteTutorId)
                      .OnDelete(DeleteBehavior.Restrict);
            });


            modelBuilder.Entity<AsignacionDocente>(entity =>
            {

                entity.HasIndex(a => new { a.CursoId, a.AsignaturaId })
                      .IsUnique()
                      .HasDatabaseName("UQ_AsignacionDocente_Curso_Asignatura");

                entity.Property(a => a.Activo).HasDefaultValue(true);

                entity.HasOne(a => a.Curso)
                      .WithMany(c => c.AsignacionDocentes)
                      .HasForeignKey(a => a.CursoId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(a => a.Asignatura)
                      .WithMany()
                      .HasForeignKey(a => a.AsignaturaId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(a => a.Docente)
                      .WithMany()
                      .HasForeignKey(a => a.DocenteId)
                      .OnDelete(DeleteBehavior.Restrict);
            });


            modelBuilder.Entity<NotaParcial>(entity =>
            {
                // Unique: un estudiante no puede tener dos notas para la misma asignatura y el mismo parcial
                entity.HasIndex(n => new { n.EstudianteId, n.AsignaturaId, n.Parcial })
                      .IsUnique()
                      .HasDatabaseName("UQ_NotasParciales_Est_Asig_Parcial");

                entity.Property(n => n.FechaRegistro)
                      .HasDefaultValueSql("SYSUTCDATETIME()");

                entity.Property(n => n.Nota)
                      .HasColumnType("decimal(5,2)");

                
                entity.HasOne(n => n.Estudiante)
                      .WithMany(e => e.NotasParciales)
                      .HasForeignKey(n => n.EstudianteId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Restrict desde Asignatura: no se puede borrar una asignatura
                // que tenga notas registradas
                entity.HasOne(n => n.Asignatura)
                      .WithMany()
                      .HasForeignKey(n => n.AsignaturaId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Estudiante
            modelBuilder.Entity<Estudiante>(entity =>
            {
                // Índice: correo único por curso
                entity.HasIndex(e => new { e.Correo, e.CursoId })
                      .IsUnique()
                      .HasDatabaseName("UQ_Estudiante_Correo_Curso");

                entity.Property(e => e.Activo).HasDefaultValue(true);
                entity.Property(e => e.FechaRegistro).HasDefaultValueSql("GETDATE()");

                // Aqui Promedio calculado (4 parciales) 
                entity.Property(e => e.Promedio)
                      .HasComputedColumnSql(
                          "CASE " +
                          "WHEN Nota1 IS NOT NULL AND Nota2 IS NOT NULL AND Nota3 IS NOT NULL AND Nota4 IS NOT NULL " +
                              "THEN ROUND((Nota1+Nota2+Nota3+Nota4)/4.0, 2) " +
                          "WHEN Nota1 IS NOT NULL AND Nota2 IS NOT NULL AND Nota3 IS NOT NULL " +
                              "THEN ROUND((Nota1+Nota2+Nota3)/3.0, 2) " +
                          "WHEN Nota1 IS NOT NULL AND Nota2 IS NOT NULL " +
                              "THEN ROUND((Nota1+Nota2)/2.0, 2) " +
                          "WHEN Nota1 IS NOT NULL THEN Nota1 " +
                          "ELSE NULL END",
                          stored: true);


                const string prom =
                    "(CASE " +
                    "WHEN Nota1 IS NOT NULL AND Nota2 IS NOT NULL AND Nota3 IS NOT NULL AND Nota4 IS NOT NULL " +
                        "THEN (Nota1+Nota2+Nota3+Nota4)/4.0 " +
                    "WHEN Nota1 IS NOT NULL AND Nota2 IS NOT NULL AND Nota3 IS NOT NULL " +
                        "THEN (Nota1+Nota2+Nota3)/3.0 " +
                    "WHEN Nota1 IS NOT NULL AND Nota2 IS NOT NULL " +
                        "THEN (Nota1+Nota2)/2.0 " +
                    "WHEN Nota1 IS NOT NULL THEN Nota1 " +
                    "ELSE NULL END)";

                entity.Property(e => e.Estado)
                      .HasComputedColumnSql(
                          $"CASE WHEN {prom} >= 60 THEN N'Aprobado' " +
                          $"WHEN {prom} < 60 THEN N'Reprobado' " +
                          "ELSE N'Sin Notas' END",
                          stored: true);

                entity.HasOne(e => e.Curso)
                      .WithMany(c => c.Estudiantes)
                      .HasForeignKey(e => e.CursoId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
