using ACD6.Entities;

namespace ACD6.Repository
{
    public interface IProfesorRepository
    {
        Task<int> CrearAsync(Profesor profesor);
        Task ActualizarAsync(Profesor profesor);
        Task EliminarAsync(int idProfesor);
        Task<IEnumerable<Profesor>> ObtenerTodosAsync();
        Task<Profesor?> ObtenerPorIdAsync(int idProfesor);
        Task<Profesor?> ObtenerPorEmailAsync(string email);
    }
}
