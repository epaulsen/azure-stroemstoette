using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using StromStotteKalkulator.Controllers;
using StromStotteKalkulator.Data;
using StromStotteKalkulator.Data.Model;

namespace StromStotteKalkulator.Test
{
    public class BasicTests
    {
        [Fact]
        public void DataController_Insert()
        {
            var root = JsonSerializer.Deserialize<Root>(File.ReadAllText("TestData/TestData.json"));
            var controller = new DataController();
            controller.Update(root);
        }

        public static IEnumerable<object[]> TestData
        {
            get
            {
                yield return new[] {"NO1"};
                yield return new[] {"NO2"};
                yield return new[] {"NO3"};
                yield return new[] {"NO4"};
                yield return new[] {"NO5"};
                yield return new[] {"Oslo"};
                yield return new[] {"Kr.sand"};
                yield return new[] {"Tr.heim"};
                yield return new[] {"Bergen"};
                yield return new[] {"Tromsø"};
            }
        }

        [Fact]
        public async Task GetAll_Works()
        {
            var root = JsonSerializer.Deserialize<Root>(File.ReadAllText("TestData/TestData.json"));
            var dataController = new DataController();

            var retreiverMock = new Mock<IDataRetreiver>();
            retreiverMock.Setup(m => m.FetchPricesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(root);

            var webController = new StromStotteController(dataController, retreiverMock.Object);

            var result = await webController.GetAll();
            //result?.Value.Should().NotBeNull();

            //var ss = (result.Value as IEnumerable<StromStotte>).ToArray();
        }

        [Theory]
        [MemberData(nameof(TestData))]
        public async Task PriceController_Works(string area)
        {
            var root = JsonSerializer.Deserialize<Root>(File.ReadAllText("TestData/TestData.json"));
            var dataController = new DataController();
            
            var retreiverMock = new Mock<IDataRetreiver>();
            retreiverMock.Setup(m => m.FetchPricesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(root);
            
            var webController = new StromStotteController(dataController, retreiverMock.Object);

            var result = (await webController.Get(area)) as OkObjectResult;
            result?.Value.Should().NotBeNull();

            var ss = result.Value as StromStotte;
            ss.Value.Should().BeGreaterOrEqualTo(0);
            ss.ValueWithVAT.Should().BeGreaterOrEqualTo(ss.Value);
        }
    }
}