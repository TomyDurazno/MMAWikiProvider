using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using MMAWikiProvider.Logic;
using MMAWikiProvider.Models;

namespace MMAWikiProvider.Logic
{
    public interface IFighterStats
    {
        public int BuchholzRatio(string name);

        public Task<IEnumerable<Fighter>> TopBy<T>(int number, Func<Fighter, T> By);

        public Task<IDictionary<string, object>> TopByDic<T>(int number, Func<Fighter, T> By);

        public Task<IDictionary<string, object>> TopByDic<T, K>(int number, Func<Fighter, T> By, Func<Fighter, K> Proj);

        public StatsDTO Stats(string name);
    }

    public class FighterStats : IFighterStats
    {
        IFighterStoreHandler fighterStore;
        public FighterStats(IFighterStoreHandler fighterStore)
        {
            this.fighterStore = fighterStore;
        }

        public int BuchholzRatio(string name)
        {
            var fighter = fighterStore.GetFighterWithOpponents(name);

            return fighter.Record.Sum(a => a.Opponent.Record.WinLoseRatio());
        }

        public StatsDTO Stats(string name)
        {
            var fighter = fighterStore.GetFighterWithOpponents(name);
            return new StatsDTO()
            {
                Name = fighter.Name,
                Fights = fighter.Record.Count,
                UFCFights = fighter.Record.UFCFights(),
                TotalTime = TimeSpan.FromSeconds(fighter.Record.Select(r => r.ElapsedTime?.TotalSeconds ?? 0).Sum()),
                AverageTime = TimeSpan.FromSeconds(fighter.Record.Select(r => r.ElapsedTime?.TotalSeconds ?? 0).Average()),
                Wins = fighter.Record.Wins(),
                Losses = fighter.Record.Losses(),
                Draws = fighter.Record.Draws(),
                Submissions = fighter.Record.Submissions(),
                KOTKO = fighter.Record.KOTKO(),
                NoContest = fighter.Record.NoContest(),
                WonOrDefendedUFCChampionship = fighter.Record.WonOrDefendedUFCChampionship(),
                WinLoseRatio = fighter.Record.WinLoseRatio(),
            };
        }

        #region TopBy

        public async Task<IEnumerable<Fighter>> TopBy<T>(int number, Func<Fighter, T> By)
        {
            var state = fighterStore.ConcurrentState;

            async Task Spin(Fighter f)
            {
                await Task.WhenAll(f.Record.Select(r =>
                {
                    if (state.TryGetValue(r.Opponent.Name, out var opp))
                        r.Opponent = opp;
                    return Task.CompletedTask;
                }));
            }

            var ufcFighters = state.Where(f => f.Value.Record.Any(r => r.Event.IsUFCEvent))
                                   .ToList();

            await Task.WhenAll(ufcFighters.Select(f => Spin(f.Value)));

            return ufcFighters.Select(k => k.Value)
                              .OrderByDescending(By)
                              .Take(number);
        }

        public async Task<IDictionary<string, object>> TopByDic<T>(int number, Func<Fighter, T> By)
        {
            var top = await TopBy(number, By);

            var exp = new ExpandoObject() as IDictionary<string, object>;

            foreach (var e in top)
                exp.Add(e.Name, By(e));

            return exp;
        }

        public async Task<IDictionary<string, object>> TopByDic<T, K>(int number, Func<Fighter, T> By, Func<Fighter, K> Proj)
        {
            var top = await TopBy(number, By);

            var exp = new ExpandoObject() as IDictionary<string, object>;

            foreach(var e in top)
                exp.Add(e.Name, Proj(e));

            return exp;
        }

        #endregion
    }
}
