using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UFCWikiProvider.Models;

namespace UFCWikiProvider.Logic
{
    public interface IFighterListHandler
    {
        public void Init(ConcurrentDictionary<string, Fighter> fighters);
        public IEnumerable<Fighter> GetValues();

        public Fighter GetFighter(string name);
    }

    public class FighterListHandler : IFighterListHandler
    {
        ConcurrentDictionary<string, Fighter> fighters;

        public void Init(ConcurrentDictionary<string, Fighter> fighters)
        {
            this.fighters = fighters;
        }

        public IEnumerable<Fighter> GetValues() => fighters.Select(v => v.Value);

        public Fighter GetFighter(string name)
        {
            string Replace(string s) => s.Replace("_", string.Empty).Replace("_(fighter)", string.Empty);

            return GetValues().FirstOrDefault(f => Replace(f.Name) == Replace(name));
        }
    }
}
