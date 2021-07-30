using EventBus.Messages.Events;
using MassTransit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Stock.API.Services;
using Stock.API.Models;

namespace Stock.API.EventBusConsumer
{
    public class CompanyDeletedConsumer : IConsumer<CompanyDeleteEvent>
    {
        private readonly ILogger<CompanyDeletedConsumer> _logger;
        private readonly ICosmosDbService _cosmosDbService;

        public CompanyDeletedConsumer(ILogger<CompanyDeletedConsumer> logger, ICosmosDbService cosmosDbService)
        {
            _logger = logger;
            _cosmosDbService = cosmosDbService;
        }

        public async Task Consume(ConsumeContext<CompanyDeleteEvent> context)
        {
            var msg = context.Message;
            var stocks = await _cosmosDbService.GetItemsAsync($"SELECT * FROM c WHERE c.code = '{msg.Code}' AND c.isActive=true");

            foreach (StockDetails stock in stocks)
            {
                stock.IsActive = false;
                await _cosmosDbService.UpdateItemAsync(stock.ID, stock);
            }
            _logger.LogInformation("Stocks deleted");
        }
    }
}
