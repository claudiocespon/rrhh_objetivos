using Microsoft.EntityFrameworkCore;
using Objetivos.Web.Domain.Entities;

namespace Objetivos.Web.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Pais> Paises => Set<Pais>();
    public DbSet<Area> Areas => Set<Area>();
    public DbSet<Jefe> Jefes => Set<Jefe>();
    public DbSet<Empleado> Empleados => Set<Empleado>();
    public DbSet<Pilar> Pilares => Set<Pilar>();
    public DbSet<SoftSkill> SoftSkills => Set<SoftSkill>();
    public DbSet<Objetivo> Objetivos => Set<Objetivo>();
    public DbSet<RevisionCuatrimestral> RevisionesCuatrimestrales => Set<RevisionCuatrimestral>();
    public DbSet<EvaluacionFinal> EvaluacionesFinales => Set<EvaluacionFinal>();
    public DbSet<Autoevaluacion> Autoevaluaciones => Set<Autoevaluacion>();
    public DbSet<BitacoraEntrada> BitacoraEntradas => Set<BitacoraEntrada>();
    public DbSet<MensajeChat> MensajesChat => Set<MensajeChat>();
    public DbSet<EventoCalendario> EventosCalendario => Set<EventoCalendario>();
    public DbSet<AuditoriaLog> AuditoriaLogs => Set<AuditoriaLog>();
    public DbSet<Notificacion> Notificaciones => Set<Notificacion>();
    public DbSet<Curso> Cursos => Set<Curso>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Constraint único obligatorio: pilarId + empleadoId + año
        modelBuilder.Entity<Objetivo>()
            .HasIndex(o => new { o.PilarId, o.EmpleadoId, o.Anio })
            .IsUnique();

        // Soft skills como FKs directas
        modelBuilder.Entity<Objetivo>()
            .HasOne(o => o.SoftSkill1).WithMany().HasForeignKey(o => o.SoftSkill1Id)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Objetivo>()
            .HasOne(o => o.SoftSkill2).WithMany().HasForeignKey(o => o.SoftSkill2Id)
            .OnDelete(DeleteBehavior.Restrict);

        // EvaluacionFinal: 1 por objetivo
        modelBuilder.Entity<EvaluacionFinal>()
            .HasIndex(e => e.ObjetivoId).IsUnique();

        // Autoevaluacion: 1 por objetivo
        modelBuilder.Entity<Autoevaluacion>()
            .HasIndex(a => a.ObjetivoId).IsUnique();
            
        // Relationships
        modelBuilder.Entity<Empleado>()
            .HasOne(e => e.Area)
            .WithMany()
            .HasForeignKey(e => e.AreaId);

        modelBuilder.Entity<Empleado>()
            .HasOne(e => e.Jefe)
            .WithMany()
            .HasForeignKey(e => e.JefeId);

        modelBuilder.Entity<Empleado>()
            .HasOne(e => e.Pais)
            .WithMany()
            .HasForeignKey(e => e.PaisId);

        modelBuilder.Entity<Jefe>()
            .HasOne(j => j.Area)
            .WithMany()
            .HasForeignKey(j => j.AreaId);

        modelBuilder.Entity<Jefe>()
            .HasOne(j => j.Pais)
            .WithMany()
            .HasForeignKey(j => j.PaisId);
            
        modelBuilder.Entity<Objetivo>()
            .HasMany(o => o.Revisiones)
            .WithOne(r => r.Objetivo)
            .HasForeignKey(r => r.ObjetivoId);

        modelBuilder.Entity<Objetivo>()
            .HasMany(o => o.Bitacora)
            .WithOne(b => b.Objetivo)
            .HasForeignKey(b => b.ObjetivoId);

        // Unique Email index for login
        modelBuilder.Entity<Jefe>()
            .HasIndex(j => j.Email).IsUnique();

        modelBuilder.Entity<Empleado>()
            .HasIndex(e => e.Email).IsUnique();
    }
}
