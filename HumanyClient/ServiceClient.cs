using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.Caching;
using System.Text;
using System.Threading.Tasks;

namespace HumanyClient
{
	/// <summary>
	/// This service client maps commonly used properies from the REST API to a strongly typed class.
	/// </summary>
	public partial class ServiceClient : ServiceClientBase<ServiceClient.GuideDto, ServiceClient.CategoryDto, ServiceClient.PortalDto>
	{
		public ServiceClient(string serviceUrl, ValueProvider client = null, ValueProvider language = null, ValueProvider site = null, ValueProvider funnel = null, bool enableStatistics = false)
			: base(serviceUrl, language, site, client, funnel, enableStatistics)
		{
		}

		#region GuideDto
		public class GuideDto
		{
			public int Id { get; set; }
			public string Title { get; set; }
			public string Body { get; set; }
			public DateTime Created { get; set; }
			public DateTime Modified { get; set; }
			public Dictionary<string, string> Attributes { get; set; }
			public List<int> Categories { get; set; }
			public Dictionary<string, string> Perspectives { get; set; }
			public Dictionary<string, int> Translations { get; set; }
			public string Type { get; set; }
		}
		#endregion

		#region GuideDto
		public class CategoryDto
		{
			public int Id { get; set; }
			public string Name { get; set; }
			public Dictionary<string, string> Attributes { get; set; }
			public List<CategoryDto> Children { get; set; }
			public int GuidesCount { get; set; }
		}
		#endregion

		#region PortalDto
		public class PortalDto
		{
			public string UriName { get; set; }
			public int Id { get; set; }
			public string Title { get; set; }
			public string Language { get; set; }
			public DateTime LastModified { get; set; }
			public Dictionary<string, string> Attributes { get; set; }
			public PortalInterfaceDto Interface { get; set; }
			//public PortalAccessibilityDto Accessibility { get; set; }
			//public PortalContactsDto Contacts { get; set; }
			public List<int> Perspectives { get; set; }
			public List<string> PerspectiveNames { get; set; }
			public List<PortalTranslationDto> EnabledLanguages { get; set; }
		}


		public class PortalInterfaceDto
		{
			public int PageSize { get; set; }
			public bool ShowLastCreated { get; set; }
			public bool LastCreatedGuidesPageSize { get; set; }
			public bool ShowPortalsLanguageSwitch { get; set; }
			public bool ShowGuidesLanguageSwitch { get; set; }
			public bool ShowGuidePerspectivesSwitch { get; set; }
			public bool ShowCategoriesInResults { get; set; }
			public bool ShowAlertMessage { get; set; }
			public string AlertMessage { get; set; }
			public bool ShowFreeText { get; set; }
			public string FreeText { get; set; }
			public bool ShowPortalFeedback { get; set; }
			public bool ShowGuidesFeedback { get; set; }
		}

		//public class PortalAccessibilityDto
		//{
		//	public string[] AllowedAddresses { get; set; }
		//	public string SecurityLevel { get; set; }
		//}

		//public class PortalContactsDto
		//{
		//	public string PortalFeedbackEmail { get; set; }
		//	public string GuidesFeedbackEmail { get; set; }
		//}

		public class PortalTranslationDto
		{
			public string LanguageCode { get; set; }
			public string PortalName { get; set; }
			public string LanguageName { get; set; }
			public string UriName { get; set; }
		}
		#endregion
	}

	/// <summary>
	/// This service client expose all REST API results to a dynamic object
	/// </summary>
	public class DynamicServiceClient : ServiceClientBase<dynamic, dynamic, dynamic>
	{
		public DynamicServiceClient(string serviceUrl, ValueProvider client = null, ValueProvider language = null, ValueProvider site = null, ValueProvider funnel = null)
			: base(serviceUrl, language, site, client, funnel)
		{
		}
	}

