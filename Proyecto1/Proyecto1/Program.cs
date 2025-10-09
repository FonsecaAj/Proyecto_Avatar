using USR2.Entities;
using USR2.Repository;
using USR2.Services;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IRolRepository, RolRepository>();
builder.Services.AddHttpClient<IAutenticacionService, AutenticacionService>();
builder.Services.AddHttpClient<IBitacoraService, BitacoraService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// POST /rol - Crear rol
app.MapPost("/rol", async (
    [FromHeader(Name = "Authorization")] string? authorization,
    [FromBody] RolDto dto,
    IRolRepository repository,
    IAutenticacionService autenticacionService,
    IBitacoraService bitacoraService) =>
{
    if (!await autenticacionService.ValidarTokenAsync(authorization))
        return Results.Json(new { error = "No autorizado" }, statusCode: 401);

    var usuario = await autenticacionService.ObtenerUsuarioDelTokenAsync(authorization) ?? "sistema";

    var validacion = ValidarRol(dto.Nombre);
    if (!validacion.esValido)
        return Results.BadRequest(new { error = validacion.mensaje });

    // Validar duplicados
    var rolExistente = await repository.ObtenerPorNombreAsync(dto.Nombre.Trim());
    if (rolExistente != null)
        return Results.BadRequest(new { error = "Ya existe un rol con ese nombre" });

    var rol = new Rol { Nombre = dto.Nombre.Trim() };
    var id = await repository.CrearAsync(rol);

    var nuevoRegistro = new
    {
        IdRol = id,
        rol.Nombre
    };

    await bitacoraService.RegistrarAsync(
        usuario,
        $"Rol creado - {JsonSerializer.Serialize(nuevoRegistro)}"
    );

    return Results.Created($"/rol/{id}", new { id, nombre = rol.Nombre });
})
.WithName("CrearRol")
.WithOpenApi();

// PUT /rol/{id} - Modificar rol
app.MapPut("/rol/{id}", async (
    int id,
    [FromHeader(Name = "Authorization")] string? authorization,
    [FromBody] RolDto dto,
    IRolRepository repository,
    IAutenticacionService autenticacionService,
    IBitacoraService bitacoraService) =>
{
    if (!await autenticacionService.ValidarTokenAsync(authorization))
        return Results.Json(new { error = "No autorizado" }, statusCode: 401);

    var usuario = await autenticacionService.ObtenerUsuarioDelTokenAsync(authorization) ?? "sistema";

    var validacion = ValidarRol(dto.Nombre);
    if (!validacion.esValido)
        return Results.BadRequest(new { error = validacion.mensaje });

    var rolExistente = await repository.ObtenerPorIdAsync(id);
    if (rolExistente == null)
        return Results.NotFound(new { error = "Rol no encontrado" });

    // Validar duplicados (excepto el mismo registro)
    var rolDuplicado = await repository.ObtenerPorNombreAsync(dto.Nombre.Trim());
    if (rolDuplicado != null && rolDuplicado.IdRol != id)
        return Results.BadRequest(new { error = "Ya existe un rol con ese nombre" });

    var registroAnterior = new
    {
        rolExistente.IdRol,
        rolExistente.Nombre
    };

    var rol = new Rol { IdRol = id, Nombre = dto.Nombre.Trim() };
    await repository.ActualizarAsync(rol);

    var registroActual = new
    {
        rol.IdRol,
        rol.Nombre
    };

    await bitacoraService.RegistrarAsync(
        usuario,
        $"Rol actualizado - Anterior: {JsonSerializer.Serialize(registroAnterior)}, Actual: {JsonSerializer.Serialize(registroActual)}"
    );

    return Results.Ok(new { id = rol.IdRol, nombre = rol.Nombre });
})
.WithName("ActualizarRol")
.WithOpenApi();

// DELETE /rol/{id} - Eliminar rol
app.MapDelete("/rol/{id}", async (
    int id,
    [FromHeader(Name = "Authorization")] string? authorization,
    IRolRepository repository,
    IAutenticacionService autenticacionService,
    IBitacoraService bitacoraService) =>
{
    if (!await autenticacionService.ValidarTokenAsync(authorization))
        return Results.Json(new { error = "No autorizado" }, statusCode: 401);

    var usuario = await autenticacionService.ObtenerUsuarioDelTokenAsync(authorization) ?? "sistema";

    var rol = await repository.ObtenerPorIdAsync(id);
    if (rol == null)
        return Results.NotFound(new { error = "Rol no encontrado" });

    var registroEliminado = new
    {
        rol.IdRol,
        rol.Nombre
    };

    await repository.EliminarAsync(id);

    await bitacoraService.RegistrarAsync(
        usuario,
        $"Rol eliminado - {JsonSerializer.Serialize(registroEliminado)}"
    );

    return Results.NoContent();
})
.WithName("EliminarRol")
.WithOpenApi();

// GET /rol - Obtener todos los roles
app.MapGet("/rol", async (
    [FromHeader(Name = "Authorization")] string? authorization,
    IRolRepository repository,
    IAutenticacionService autenticacionService,
    IBitacoraService bitacoraService) =>
{
    if (!await autenticacionService.ValidarTokenAsync(authorization))
        return Results.Json(new { error = "No autorizado" }, statusCode: 401);

    var usuario = await autenticacionService.ObtenerUsuarioDelTokenAsync(authorization) ?? "sistema";

    var roles = await repository.ObtenerTodosAsync();

    await bitacoraService.RegistrarAsync(
        usuario,
        "El usuario consulta roles"
    );

    return Results.Ok(roles);
})
.WithName("ObtenerTodosRoles")
.WithOpenApi();

// GET /rol/{id} - Obtener rol por ID
app.MapGet("/rol/{id}", async (
    int id,
    [FromHeader(Name = "Authorization")] string? authorization,
    IRolRepository repository,
    IAutenticacionService autenticacionService,
    IBitacoraService bitacoraService) =>
{
    if (!await autenticacionService.ValidarTokenAsync(authorization))
        return Results.Json(new { error = "No autorizado" }, statusCode: 401);

    var usuario = await autenticacionService.ObtenerUsuarioDelTokenAsync(authorization) ?? "sistema";

    var rol = await repository.ObtenerPorIdAsync(id);

    if (rol == null)
        return Results.NotFound(new { error = "Rol no encontrado" });

    await bitacoraService.RegistrarAsync(
        usuario,
        "El usuario consulta rol"
    );

    return Results.Ok(rol);
})
.WithName("ObtenerRolPorId")
.WithOpenApi();

app.Run();

static (bool esValido, string mensaje) ValidarRol(string nombre)
{
    if (string.IsNullOrWhiteSpace(nombre))
        return (false, "El nombre del rol es requerido");

    if (!Regex.IsMatch(nombre.Trim(), @"^[a-zA-Z·ÈÌÛ˙¡…Õ”⁄Ò—\s]+$"))
        return (false, "El nombre del rol solo puede contener letras y espacios");

    return (true, string.Empty);
}

record RolDto(string Nombre);