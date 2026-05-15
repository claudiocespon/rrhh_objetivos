using Microsoft.EntityFrameworkCore;
using Objetivos.Web.Data;
using Objetivos.Web.Domain.Entities;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();
services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=c:\\Development\\Antigravity\\RRHH_Objetivos\\Objetivos.Web\\objetivos.db"));

var serviceProvider = services.BuildServiceProvider();
using var scope = serviceProvider.CreateScope();
var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

var user = await db.Empleados.FirstOrDefaultAsync(e => e.Email == "ccespon@permaquim.com") 
           ?? (object)await db.Jefes.FirstOrDefaultAsync(j => j.Email == "ccespon@permaquim.com");

if (user != null)
{
    if (user is Empleado e) Console.WriteLine($"USER_DATA: {e.Email}|{e.Legajo}");
    else if (user is Jefe j) Console.WriteLine($"USER_DATA: {j.Email}|{j.Legajo}");
}
else
{
    Console.WriteLine("USER_DATA: NOT_FOUND");
}
