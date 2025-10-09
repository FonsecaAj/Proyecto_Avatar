using GEN1.Entities;

namespace GEN1.Repository
{
    public interface IBitacoraRepository
    {
        Task<int> CrearBitacoraAsync(Bitacora bitacora);
    }
}
