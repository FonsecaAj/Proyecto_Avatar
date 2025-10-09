//using Estudiantes_Matriculados.Services;
//using Microsoft.AspNetCore.Mvc;

//namespace Estudiantes_Matriculados
//{
//    //public class EstudiantesEndPoints
//    //{
//    //    public static void MapEstudiantesEndpoints(this IEndpointRouteBuilder routes)
//    //    {
//    //        var group = routes.MapGroup("/api/estudiantes").WithTags("Estudiantes");

//    //        group.MapGet("/listadoestudiantes", async (
//    //            [FromHeader(Name = "Authorization")] string? authorization,
//    //            [FromQuery] int periodo,
//    //            [FromServices] IEstudiantesService service,
//    //            [FromServices] IEstudiantesService autenticacion) =>
//    //        {
//    //            if (!await autenticacion.ValidarTokenAsync(authorization))
//    //                return Results.Json(new { error = "No autorizado" }, statusCode: 401);

//    //            var usuario = await autenticacion.ObtenerUsuarioDelTokenAsync(authorization) ?? "sistema";

//    //            var response = await service.ObtenerListadoPorPeriodo(periodo);

//    //            return Results.Json(response, statusCode: response.StatusCode);
//    //        })
//    //        .WithName("ListadoEstudiantes")
//    //        .WithOpenApi();
//    //    }

//    //}

//}

using Estudiantes_Matriculados.Services;
using Microsoft.AspNetCore.Mvc;

namespace Estudiantes_Matriculados
{
    public static class EstudiantesEndPoints
    {
        public static void MapEstudiantesEndpoints(this IEndpointRouteBuilder routes)
        {
            var group = routes.MapGroup("/api/estudiantes").WithTags("Estudiantes");

            group.MapGet("/listadoestudiantes", async (
                [FromQuery] int periodo,
                [FromServices] IEstudiantesService service) =>
            {
                // 🔹 Validar parámetro
                if (periodo <= 0)
                    return Results.Json(new { error = "Debe indicar un ID de periodo válido." }, statusCode: 400);

                // 🔹 Ejecutar servicio directamente (sin token)
                var response = await service.ObtenerListadoPorPeriodo(periodo);

                return Results.Json(response, statusCode: response.StatusCode);
            })
            .WithName("ListadoEstudiantesSinToken")
            .WithOpenApi();
        }
    }
}
    