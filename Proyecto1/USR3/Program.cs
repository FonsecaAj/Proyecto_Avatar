using USR3.Entities;
using USR3.Repository;
using USR3.Services;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IParametroRepository, ParametroRepository>();
builder.Services.AddHttpClient<IAutenticacionService, AutenticacionService>();
builder.Services.AddHttpClient<IBitacoraService, BitacoraService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// POST /parametro - Crear parámetro
app.MapPost("/parametro", async (
    [FromHeader(Name = "Authorization")] string? authorization,
    [FromBody] ParametroDto dto,
    IParametroRepository repository,
    IAutenticacionService autenticacionService,
    IBitacoraService bitacoraService) =>
{
    if (!await autenticacionService.ValidarTokenAsync(authorization))
        return Results.Json(new { error = "No autorizado" }, statusCode: 401);

    var usuario = await autenticacionService.ObtenerUsuarioDelTokenAsync(authorization) ?? "sistema";

    var validacion = ValidarParametro(dto.IdParametro, dto.Valor);
    if (!validacion.esValido)
        return Results.BadRequest(new { error = validacion.mensaje });

    var parametroExistente = await repository.ObtenerPorIdAsync(dto.IdParametro.Trim().ToUpper());
    if (parametroExistente != null)
        return Results.BadRequest(new { error = "El parámetro ya existe" });

    var parametro = new Parametro
    {
        IdParametro = dto.IdParametro.Trim().ToUpper(),
        Valor = dto.Valor.Trim()
    };
    await repository.CrearAsync(parametro);

    var nuevoRegistro = new
    {
        parametro.IdParametro,
        parametro.Valor
    };

    await bitacoraService.RegistrarAsync(
        usuario,
        $"Parámetro creado - {JsonSerializer.Serialize(nuevoRegistro)}"
    );

    return Results.Created($"/parametro/{parametro.IdParametro}",
        new { idParametro = parametro.IdParametro, valor = parametro.Valor });
})
.WithName("CrearParametro")
.WithOpenApi();

// PUT /parametro/{id} - Modificar parámetro
app.MapPut("/parametro/{id}", async (
    string id,
    [FromHeader(Name = "Authorization")] string? authorization,
    [FromBody] ParametroDto dto,
    IParametroRepository repository,
    IAutenticacionService autenticacionService,
    IBitacoraService bitacoraService) =>
{
    if (!await autenticacionService.ValidarTokenAsync(authorization))
        return Results.Json(new { error = "No autorizado" }, statusCode: 401);

    var usuario = await autenticacionService.ObtenerUsuarioDelTokenAsync(authorization) ?? "sistema";

    var validacion = ValidarParametro(dto.IdParametro, dto.Valor);
    if (!validacion.esValido)
        return Results.BadRequest(new { error = validacion.mensaje });

    if (id.ToUpper() != dto.IdParametro.Trim().ToUpper())
        return Results.BadRequest(new { error = "El ID del parámetro no coincide" });

    var parametroExistente = await repository.ObtenerPorIdAsync(id.ToUpper());
    if (parametroExistente == null)
        return Results.NotFound(new { error = "Parámetro no encontrado" });

    var registroAnterior = new
    {
        parametroExistente.IdParametro,
        parametroExistente.Valor
    };

    var parametro = new Parametro
    {
        IdParametro = dto.IdParametro.Trim().ToUpper(),
        Valor = dto.Valor.Trim()
    };
    await repository.ActualizarAsync(parametro);

    var registroActual = new
    {
        parametro.IdParametro,
        parametro.Valor
    };

    await bitacoraService.RegistrarAsync(
        usuario,
        $"Parámetro actualizado - Anterior: {JsonSerializer.Serialize(registroAnterior)}, Actual: {JsonSerializer.Serialize(registroActual)}"
    );

    return Results.Ok(new { idParametro = parametro.IdParametro, valor = parametro.Valor });
})
.WithName("ActualizarParametro")
.WithOpenApi();

