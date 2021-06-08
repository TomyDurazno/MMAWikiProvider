using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using UFCWikiProvider.Models;

namespace UFCWikiProvider.Logic
{
    public class FighterStoreInitConsumer : BackgroundService
    {
        IFighterStoreHandler handler;
        string wikiJsonPath = $"{AppContext.BaseDirectory.Split("bin")[0]}ufcwiki.json";
        ILogger<FighterStoreInitConsumer> logger;

        public FighterStoreInitConsumer(IFighterStoreHandler handler, ILogger<FighterStoreInitConsumer> logger)
        {
            this.handler = handler;
            this.logger = logger;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                var values = JsonSerializer.Deserialize<List<Fighter>>(File.ReadAllText(wikiJsonPath));
                handler.Init(new ConcurrentDictionary<string, Fighter>(values.ToDictionary(v => v.Name, v => v)));
            }
            catch (Exception ex)
            {
                logger.LogError("Unable to load wiki list", ex);
            }

            return Task.CompletedTask;
        }
    }
}
