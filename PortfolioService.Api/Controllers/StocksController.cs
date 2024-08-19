using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using CodeTest.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using System.Text.Json;
using System.Threading.Tasks;
using PortfolioService.Core.Interfaces;
using PortfolioService.Core.Models;
using StockService;

namespace CodeTest.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PortfolioController : ControllerBase
    {

        private readonly IPortfolioService _portfolioService;

        public PortfolioController(IPortfolioService portfolioService)
        {
            _portfolioService = portfolioService;
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> Get(string id)
        {
            var portfolio = await _portfolioService.GetPortfolio(id);
            return Ok(portfolio);
        }

        [HttpGet("/value")]
        public async Task<IActionResult> GetTotalPortfolioValue(string portfolioId, string currency = "USD")
        {

            var totalAmount = await _portfolioService.GetTotalPortfolioValue(portfolioId, currency);
            return Ok(totalAmount);
        }

        [HttpDelete("/delete")]
        public async Task<IActionResult> DeletePortfolio(string portfolioId)
        {
            await _portfolioService.DeletePortfolio(portfolioId);
            return Ok();
        }
    }
}