using Microsoft.AspNetCore.Mvc;
using Mod_Matricula.Entities;
using Mod_Matricula.Services;

namespace Mod_Matricula
{
    public static class UbicacionesEndPoints
    {

        public static void MapUbicacionesEndpoints(this IEndpointRouteBuilder routes)
        {
            // ======== PROVINCIAS ========
            var provincias = routes.MapGroup("/api/provincias").WithTags(nameof(Provincia));

            provincias.MapGet("/", async ([FromServices] IUbicacionesServices ubicacionesServices) =>
            {
                var response = await ubicacionesServices.Obtener_Provincias();
                
                return Results.Json(response, statusCode: response.StatusCode);
            })
            
            .WithName("GetAllProvincias")
            
            .WithOpenApi();


            // ======== CANTONES ========
            var cantones = routes.MapGroup("/api/cantones").WithTags(nameof(Canton));

            cantones.MapGet("/", async (
                
                [FromQuery] string? provincia,
                
                [FromServices] IUbicacionesServices ubicacionesServices) =>
            {
                // Validar si el parámetro viene vacío o nulo
                if (string.IsNullOrWhiteSpace(provincia))
                {
                    return Results.Json(new BusinessLogicResponse
                    {
                        StatusCode = 400,
                        Message = "El parámetro 'provincia' es requerido y debe ser válido."
                    }, statusCode: 400);
                }

                // convertir manualmente
                if (!int.TryParse(provincia, out int idProvincia) || idProvincia <= 0)
                {
                    return Results.Json(new BusinessLogicResponse
                    {
                        StatusCode = 400,
                        Message = "El parámetro 'provincia' debe ser un número válido mayor que 0."
                    }, statusCode: 400);
                }

                var response = await ubicacionesServices.Obtener_Cantones(idProvincia);
                
                return Results.Json(response, statusCode: response.StatusCode);
            })

            .WithName("GetCantonesPorProvincia")
            
            .WithOpenApi();


            // ======== DISTRITOS ========
            var distritos = routes.MapGroup("/api/distritos").WithTags(nameof(Distrito));

            distritos.MapGet("/", async (
                
                [FromQuery] string? provincia,
                
                [FromQuery] string? canton,
                
                [FromServices] IUbicacionesServices ubicacionesServices) =>
            {
                // Validar si faltan parámetros o vienen vacíos
                if (string.IsNullOrWhiteSpace(provincia) || string.IsNullOrWhiteSpace(canton))
                {
                    return Results.Json(new BusinessLogicResponse
                    {
                        StatusCode = 400,
                        Message = "Debe especificar los parámetros 'provincia' y 'canton' válidos."
                    }, statusCode: 400);
                }

                // convertir ambos
                if (!int.TryParse(provincia, out int idProvincia) ||
                    !int.TryParse(canton, out int idCanton) ||
                    idProvincia <= 0 || idCanton <= 0)
                {
                    return Results.Json(new BusinessLogicResponse
                    {
                        StatusCode = 400,
                        Message = "Los parámetros 'provincia' y 'canton' deben ser números válidos mayores que 0."
                    }, statusCode: 400);
                }

                var response = await ubicacionesServices.Obtener_Distritos(idProvincia, idCanton);
                
                return Results.Json(response, statusCode: response.StatusCode);
            })

            .WithName("GetDistritosPorProvinciaYCanton")
            
            .WithOpenApi();
        }


    }
}
