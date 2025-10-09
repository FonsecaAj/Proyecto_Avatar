using Adm_Notas.Entities;
using Adm_Notas.Services;
using Microsoft.AspNetCore.Mvc;

namespace Adm_Notas
{
    public static class NotasEndPoints
    {

        public static void MapRubrosEndpoints(this IEndpointRouteBuilder routes)
        {
            var group = routes.MapGroup("/api/rubros").WithTags(nameof(Rubro));

            // POST /api/rubros/cargardesglose
            group.MapPost("/cargardesglose", async ([FromBody] DesgloseRequest request, [FromServices] IRubroServices service) =>
            {
                var response = await service.CargarDesglose(request);
                return Results.Json(response, statusCode: response.StatusCode);
            })
            .WithName("CargarDesglose")
            .WithOpenApi();

            // GET /api/rubros/obtenerdesglose?idGrupo=#
            group.MapGet("/obtenerdesglose", async ([FromQuery] int idGrupo, [FromServices] IRubroServices service) =>
            {
                var response = await service.ObtenerDesglose(idGrupo);
                return Results.Json(response, statusCode: response.StatusCode);
            })
            .WithName("ObtenerDesglose")
            .WithOpenApi();

            // POST /api/rubros/asignarnotarubro
            group.MapPost("/asignarnotarubro", async ([FromBody] NotaRequest request, [FromServices] INotasServices service) =>
            {
                var response = await service.AsignarNota(request);
                return Results.Json(response, statusCode: response.StatusCode);
            })
            .WithName("AsignarNotaRubro")
            .WithOpenApi();

            // GET /api/rubros/obtenernotas?idEstudiante=#&idCurso=#
            group.MapGet("/obtenernotas", async ([FromQuery] int idEstudiante, [FromQuery] int idCurso, [FromServices] INotasServices service) =>
            {
                var response = await service.ObtenerNotas(idEstudiante, idCurso);
                return Results.Json(response, statusCode: response.StatusCode);
            })
            .WithName("ObtenerNotasEstudiante")
            .WithOpenApi();
        }


    }
}
