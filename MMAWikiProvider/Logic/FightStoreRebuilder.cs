using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Diagnostics;
using MMAWikiProvider.Models;
using MMAWikiProvider.Extensions;
using Microsoft.Extensions.Configuration;

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
        bool rebuildOnRestart = false;
        bool rebuildFlag = false;
        int? IOBatchingSize;

        ILogger<FighterStoreInitConsumer> logger;
        IRunsState runsState;

        public FightStoreRebuilder(IFighterStoreHandler handler, ILogger<FighterStoreInitConsumer> logger, IFighterProvider provider, IConfiguration configuration, IRunsState runsState)
        {
            this.handler = handler;
            this.logger = logger;
            this.fighterProvider = provider;
            this.runsState = runsState;
            rebuildOnRestart = bool.TryParse(configuration["RebuildStoreOnStart"], out var output) ? output : false;
            IOBatchingSize = int.TryParse(configuration["IOBatchingSize"], out var io) ? io : default;
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
                    if (rebuildOnRestart && !rebuildFlag)
                    {
                        logger.LogInformation($"setting RebuildStoreOnStart is set to {rebuildOnRestart}, rebuilding store");
                        DeleteDirectory(baseFolder);
                        rebuildFlag = true;
                    }

                    if (!Directory.Exists(baseFolder))
                        Directory.CreateDirectory(baseFolder);

                    var runs = Deserialize<List<Runs>>(runsPath);

                    runsState.SetRuns(runs);

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
                                logger.LogInformation(Serialize(new { Added = opp.Name }));
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
                                logger.LogInformation(Serialize(new { Added = fighter.Name }));

                                await AddRecordToDic(fighter);
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex);
                        }
                    }

                    #endregion

                    var snames = names.Except(listed)
                                      .Except(failedWikiBag)
                                      .Except(nameDesambiguation);

                    var snamesCount = snames.Count();

                    var stack = new Stack<string>(snames);
                    
                    if(snamesCount == 0)
                    {
                        logger.LogInformation($"List size: {snamesCount}");
                        //Break main loop
                        break;
                    }

                    while(stack.Any())
                    {
                        var watch = Stopwatch.StartNew();

                        var round = IOBatchingSize.HasValue ? stack.PopRange(IOBatchingSize.Value) : stack.PopRange(snamesCount);

                        await Task.WhenAll(round.Select(s => Spin(s)));

                        handler.Init(fighters);

                        var list = fighters.Select(f => f.Value)
                                           .ToList();

                        var missing = list.SelectMany(o => o.Record.Select(r => r.Opponent.Name))
                                          .Except(listed)
                                          .Except(failedWikiBag)
                                          .Except(nameDesambiguation);

                        var auxFailedWikiList = failedWikiBag.ToList();

                        var run = new Runs {
                            InList = list.Count(),
                            Missing = missing.Count(),
                            FailedWiki = auxFailedWikiList.Count(),
                            ElapsedSeconds = watch.Elapsed.TotalSeconds
                        };

                        runs.Add(run);

                        runsState.SetRuns(runs);

                        logger.LogInformation(Serialize(run));

                        //Serialize
                        SerializeToFile(runsPath, runs);

                        SerializeToFile(missingPath, missing);

                        SerializeToFile(wikiJsonPath, list);

                        SerializeToFile(failedwikifetchPath, auxFailedWikiList);

                        if (run.Missing == 0)
                            break;
                    }

                    await Task.Delay(TimeSpan.FromSeconds(1));
                }
                catch (Exception ex)
                {
                    logger.LogError("Unable to load wiki list", ex);
                }
            }
        }

        string Serialize<T>(T value)
        {
            var jso = new JsonSerializerOptions()
            {
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };
 
            return JsonSerializer.Serialize(value, jso);
        }

        void SerializeToFile<T>(string path, T value)
        {
            File.WriteAllText(path, JsonSerializer.Serialize(value, new JsonSerializerOptions()
            {
                WriteIndented = true,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            }));
        }

        T Deserialize<T>(string path) where T : new()
        {
            if (!File.Exists(path))
                return new T();

            return JsonSerializer.Deserialize<T>(File.ReadAllText(path));
        }

        void DeleteDirectory(string target_dir)
        {
            if(!Directory.Exists(target_dir))
            {
                return;
            }

            var files = Directory.GetFiles(target_dir);
            var dirs = Directory.GetDirectories(target_dir);

            foreach (string file in files)
            {
                File.SetAttributes(file, FileAttributes.Normal);
                File.Delete(file);
            }

            foreach (string dir in dirs)
            {
                DeleteDirectory(dir);
            }

            Directory.Delete(target_dir, false);
        }
    }
}
