using AngleSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UFCWikiProvider.Models;

namespace UFCWikiProvider.Logic
{
    public interface IWikiProvider
    {
        public Task<WikiArticle> GetFighterWikiArticle(string fighterName);
    }

    public class WikiProvider : IWikiProvider
    {
        public string WikiRoot => "https://en.wikipedia.org/wiki/";

		#region Implementation

		async Task<WikiArticle> GetFighterWikiArticleCore(string fighterName)
		{
			var config = Configuration.Default.WithDefaultLoader();

			var fname = fighterName.Replace(" ", "_");

			var address = $"{WikiRoot}/{fname}";

			var document = await BrowsingContext.New(config).OpenAsync(address);

			var wikiTables = document.QuerySelectorAll(".wikitable");

			//should be one
			var record = wikiTables.FirstOrDefault(d => d.QuerySelectorAll("th").Length == 10);

			if (record == null) //Maybe wrong name of fighter
				return null;

			var rows = record.QuerySelectorAll("tr");

			var firstRowThs = rows.First().QuerySelectorAll("th");

			var fightRows = rows.Skip(1)
								.Select(tr => tr.QuerySelectorAll("td")
												.Select(t => t.TextContent));

			return new WikiArticle(fighterName, fightRows.Select(r => r.ToArray()));
		}

		#endregion

		public async Task<WikiArticle> GetFighterWikiArticle(string fighterName)
		{
			var article = await GetFighterWikiArticleCore(fighterName);

			if (article == null)
				article = await GetFighterWikiArticleCore($"{fighterName}_(fighter)");

			return article;
		}
	}
}
