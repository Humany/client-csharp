using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HumanyClient
{
	class Program
	{
		static void Main(string[] args)
		{
			// Please store this id between calls to humany on a per-user level
			var userDistinctClientId = Guid.NewGuid().ToString();
			var client = new ServiceClient("https://help.humany.net/admin-help-en/", client: new ValueProvider(() => userDistinctClientId), enableStatistics: true);

			// Prints top-level categories
			Console.WriteLine("ALL CATEGORIES");
			var categories = client.GetCategories();
			foreach (var c in categories.Children)
				Console.WriteLine("\t" + c.Name);

			Console.WriteLine("");
			Console.WriteLine("POPULAR GUIDES");
			var guides = client.GetGuides();
			foreach (var g in guides.Matches)
				Console.WriteLine("\t" + g.Title);

			Console.WriteLine("");
			Console.WriteLine("GUIDE #" + guides.Matches[0].Id);
			var guide = client.GetGuide(guides.Matches[0].Id);
			Console.WriteLine(guide.Title);
			Console.WriteLine(guide.Body.Split('\r', '\n')[0] + "...");


			Console.WriteLine("");
			Console.WriteLine("SEARCH GUIDES");
			var search = client.GetGuides(new { phrase = "guide" });
			foreach (var g in search.Matches)
				Console.WriteLine("\t" + g.Title);

			Console.WriteLine("");
			Console.WriteLine("CATEGORY FACETS");
			var searchCategories = client.GetCategories(new { phrase = "guide" });
			foreach (var c in searchCategories.Children.Where(c => c.GuidesCount > 0))
				Console.WriteLine($"\t{c.Name} ({c.GuidesCount})");
		}
	}
}