// DELETE /parametro/{id} - Eliminar parámetro
app.MapDelete("/parametro/{id}", async (
    string id,
    [FromHeader(Name = "Authorization")] string? authorization,
    IParametroRepository repository,
    IAutenticacionService autenticacionService,
    IBitacoraService bitacoraService) =>
{
    if (!await autenticacionService.ValidarTokenAsync(authorization))
        return Results.Json(new { error = "No autorizado" }, statusCode: 401);

    var usuario = await autenticacionService.ObtenerUsuarioDelTokenAsync(authorization) ?? "sistema";

    var parametro = await repository.ObtenerPorIdAsync(id.ToUpper());
    if (parametro == null)
        return Results.NotFound(new { error = "Parámetro no encontrado" });

    var registroEliminado = new
    {
        parametro.IdParametro,
        parametro.Valor
    };

    await repository.EliminarAsync(id.ToUpper());

    await bitacoraService.RegistrarAsync(
        usuario,
        $"Parámetro eliminado - {JsonSerializer.Serialize(registroEliminado)}"
    );

    return Results.NoContent();
})
.WithName("EliminarParametro")
.WithOpenApi();

// GET /parametro - Obtener todos los parámetros
app.MapGet("/parametro", async (
    [FromHeader(Name = "Authorization")] string? authorization,
    IParametroRepository repository,
    IAutenticacionService autenticacionService,
    IBitacoraService bitacoraService) =>
{
    if (!await autenticacionService.ValidarTokenAsync(authorization))
        return Results.Json(new { error = "No autorizado" }, statusCode: 401);

    var usuario = await autenticacionService.ObtenerUsuarioDelTokenAsync(authorization) ?? "sistema";

    var parametros = await repository.ObtenerTodosAsync();

    await bitacoraService.RegistrarAsync(
        usuario,
        "El usuario consulta parámetros"
    );

    return Results.Ok(parametros);
})
.WithName("ObtenerTodosParametros")
.WithOpenApi();

// GET /parametro/{id} - Obtener parámetro por ID
app.MapGet("/parametro/{id}", async (
    string id,
    [FromHeader(Name = "Authorization")] string? authorization,
    IParametroRepository repository,
    IAutenticacionService autenticacionService,
    IBitacoraService bitacoraService) =>
{
    if (!await autenticacionService.ValidarTokenAsync(authorization))
        return Results.Json(new { error = "No autorizado" }, statusCode: 401);

    var usuario = await autenticacionService.ObtenerUsuarioDelTokenAsync(authorization) ?? "sistema";

    var parametro = await repository.ObtenerPorIdAsync(id.ToUpper());

    if (parametro == null)
        return Results.NotFound(new { error = "Parámetro no encontrado" });

    await bitacoraService.RegistrarAsync(
        usuario,
        "El usuario consulta parámetro"
    );

    return Results.Ok(parametro);
})
.WithName("ObtenerParametroPorId")
.WithOpenApi();

app.Run();

static (bool esValido, string mensaje) ValidarParametro(string idParametro, string valor)
{
    if (string.IsNullOrWhiteSpace(idParametro))
        return (false, "El identificador del parámetro es requerido");

    if (string.IsNullOrWhiteSpace(valor))
        return (false, "El valor del parámetro es requerido");

    var idTrimmed = idParametro.Trim();

    if (idTrimmed.Length > 10)
        return (false, "El identificador del parámetro no puede exceder 10 caracteres");

    if (!Regex.IsMatch(idTrimmed, @"^[A-Z_]+$"))
        return (false, "El identificador del parámetro solo puede contener letras en mayúscula y guiones bajos");

    if (valor.Trim().Length > 500)
        return (false, "El valor del parámetro no puede exceder 500 caracteres");

    return (true, string.Empty);
}

record ParametroDto(string IdParametro, string Valor);