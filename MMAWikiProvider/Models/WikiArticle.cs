using MMAWikiProvider.Models;
using System.Collections.Generic;
using System.Linq;

namespace MMAWikiProvider.Models
{
    public class WikiArticle
    {
        public string Name { get; set; }
        public WikiBio Bio { get; set; }


        public WikiArticle(string name, IEnumerable<string[]> record)
        {
            Name = name;
            Record = record.ToList();
        }

        public List<string[]> Record { get; internal set; }
    }
}
