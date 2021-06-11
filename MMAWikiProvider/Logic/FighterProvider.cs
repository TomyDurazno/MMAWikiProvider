using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MMAWikiProvider.Models;

namespace MMAWikiProvider.Logic
{
    public interface IFighterProvider
    {
        public Fighter Parse(WikiArticle article);

        Task<Fighter> Get(string name);
    }

    public class FighterProvider : IFighterProvider
    {
        public FighterProvider()
        {

        }

        IWikiProvider wikiprovider;
        public FighterProvider(IWikiProvider wikiprovider)
        {
            this.wikiprovider = wikiprovider;
        }

        public async Task<Fighter> Get(string name)
        {
            WikiArticle article = default;
            try
            {
                article = await wikiprovider.GetFighterWikiArticle(name);

                var fighter = Parse(article);

                return fighter;
            }
            catch 
            {
                return await Task.FromResult<Fighter>(null);
            }
        }

        public Fighter Parse(WikiArticle article) => Fighter.Parse(article.Name, article.Record);
    }
}
