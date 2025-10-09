namespace ACD6.Services
{
    public interface IParametroService
    {
        Task<string?> ObtenerValorAsync(string idParametro, string? authToken);
    }

    public class ParametroService : IParametroService
    {
        private readonly HttpClient _httpClient;
        private readonly string _parametroApiUrl;

        public ParametroService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _parametroApiUrl = configuration["ParametroApiUrl"] ?? "http://localhost:5203";
        }

        public async Task<string?> ObtenerValorAsync(string idParametro, string? authToken)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, $"{_parametroApiUrl}/parametro/{idParametro}");

                if (!string.IsNullOrWhiteSpace(authToken))
                {
                    request.Headers.Add("Authorization", authToken);
                }

                var response = await _httpClient.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                    return null;

                var parametro = await response.Content.ReadFromJsonAsync<ParametroDto>();
                return parametro?.Valor;
            }
            catch
            {
                return null;
            }
        }
    }

    public record ParametroDto(string IdParametro, string Valor);
}