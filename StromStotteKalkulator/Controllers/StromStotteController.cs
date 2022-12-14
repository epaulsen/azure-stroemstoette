using Microsoft.AspNetCore.Mvc;
using StromStotteKalkulator.Data;

namespace StromStotteKalkulator.Controllers
{
    [Route("api/stotte")]
    public class StromStotteController : ControllerBase
    {
        private readonly DataController _dataController;
        private readonly IDataRetreiver _dataRetreiver;

        private static readonly Dictionary<string, string> _areaTranslation = new Dictionary<string, string>
        {
            {"NO1","Oslo"},
            {"NO2","Kr.sand"},
            {"NO3","Tr.heim"},
            {"NO4","Tromsø"},
            {"NO5","Bergen"},
        };

        public StromStotteController(DataController dataController, IDataRetreiver retreiver)
        {
            _dataController = dataController;
            _dataRetreiver = retreiver;
        }

        [HttpGet("{area}")]
        public async Task<IActionResult> Get(string area)
        {
            var stotte = await Calculate(area).ConfigureAwait(false);
            if (stotte == null)
            {
                return NotFound();
            }

            return new OkObjectResult(stotte);
        }

        private async Task<StromStotte?> Calculate(string area)
        {
            string stromArea = _areaTranslation.ContainsKey(area) ? _areaTranslation[area] : area;

            var data = await _dataController.GetDataForArea(_dataRetreiver, stromArea).ConfigureAwait(false);
            var result = data.ToList();
            if (result.Count == 0)
            {
                return null;
            }

            var stotte = CalculateStotteFromAverage(result
                .Where(d => d.Date.Month == DateTime.Now.Month)
                .Average(p => p.Price));

            return new StromStotte()
            {
                Value = stotte
            };
        }

        private static double CalculateStotteFromAverage(double value)
        {
            value -= 0.7; // Goverment cap
            value *= 0.9; // 90 percent of everything about 70 cents
            if (value <= 0 ) { return 0; }

            return value;
        }
    }

    public class StromStotte
    {
        public double Value { get; set; }

        public double ValueWithVAT => Value * 1.25;

    }
}
