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

    // Obtener el registro completo con las fechas generadas
    var parametroCreado = await repository.ObtenerPorIdAsync(parametro.IdParametro);

    var nuevoRegistro = new
    {
        parametroCreado.IdParametro,
        parametroCreado.Valor,
        parametroCreado.FechaCreacion,
        parametroCreado.FechaModificacion
    };

    await bitacoraService.RegistrarAsync(
        usuario,
        $"Parámetro creado - {JsonSerializer.Serialize(nuevoRegistro)}"
    );

    return Results.Created($"/parametro/{parametroCreado.IdParametro}", parametroCreado);
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
        parametroExistente.Valor,
        parametroExistente.FechaCreacion,
        parametroExistente.FechaModificacion
    };

    var parametro = new Parametro
    {
        IdParametro = dto.IdParametro.Trim().ToUpper(),
        Valor = dto.Valor.Trim()
    };
    await repository.ActualizarAsync(parametro);

    // Obtener el registro completo actualizado
    var parametroActualizado = await repository.ObtenerPorIdAsync(parametro.IdParametro);

    var registroActual = new
    {
        parametroActualizado.IdParametro,
        parametroActualizado.Valor,
        parametroActualizado.FechaCreacion,
        parametroActualizado.FechaModificacion
    };

    await bitacoraService.RegistrarAsync(
        usuario,
        $"Parámetro actualizado - Anterior: {JsonSerializer.Serialize(registroAnterior)}, Actual: {JsonSerializer.Serialize(registroActual)}"
    );

    return Results.Ok(parametroActualizado);
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
        parametro.Valor,
        parametro.FechaCreacion,
        parametro.FechaModificacion
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


app.MapGet("/parametro/public/{id}", async (
    string id,
    IParametroRepository repository) =>
{
    // Lista de parámetros que pueden consultarse públicamente (solo configuración del sistema)
    var parametrosPermitidos = new[] { "JWTEXPMIN", "REFEXPMIN" };

    var idUpper = id.ToUpper();
    if (!parametrosPermitidos.Contains(idUpper))
        return Results.NotFound(new { error = "Parámetro no disponible públicamente" });

    var parametro = await repository.ObtenerPorIdAsync(idUpper);

    if (parametro == null)
        return Results.NotFound(new { error = "Parámetro no encontrado" });

    return Results.Ok(parametro);
})
.WithName("ObtenerParametroPublico")
.WithOpenApi()
.WithTags("Público");

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