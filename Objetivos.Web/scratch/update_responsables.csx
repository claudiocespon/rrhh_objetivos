#r "nuget: CsvHelper, 30.0.1"
#r "bin/Debug/net10.0/Microsoft.EntityFrameworkCore.dll"
#r "bin/Debug/net10.0/Microsoft.EntityFrameworkCore.Sqlite.dll"
#r "bin/Debug/net10.0/Microsoft.EntityFrameworkCore.Relational.dll"
#r "bin/Debug/net10.0/Objetivos.Web.dll"

using System;
using System.IO;
using System.Linq;
using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.EntityFrameworkCore;
using Objetivos.Web.Data;
using Objetivos.Web.Domain.Entities;

var dbContextOptions = new DbContextOptionsBuilder<AppDbContext>()
    .UseSqlite("Data Source=objetivos.db")
    .Options;

using var db = new AppDbContext(dbContextOptions);

var config = new CsvConfiguration(CultureInfo.InvariantCulture)
{
    Delimiter = ";",
    HasHeaderRecord = true,
};

using var reader = new StreamReader(@"..\usuarios_responsables.csv");
using var csv = new CsvReader(reader, config);

var records = csv.GetRecords<dynamic>().ToList();

var jefes = db.Jefes.ToList();
var empleados = db.Empleados.ToList();

int updated = 0;
int notFound = 0;

// Normalize function: remove accents, extra spaces, etc.
string Normalize(string text)
{
    if (string.IsNullOrWhiteSpace(text)) return "";
    var normalizedString = text.Normalize(System.Text.NormalizationForm.FormD);
    var stringBuilder = new System.Text.StringBuilder();

    foreach (var c in normalizedString)
    {
        var unicodeCategory = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c);
        if (unicodeCategory != System.Globalization.UnicodeCategory.NonSpacingMark)
        {
            stringBuilder.Append(c);
        }
    }
    return stringBuilder.ToString().Normalize(System.Text.NormalizationForm.FormC).ToLowerInvariant().Replace(" ", " ").Trim();
}

foreach (var record in records)
{
    string email = record.Email;
    string responsableStr = record.Responsable;

    if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(responsableStr) || responsableStr == "-")
        continue;

    var emp = empleados.FirstOrDefault(e => e.Email.ToLower() == email.ToLower());
    if (emp == null) continue;

    // Split Responsable string "Apellido, Nombre"
    var parts = responsableStr.Split(',');
    string respApellido = parts[0].Trim();
    string respNombre = parts.Length > 1 ? parts[1].Trim() : "";

    // Try to find the Jefe
    var jefe = jefes.FirstOrDefault(j => Normalize(j.Apellido) == Normalize(respApellido) && Normalize(j.Nombre) == Normalize(respNombre));

    // If not found, try a less strict match (e.g. contains)
    if (jefe == null)
    {
        jefe = jefes.FirstOrDefault(j => Normalize(responsableStr).Contains(Normalize(j.Apellido)) && Normalize(responsableStr).Contains(Normalize(j.Nombre)));
    }

    if (jefe != null)
    {
        if (emp.JefeId != jefe.Id)
        {
            emp.JefeId = jefe.Id;
            updated++;
        }
    }
    else
    {
        Console.WriteLine($"[WARNING] Jefe no encontrado para responsable: '{responsableStr}' (Empleado: {email})");
        notFound++;
    }
}

db.SaveChanges();
Console.WriteLine($"\nProceso completado. Empleados actualizados: {updated}. Jefes no encontrados: {notFound}");
