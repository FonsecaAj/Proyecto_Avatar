using Mod_General.Entities;

namespace Mod_General.Services
{
    public interface IBitacoraService
    {
        Task<BusinessLogicResponse> Registrar(BitacoraRequest request);

    }
}
