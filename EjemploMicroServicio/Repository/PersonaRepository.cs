using Dapper;
using EjemploMicroServicio.Entities;

namespace EjemploMicroServicio.Repository
{
    public class PersonaRepository
    {
        private readonly IDbConnectionFactory _dbConnectionFactory;

        public PersonaRepository(IDbConnectionFactory dbConnectionFactory)
        {
            _dbConnectionFactory = dbConnectionFactory;
        }

        public async Task<IEnumerable<Persona>> GetAllAsync()
        {
            using (var connection = _dbConnectionFactory.CreateConnection())
            {
                return await connection.QueryAsync<Persona>("SELECT PersonaID, Nombre, Tipo, Gender, Password FROM Persona");
            }
        }
    }
}
