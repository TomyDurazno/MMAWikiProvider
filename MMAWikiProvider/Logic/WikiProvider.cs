using AngleSharp;
using AngleSharp.Dom;
using MMAWikiProvider.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MMAWikiProvider.Models;

namespace MMAWikiProvider.Logic
{
    public interface IWikiProvider
    {
        public Task<WikiArticle> GetFighterWikiArticle(string fighterName);
    }

	public class WikiProvider : IWikiProvider
	{
		public string WikiRoot => "https://en.wikipedia.org/wiki";

		#region Implementation

		#region Formatters

		Dictionary<string, Func<IHtmlCollection<IElement>, IEnumerable<KeyValuePair<string, string>>>> Formatters;

		void InitFormatters()
		{
			var formatters = new Dictionary<string, Func<IHtmlCollection<IElement>, IEnumerable<KeyValuePair<string, string>>>>();

			formatters.Add("Division", DivisionFormat);

			Formatters = formatters;
		}

		public IEnumerable<KeyValuePair<string, string>> DivisionFormat(IHtmlCollection<IElement> item)
		{
			var th = item.ElementAt(0);
			var td = item.ElementAt(1);

			foreach (var child in td.Children)
				if (child.TextContent.IsNotEmpty())
					yield return KeyValuePair.Create(th.TextContent, child.TextContent);
		}

		#endregion

		public WikiProvider()
		{
			InitFormatters();
		}

		IEnumerable<KeyValuePair<string, string>> GetBio(IDocument document)
        {
			var rows = document.QuerySelectorAll(".infobox")[0]
							   .QuerySelectorAll("tr")
							   .Select(tr => tr.Children);

			yield return KeyValuePair.Create("Name", rows.ElementAt(0)[0].TextContent);

			var imagetd = rows.ElementAt(1)[0];

			var img = imagetd.QuerySelector("img");

			int skip = 1;
			if (img != null)
			{
				yield return KeyValuePair.Create("src", img.GetAttribute("src"));

				var caption = imagetd.QuerySelector(".infobox-caption");

				if(caption != null)
					yield return KeyValuePair.Create("caption", caption.TextContent);

				skip++;
			}

			//Rows
            foreach (var item in rows.Skip(skip))
            {
				if (item.Count() == 1)
                {
					var th = item.ElementAt(0);
					yield return KeyValuePair.Create(th.TextContent, th.TextContent);
				}

				if (item.Count() == 2)
                {
					var th = item.ElementAt(0);

					if(Formatters.TryGetValue(th.TextContent, out var func))
						foreach (var pair in func(item))
							yield return pair;
					else
						yield return KeyValuePair.Create(th.TextContent, item.ElementAt(1).TextContent);
                }
			}
		}

		IEnumerable<string[]> GetRecordTable(IDocument document)
        {
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

			return fightRows.Select(r => r.ToArray());
		}

		async Task<WikiArticle> GetFighterWikiArticleCore(string fighterName)
		{
			var config = Configuration.Default.WithDefaultLoader();

			var fname = fighterName.Replace(" ", "_");

			var document = await BrowsingContext.New(config).OpenAsync($"{WikiRoot}/{fname}");

			var record = GetRecordTable(document);

			var wikiarticle = new WikiArticle(fighterName, record);

			wikiarticle.Bio = new WikiBio(GetBio(document));

			return wikiarticle;
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
