using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using PortfolioService.Core.Interfaces;
using PortfolioService.Core.Models;
using Microsoft.Extensions.Caching.Memory;
using PortfolioService.Core.Dtos;
using Microsoft.Extensions.Options;

namespace PortfolioService.Core.Helper
{
    public class CurrencyLayerService : ICurrencyLayerService
    {

        private readonly CurrencyLayerOptions _options;
        private readonly IMemoryCache _cache;
        private readonly HttpClient _httpClient;

        public CurrencyLayerService(HttpClient httpClient, IMemoryCache cache, IOptions<CurrencyLayerOptions> options)
        {
            _httpClient = httpClient;
            _cache = cache;
            _options = options.Value;
        }


        public async Task<QuoteDto> GetLiveCurrencyRates()
        {

            // Check if the cached data exists
            if (_cache.TryGetValue(_options.CacheKey, out QuoteDto cachedQuote))
            {
                return cachedQuote;
            }

            // See https://currencylayer.com/documentation for details about the api
            var apiResponse = await _httpClient.GetAsync($"live?access_key={_options.AccessKey}");
            var apiResponseDes = await apiResponse.Content.ReadAsStreamAsync();
            QuoteDto quote = await JsonSerializer.DeserializeAsync<QuoteDto>(apiResponseDes);


            // Cache the result with an expiration time of 24 hours
            var cacheOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromHours(24));

            _cache.Set(_options.CacheKey, quote, cacheOptions);

            return quote;
        }
    }
}
