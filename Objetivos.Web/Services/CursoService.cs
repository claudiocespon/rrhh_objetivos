using Microsoft.EntityFrameworkCore;
using Objetivos.Web.Data;
using Objetivos.Web.Domain.Entities;

namespace Objetivos.Web.Services;

public class CursoService
{
    private readonly AppDbContext _db;

    public CursoService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<Curso>> GetCursosAsync()
    {
        var cursos = await _db.Cursos.ToListAsync();
        
        // Si no hay cursos (primera vez), insertamos unos por defecto para no mostrar pantalla vacía
        if (!cursos.Any())
        {
            cursos = new List<Curso>
            {
                new Curso { Nombre = "Liderazgo Efectivo", Categoria = "Soft Skills", DuracionHoras = 20, EsObligatorio = true, UrlImagen = "https://images.unsplash.com/photo-1542744173-8e7e53415bb0?q=80&w=400", Descripcion = "Desarrolla habilidades para guiar equipos de alto rendimiento." },
                new Curso { Nombre = "Gestión del Tiempo", Categoria = "Productividad", DuracionHoras = 10, EsObligatorio = false, UrlImagen = "https://images.unsplash.com/photo-1506784983877-45594efa4cbe?q=80&w=400", Descripcion = "Optimiza tu jornada laboral con técnicas avanzadas." },
                new Curso { Nombre = "Excel Avanzado", Categoria = "Técnica", DuracionHoras = 30, EsObligatorio = true, UrlImagen = "https://images.unsplash.com/photo-1596495573105-d14658ce6091?q=80&w=400", Descripcion = "Domina tablas dinámicas y macros." }
            };
            _db.Cursos.AddRange(cursos);
            await _db.SaveChangesAsync();
        }
        
        return cursos;
    }
}
