using Microsoft.AspNetCore.Mvc;
using Mod_Academico.Services;

namespace Mod_Academico
{
    public static class HistorialAcademicoEndPoints
    {

        public static void MapHistorialEndpoints(this IEndpointRouteBuilder routes)
        {
            var group = routes.MapGroup("/api/historialacademico.").WithTags("Historial Académico");

            group.MapGet("/", async ([FromQuery] string tipo, [FromQuery] string identificacion, [FromServices] IHistorialAcademicoService service) =>
            {
                var response = await service.ObtenerHistorial(tipo, identificacion);
                return Results.Json(response, statusCode: response.StatusCode);
            })
            .WithName("ObtenerHistorialAcademico")
            .WithOpenApi();
        }



    }
}
