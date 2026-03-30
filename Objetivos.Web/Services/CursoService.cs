using Microsoft.EntityFrameworkCore;
using Objetivos.Web.Data;
using Objetivos.Web.Domain.Entities;

namespace Objetivos.Web.Services;

public class CursoService(AppDbContext db)
{
    private readonly AppDbContext _db = db;

    public async Task<List<Curso>> GetCursosAsync()
    {
        return await _db.Cursos.ToListAsync();
    }
}
