using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MMAWikiProvider.Models
{
    public class FightEvent
    {
        #region String Patterns

        string uFCNumbered = "UFC [0-9]{1,3}";

        #endregion

        public string Description { get; set; }

        public bool IsUFCEvent => Description.Contains("UFC");

        public bool IsUFCPPV => Regex.Match(Description, uFCNumbered).Success;

        public FightEvent()
        {

        }

        public FightEvent(string s)
        {
            Description = s.Trim();
        }

        public FightEvent Clone() => new FightEvent(Description);
    }
}
