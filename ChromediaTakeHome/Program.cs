using System.Text.Json.Serialization;
using RestSharp;

namespace ChromediaTakeHome
{
    public class Program
    {
        public static void Main(string[] args)
        {
            int limit = 0;
            if (args.Length == 1)
            {
                if (!Int32.TryParse(args[0], out limit) || limit < 1)
                {
                    Console.WriteLine("Invalid parameter, must be a positive integer");
                }
            }
            else
            {
                Console.WriteLine("limit: ");
                string? limitAsString = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(limitAsString) || !Int32.TryParse(limitAsString, out limit) || limit < 1)
                {
                    Console.WriteLine("Invalid input, must be a positive integer");
                }
            }

            string[] topArticleTitles = TopArticles(limit);
            foreach(string title in topArticleTitles)
            {
                Console.WriteLine(title);
            }
        }

        private static string[] TopArticles(int limit)
        {
            var articles = Task.Run(() => GetAllArticlesAsync()).Result;
            if (articles == null) { return new string[] { }; }
            string[] results = articles.Where(x => x.EffectiveTitle != null)
                    .OrderByDescending(x => x.Comments)
                    .ThenByDescending(x => x.Title)
                    .Take(limit)
                    .Select(x => x.EffectiveTitle!)
                    .ToArray();
            return results;
        }

        private static async Task<List<Article>> GetAllArticlesAsync()
        {
            const string url = @"https://jsonmock.hackerrank.com/api/articles";

            var restClient = new RestClient(url);

            var restRequest = (new RestRequest()).AddQueryParameter("page", "1");
            var restResponse = await restClient.GetAsync<QueryResult>(restRequest);
            if (restResponse == null) { return new List<Article>(); }    //return an empty list if there is no response

            List<Article> articles = new List<Article>();
            articles.AddRange(restResponse.Data);

            int totalPages = restResponse.TotalPages;
            for (int i = 2; i <= totalPages; i++)
            {
                restRequest = (new RestRequest()).AddQueryParameter("page", i.ToString());
                restResponse = await restClient.GetAsync<QueryResult>(restRequest);
                if (restResponse == null) continue;
                articles.AddRange(restResponse.Data);
            }

            return articles;
        }
    }

    public class QueryResult
    {
        [JsonPropertyName("page")]
        public int Page { get; set; }

        [JsonPropertyName("total_pages")]
        public int TotalPages { get; set; }

        [JsonPropertyName("data")]
        public Article[] Data { get; set; }
    }

    public class Article
    {
        [JsonPropertyName("title")]
        public string? Title { get; set; }

        [JsonPropertyName("story_title")]
        public string? StoryTitle { get; set; }

        [JsonPropertyName("num_comments")]
        public int? Comments { get; set; }

        public string? EffectiveTitle
        {
            get
            {
                return Title != null ? Title : StoryTitle;
            }
        }
    }


}




