using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UFCWikiProvider.Models;

namespace UFCWikiProvider.Logic
{
    public interface IFighterProvider
    {
        public Fighter Parse(WikiArticle article);
    }

    public class FighterProvider : IFighterProvider
    {
        public Fighter Parse(WikiArticle article) => Fighter.Parse(article.Name, article.Record);
    }
}
