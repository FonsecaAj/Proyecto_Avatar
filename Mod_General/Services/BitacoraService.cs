using Mod_General.Entities;
using Mod_General.Repository;
using System.Text.Json;

namespace Mod_General.Services
{
    public class BitacoraService : IBitacoraService
    {
        private readonly BitacoraRepository _bitacoraRepository;

        public BitacoraService(BitacoraRepository bitacoraRepository)
        {
            _bitacoraRepository = bitacoraRepository;
        }

        public async Task<BusinessLogicResponse> Registrar(BitacoraRequest request)
        {
            try
            {
                // Validaciones requeridas
                if (string.IsNullOrWhiteSpace(request.Usuario))
                    return new BusinessLogicResponse
                    {
                        StatusCode = 400,
                        Message = "El campo 'Usuario' es requerido y no puede estar vacío."
                    };

                if (string.IsNullOrWhiteSpace(request.Tipo_Accion))
                    return new BusinessLogicResponse
                    {
                        StatusCode = 400,
                        Message = "El campo 'Tipo_Accion' es requerido y no puede estar vacío."
                    };

                if (string.IsNullOrWhiteSpace(request.Descripcion))
                    return new BusinessLogicResponse
                    {
                        StatusCode = 400,
                        Message = "El campo 'Descripción' es requerida y no puede estar vacía."
                    };

                // Validación del formato JSON (CRUD)
                if (request.Tipo_Accion.ToUpper() != "SELECT" && !IsValidJson(request.Descripcion))
                    return new BusinessLogicResponse
                    {
                        StatusCode = 400,
                        Message = "El campo 'Descripción' debe tener un formato JSON válido para operaciones de tipo INSERT, UPDATE o DELETE."
                    };

                // Construir entidad
                var bitacora = new Bitacora
                {
                    Usuario = request.Usuario.Trim(),
                    Tipo_Accion = request.Tipo_Accion.Trim().ToUpper(),
                    Descripcion = request.Descripcion.Trim(),
                    Fecha_Registro = DateTime.Now
                };

                // Registrar bitácora usando la sintaxis estándar
                var id = await _bitacoraRepository.Registrar(bitacora);

                // Respuesta
                return new BusinessLogicResponse
                {
                    StatusCode = 201,
                    Message = "Bitácora registrada exitosamente.",
                    ResponseObject = new
                    {
                        ID_Bitacora = id,
                        bitacora.Usuario,
                        bitacora.Tipo_Accion,
                        bitacora.Fecha_Registro
                    }
                };
            }
            catch (Exception ex)
            {
                // Manejo de errores técnicos
                return new BusinessLogicResponse
                {
                    StatusCode = 500,
                    Message = $"Error al registrar bitácora: {ex.Message}"
                };
            }
        }

        // Método auxiliar para validar JSON
        private bool IsValidJson(string str)
        {
            try
            {
                JsonDocument.Parse(str);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
