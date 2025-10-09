using ACD1.Entities;
using ACD1.Repository;
using ACD1.Services;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IInstitucionRepository, InstitucionRepository>();
builder.Services.AddHttpClient<IAutenticacionService, AutenticacionService>();
builder.Services.AddHttpClient<IBitacoraService, BitacoraService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// POST /institucion - Crear institución
app.MapPost("/institucion", async (
    [FromHeader(Name = "Authorization")] string? authorization,
    [FromBody] InstitucionDto dto,
    IInstitucionRepository repository,
    IAutenticacionService autenticacionService,
    IBitacoraService bitacoraService) =>
{
    if (!await autenticacionService.ValidarTokenAsync(authorization))
        return Results.Json(new { error = "No autorizado" }, statusCode: 401);

    var validacion = ValidarInstitucion(dto);
    if (!validacion.esValido)
        return Results.BadRequest(new { error = validacion.mensaje });

    var institucion = new Institucion
    {
        Nombre = dto.Nombre.Trim()
    };

    var id = await repository.CrearAsync(institucion);

    var nuevoRegistro = new
    {
        IdInstitucion = id,
        institucion.Nombre
    };

    // Obtener usuario del token
    var usuario = await autenticacionService.ObtenerUsuarioDelTokenAsync(authorization) ?? "sistema";

    await bitacoraService.RegistrarAsync(
        usuario,
        $"Institución creada - {JsonSerializer.Serialize(nuevoRegistro)}"
    );

    return Results.Created($"/institucion/{id}", new { idInstitucion = id, nombre = institucion.Nombre });
})
.WithName("CrearInstitucion")
.WithOpenApi();

// PUT /institucion/{id} - Modificar institución
app.MapPut("/institucion/{id}", async (
    int id,
    [FromHeader(Name = "Authorization")] string? authorization,
    [FromBody] InstitucionDto dto,
    IInstitucionRepository repository,
    IAutenticacionService autenticacionService,
    IBitacoraService bitacoraService) =>
{
    if (!await autenticacionService.ValidarTokenAsync(authorization))
        return Results.Json(new { error = "No autorizado" }, statusCode: 401);

    var validacion = ValidarInstitucion(dto);
    if (!validacion.esValido)
        return Results.BadRequest(new { error = validacion.mensaje });

    var institucionExistente = await repository.ObtenerPorIdAsync(id);
    if (institucionExistente == null)
        return Results.NotFound(new { error = "Institución no encontrada" });

    var registroAnterior = new
    {
        institucionExistente.IdInstitucion,
        institucionExistente.Nombre
    };

    var institucion = new Institucion
    {
        IdInstitucion = id,
        Nombre = dto.Nombre.Trim()
    };

    await repository.ActualizarAsync(institucion);

    var registroActual = new
    {
        institucion.IdInstitucion,
        institucion.Nombre
    };

    // Obtener usuario del token
    var usuario = await autenticacionService.ObtenerUsuarioDelTokenAsync(authorization) ?? "sistema";

    await bitacoraService.RegistrarAsync(
        usuario,
        $"Institución actualizada - Anterior: {JsonSerializer.Serialize(registroAnterior)}, Actual: {JsonSerializer.Serialize(registroActual)}"
    );

    return Results.Ok(new { idInstitucion = institucion.IdInstitucion, nombre = institucion.Nombre });
})
.WithName("ActualizarInstitucion")
.WithOpenApi();

// DELETE /institucion/{id} - Eliminar institución
app.MapDelete("/institucion/{id}", async (
    int id,
    [FromHeader(Name = "Authorization")] string? authorization,
    IInstitucionRepository repository,
    IAutenticacionService autenticacionService,
    IBitacoraService bitacoraService) =>
{
    if (!await autenticacionService.ValidarTokenAsync(authorization))
        return Results.Json(new { error = "No autorizado" }, statusCode: 401);

    var institucion = await repository.ObtenerPorIdAsync(id);
    if (institucion == null)
        return Results.NotFound(new { error = "Institución no encontrada" });

    var registroEliminado = new
    {
        institucion.IdInstitucion,
        institucion.Nombre
    };

    await repository.EliminarAsync(id);

    // Obtener usuario del token
    var usuario = await autenticacionService.ObtenerUsuarioDelTokenAsync(authorization) ?? "sistema";

    await bitacoraService.RegistrarAsync(
        usuario,
        $"Institución eliminada - {JsonSerializer.Serialize(registroEliminado)}"
    );

    return Results.NoContent();
})
.WithName("EliminarInstitucion")
.WithOpenApi();

// GET /institucion - Obtener todas las instituciones
app.MapGet("/institucion", async (
    [FromHeader(Name = "Authorization")] string? authorization,
    IInstitucionRepository repository,
    IAutenticacionService autenticacionService,
    IBitacoraService bitacoraService) =>
{
    if (!await autenticacionService.ValidarTokenAsync(authorization))
        return Results.Json(new { error = "No autorizado" }, statusCode: 401);

    var instituciones = await repository.ObtenerTodosAsync();

    // Obtener usuario del token
    var usuario = await autenticacionService.ObtenerUsuarioDelTokenAsync(authorization) ?? "sistema";

    await bitacoraService.RegistrarAsync(
        usuario,
        "El usuario consulta instituciones"
    );

    return Results.Ok(instituciones.Select(i => new
    {
        i.IdInstitucion,
        i.Nombre,
        i.FechaCreacion,
        i.FechaModificacion,
        i.Activo
    }));
})
.WithName("ObtenerTodasInstituciones")
.WithOpenApi();

// GET /institucion/{id} - Obtener institución por ID
app.MapGet("/institucion/{id}", async (
    int id,
    [FromHeader(Name = "Authorization")] string? authorization,
    IInstitucionRepository repository,
    IAutenticacionService autenticacionService,
    IBitacoraService bitacoraService) =>
{
    if (!await autenticacionService.ValidarTokenAsync(authorization))
        return Results.Json(new { error = "No autorizado" }, statusCode: 401);

    var institucion = await repository.ObtenerPorIdAsync(id);
    if (institucion == null)
        return Results.NotFound(new { error = "Institución no encontrada" });

    // Obtener usuario del token
    var usuario = await autenticacionService.ObtenerUsuarioDelTokenAsync(authorization) ?? "sistema";

    await bitacoraService.RegistrarAsync(
        usuario,
        "El usuario consulta institución"
    );

    return Results.Ok(new
    {
        institucion.IdInstitucion,
        institucion.Nombre,
        institucion.FechaCreacion,
        institucion.FechaModificacion,
        institucion.Activo
    });
})
.WithName("ObtenerInstitucionPorId")
.WithOpenApi();

app.Run();

static (bool esValido, string mensaje) ValidarInstitucion(InstitucionDto dto)
{
    if (string.IsNullOrWhiteSpace(dto.Nombre))
        return (false, "El nombre de la institución es requerido");

    if (!Regex.IsMatch(dto.Nombre.Trim(), @"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$"))
        return (false, "El nombre de la institución solo puede contener letras y espacios");

    return (true, string.Empty);
}

record InstitucionDto(string Nombre);