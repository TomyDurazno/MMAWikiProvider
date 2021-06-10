using System.Collections.Generic;

namespace MMAWikiProvider.Models
{
    public class WikiBio
    {
        public IEnumerable<KeyValuePair<string, string>> Rows { get; set; }

        public WikiBio(IEnumerable<KeyValuePair<string, string>> rows)
        {
            Rows = rows;
        }
    }
}
