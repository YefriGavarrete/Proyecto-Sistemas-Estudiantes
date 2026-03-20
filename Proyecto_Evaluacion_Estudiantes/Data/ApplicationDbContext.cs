using Microsoft.EntityFrameworkCore;
using Proyecto_Evaluacion_Estudiantes.Models;

namespace Proyecto_Evaluacion_Estudiantes.Data
{
    /// <summary>
    /// Contexto principal de Entity Framework Core.
    /// Gestiona las tablas: Administradores, Docentes, Cursos, Estudiantes.
    /// </summary>
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // ── DbSets (tablas) ─────────────────────────────────────
        public DbSet<Administrador> Administradores { get; set; }
        public DbSet<Docente>       Docentes        { get; set; }
        public DbSet<Curso>         Cursos          { get; set; }
        public DbSet<Estudiante>    Estudiantes     { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ── Administrador ────────────────────────────────────
            modelBuilder.Entity<Administrador>(entity =>
            {
                entity.HasIndex(a => a.NombreUsuario).IsUnique();
                entity.Property(a => a.Activo).HasDefaultValue(true);
            });

            // ── Docente ─────────────────────────────────────────
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

            // ── Curso ────────────────────────────────────────────
            modelBuilder.Entity<Curso>(entity =>
            {
                entity.HasIndex(c => c.Codigo).IsUnique();

                entity.HasOne(c => c.Docente)
                      .WithMany(d => d.Cursos)
                      .HasForeignKey(c => c.DocenteId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ── Estudiante ───────────────────────────────────────
            modelBuilder.Entity<Estudiante>(entity =>
            {
                // Índice compuesto: un correo no puede repetirse en el mismo curso
                entity.HasIndex(e => new { e.Correo, e.CursoId })
                      .IsUnique()
                      .HasDatabaseName("UQ_Estudiante_Correo_Curso");

                // Columnas calculadas en SQL Server (no las maneja EF)
                entity.Property(e => e.Promedio)
                      .HasComputedColumnSql(
                          "CASE " +
                          "WHEN Nota1 IS NOT NULL AND Nota2 IS NOT NULL AND Nota3 IS NOT NULL " +
                              "THEN ROUND((Nota1 + Nota2 + Nota3) / 3.0, 2) " +
                          "WHEN Nota1 IS NOT NULL AND Nota2 IS NOT NULL " +
                              "THEN ROUND((Nota1 + Nota2) / 2.0, 2) " +
                          "WHEN Nota1 IS NOT NULL THEN Nota1 " +
                          "ELSE NULL END",
                          stored: true);

                entity.Property(e => e.Estado)
                      .HasComputedColumnSql(
                          "CASE " +
                          "WHEN (CASE WHEN Nota1 IS NOT NULL AND Nota2 IS NOT NULL AND Nota3 IS NOT NULL " +
                                     "THEN (Nota1+Nota2+Nota3)/3.0 " +
                                     "WHEN Nota1 IS NOT NULL AND Nota2 IS NOT NULL " +
                                     "THEN (Nota1+Nota2)/2.0 " +
                                     "WHEN Nota1 IS NOT NULL THEN Nota1 " +
                                     "ELSE NULL END) >= 60 THEN N'Aprobado' " +
                          "WHEN (CASE WHEN Nota1 IS NOT NULL AND Nota2 IS NOT NULL AND Nota3 IS NOT NULL " +
                                     "THEN (Nota1+Nota2+Nota3)/3.0 " +
                                     "WHEN Nota1 IS NOT NULL AND Nota2 IS NOT NULL " +
                                     "THEN (Nota1+Nota2)/2.0 " +
                                     "WHEN Nota1 IS NOT NULL THEN Nota1 " +
                                     "ELSE NULL END) < 60 THEN N'Reprobado' " +
                          "ELSE N'Sin Notas' END",
                          stored: true);

                entity.Property(e => e.FechaRegistro)
                      .HasDefaultValueSql("GETDATE()");

                entity.HasOne(e => e.Curso)
                      .WithMany(c => c.Estudiantes)
                      .HasForeignKey(e => e.CursoId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
