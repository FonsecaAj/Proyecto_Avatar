using EjemploMicroServicio.Entities;
using EjemploMicroServicio.Repository;

namespace EjemploMicroServicio.Services
{
    public class PersonaService : IPersonaService
    {

        private readonly PersonaRepository _personaRepository;
        public PersonaService(PersonaRepository personaRepository)
        {
            _personaRepository = personaRepository;
        }

        public async Task<IEnumerable<Persona>> GetAllAsync()
        {
            return await _personaRepository.GetAllAsync();
        }

    }
}
