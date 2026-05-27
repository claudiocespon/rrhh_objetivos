using Microsoft.EntityFrameworkCore;
using Objetivos.Web.Data;
using Objetivos.Web.Domain.Entities;

namespace Objetivos.Web.Services
{
    public class AdminPuestoService
    {
        private readonly IDbContextFactory<AppDbContext> _dbFactory;

        public AdminPuestoService(IDbContextFactory<AppDbContext> dbFactory)
        {
            _dbFactory = dbFactory;
        }

        public async Task<List<Puesto>> GetAllPuestosAsync()
        {
            using var db = await _dbFactory.CreateDbContextAsync();
            return await db.Puestos.OrderBy(p => p.Nombre).ToListAsync();
        }

        public async Task<Puesto?> GetPuestoAsync(int id)
        {
            using var db = await _dbFactory.CreateDbContextAsync();
            return await db.Puestos.FindAsync(id);
        }

        public async Task<bool> SavePuestoAsync(Puesto puesto)
        {
            using var db = await _dbFactory.CreateDbContextAsync();
            
            if (puesto.Id == 0)
            {
                db.Puestos.Add(puesto);
            }
            else
            {
                var existing = await db.Puestos.FindAsync(puesto.Id);
                if (existing == null) return false;
                
                existing.Nombre = puesto.Nombre;
                existing.Descripcion = puesto.Descripcion;
                existing.Activo = puesto.Activo;
                db.Puestos.Update(existing);
            }

            return await db.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeletePuestoAsync(int id)
        {
            using var db = await _dbFactory.CreateDbContextAsync();
            var puesto = await db.Puestos.FindAsync(id);
            if (puesto == null) return false;
            
            // Check if it's used
            bool hasUsers = await db.Empleados.AnyAsync(e => e.PuestoId == id);
            if (hasUsers)
            {
                // Soft delete or just return false
                // Returning false so we can show an error
                return false;
            }

            db.Puestos.Remove(puesto);
            return await db.SaveChangesAsync() > 0;
        }
    }
}
