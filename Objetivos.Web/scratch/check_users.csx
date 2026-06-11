#r "bin/Debug/net10.0/Microsoft.EntityFrameworkCore.dll"
#r "bin/Debug/net10.0/Microsoft.EntityFrameworkCore.Sqlite.dll"
#r "bin/Debug/net10.0/Objetivos.Web.dll"

using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Objetivos.Web.Data;

var options = new DbContextOptionsBuilder<AppDbContext>().UseSqlite("Data Source=objetivos.db").Options;
using var db = new AppDbContext(options);

var jefes = db.Jefes.Select(j => new { j.Id, j.Email, j.Nombre, j.Apellido }).ToList();
var empleados = db.Empleados.Select(e => new { e.Id, e.Email, e.Nombre, e.Apellido, e.JefeId }).ToList();

Console.WriteLine("--- Jefes ---");
foreach (var j in jefes) Console.WriteLine($"{j.Id}: {j.Nombre} {j.Apellido} ({j.Email})");

Console.WriteLine("\n--- Empleados ---");
foreach (var e in empleados) Console.WriteLine($"{e.Id}: {e.Nombre} {e.Apellido} ({e.Email}) - JefeId: {e.JefeId}");
