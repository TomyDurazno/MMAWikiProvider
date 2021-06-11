using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using MMAWikiProvider.Models;

namespace MMAWikiProvider.Logic
{
    public interface IFighterStoreHandler
    {
        public void Init(IDictionary<string, Fighter> fighters);

        public Fighter GetFighter(string name);

        public Fighter GetFighterWithOpponents(string name);

        public List<Fighter> FightersByName(string name);

        public Dictionary<string, Fighter> State { get; }

        public ConcurrentDictionary<string, Fighter> ConcurrentState { get; }
}

    public class FighterStoreHandler : IFighterStoreHandler
    {
        IDictionary<string, Fighter> fighters;

        public void Init(IDictionary<string, Fighter> fighters)
        {
            this.fighters = fighters;
        }

        public Dictionary<string, Fighter> State => fighters.ToDictionary(f => f.Key, f => f.Value.Clone());

        public ConcurrentDictionary<string, Fighter> ConcurrentState => new ConcurrentDictionary<string, Fighter>(State);

        public Fighter GetFighter(string name) 
        {
            var fighter = State.FirstOrDefault(f => f.Value.EqualsName(name));

            return fighter.Value;
        }

        Fighter GetFighterWithOpponents(Fighter fighter)
        {
            var state = State;

            foreach (var rec in fighter.Record)
            {
                if (state.TryGetValue(rec.Opponent.Name, out var opp))
                    rec.Opponent = opp;
            }

            return fighter;
        }

        public Fighter GetFighterWithOpponents(string name)
        {
            var fighter = GetFighter(name);

            return GetFighterWithOpponents(fighter);
        }

        public List<Fighter> FightersByName(string name)
        {
            var list = State.Where(f => f.Value.ContainsName(name))
                            .Select(k => k.Value)
                            .ToList();

            return list;
        }
    }
}
