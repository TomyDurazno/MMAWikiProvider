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

        public Task<IEnumerable<Fighter>> TopBy(int number, Func<Fighter, int> By);

        public Task<IDictionary<string, object>> TopByDic(int number, Func<Fighter, int> By);

        public Task<IDictionary<string, object>> TopByDic<T>(int number, Func<Fighter, int> By, Func<Fighter, T> Proj);
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

        #region TopBy

        public async Task<IEnumerable<Fighter>> TopBy(int number, Func<Fighter, int> By)
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

        public async Task<IDictionary<string, object>> TopByDic(int number, Func<Fighter, int> By)
        {
            var top = await TopBy(number, By);

            var exp = new ExpandoObject() as IDictionary<string, object>;

            foreach (var e in top)
                exp.Add(e.Name, By(e));

            return exp;
        }

        public async Task<IDictionary<string, object>> TopByDic<T>(int number, Func<Fighter, int> By, Func<Fighter, T> Proj)
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
