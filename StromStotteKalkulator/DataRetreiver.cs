using StromStotteKalkulator.Controllers;
using StromStotteKalkulator.Data.Model;

namespace StromStotteKalkulator
{
    public interface IDataRetreiver
    {
        Task<Root?> FetchPricesAsync(CancellationToken cancellationToken);
    }

    public class DataRetreiver : IDataRetreiver
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<DataRetreiver> _logger;

        private const string NordPoolUrl = "https://www.nordpoolgroup.com/api/marketdata/page/24?currency=,,,NOK";

        public DataRetreiver(HttpClient httpClient, ILogger<DataRetreiver> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<Root?> FetchPricesAsync(CancellationToken cancellationToken)
        {
            try
            {
                var result = await _httpClient.GetFromJsonAsync<Root>(NordPoolUrl, cancellationToken: cancellationToken);
                return result;
            }
            catch (Exception e)
            {
                _logger.LogError(e,"Failed to fetch data from Nordpool");
                throw;
            }
            
        }
    }
}
