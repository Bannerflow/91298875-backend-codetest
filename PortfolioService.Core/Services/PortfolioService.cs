using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeTest.Infrastructure.Models;
using CodeTest.Infrastructure.Persistence;
using MongoDB.Bson;
using PortfolioService.Core.Interfaces;
using PortfolioService.Infrastructure.Persistence;
using StockService;

namespace PortfolioService.Core.Services
{
    public class PortfolioService : IPortfolioService
    {


        private readonly IDataService _dataService;
        private readonly IStockService _stockService;
        private readonly ICurrencyLayerService _currencyLayerService;

        public PortfolioService(IDataService dataService, IStockService stockService, ICurrencyLayerService currencyLayerService)
        {
            _dataService = dataService;
            _stockService = stockService;
            _currencyLayerService = currencyLayerService;
        }


        public async Task<PortfolioData> GetPortfolio(string id)
        {
            var portfolio = await _dataService.GetPortfolio(ObjectId.Parse(id));
            return portfolio;
        }

        public async Task<decimal> GetTotalPortfolioValue(string portfolioId, string currency = "USD")
        {
            var portfolio = await _dataService.GetPortfolio(ObjectId.Parse(portfolioId));
            var totalAmount = 0m;
            var quote = await _currencyLayerService.GetLiveCurrencyRates();

            foreach (var stock in portfolio.Stocks)
            {

                //base usd and current sek
                if (stock.BaseCurrency == currency)
                {
                    totalAmount += _stockService.GetCurrentStockPrice(stock.Ticker).Result.Price *
                                   stock.NumberOfShares;
                }
                else
                {

                    if (currency == "USD")
                    {
                        var stockPrice = _stockService.GetCurrentStockPrice(stock.Ticker).Result.Price;
                        var rateUsd = quote.quotes["USD" + stock.BaseCurrency];
                        totalAmount += stockPrice / rateUsd * stock.NumberOfShares;
                    }
                    else
                    {
                        decimal stockValueInUsd;
                        var stockPrice = _stockService.GetCurrentStockPrice(stock.Ticker).Result.Price;

                        // Case 1: Stock is already in USD
                        if (stock.BaseCurrency == "USD")
                        {
                            stockValueInUsd = stockPrice * stock.NumberOfShares;

                        }
                        // Case 2: Stock's Base Currency is not USD, convert to USD first
                        else
                        {
                            var rateToUsd = quote.quotes["USD" + stock.BaseCurrency];
                            stockValueInUsd = (stockPrice / rateToUsd) * stock.NumberOfShares;
                        }


                        // Case 1: Stock is already in USD
                        if (currency == "USD")
                        {
                            totalAmount += stockValueInUsd;
                        }
                        // Case 2: Convert the USD value to the target currency
                        else
                        {
                            var targetRateUsd = quote.quotes["USD" + currency];
                            totalAmount += stockValueInUsd * targetRateUsd;
                        }
                    }
                }
            }

            return totalAmount;
        }

        public async Task DeletePortfolio(string portfolioId)
        {
            await _dataService.DeletePortfolio(ObjectId.Parse(portfolioId));
        }
    }
}
