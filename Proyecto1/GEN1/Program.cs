using Microsoft.AspNetCore.Mvc;
using GEN1.Entities;
using GEN1.Repository;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IBitacoraRepository, BitacoraRepository>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// POST /bitacora - Registrar bit�cora (sin validaci�n de token para evitar dependencia circular)
app.MapPost("/bitacora", async (
    [FromBody] BitacoraRequest request,
    IBitacoraRepository repository) =>
{
    if (string.IsNullOrWhiteSpace(request.Usuario))
        return Results.BadRequest(new { error = "Usuario es requerido y no puede ser vac�o" });

    if (string.IsNullOrWhiteSpace(request.Descripcion))
        return Results.BadRequest(new { error = "Descripci�n es requerida y no puede ser vac�a" });

    var bitacora = new Bitacora
    {
        FechaRegistro = DateTime.Now,
        Usuario = request.Usuario.Trim(),
        Descripcion = request.Descripcion.Trim(),
        TipoAccion = "INFO"
    };

    var id = await repository.CrearBitacoraAsync(bitacora);

    return Results.Created($"/bitacora/{id}", new { id, mensaje = "Bit�cora registrada exitosamente" });
})
.WithName("RegistrarBitacora")
.WithOpenApi();

app.Run();

record BitacoraRequest(string Usuario, string Descripcion);