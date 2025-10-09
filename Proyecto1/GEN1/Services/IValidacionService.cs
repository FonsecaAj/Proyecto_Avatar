namespace GEN1.Services
{
    public interface IValidacionService
    {
        Task<bool> ValidarTokenAsync(string token);
    }

    public class ValidacionService : IValidacionService
    {
        private readonly HttpClient _httpClient;
        private readonly string _usr5ApiUrl;

        public ValidacionService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _usr5ApiUrl = configuration["USR5ApiUrl"] ?? "http://localhost:5233";
        }

        public async Task<bool> ValidarTokenAsync(string token)
        {
            try
            {
                var request = new { token };
                var response = await _httpClient.PostAsJsonAsync($"{_usr5ApiUrl}/validate", request);

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var resultado = await response.Content.ReadFromJsonAsync<bool>();
                    return resultado;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }
    }
}