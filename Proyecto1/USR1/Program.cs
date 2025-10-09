using Dapper;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text.RegularExpressions;
using USR1.Entities;
using USR1.Repository;
using USR1.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddHttpClient<IRolService, RolService>();
builder.Services.AddHttpClient<IAutenticacionService, AutenticacionService>();
builder.Services.AddHttpClient<IBitacoraService, BitacoraService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// POST /usuario - Crear usuario
app.MapPost("/usuario", async (
    [FromHeader(Name = "Authorization")] string? authorization,
    [FromBody] UsuarioDto dto,
    IUsuarioRepository repository,
    IRolService rolService,
    IAutenticacionService autenticacionService,
    IBitacoraService bitacoraService) =>
{
    if (!await autenticacionService.ValidarTokenAsync(authorization))
        return Results.Json(new { error = "No autorizado" }, statusCode: 401);

    var validacion = await ValidarUsuarioAsync(dto, rolService, authorization);
    if (!validacion.esValido)
        return Results.BadRequest(new { error = validacion.mensaje });

    var usuarioExistente = await repository.ObtenerPorEmailAsync(dto.Email);
    if (usuarioExistente != null)
        return Results.BadRequest(new { error = "El email ya está registrado" });

    var email = dto.Email.Trim().ToLower();
    var rolAsignado = await ObtenerRolPorDominioAsync(email, dto.RolDeseado, rolService, authorization);

    if (rolAsignado == null)
        return Results.BadRequest(new { error = "No se pudo asignar el rol correspondiente al dominio" });

    var usuario = new Usuario
    {
        Email = email,
        IdTipoIdentificacion = dto.IdTipoIdentificacion,
        Identificacion = dto.Identificacion.Trim(),
        Nombre = dto.Nombre.Trim(),
        IdRol = rolAsignado.IdRol,
        Contrasenna = dto.Contrasenna
    };

    var emailCreado = await repository.CrearAsync(usuario);

    var nuevoRegistro = new
    {
        usuario.Email,
        usuario.IdTipoIdentificacion,
        usuario.Identificacion,
        usuario.Nombre,
        usuario.IdRol,
        RolAsignado = rolAsignado.Nombre
    };

    // CAMBIO: Obtener usuario del token
    var usuarioDelToken = await autenticacionService.ObtenerUsuarioDelTokenAsync(authorization) ?? "sistema";

    await bitacoraService.RegistrarAsync(
        usuarioDelToken,
        $"Usuario creado - {JsonSerializer.Serialize(nuevoRegistro)}"
    );

    return Results.Created($"/usuario/{emailCreado}", new { email = emailCreado, nombre = usuario.Nombre, rol = rolAsignado.Nombre });
})
.WithName("CrearUsuario")
.WithOpenApi();

// PUT /usuario/{email} - Modificar usuario
app.MapPut("/usuario/{email}", async (
    string email,
    [FromHeader(Name = "Authorization")] string? authorization,
    [FromBody] UsuarioDto dto,
    IUsuarioRepository repository,
    IRolService rolService,
    IAutenticacionService autenticacionService,
    IBitacoraService bitacoraService) =>
{
    if (!await autenticacionService.ValidarTokenAsync(authorization))
        return Results.Json(new { error = "No autorizado" }, statusCode: 401);

    var validacion = await ValidarUsuarioAsync(dto, rolService, authorization);
    if (!validacion.esValido)
        return Results.BadRequest(new { error = validacion.mensaje });

    var usuarioExistente = await repository.ObtenerPorEmailAsync(email);
    if (usuarioExistente == null)
        return Results.NotFound(new { error = "Usuario no encontrado" });

    var registroAnterior = new
    {
        usuarioExistente.Email,
        usuarioExistente.IdTipoIdentificacion,
        usuarioExistente.Identificacion,
        usuarioExistente.Nombre,
        usuarioExistente.IdRol
    };

    var emailNuevo = dto.Email.Trim().ToLower();
    var rolAsignado = await ObtenerRolPorDominioAsync(emailNuevo, dto.RolDeseado, rolService, authorization);

    if (rolAsignado == null)
        return Results.BadRequest(new { error = "No se pudo asignar el rol correspondiente al dominio" });

    var usuario = new Usuario
    {
        Email = email,
        IdTipoIdentificacion = dto.IdTipoIdentificacion,
        Identificacion = dto.Identificacion.Trim(),
        Nombre = dto.Nombre.Trim(),
        IdRol = rolAsignado.IdRol,
        Contrasenna = dto.Contrasenna
    };

    await repository.ActualizarAsync(usuario);

    var registroActual = new
    {
        usuario.Email,
        usuario.IdTipoIdentificacion,
        usuario.Identificacion,
        usuario.Nombre,
        usuario.IdRol
    };

    // CAMBIO: Obtener usuario del token
    var usuarioDelToken = await autenticacionService.ObtenerUsuarioDelTokenAsync(authorization) ?? "sistema";

    await bitacoraService.RegistrarAsync(
        usuarioDelToken,
        $"Usuario actualizado - Anterior: {JsonSerializer.Serialize(registroAnterior)}, Actual: {JsonSerializer.Serialize(registroActual)}"
    );

    return Results.Ok(new { email = usuario.Email, nombre = usuario.Nombre, rol = rolAsignado.Nombre });
})
.WithName("ActualizarUsuario")
.WithOpenApi();

