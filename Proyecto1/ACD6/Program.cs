using ACD6.Entities;
using ACD6.Repository;
using ACD6.Services;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IProfesorRepository, ProfesorRepository>();
builder.Services.AddHttpClient<IAutenticacionService, AutenticacionService>();
builder.Services.AddHttpClient<IBitacoraService, BitacoraService>();
builder.Services.AddHttpClient<IParametroService, ParametroService>();
builder.Services.AddHttpClient<ITipoIdentificacionService, TipoIdentificacionService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// POST /profesor - Crear profesor
app.MapPost("/profesor", async (
    [FromHeader(Name = "Authorization")] string? authorization,
    [FromBody] ProfesorDto dto,
    IProfesorRepository repository,
    IAutenticacionService autenticacionService,
    IBitacoraService bitacoraService,
    IParametroService parametroService,
    ITipoIdentificacionService tipoIdentificacionService) =>
{
    if (!await autenticacionService.ValidarTokenAsync(authorization))
        return Results.Json(new { error = "No autorizado" }, statusCode: 401);

    var validacion = await ValidarProfesorAsync(dto, parametroService, tipoIdentificacionService, authorization);
    if (!validacion.esValido)
        return Results.BadRequest(new { error = validacion.mensaje });

    var profesorExistente = await repository.ObtenerPorEmailAsync(dto.Email);
    if (profesorExistente != null)
        return Results.BadRequest(new { error = "El email ya est· registrado" });

    var profesor = new Profesor
    {
        NumeroIdentificacion = dto.NumeroIdentificacion.Trim(),
        IdTipoIdentificacion = dto.IdTipoIdentificacion,
        Email = dto.Email.Trim().ToLower(),
        NombreCompleto = dto.NombreCompleto.Trim(),
        FechaNacimiento = dto.FechaNacimiento,
        Telefonos = dto.Telefonos.Trim()
    };

    var idCreado = await repository.CrearAsync(profesor);

    // Obtener el registro completo con las fechas generadas
    var profesorCreado = await repository.ObtenerPorIdAsync(idCreado);

    var nuevoRegistro = new
    {
        profesorCreado.IdProfesor,
        profesorCreado.NumeroIdentificacion,
        profesorCreado.IdTipoIdentificacion,
        profesorCreado.Email,
        profesorCreado.NombreCompleto,
        profesorCreado.FechaNacimiento,
        profesorCreado.Telefonos,
        profesorCreado.FechaCreacion,
        profesorCreado.FechaModificacion,
        profesorCreado.Activo
    };

    var usuarioDelToken = await autenticacionService.ObtenerUsuarioDelTokenAsync(authorization) ?? "sistema";

    await bitacoraService.RegistrarAsync(
        usuarioDelToken,
        $"Profesor creado - {JsonSerializer.Serialize(nuevoRegistro)}"
    );

    return Results.Created($"/profesor/{idCreado}", profesorCreado);
})
.WithName("CrearProfesor")
.WithOpenApi();

