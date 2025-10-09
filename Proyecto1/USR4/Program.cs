using USR4.Entities;
using USR4.Repository;
using USR4.Services;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IModuloRepository, ModuloRepository>();
builder.Services.AddHttpClient<IAutenticacionService, AutenticacionService>();
builder.Services.AddHttpClient<IBitacoraService, BitacoraService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// POST /modulo - Crear mÛdulo
app.MapPost("/modulo", async (
    [FromHeader(Name = "Authorization")] string? authorization,
    [FromBody] ModuloDto dto,
    IModuloRepository repository,
    IAutenticacionService autenticacionService,
    IBitacoraService bitacoraService) =>
{
    if (!await autenticacionService.ValidarTokenAsync(authorization))
        return Results.Json(new { error = "No autorizado" }, statusCode: 401);

    var usuario = await autenticacionService.ObtenerUsuarioDelTokenAsync(authorization) ?? "sistema";

    var validacion = ValidarModulo(dto.Nombre);
    if (!validacion.esValido)
        return Results.BadRequest(new { error = validacion.mensaje });

    var moduloExistente = await repository.ObtenerPorNombreAsync(dto.Nombre.Trim());
    if (moduloExistente != null)
        return Results.BadRequest(new { error = "Ya existe un mÛdulo con ese nombre" });

    var modulo = new Modulo { Nombre = dto.Nombre.Trim() };
    var id = await repository.CrearAsync(modulo);

    var nuevoRegistro = new
    {
        IdModulo = id,
        modulo.Nombre
    };

    await bitacoraService.RegistrarAsync(
        usuario,
        $"MÛdulo creado - {JsonSerializer.Serialize(nuevoRegistro)}"
    );

    return Results.Created($"/modulo/{id}", new { id, nombre = modulo.Nombre });
})
.WithName("CrearModulo")
.WithOpenApi();

// PUT /modulo/{id} - Modificar mÛdulo
app.MapPut("/modulo/{id}", async (
    int id,
    [FromHeader(Name = "Authorization")] string? authorization,
    [FromBody] ModuloDto dto,
    IModuloRepository repository,
    IAutenticacionService autenticacionService,
    IBitacoraService bitacoraService) =>
{
    if (!await autenticacionService.ValidarTokenAsync(authorization))
        return Results.Json(new { error = "No autorizado" }, statusCode: 401);

    var usuario = await autenticacionService.ObtenerUsuarioDelTokenAsync(authorization) ?? "sistema";

    var validacion = ValidarModulo(dto.Nombre);
    if (!validacion.esValido)
        return Results.BadRequest(new { error = validacion.mensaje });

    var moduloExistente = await repository.ObtenerPorIdAsync(id);
    if (moduloExistente == null)
        return Results.NotFound(new { error = "MÛdulo no encontrado" });

    var moduloDuplicado = await repository.ObtenerPorNombreAsync(dto.Nombre.Trim());
    if (moduloDuplicado != null && moduloDuplicado.IdModulo != id)
        return Results.BadRequest(new { error = "Ya existe un mÛdulo con ese nombre" });

    var registroAnterior = new
    {
        moduloExistente.IdModulo,
        moduloExistente.Nombre
    };

    var modulo = new Modulo { IdModulo = id, Nombre = dto.Nombre.Trim() };
    await repository.ActualizarAsync(modulo);

    var registroActual = new
    {
        modulo.IdModulo,
        modulo.Nombre
    };

    await bitacoraService.RegistrarAsync(
        usuario,
        $"MÛdulo actualizado - Anterior: {JsonSerializer.Serialize(registroAnterior)}, Actual: {JsonSerializer.Serialize(registroActual)}"
    );

    return Results.Ok(new { id = modulo.IdModulo, nombre = modulo.Nombre });
})
.WithName("ActualizarModulo")
.WithOpenApi();

// DELETE /modulo/{id} - Eliminar mÛdulo
app.MapDelete("/modulo/{id}", async (
    int id,
    [FromHeader(Name = "Authorization")] string? authorization,
    IModuloRepository repository,
    IAutenticacionService autenticacionService,
    IBitacoraService bitacoraService) =>
{
    if (!await autenticacionService.ValidarTokenAsync(authorization))
        return Results.Json(new { error = "No autorizado" }, statusCode: 401);

    var usuario = await autenticacionService.ObtenerUsuarioDelTokenAsync(authorization) ?? "sistema";

    var modulo = await repository.ObtenerPorIdAsync(id);
    if (modulo == null)
        return Results.NotFound(new { error = "MÛdulo no encontrado" });

    var registroEliminado = new
    {
        modulo.IdModulo,
        modulo.Nombre
    };

    await repository.EliminarAsync(id);

    await bitacoraService.RegistrarAsync(
        usuario,
        $"MÛdulo eliminado - {JsonSerializer.Serialize(registroEliminado)}"
    );

    return Results.NoContent();
})
.WithName("EliminarModulo")
.WithOpenApi();

// GET /modulo - Obtener todos los mÛdulos
app.MapGet("/modulo", async (
    [FromHeader(Name = "Authorization")] string? authorization,
    IModuloRepository repository,
    IAutenticacionService autenticacionService,
    IBitacoraService bitacoraService) =>
{
    if (!await autenticacionService.ValidarTokenAsync(authorization))
        return Results.Json(new { error = "No autorizado" }, statusCode: 401);

    var usuario = await autenticacionService.ObtenerUsuarioDelTokenAsync(authorization) ?? "sistema";

    var modulos = await repository.ObtenerTodosAsync();

    await bitacoraService.RegistrarAsync(
        usuario,
        "El usuario consulta mÛdulos"
    );

    return Results.Ok(modulos);
})
.WithName("ObtenerTodosModulos")
.WithOpenApi();

// GET /modulo/{id} - Obtener mÛdulo por ID
app.MapGet("/modulo/{id}", async (
    int id,
    [FromHeader(Name = "Authorization")] string? authorization,
    IModuloRepository repository,
    IAutenticacionService autenticacionService,
    IBitacoraService bitacoraService) =>
{
    if (!await autenticacionService.ValidarTokenAsync(authorization))
        return Results.Json(new { error = "No autorizado" }, statusCode: 401);

    var usuario = await autenticacionService.ObtenerUsuarioDelTokenAsync(authorization) ?? "sistema";

    var modulo = await repository.ObtenerPorIdAsync(id);

    if (modulo == null)
        return Results.NotFound(new { error = "MÛdulo no encontrado" });

    await bitacoraService.RegistrarAsync(
        usuario,
        "El usuario consulta mÛdulo"
    );

    return Results.Ok(modulo);
})
.WithName("ObtenerModuloPorId")
.WithOpenApi();

app.Run();

static (bool esValido, string mensaje) ValidarModulo(string nombre)
{
    if (string.IsNullOrWhiteSpace(nombre))
        return (false, "El nombre del mÛdulo es requerido");

    if (!Regex.IsMatch(nombre.Trim(), @"^[a-zA-Z·ÈÌÛ˙¡…Õ”⁄Ò—\s]+$"))
        return (false, "El nombre del mÛdulo solo puede contener letras y espacios");

    return (true, string.Empty);
}

record ModuloDto(string Nombre);