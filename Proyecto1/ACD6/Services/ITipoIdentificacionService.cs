namespace ACD6.Services
{
    public interface ITipoIdentificacionService
    {
        Task<bool> ValidarTipoIdentificacionAsync(int idTipo, string? authToken);
    }

    public class TipoIdentificacionService : ITipoIdentificacionService
    {
        private readonly HttpClient _httpClient;
        private readonly string _usuarioApiUrl;

        public TipoIdentificacionService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _usuarioApiUrl = configuration["UsuarioApiUrl"] ?? "http://localhost:5205";
        }

        public async Task<bool> ValidarTipoIdentificacionAsync(int idTipo, string? authToken)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, $"{_usuarioApiUrl}/tipoidentificacion/{idTipo}");

                if (!string.IsNullOrWhiteSpace(authToken))
                {
                    request.Headers.Add("Authorization", authToken);
                }

                var response = await _httpClient.SendAsync(request);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }
    }
}