// PUT /profesor/{id} - Modificar profesor
app.MapPut("/profesor/{id}", async (
    int id,
    [FromHeader(Name = "Authorization")] string? authorization,
    [FromBody] ProfesorDto dto,
    IProfesorRepository repository,
    IAutenticacionService autenticacionService,
    IBitacoraService bitacoraService,
    IParametroService parametroService,
    ITipoIdentificacionService tipoIdentificacionService) =>
{
    if (!await autenticacionService.ValidarTokenAsync(authorization))
        return Results.Json(new { error = "No autorizado" }, statusCode: 401);

    var validacion = await ValidarProfesorAsync(dto, parametroService, tipoIdentificacionService, authorization);
    if (!validacion.esValido)
        return Results.BadRequest(new { error = validacion.mensaje });

    var profesorExistente = await repository.ObtenerPorIdAsync(id);
    if (profesorExistente == null)
        return Results.NotFound(new { error = "Profesor no encontrado" });

    var registroAnterior = new
    {
        profesorExistente.IdProfesor,
        profesorExistente.NumeroIdentificacion,
        profesorExistente.IdTipoIdentificacion,
        profesorExistente.Email,
        profesorExistente.NombreCompleto,
        profesorExistente.FechaNacimiento,
        profesorExistente.Telefonos,
        profesorExistente.FechaCreacion,
        profesorExistente.FechaModificacion,
        profesorExistente.Activo
    };

    var profesor = new Profesor
    {
        IdProfesor = id,
        NumeroIdentificacion = dto.NumeroIdentificacion.Trim(),
        IdTipoIdentificacion = dto.IdTipoIdentificacion,
        Email = dto.Email.Trim().ToLower(),
        NombreCompleto = dto.NombreCompleto.Trim(),
        FechaNacimiento = dto.FechaNacimiento,
        Telefonos = dto.Telefonos.Trim()
    };

    await repository.ActualizarAsync(profesor);

    // Obtener el registro completo actualizado
    var profesorActualizado = await repository.ObtenerPorIdAsync(id);

    var registroActual = new
    {
        profesorActualizado.IdProfesor,
        profesorActualizado.NumeroIdentificacion,
        profesorActualizado.IdTipoIdentificacion,
        profesorActualizado.Email,
        profesorActualizado.NombreCompleto,
        profesorActualizado.FechaNacimiento,
        profesorActualizado.Telefonos,
        profesorActualizado.FechaCreacion,
        profesorActualizado.FechaModificacion,
        profesorActualizado.Activo
    };

    var usuarioDelToken = await autenticacionService.ObtenerUsuarioDelTokenAsync(authorization) ?? "sistema";

    await bitacoraService.RegistrarAsync(
        usuarioDelToken,
        $"Profesor actualizado - Anterior: {JsonSerializer.Serialize(registroAnterior)}, Actual: {JsonSerializer.Serialize(registroActual)}"
    );

    return Results.Ok(profesorActualizado);
})
.WithName("ActualizarProfesor")
.WithOpenApi();

// DELETE /profesor/{id} - Eliminar profesor
app.MapDelete("/profesor/{id}", async (
    int id,
    [FromHeader(Name = "Authorization")] string? authorization,
    IProfesorRepository repository,
    IAutenticacionService autenticacionService,
    IBitacoraService bitacoraService) =>
{
    if (!await autenticacionService.ValidarTokenAsync(authorization))
        return Results.Json(new { error = "No autorizado" }, statusCode: 401);

    var profesor = await repository.ObtenerPorIdAsync(id);
    if (profesor == null)
        return Results.NotFound(new { error = "Profesor no encontrado" });

    var registroEliminado = new
    {
        profesor.IdProfesor,
        profesor.NumeroIdentificacion,
        profesor.IdTipoIdentificacion,
        profesor.Email,
        profesor.NombreCompleto,
        profesor.FechaNacimiento,
        profesor.Telefonos,
        profesor.FechaCreacion,
        profesor.FechaModificacion,
        profesor.Activo
    };

    await repository.EliminarAsync(id);

    var usuarioDelToken = await autenticacionService.ObtenerUsuarioDelTokenAsync(authorization) ?? "sistema";

    await bitacoraService.RegistrarAsync(
        usuarioDelToken,
        $"Profesor eliminado - {JsonSerializer.Serialize(registroEliminado)}"
    );

    return Results.NoContent();
})
.WithName("EliminarProfesor")
.WithOpenApi();

// GET /profesor - Obtener todos los profesores
app.MapGet("/profesor", async (
    [FromHeader(Name = "Authorization")] string? authorization,
    IProfesorRepository repository,
    IAutenticacionService autenticacionService,
    IBitacoraService bitacoraService) =>
{
    if (!await autenticacionService.ValidarTokenAsync(authorization))
        return Results.Json(new { error = "No autorizado" }, statusCode: 401);

    var profesores = await repository.ObtenerTodosAsync();

    var usuarioDelToken = await autenticacionService.ObtenerUsuarioDelTokenAsync(authorization) ?? "sistema";

    await bitacoraService.RegistrarAsync(
        usuarioDelToken,
        "El usuario consulta profesores"
    );

    return Results.Ok(profesores);
})
.WithName("ObtenerTodosProfesores")
.WithOpenApi();

