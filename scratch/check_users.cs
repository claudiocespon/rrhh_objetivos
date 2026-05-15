using Microsoft.EntityFrameworkCore;
using Objetivos.Web.Data;
using Objetivos.Web.Domain.Entities;

var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
optionsBuilder.UseSqlite("Data Source=Objetivos.Web/objetivos.db");

using var db = new AppDbContext(optionsBuilder.Options);

var jefes = await db.Jefes.ToListAsync();
var empleados = await db.Empleados.ToListAsync();

Console.WriteLine($"Jefes: {jefes.Count}");
foreach (var j in jefes.Take(5))
{
    Console.WriteLine($"Jefe: {j.Email}, Legajo: {j.Legajo}");
}

Console.WriteLine($"Empleados: {empleados.Count}");
foreach (var e in empleados.Take(5))
{
    Console.WriteLine($"Empleado: {e.Email}, Legajo: {e.Legajo}");
}

var user = await db.Empleados.FirstOrDefaultAsync(e => e.Email == "ccespon@permaquim.com") 
           ?? (object)await db.Jefes.FirstOrDefaultAsync(j => j.Email == "ccespon@permaquim.com");

if (user != null)
{
    if (user is Empleado e) Console.WriteLine($"Found ccespon: {e.Email}, Legajo: {e.Legajo}");
    else if (user is Jefe j) Console.WriteLine($"Found ccespon: {j.Email}, Legajo: {j.Legajo}");
}
else
{
    Console.WriteLine("ccespon@permaquim.com NOT FOUND");
}
