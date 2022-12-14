using System.Globalization;
using StromStotteKalkulator.Data.Model;

namespace StromStotteKalkulator.Data
{
    public class PriceData
    {
        public DateOnly Date { get; set; }

        public string Area { get; set; }

        public double Price { get; set; }
    }

    public class DataController
    {
        private readonly List<PriceData> _data = new();

        private DateTime _lastUpdated = DateTime.UtcNow;

        private static readonly SemaphoreSlim _semaphore = new(1, 1);

        public void Update(Root data)
        {
            var month = DateTime.Now.Month;

            List<PriceData> priceList = new List<PriceData>();

            foreach (var dataRow in data.data.Rows.Where(dr => dr.StartTime.Month == month))
            {
                priceList.AddRange(dataRow.Columns
                    .Select(c =>
                    {
                        var priceData = new PriceData()
                        {
                            Area = c.Name.Trim(),
                            Price = ConvertMWhTokWh(c.Value),
                            Date = DateOnly.FromDateTime(dataRow.StartTime)
                        };
                        return priceData;
                    }));
            }

            _data.Clear();
            _data.AddRange(priceList);
        }

        private static CultureInfo scandinavian = CultureInfo.GetCultureInfo("nb-NO");

        private static double ConvertMWhTokWh(string value)
        {
            var result = double.TryParse(value, scandinavian, out var dbl) ? dbl : 0;
            return result / 1000;
        }

        public async Task<IEnumerable<PriceData>> GetDataForArea(IDataRetreiver retreiver, string area)
        {
            if (_lastUpdated.AddHours(1) < DateTime.UtcNow || _data.Count == 0)
            {
                try
                {
                    await _semaphore.WaitAsync(TimeSpan.FromSeconds(10));
                    if (_lastUpdated.AddHours(1) < DateTime.UtcNow || _data.Count == 0)
                    {
                        var freshData = await retreiver.FetchPricesAsync(CancellationToken.None);
                        if (freshData != null)
                        {
                            Update(freshData);
                        }
                    }
                }
                finally
                {
                    _semaphore.Release();
                }
            }

            try
            {
                await _semaphore.WaitAsync(TimeSpan.FromSeconds(1));
                return _data.Where(pd => area.Equals(pd.Area, StringComparison.InvariantCultureIgnoreCase));
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }
}
