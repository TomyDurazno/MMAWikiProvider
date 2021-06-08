using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UFCWikiProvider.Models
{
    public class WikiArticle
    {
        public string Name { get; set; }

        public WikiArticle(string name, IEnumerable<string[]> record)
        {
            Name = name;
            Record = record.ToList();
        }

        public List<string[]> Record { get; internal set; }
    }
}