	/// <summary>
	/// A service client intended to simplify access to the humany REST API.
	/// </summary>
	public partial class ServiceClientBase<TGuide, TCategory, TInterface>
		where TInterface: new()
	{
		private HttpClient http;
		private string serviceUrl;
		private Dictionary<string, ValueProvider> defaults;
		private bool enableStatistics;

		/// <summary>
		/// Creates an instance of the <see cref="ServiceClientBase"/> class.
		/// </summary>
		/// <example>
		/// var client = new ServiceClient("https://{myapp}.humany.net/{myinterface}/", 
		///		client: Guid.NewGuid(), 
		///		funnel: new ServiceClient.ValueProvider(() =&lt; DateTime.Now.Second &gt; 30 ? "unlucky" : "lucky"), 
		///		site: "/faq",
		///		language: "en");
		/// </example>
		/// <param name="serviceUrl">The url to the service, e.g. "https://{myapp}.humany.net/{myinterface}/"</param>
		/// <param name="language"></param>
		/// <param name="site"></param>
		/// <param name="client"></param>
		/// <param name="funnel"></param>
		public ServiceClientBase(string serviceUrl, ValueProvider language = null, ValueProvider site = null, ValueProvider client = null, ValueProvider funnel = null, bool enableStatistics = false)
			: this(serviceUrl, new Dictionary<string,ValueProvider>())
		{
			this.http = new HttpClient();
			this.serviceUrl = serviceUrl;

			AddDefault("client", client);
			AddDefault("language", language);
			AddDefault("site", site);
			AddDefault("funnel", funnel);
			this.enableStatistics = enableStatistics;
		}

		/// <summary>
		/// Creates an instance of the <see cref="ServiceClientBase"/> class.
		/// </summary>
		/// <param name="serviceUrl"></param>
		/// <param name="defaults"></param>
		public ServiceClientBase(string serviceUrl, Dictionary<string, ValueProvider> defaults)
		{
			this.http = new HttpClient();
			this.serviceUrl = serviceUrl;
			this.defaults = defaults ?? new Dictionary<string, ValueProvider>();
			Cache = (key, factory) => factory();
		}

		public Func<string, Func<string>, string> Cache { get; set; }

		// API

		/// <summary>
		/// Gets guides
		/// </summary>
		/// <param name="query">
		/// new 
		/// {
		///		language = "en"|"sv"|"da"|"nn"|"fi",
		///		expand = "None"|"Children"|"Descendant",
		///		categories = "1,2,3",
		///		attributes = "AttributeName=value",
		///		parameters = new Dictionary&lt;string,string&gt;() { { "parameter1", "value" }, { "parameter2", "value" } },
		///		// --or--
		///		pParameter1 = "value",
		///		pParameter2 = "value"
		///	}
		/// </param>
		/// <returns></returns>
		public GuidesResult GetGuides(object query = null)
		{
			var json = GetOrPostContentDependingOnStatisticsSetting("guides", query);
			return JsonConvert.DeserializeObject<GuidesResult>(json);
		}

		/// <summary>
		/// Gets guides
		/// </summary>
		/// <param name="query">
		/// new 
		/// {
		///		language = "en"|"sv"|"da"|"nn"|"fi",
		///		expand = "None"|"Children"|"Descendant",
		///		categories = "1,2,3",
		///		attributes = "AttributeName=value",
		///		parameters = new Dictionary&lt;string,string&gt;() { { "parameter1", "value" }, { "parameter2", "value" } },
		///		// --or--
		///		pParameter1 = "value",
		///		pParameter2 = "value"
		///	}
		/// </param>
		/// <returns></returns>
		public GuidesResult GetGuides(IDictionary<string, object> query)
		{
			var json = GetOrPostContentDependingOnStatisticsSetting("guides", query);
			return JsonConvert.DeserializeObject<GuidesResult>(json);
		}

		/// <summary>
		/// Gets a single guide
		/// </summary>
		/// <param name="id"></param>
		/// <param name="query">
		/// new 
		/// {
		///	}
		/// </param>
		/// <returns></returns>
		public TGuide GetGuide(int id, object query = null)
		{
			var json = GetOrPostContentDependingOnStatisticsSetting("guides/" + id, query);
			return JsonConvert.DeserializeObject<TGuide>(json);
		}

		/// <summary>
		/// Gets a single guide
		/// </summary>
		/// <param name="id"></param>
		/// <param name="query">
		/// new 
		/// {
		///	}
		/// </param>
		/// <returns></returns>
		public TGuide GetOrPostContentDependingOnStatisticsSetting(int id, IDictionary<string, object> query)
		{
			var json = GetContent("guides/" + id, query);
			return JsonConvert.DeserializeObject<TGuide>(json);
		}

		/// <summary>
		/// Gets categories
		/// </summary>
		/// <param name="query">
		/// new 
		/// {
		///		phrase = "",
		///		language = "en"|"sv"|"da"|"nn"|"fi",
		///		skip = 0,
		///		take = 10,
		///		expand = "None"|"Children"|"Descendant",
		///		categories = "1,2,3",
		///		attributes = "AttributeName=value",
		///		parameters = new Dictionary&lt;string,string&gt;() { { "parameter1", "value" }, { "parameter2", "value" } },
		///		// --or--
		///		pParameter1 = "value",
		///		pParameter2 = "value"
		///	}
		/// </param>
		/// <returns></returns>
		public CategoriesResult GetCategories(object query = null)
		{
			var json = GetContent("categories", query);
			return JsonConvert.DeserializeObject<CategoriesResult>(json);
		}

		/// <summary>
		/// Gets categories
		/// </summary>
		/// <param name="query">
		/// new 
		/// {
		///		language = "en"|"sv"|"da"|"nn"|"fi",
		///		skip = 0,
		///		take = 10,
		///		expand = "None"|"Children"|"Descendant",
		///		categories = "1,2,3",
		///		attributes = "AttributeName=value",
		///		parameters = new Dictionary&lt;string,string&gt;() { { "parameter1", "value" }, { "parameter2", "value" } },
		///		// --or--
		///		pParameter1 = "value",
		///		pParameter2 = "value"
		///	}
		/// </param>
		/// <returns></returns>
		public CategoriesResult GetCategories(IDictionary<string, object> query)
		{
			var json = GetContent("categories", query);
			return JsonConvert.DeserializeObject<CategoriesResult>(json);
		}

		/// <summary>
		/// Gets a single category
		/// </summary>
		/// <param name="id"></param>
		/// <param name="query">
		/// new 
		/// {
		///		expand = "None"|"Children"|"Descendant"
		///	}
		/// </param>
		/// <returns></returns>
		public TCategory GetCategory(int id, object query = null)
		{
			var json = GetContent("categories/" + id, query);
			return JsonConvert.DeserializeObject<TCategory>(json);
		}

		/// <summary>
		/// Gets a single category
		/// </summary>
		/// <param name="id"></param>
		/// <param name="query">
		/// new 
		/// {
		///		expand = "None"|"Children"|"Descendant"
		///	}
		/// </param>
		/// <returns></returns>
		public TCategory GetCategory(int id, IDictionary<string, object> query)
		{
			var json = GetContent("categories/" + id, query);
			return JsonConvert.DeserializeObject<TCategory>(json);
		}

		/// <summary>
		/// Gets interface configuration
		/// </summary>
		/// <param name="query"></param>
		/// <returns></returns>
		public TInterface GetConfig(object query = null)
		{
			var json = GetContent("config", query);

			return JsonConvert.DeserializeObject<TInterface>(json);
		}

		/// <summary>
		/// Gets interface configuration
		/// </summary>
		/// <param name="query"></param>
		/// <returns></returns>
		public TInterface GetConfig(IDictionary<string, object> query)
		{
			var json = GetContent("config", query);
			return JsonConvert.DeserializeObject<TInterface>(json);
		}

		// HELPER API

		public virtual string GetOrPostContentDependingOnStatisticsSetting(string subpath, object query = null)
		{
			return enableStatistics 
				? PostContent(subpath, null, query) 
				: GetContent(subpath, query);
		}

		public virtual string GetContent(string subpath, object query = null)
		{
			var url = serviceUrl + subpath + ToQueryString(query);
			return Cache(url, () =>
			{
				var request = http.GetAsync(url);
				return request.Result.Content.ReadAsStringAsync().Result;
			});
		}

		public virtual HttpResponseMessage Post(string subpath, HttpContent content, object query = null)
		{
			var url = serviceUrl + subpath + ToQueryString(query);
			var request = http.PostAsync(url, content);
			return request.Result;
		}

		public virtual string PostContent(string subpath, HttpContent content, object query = null)
		{
			var result = Post(subpath, content, query);
			return result.Content.ReadAsStringAsync().Result;
		}

		// HELPERS

		private void AddDefault(string key, ValueProvider client)
		{
			if (client != null)
				defaults[key] = client;
		}

		private string ToQueryString(object query)
		{
			var queryParameters = query as IDictionary<string, object>
				?? query.ToDictionary();

			foreach (var value in defaults.Select(kvp => new KeyValuePair<string, string>(kvp.Key, kvp.Value.Value)))
				if (!queryParameters.ContainsKey(value.Key))
					queryParameters[value.Key] = value.Value;

			foreach(var kvp in queryParameters.ToList())
				if (kvp.Key.StartsWith("p") && kvp.Key.Length > 1 && char.IsUpper(kvp.Key[1]))
				{
					queryParameters.Remove(kvp.Key);
					queryParameters["p." + kvp.Key[1].ToString().ToLower() + kvp.Key.Substring(2)] = kvp.Value;
				}

			ExtractParameters("parameters", queryParameters);
			ExtractParameters("Parameters", queryParameters);

			return "?" + string.Join("&", queryParameters.Where(kvp => kvp.Value != null).Select(kvp => kvp.Key + "=" + kvp.Value));
		}

		private static void ExtractParameters(string key, IDictionary<string, object> queryParameters)
		{
			if (queryParameters.ContainsKey(key))
			{
				var parameters = queryParameters[key] as IDictionary<string, string>;
				if (parameters != null)
				{
					queryParameters.Remove(key);
					foreach (var p in parameters)
						queryParameters["p." + p.Key] = p.Value;
				}
			}
		}

		// DTO:s

		public class GuidesResult
		{
			public GuidesResult()
			{
				Matches = new List<TGuide>();
			}
			public int TotalMatches { get; set; }
			public List<TGuide> Matches { get; set; }
		}
	
		public class CategoriesResult
		{
			public CategoriesResult()
			{
				Children = new List<TCategory>();
			}
			public int GuidesCount { get; set; }
			public List<TCategory> Children { get; set; }
		}
	}

