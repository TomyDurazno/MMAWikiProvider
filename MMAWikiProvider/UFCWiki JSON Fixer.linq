<Query Kind="Program">
  <Reference Relative="bin\Debug\netcoreapp5.0\MMAWikiProvider.dll">C:\Users\Tomy\Desktop\New Side\MMAWikiProvider\MMAWikiProvider\bin\Debug\netcoreapp5.0\MMAWikiProvider.dll</Reference>
  <Namespace>AngleSharp</Namespace>
  <Namespace>MMAWikiProvider.Models</Namespace>
  <Namespace>Newtonsoft.Json.Linq</Namespace>
  <Namespace>System.Text.Json</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
</Query>

async Task Main()
{
	var path = $"{Util.CurrentQuery.Location}/ufcwiki.json";
	
	var backList = JsonSerializer.Deserialize<List<Fighter>>(File.ReadAllText(path));
				
	var f = backList.FirstOrDefault(l => l.Name == "Kim Couture");						   
				
	backList.Remove(f);						   
						   
	var serialized = JsonSerializer.Serialize(backList);

	File.WriteAllText(path, serialized);
}