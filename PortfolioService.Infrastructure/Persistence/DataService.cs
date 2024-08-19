using System.Threading.Tasks;
using CodeTest.Infrastructure.Models;
using Mongo2Go;
using MongoDB.Bson;
using MongoDB.Driver;
using PortfolioService.Infrastructure.Persistence;

namespace CodeTest.Infrastructure.Persistence
{


    public class DataService : IDataService
    {
        private readonly IMongoCollection<PortfolioData> _portfolioCollection;
        private readonly MongoDbRunner _runner;

        // Inject MongoDbRunner as a dependency, making it easier to test and control.
        public DataService(IMongoClient mongoClient, MongoDbRunner runner)
        {
            _runner = runner;
            _portfolioCollection = mongoClient.GetDatabase("portfolioServiceDb").GetCollection<PortfolioData>("Portfolios");
        }

        public async Task<PortfolioData> GetPortfolio(ObjectId id, bool includeSoftDelete = false)
        {
            var idFilter = Builders<PortfolioData>.Filter.Eq(portfolio => portfolio.Id, id);
            var isDeletedFilter = Builders<PortfolioData>.Filter.Eq(portfolio => portfolio.IsDeleted, includeSoftDelete ? true : false);
            var filter = Builders<PortfolioData>.Filter.And(idFilter, isDeletedFilter);

            var entity = await _portfolioCollection.Find(filter).FirstOrDefaultAsync(); ;
            return entity;
        }

        public async Task DeletePortfolio(ObjectId id, bool softDelete = true)
        {
            if (!softDelete)
            {
                await _portfolioCollection.DeleteOneAsync(Builders<PortfolioData>.Filter.Eq(portfolio => portfolio.Id, id));
            }
            else
            {
                var softDeleteFilter = Builders<PortfolioData>.Update.Set(portfolio => portfolio.IsDeleted, true);
                await _portfolioCollection.UpdateOneAsync(Builders<PortfolioData>.Filter.Eq(portfolio => portfolio.Id, id), softDeleteFilter);
            }


        }

    }
}