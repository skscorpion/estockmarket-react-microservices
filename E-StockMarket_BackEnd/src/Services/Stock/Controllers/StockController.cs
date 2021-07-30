﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Stock.API.Models;
using Stock.API.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Stock.API.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class StockController : ControllerBase
    {
        private readonly ICosmosDbService _cosmosDbService;
        private readonly ILogger<StockController> _logger;

        public StockController(ILogger<StockController> logger, ICosmosDbService cosmosDbService)
        {
            _logger = logger;
            _cosmosDbService = cosmosDbService;
        }

        [HttpGet]
        [Route("[action]/{code}", Name = "GetStocksByCompanyCode")]
        public async Task<IActionResult> Get(string code)
        {
            _logger.LogInformation("return all stocks related to {code}", code);
            return Ok(await _cosmosDbService.GetItemsAsync($"SELECT * FROM c WHERE c.code = '{code}' AND c.isActive=true"));
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _cosmosDbService.GetItemsAsync("SELECT * FROM c WHERE c.isActive=true"));
        }

        [HttpPost]
        public async Task<ActionResult> CreateAsync(StockDetails item)
        {
            if (ModelState.IsValid)
            {
                item.ID = Guid.NewGuid().ToString();
                await _cosmosDbService.AddItemAsync(item);
                return Ok(item);
            }

            return Ok(item);
        }

        [HttpDelete]
        public async Task<ActionResult> RemoveAsync(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                return BadRequest(code);

            var stocks = await _cosmosDbService.GetItemsAsync($"SELECT * FROM c WHERE c.code = '{code}' AND c.isActive=true");

            foreach(StockDetails stock in stocks)
            {
                stock.IsActive = false;
                await _cosmosDbService.UpdateItemAsync(stock.ID, stock);
            }
            return Ok();
        }
    }
}
