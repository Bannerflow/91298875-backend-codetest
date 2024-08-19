using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PortfolioService.Core.Models;

namespace PortfolioService.Core.Interfaces
{
    public interface ICurrencyLayerService
    {
        Task<QuoteDto> GetLiveCurrencyRates();
    }
}
