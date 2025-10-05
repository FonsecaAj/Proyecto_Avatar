using EjemploMicroServicio.Entities;
using EjemploMicroServicio.Services;
using Microsoft.AspNetCore.Mvc;

namespace EjemploMicroServicio
{
    public static class PersonaEndpoints
    {
        public static void MapPersonaEndpoints(this IEndpointRouteBuilder routes)
        {
            var group = routes.MapGroup("/api/Persona").WithTags(nameof(Persona));

            group.MapGet("/", async ([FromServices] IPersonaService personaService) =>
            {
                return await personaService.GetAllAsync();
            })
            .WithName("GetAllPersonas")
            .WithOpenApi();


        }
    }
}
