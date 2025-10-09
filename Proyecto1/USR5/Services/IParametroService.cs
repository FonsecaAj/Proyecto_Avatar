namespace USR5.Services
{
    public interface IParametroService
    {
        Task<int> ObtenerTiempoExpiracionJwtAsync();
        Task<int> ObtenerTiempoExpiracionRefreshAsync();
    }

    public class ParametroService : IParametroService
    {
        private readonly HttpClient _httpClient;
        private readonly string _parametroApiUrl;

        public ParametroService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _parametroApiUrl = configuration["ParametroApiUrl"] ?? "http://localhost:5279";
        }

        public async Task<int> ObtenerTiempoExpiracionJwtAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_parametroApiUrl}/parametro/JWTEXPMIN");
                if (!response.IsSuccessStatusCode)
                    return 5;

                var parametro = await response.Content.ReadFromJsonAsync<ParametroDto>();
                if (parametro == null || !int.TryParse(parametro.Valor, out int minutos))
                    return 5;

                return minutos;
            }
            catch
            {
                return 5;
            }
        }

        public async Task<int> ObtenerTiempoExpiracionRefreshAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_parametroApiUrl}/parametro/REFEXPMIN");
                if (!response.IsSuccessStatusCode)
                    return 60;

                var parametro = await response.Content.ReadFromJsonAsync<ParametroDto>();
                if (parametro == null || !int.TryParse(parametro.Valor, out int minutos))
                    return 60;

                return minutos;
            }
            catch
            {
                return 60;
            }
        }
    }

    public record ParametroDto(string IdParametro, string Valor);
}