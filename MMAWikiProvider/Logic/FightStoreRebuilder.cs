using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Diagnostics;
using MMAWikiProvider.Models;
using MMAWikiProvider.Extensions;

namespace MMAWikiProvider.Logic
{
    public class FightStoreRebuilder : BackgroundService
    {
        IFighterStoreHandler handler;
        IFighterProvider fighterProvider;
        static string baseFolder = $"{AppContext.BaseDirectory.Split("bin")[0]}TEST";

        string wikiJsonPath = $"{baseFolder}/ufcwiki.json";
        string missingPath = $"{baseFolder}/missing.json";
        string failedwikifetchPath = $"{baseFolder}/failedwikifetch.json";
        string runsPath = $"{baseFolder}/runs.json";
       
        string starterPath = "MMAWikiProvider.Resources.starter.json";
        ILogger<FighterStoreInitConsumer> logger;
        public FightStoreRebuilder(IFighterStoreHandler handler, ILogger<FighterStoreInitConsumer> logger, IFighterProvider provider)
        {
            this.handler = handler;
            this.logger = logger;
            this.fighterProvider = provider;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            logger.LogInformation($"Path: {wikiJsonPath}");

            var task = Task.Run(() => Rebuild(stoppingToken));

            return task;
        }

        async Task Rebuild(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var watch = new Stopwatch();

                    watch.Start();

                    if (!Directory.Exists(baseFolder))
                        Directory.CreateDirectory(baseFolder);

                    var backList = Deserialize<List<Fighter>>(wikiJsonPath);

                    var backListDic = backList.ToDictionary(f => f.Name, f => f);

                    handler.Init(backListDic);

                    var listed = backList.Select(l => l.Name).Distinct();

                    var opponents = backList.SelectMany(l => l.Record.Select(r => r.Opponent.Name)).Distinct();

                    var fighters = new ConcurrentDictionary<string, Fighter>(backListDic);

                    #region AddRecordToDic

                    async Task AddRecordToDic(Fighter fighter)
                    {
                        async Task Spin(RecordRow row)
                        {
                            if (fighters.ContainsKey(row.Opponent.Name) || fighters.ContainsKey(row.Opponent.Name + "_(fighter)"))
                                return;

                            var opp = await fighterProvider.Get(row.Opponent.Name);

                            if (opp == null)
                                return;

                            if (fighters.TryAdd(opp.Name, opp))
                            {
                                logger.LogInformation(JsonSerializer.Serialize(new { Added = opp.Name }));
                            }
                        }

                        await Task.WhenAll(fighter.Record.Select(r => Spin(r)));
                    }

                    #endregion

                    var names = File.Exists(missingPath) ? Deserialize<List<string>>(missingPath).Distinct() : 
                                                           JsonSerializer.Deserialize<List<string>>(EmbeddedResource.GetResourceFileAsString(starterPath));

                    var failedWikiBag = new ConcurrentBag<string>(Deserialize<List<string>>(failedwikifetchPath));

                    var nameDesambiguation = listed.Where(l => l.Contains("_(fighter)"))
                                                   .Select(n => n.Replace("_(fighter)", string.Empty));

                    var snames = names.Except(listed)
                                      .Except(failedWikiBag)
                                      .Except(nameDesambiguation);

                    var snamesCount = snames.Count();

                    #region Spin

                    async Task Spin(string fname)
                    {
                        try
                        {
                            if (fighters.ContainsKey(fname) || fighters.ContainsKey(fname + "_(fighter)"))
                                return;

                            //fname.Dump($"{++cont}/{snamesCount}");

                            var fighter = await fighterProvider.Get(fname);

                            if (fighter == null)
                            {
                                //Failed WikiFetch	
                                failedWikiBag.Add(fname);
                                return;
                            }

                            if (fighters.TryAdd(fighter.Name, fighter))
                            {
                                logger.LogInformation(JsonSerializer.Serialize(new { Added = fighter.Name }));

                                await AddRecordToDic(fighter);
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex);
                        }
                    }

                    #endregion

                    await Task.WhenAll(snames.Select(s => Spin(s)));

                    handler.Init(fighters);

                    var list = fighters.Select(f => f.Value)
                                       .ToList();

                    var missing = list.SelectMany(o => o.Record.Select(r => r.Opponent.Name))
                                      .Except(listed)
                                      .Except(failedWikiBag)
                                      .Except(nameDesambiguation);

                    var auxFailedWikiList = failedWikiBag.ToList();

                    var runs = Deserialize<List<Runs>>(runsPath);

                    var run = new Runs {
                        InList = list.Count(),
                        Missing = missing.Count(),
                        FailedWiki = auxFailedWikiList.Count(),
                        ElapsedSeconds = watch.Elapsed.TotalSeconds
                    };

                    runs.Add(run);

                    logger.LogInformation(JsonSerializer.Serialize(run));

                    //Serialize
                    Serialize(runsPath, runs);

                    Serialize(missingPath, missing);

                    Serialize(wikiJsonPath, list);

                    Serialize(failedwikifetchPath, auxFailedWikiList);

                    if (run.Missing == 0)
                        break;

                    await Task.Delay(TimeSpan.FromSeconds(1));
                }
                catch (Exception ex)
                {
                    logger.LogError("Unable to load wiki list", ex);
                }
            }
        }

        class Runs
        {
            public int InList { get; set; }
            public int Missing { get; set; }
            public int FailedWiki { get; set; }
            public double ElapsedSeconds { get; set; }
        }

        void Serialize<T>(string path, T value)
        {
            File.WriteAllText(path, JsonSerializer.Serialize(value, new JsonSerializerOptions()
            {
                WriteIndented = true
            }));
        }

        T Deserialize<T>(string path) where T : new()
        {
            if (!File.Exists(path))
                return new T();

            return JsonSerializer.Deserialize<T>(File.ReadAllText(path));
        }
    }
}