// GET /profesor/{id} - Obtener profesor por id
app.MapGet("/profesor/{id}", async (
    int id,
    [FromHeader(Name = "Authorization")] string? authorization,
    IProfesorRepository repository,
    IAutenticacionService autenticacionService,
    IBitacoraService bitacoraService) =>
{
    if (!await autenticacionService.ValidarTokenAsync(authorization))
        return Results.Json(new { error = "No autorizado" }, statusCode: 401);

    var profesor = await repository.ObtenerPorIdAsync(id);
    if (profesor == null)
        return Results.NotFound(new { error = "Profesor no encontrado" });

    var usuarioDelToken = await autenticacionService.ObtenerUsuarioDelTokenAsync(authorization) ?? "sistema";

    await bitacoraService.RegistrarAsync(
        usuarioDelToken,
        "El usuario consulta profesor"
    );

    return Results.Ok(profesor);
})
.WithName("ObtenerProfesorPorId")
.WithOpenApi();

app.Run();

static async Task<(bool esValido, string mensaje)> ValidarProfesorAsync(
    ProfesorDto dto,
    IParametroService parametroService,
    ITipoIdentificacionService tipoIdentificacionService,
    string? authorization)
{
    if (string.IsNullOrWhiteSpace(dto.Email))
        return (false, "El email es requerido");

    var emailPattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
    if (!Regex.IsMatch(dto.Email.Trim(), emailPattern))
        return (false, "El formato del email no es v·lido");

    // Obtener el dominio parametrizable (por defecto cuc.ac.cr)
    var dominioProfesor = await parametroService.ObtenerValorAsync("DOMPROF", authorization) ?? "cuc.ac.cr";

    var email = dto.Email.Trim().ToLower();
    if (!email.EndsWith($"@{dominioProfesor}"))
        return (false, $"El email debe pertenecer al dominio {dominioProfesor}");

    if (string.IsNullOrWhiteSpace(dto.NombreCompleto))
        return (false, "El nombre completo es requerido");

    var nombrePattern = @"^[a-zA-Z·ÈÌÛ˙¡…Õ”⁄Ò—\s]+$";
    if (!Regex.IsMatch(dto.NombreCompleto.Trim(), nombrePattern))
        return (false, "El nombre solo puede contener letras y espacios");

    if (string.IsNullOrWhiteSpace(dto.NumeroIdentificacion))
        return (false, "El n˙mero de identificaciÛn es requerido");

    if (dto.IdTipoIdentificacion <= 0)
        return (false, "El tipo de identificaciÛn es requerido");

    // Validar que el tipo de identificaciÛn exista en la API de usuarios
    if (!await tipoIdentificacionService.ValidarTipoIdentificacionAsync(dto.IdTipoIdentificacion, authorization))
        return (false, "El tipo de identificaciÛn no existe");

    if (dto.FechaNacimiento == default)
        return (false, "La fecha de nacimiento es requerida");

    // Validar mayor de edad (18 aÒos)
    var edad = DateTime.Today.Year - dto.FechaNacimiento.Year;
    if (dto.FechaNacimiento.Date > DateTime.Today.AddYears(-edad)) edad--;

    if (edad < 18)
        return (false, "El profesor debe ser mayor de edad");

    if (string.IsNullOrWhiteSpace(dto.Telefonos))
        return (false, "Al menos un telÈfono es requerido");

    return (true, string.Empty);
}

record ProfesorDto(
    string NumeroIdentificacion,
    int IdTipoIdentificacion,
    string Email,
    string NombreCompleto,
    DateTime FechaNacimiento,
    string Telefonos
);