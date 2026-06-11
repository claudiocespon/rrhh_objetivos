using Microsoft.EntityFrameworkCore;
using Objetivos.Web.Domain.Entities;

namespace Objetivos.Web.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{

    public DbSet<Pais> Paises => Set<Pais>();
    public DbSet<Area> Areas => Set<Area>();
    public DbSet<Puesto> Puestos => Set<Puesto>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();
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
    public DbSet<CursoAsignacion> CursoAsignaciones => Set<CursoAsignacion>();
    public DbSet<EscalaValoracion> EscalasValoracion => Set<EscalaValoracion>();
    public DbSet<EstadoObjetivoConfig> EstadosObjetivoConfig => Set<EstadoObjetivoConfig>();
    public DbSet<EstadoEvaluacionConfig> EstadosEvaluacionConfig => Set<EstadoEvaluacionConfig>();
    public DbSet<ConfiguracionPlataforma> ConfiguracionesPlataforma => Set<ConfiguracionPlataforma>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Índice de performance: pilarId + UsuarioId + año.
        // La unicidad se controla vía ConfiguracionPlataforma "un_objetivo_por_pilar" (default: false).
        modelBuilder.Entity<Objetivo>()
            .HasIndex(o => new { o.PilarId, o.UsuarioId, o.Anio });

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
        modelBuilder.Entity<Usuario>()
            .HasOne(e => e.Area)
            .WithMany()
            .HasForeignKey(e => e.AreaId);

        modelBuilder.Entity<Usuario>()
            .HasOne(e => e.Jefe)
            .WithMany()
            .HasForeignKey(e => e.JefeId);

        modelBuilder.Entity<Usuario>()
            .HasOne(e => e.Puesto)
            .WithMany()
            .HasForeignKey(e => e.PuestoId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Usuario>()
            .HasOne(e => e.Pais)
            .WithMany()
            .HasForeignKey(e => e.PaisId);
            
        modelBuilder.Entity<Objetivo>()
            .HasMany(o => o.Revisiones)
            .WithOne(r => r.Objetivo)
            .HasForeignKey(r => r.ObjetivoId);

        modelBuilder.Entity<Objetivo>()
            .HasMany(o => o.Bitacora)
            .WithOne(b => b.Objetivo)
            .HasForeignKey(b => b.ObjetivoId);

        // CursoAsignacion: constraint único Usuario+curso (no duplicados)
        modelBuilder.Entity<CursoAsignacion>()
            .HasIndex(ca => new { ca.CursoId, ca.UsuarioId }).IsUnique();

        modelBuilder.Entity<CursoAsignacion>()
            .HasOne(ca => ca.Curso).WithMany(c => c.Asignaciones).HasForeignKey(ca => ca.CursoId);

        modelBuilder.Entity<CursoAsignacion>()
            .HasOne(ca => ca.Usuario).WithMany().HasForeignKey(ca => ca.UsuarioId);

        // Unique Email index for login
        modelBuilder.Entity<Usuario>()
            .HasIndex(u => u.Email).IsUnique();

        // ConfiguracionPlataforma: Clave es PK string
        modelBuilder.Entity<ConfiguracionPlataforma>()
            .HasKey(c => c.Clave);

        // EstadoObjetivoConfig: Slug unique
        modelBuilder.Entity<EstadoObjetivoConfig>()
            .HasIndex(e => e.Slug).IsUnique();

        // EstadoEvaluacionConfig: Slug unique
        modelBuilder.Entity<EstadoEvaluacionConfig>()
            .HasIndex(e => e.Slug).IsUnique();

        // Índices para Orden (ordenamiento)
        modelBuilder.Entity<EscalaValoracion>()
            .HasIndex(e => e.Orden);

        modelBuilder.Entity<Pilar>()
            .HasIndex(p => p.Orden);

        modelBuilder.Entity<SoftSkill>()
            .HasIndex(s => s.Orden);

        modelBuilder.Entity<EstadoObjetivoConfig>()
            .HasIndex(e => e.Orden);

        modelBuilder.Entity<EstadoEvaluacionConfig>()
            .HasIndex(e => e.Orden);

        // Relaciones para nuevos FK
        modelBuilder.Entity<Objetivo>()
            .HasOne(o => o.EstadoObjetivoConfig)
            .WithMany()
            .HasForeignKey(o => o.EstadoObjetivoConfigId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<RevisionCuatrimestral>()
            .HasOne(r => r.EscalaValoracion)
            .WithMany()
            .HasForeignKey(r => r.EscalaValoracionId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<RevisionCuatrimestral>()
            .HasOne(r => r.SoftSkill1EscalaValoracion)
            .WithMany()
            .HasForeignKey(r => r.SoftSkill1EscalaValoracionId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<RevisionCuatrimestral>()
            .HasOne(r => r.SoftSkill2EscalaValoracion)
            .WithMany()
            .HasForeignKey(r => r.SoftSkill2EscalaValoracionId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<RevisionCuatrimestral>()
            .HasOne(r => r.EstadoEvaluacionConfig)
            .WithMany()
            .HasForeignKey(r => r.EstadoEvaluacionConfigId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<EvaluacionFinal>()
            .HasOne(e => e.EscalaValoracionFinal)
            .WithMany()
            .HasForeignKey(e => e.EscalaValoracionIdFinal)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<EvaluacionFinal>()
            .HasOne(e => e.SoftSkill1EscalaValoracion)
            .WithMany()
            .HasForeignKey(e => e.SoftSkill1EscalaValoracionId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<EvaluacionFinal>()
            .HasOne(e => e.SoftSkill2EscalaValoracion)
            .WithMany()
            .HasForeignKey(e => e.SoftSkill2EscalaValoracionId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<EvaluacionFinal>()
            .HasOne(e => e.EstadoEvaluacionConfig)
            .WithMany()
            .HasForeignKey(e => e.EstadoEvaluacionConfigId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Autoevaluacion>()
            .HasOne(a => a.EscalaValoracionScore)
            .WithMany()
            .HasForeignKey(a => a.EscalaValoracionIdScore)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Autoevaluacion>()
            .HasOne(a => a.SoftSkill1EscalaValoracion)
            .WithMany()
            .HasForeignKey(a => a.SoftSkill1EscalaValoracionId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Autoevaluacion>()
            .HasOne(a => a.SoftSkill2EscalaValoracion)
            .WithMany()
            .HasForeignKey(a => a.SoftSkill2EscalaValoracionId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Autoevaluacion>()
            .HasOne(a => a.EstadoEvaluacionConfig)
            .WithMany()
            .HasForeignKey(a => a.EstadoEvaluacionConfigId)
            .OnDelete(DeleteBehavior.SetNull);
            
        modelBuilder.Entity<Pilar>()
            .HasOne(p => p.Area)
            .WithMany()
            .HasForeignKey(p => p.AreaId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var ahora = DateTime.UtcNow;
        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.State == EntityState.Modified)
            {
                var prop = entry.Entity.GetType().GetProperty("ActualizadoEn");
                if (prop != null && prop.CanWrite)
                    prop.SetValue(entry.Entity, ahora);
            }
        }
        return await base.SaveChangesAsync(cancellationToken);
    }
}
