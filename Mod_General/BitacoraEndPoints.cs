using Microsoft.AspNetCore.Mvc;
using Mod_General.Entities;
using Mod_General.Services;

namespace Mod_General
{
    public static class BitacoraEndPoints
    {

        public static void MapBitacoraEndpoints(this IEndpointRouteBuilder routes)
        {
            var group = routes.MapGroup("/api/bitacora").WithTags(nameof(Bitacora));

            // POST /api/bitacora
            group.MapPost("/", async ([FromBody] BitacoraRequest request, [FromServices] IBitacoraService service) =>
            {
                var response = await service.Registrar(request);
                return Results.Json(response, statusCode: response.StatusCode);
            })
            .WithName("RegistrarBitacora")
            .WithOpenApi();
        }

    }
}
