using EjemploMicroServicio.Entities;

namespace EjemploMicroServicio.Services
{
    public interface IPersonaService
    {
        Task<IEnumerable<Persona>> GetAllAsync();
    }
}