// DELETE /usuario/{email} - Eliminar usuario
app.MapDelete("/usuario/{email}", async (
    string email,
    [FromHeader(Name = "Authorization")] string? authorization,
    IUsuarioRepository repository,
    IAutenticacionService autenticacionService,
    IBitacoraService bitacoraService) =>
{
    if (!await autenticacionService.ValidarTokenAsync(authorization))
        return Results.Json(new { error = "No autorizado" }, statusCode: 401);

    var usuario = await repository.ObtenerPorEmailAsync(email);
    if (usuario == null)
        return Results.NotFound(new { error = "Usuario no encontrado" });

    var registroEliminado = new
    {
        usuario.Email,
        usuario.IdTipoIdentificacion,
        usuario.Identificacion,
        usuario.Nombre,
        usuario.IdRol
    };

    await repository.EliminarAsync(email);

    // CAMBIO: Obtener usuario del token
    var usuarioDelToken = await autenticacionService.ObtenerUsuarioDelTokenAsync(authorization) ?? "sistema";

    await bitacoraService.RegistrarAsync(
        usuarioDelToken,
        $"Usuario eliminado - {JsonSerializer.Serialize(registroEliminado)}"
    );

    return Results.NoContent();
})
.WithName("EliminarUsuario")
.WithOpenApi();

// GET /usuario - Obtener todos los usuarios
app.MapGet("/usuario", async (
    [FromHeader(Name = "Authorization")] string? authorization,
    IUsuarioRepository repository,
    IAutenticacionService autenticacionService,
    IBitacoraService bitacoraService) =>
{
    if (!await autenticacionService.ValidarTokenAsync(authorization))
        return Results.Json(new { error = "No autorizado" }, statusCode: 401);

    var usuarios = await repository.ObtenerTodosAsync();

    // CAMBIO: Obtener usuario del token
    var usuarioDelToken = await autenticacionService.ObtenerUsuarioDelTokenAsync(authorization) ?? "sistema";

    await bitacoraService.RegistrarAsync(
        usuarioDelToken,
        "El usuario consulta usuarios"
    );

    return Results.Ok(usuarios.Select(u => new
    {
        u.Email,
        u.IdTipoIdentificacion,
        u.Identificacion,
        u.Nombre,
        u.IdRol,
        u.Contrasenna,
        u.FechaCreacion,
        u.FechaModificacion,
        u.Activo
    }));
})
.WithName("ObtenerTodosUsuarios")
.WithOpenApi();

// GET /usuario/{email} - Obtener usuario por email
app.MapGet("/usuario/{email}", async (
    string email,
    [FromHeader(Name = "Authorization")] string? authorization,
    IUsuarioRepository repository,
    IAutenticacionService autenticacionService,
    IBitacoraService bitacoraService) =>
{
    if (!await autenticacionService.ValidarTokenAsync(authorization))
        return Results.Json(new { error = "No autorizado" }, statusCode: 401);

    var usuario = await repository.ObtenerPorEmailAsync(email);
    if (usuario == null)
        return Results.NotFound(new { error = "Usuario no encontrado" });

    // CAMBIO: Obtener usuario del token
    var usuarioDelToken = await autenticacionService.ObtenerUsuarioDelTokenAsync(authorization) ?? "sistema";

    await bitacoraService.RegistrarAsync(
        usuarioDelToken,
        "El usuario consulta usuario"
    );

    return Results.Ok(new
    {
        usuario.Email,
        usuario.IdTipoIdentificacion,
        usuario.Identificacion,
        usuario.Nombre,
        usuario.IdRol,
        usuario.Contrasenna,
        usuario.FechaCreacion,
        usuario.FechaModificacion,
        usuario.Activo
    });
})
.WithName("ObtenerUsuarioPorEmail")
.WithOpenApi();


app.MapGet("/tipoidentificacion/{id}", async (
    int id,
    [FromHeader(Name = "Authorization")] string? authorization,
    IConfiguration configuration,
    IAutenticacionService autenticacionService) =>
{
    if (!await autenticacionService.ValidarTokenAsync(authorization))
        return Results.Json(new { error = "No autorizado" }, statusCode: 401);

    using var connection = new Microsoft.Data.SqlClient.SqlConnection(
        configuration.GetConnectionString("DefaultConnection"));

    var sql = @"SELECT ID_TIPO_IDENTIFICACION as IdTipoIdentificacion, 
                       NOMBRE as Nombre 
                FROM TIPO_IDENTIFICACION 
                WHERE ID_TIPO_IDENTIFICACION = @Id";

    var tipoIdentificacion = await connection.QueryFirstOrDefaultAsync<TipoIdentificacionDto>(
        sql,
        new { Id = id });

    if (tipoIdentificacion == null)
        return Results.NotFound(new { error = "Tipo de identificación no encontrado" });

    return Results.Ok(tipoIdentificacion);
})
.WithName("ObtenerTipoIdentificacionPorId")
.WithOpenApi();

