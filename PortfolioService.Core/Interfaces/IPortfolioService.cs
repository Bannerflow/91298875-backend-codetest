using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeTest.Infrastructure.Models;

namespace PortfolioService.Core.Interfaces
{
    public interface IPortfolioService
    {
        Task<PortfolioData> GetPortfolio(string id);
        Task<decimal> GetTotalPortfolioValue(string portfolioId, string currency = "USD");
        Task DeletePortfolio(string portfolioId);

    }
}