	public static class ServiceClientHelpers
	{
		public static string Cache5Minutes(string key, Func<string> resultFactory)
		{
			var cache = new MemoryCache("ServiceClient");
			var result = cache.Get("categories") as string;

			if (result == null)
			{
				result = resultFactory();
				cache.Set("categories", result, new CacheItemPolicy { AbsoluteExpiration = DateTime.UtcNow.AddMinutes(5) });
			}
			return result;
		}

		public static ServiceClient.CategoriesResult BypassSingleTopNode(this ServiceClient.CategoriesResult categories)
		{
			if (categories.Children.Count != 1)
				return categories;

			return new ServiceClient.CategoriesResult
			{
				GuidesCount = categories.GuidesCount,
				Children = categories.Children[0].Children
			};
		}

		public static ServiceClient.CategoryDto FindTopAncestor(this IEnumerable<ServiceClient.CategoryDto> categories, int categoryId)
		{
			foreach (var category in categories)
			{
				if (category.Id == categoryId)
					return category;

				if (FindTopAncestor(category.Children, categoryId) != null)
					return category;
			}
			return null;
		}

		public static ServiceClient.CategoryDto FindCategoryByNameRecursive(this List<ServiceClient.CategoryDto> categories, string segment)
		{
			if (string.IsNullOrEmpty(segment))
				return null;

			foreach (var category in categories)
			{
				if (category.Name.IndexOf(segment, StringComparison.InvariantCultureIgnoreCase) >= 0)
					return category;
			}

			foreach (var category in categories)
			{
				var foundChild = FindCategoryByNameRecursive(category.Children, segment);
				if (foundChild != null)
					return foundChild;
			}

			return null;
		}
	}

	public class ValueProvider
	{
		static Func<string> EmptyValueFactory = () => null;

		private Func<string> valueFactory;

		public ValueProvider(Func<string> valueFactory)
		{
			this.valueFactory = valueFactory ?? EmptyValueFactory;
		}

		public override string ToString()
		{
			return Value;
		}

		public string Value 
		{
			get { return valueFactory(); }
		}

		#region Operators
		public static implicit operator ValueProvider(string value)
		{
			return new ValueProvider(() => value);
		}

		public static implicit operator ValueProvider(Guid value)
		{
			return new ValueProvider(() => value.ToString());
		}

		public static implicit operator ValueProvider(int value)
		{
			return new ValueProvider(() => value.ToString());
		}

		public static implicit operator ValueProvider(bool value)
		{
			return new ValueProvider(() => value.ToString());
		}
		#endregion
	}

	internal static class ServiceClientExtensions
	{
		public static Dictionary<string, object> ToDictionary(this object query)
		{
			if (query == null)
				return new Dictionary<string, object>();
			return query.GetType().GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance).ToDictionary(pi => pi.Name, pi => pi.GetValue(query));
		}
	}
}