// GET /usuario/filtrar - Filtrar usuarios
app.MapGet("/usuario/filtrar", async (
    [FromHeader(Name = "Authorization")] string? authorization,
    [FromQuery] string? identificacion,
    [FromQuery] string? nombre,
    [FromQuery] int? tipo,
    IUsuarioRepository repository,
    IAutenticacionService autenticacionService,
    IBitacoraService bitacoraService) =>
{
    if (!await autenticacionService.ValidarTokenAsync(authorization))
        return Results.Json(new { error = "No autorizado" }, statusCode: 401);

    var usuarios = await repository.FiltrarAsync(identificacion, nombre, tipo);

    // CAMBIO: Obtener usuario del token
    var usuarioDelToken = await autenticacionService.ObtenerUsuarioDelTokenAsync(authorization) ?? "sistema";

    await bitacoraService.RegistrarAsync(
        usuarioDelToken,
        "El usuario consulta usuarios filtrados"
    );

    return Results.Ok(usuarios.Select(u => new
    {
        u.Email,
        u.IdTipoIdentificacion,
        u.Identificacion,
        u.Nombre,
        u.IdRol,
        u.Contrasenna,
        u.FechaCreacion,
        u.FechaModificacion,
        u.Activo
    }));
})
.WithName("FiltrarUsuarios")
.WithOpenApi();

app.Run();


static async Task<RolDto?> ObtenerRolPorDominioAsync(string email, string? rolDeseado, IRolService rolService, string? authorization)
{
    string nombreRolRequerido;

    if (email.EndsWith("@cuc.cr"))
    {
        // Para cuc.cr SOLO puede ser estudiante
        nombreRolRequerido = "estudiante";
    }
    else if (email.EndsWith("@cuc.ac.cr"))
    {
        // Para cuc.ac.cr puede ser profesor o administrador
        if (string.IsNullOrWhiteSpace(rolDeseado))
        {
            // Si no especifica, asignamos profesor por defecto
            nombreRolRequerido = "profesor";
        }
        else
        {
            var rolLower = rolDeseado.Trim().ToLower();
            if (rolLower == "profesor" || rolLower == "administrador")
            {
                nombreRolRequerido = rolLower;
            }
            else
            {
                // Si especifica un rol inválido para cuc.ac.cr
                return null;
            }
        }
    }
    else
    {
        return null;
    }

    return await rolService.ObtenerRolPorNombreAsync(nombreRolRequerido, authorization);
}

static async Task<(bool esValido, string mensaje)> ValidarUsuarioAsync(
    UsuarioDto dto,
    IRolService rolService,
    string? authorization)
{
    if (string.IsNullOrWhiteSpace(dto.Email))
        return (false, "El email es requerido");

    var emailPattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
    if (!Regex.IsMatch(dto.Email.Trim(), emailPattern))
        return (false, "El formato del email no es válido");

    var email = dto.Email.Trim().ToLower();
    if (!email.EndsWith("@cuc.cr") && !email.EndsWith("@cuc.ac.cr"))
        return (false, "El email debe pertenecer a los dominios cuc.cr o cuc.ac.cr");

    // Validar que el rol deseado sea válido según el dominio
    if (email.EndsWith("@cuc.cr") && !string.IsNullOrWhiteSpace(dto.RolDeseado))
    {
        var rolLower = dto.RolDeseado.Trim().ToLower();
        if (rolLower != "estudiante")
            return (false, "Los usuarios con dominio cuc.cr solo pueden ser estudiantes");
    }

    if (email.EndsWith("@cuc.ac.cr") && !string.IsNullOrWhiteSpace(dto.RolDeseado))
    {
        var rolLower = dto.RolDeseado.Trim().ToLower();
        if (rolLower != "profesor" && rolLower != "administrador")
            return (false, "Los usuarios con dominio cuc.ac.cr solo pueden ser profesores o administradores");
    }

    if (string.IsNullOrWhiteSpace(dto.Nombre))
        return (false, "El nombre completo es requerido");

    if (string.IsNullOrWhiteSpace(dto.Identificacion))
        return (false, "La identificación es requerida");

    if (dto.IdTipoIdentificacion <= 0)
        return (false, "El tipo de identificación es requerido");

    if (string.IsNullOrWhiteSpace(dto.Contrasenna))
        return (false, "La contraseña es requerida");

    return (true, string.Empty);
}

record UsuarioDto(
    string Email,
    int IdTipoIdentificacion,
    string Identificacion,
    string Nombre,
    string Contrasenna,
    string? RolDeseado  // Para elegir si es profesor o admin
);

record TipoIdentificacionDto(int IdTipoIdentificacion, string Nombre);