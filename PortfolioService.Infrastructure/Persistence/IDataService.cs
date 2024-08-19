using CodeTest.Infrastructure.Models;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortfolioService.Infrastructure.Persistence
{
    public interface IDataService
    {
        Task<PortfolioData> GetPortfolio(ObjectId id, bool includeSoftDelete = false);
        Task DeletePortfolio(ObjectId id, bool softDelete = true);
    }

}
