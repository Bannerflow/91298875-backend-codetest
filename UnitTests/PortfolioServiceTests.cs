using Moq;
using Xunit;
using System.Threading.Tasks;
using System.Collections.Generic;
using CodeTest.Infrastructure.Models;
using CodeTest.Infrastructure.Persistence;
using MongoDB.Bson;
using PortfolioService.Core.Interfaces;
using PortfolioService.Core.Models;
using StockService;
using PortfolioService.Infrastructure.Persistence;

namespace UnitTests.PortfolioServiceTests;

public class PortfolioServiceTests
{
    private readonly Mock<IDataService> _mockDataService;
    private readonly Mock<IStockService> _mockStockService;
    private readonly Mock<ICurrencyLayerService> _mockCurrencyLayerService;
    private readonly PortfolioService.Core.Services.PortfolioService _portfolioService;

    public PortfolioServiceTests()
    {
        _mockDataService = new Mock<IDataService>();
        _mockStockService = new Mock<IStockService>();
        _mockCurrencyLayerService = new Mock<ICurrencyLayerService>();

        _portfolioService = new PortfolioService.Core.Services.PortfolioService(
            _mockDataService.Object,
            _mockStockService.Object,
            _mockCurrencyLayerService.Object);
    }

    [Fact]
    public async Task GetPortfolio_ShouldReturnPortfolio_WhenIdIsValid()
    {
        // Arrange
        var portfolioId = "64a54f805faed27e5a029f1a";
        var portfolio = new PortfolioData { Id = ObjectId.Parse(portfolioId) };
        _mockDataService.Setup(ds => ds.GetPortfolio(It.IsAny<ObjectId>(), It.IsAny<bool>()))
            .ReturnsAsync(portfolio);

        // Act
        var result = await _portfolioService.GetPortfolio(portfolioId);

        // Assert
        Assert.Equal(ObjectId.Parse(portfolioId), result.Id);
    }

    [Fact]
    public async Task GetTotalPortfolioValue_ShouldReturnTotalValue_ForSameCurrency()
    {
        // Arrange
        var portfolioId = "64a54f805faed27e5a029f1a";
        var portfolio = new PortfolioData
        {
            Stocks = new List<StockData>
            {
                new StockData { Ticker = "AAPL", NumberOfShares = 10, BaseCurrency = "USD" }
            }
        };

        _mockDataService.Setup(ds => ds.GetPortfolio(It.IsAny<ObjectId>(), It.IsAny<bool>()))
            .ReturnsAsync(portfolio);
        _mockStockService.Setup(ss => ss.GetCurrentStockPrice("AAPL"))
            .ReturnsAsync((150.0m, "USD"));
        _mockCurrencyLayerService.Setup(cl => cl.GetLiveCurrencyRates())
            .ReturnsAsync(new QuoteDto { quotes = new Dictionary<string, decimal> { { "USDUSD", 1.0m } } });

        // Act
        var result = await _portfolioService.GetTotalPortfolioValue(portfolioId, "USD");

        // Assert
        Assert.Equal(1500.0m, result);
    }

    [Fact]
    public async Task GetTotalPortfolioValue_ShouldConvertCurrency_ForDifferentCurrency()
    {
        // Arrange
        var portfolioId = "64a54f805faed27e5a029f1a";
        var portfolio = new PortfolioData
        {
            Stocks = new List<StockData>
            {
                new StockData { Ticker = "AAPL", NumberOfShares = 10, BaseCurrency = "EUR" }
            }
        };

        _mockDataService.Setup(ds => ds.GetPortfolio(It.IsAny<ObjectId>(), It.IsAny<bool>()))
            .ReturnsAsync(portfolio);
        _mockStockService.Setup(ss => ss.GetCurrentStockPrice("AAPL"))
            .ReturnsAsync((150.0m, "USD"));
        _mockCurrencyLayerService.Setup(cl => cl.GetLiveCurrencyRates())
            .ReturnsAsync(new QuoteDto
            {
                quotes = new Dictionary<string, decimal>
                {
                    { "USDEUR", 0.85m }, // Exchange rate: 1 USD = 0.85 EUR
                    { "USDUSD", 1.0m }
                }
            });

        // Act
        var result = await _portfolioService.GetTotalPortfolioValue(portfolioId, "USD");

        // Assert
        Assert.Equal(1764.71m, result, 2); // Assert with precision of 2 decimal points
    }

    [Fact]
    public async Task DeletePortfolio_ShouldCallDataServiceDelete()
    {
        // Arrange
        var portfolioId = "64a54f805faed27e5a029f1a";
        var expectedObjectId = ObjectId.Parse(portfolioId); // Parse portfolioId into ObjectId

        // Setup the mock to expect a call to DeletePortfolio with the specific ObjectId
        _mockDataService.Setup(ds => ds.DeletePortfolio(It.IsAny<ObjectId>(), It.IsAny<bool>()))
            .Returns(Task.CompletedTask);

        // Act
        await _portfolioService.DeletePortfolio(portfolioId);

        // Assert
        _mockDataService.Verify(ds => ds.DeletePortfolio(It.Is<ObjectId>(id => id == expectedObjectId), It.IsAny<bool>()), Times.Once);
    }
}
