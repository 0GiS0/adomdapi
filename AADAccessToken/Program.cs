using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using System.Configuration;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace AADAccessToken
{
    class Program
    {
        static void Main(string[] args)
        {
            string ApiUrl = ConfigurationManager.AppSettings["apiUrl"];

            var token = GetBearerToken().GetAwaiter().GetResult();

            Debug.WriteLine(token);

            if (token != null)
            {
                
                DisplayMessage("CONNECTED TO AZURE ANALYSIS SERVICES", ConsoleColor.White, ConsoleColor.DarkGreen);
                DisplayMessage("ENTER YOUR QUERY (TYPE 'quit' TO EXIT):", ConsoleColor.Yellow);
                var query = Console.ReadLine();

                while (query != "quit")
                {

                    var client = new HttpClient();
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                    var response = client.GetAsync(new Uri($"{ApiUrl}{query}"));

                    string content = response.Result.Content.ReadAsStringAsync().Result;
                    
                    DisplayMessage(content,ConsoleColor.DarkYellow);                    
                    DisplayMessage("ENTER YOUR QUERY (TYPE 'quit' TO EXIT):",ConsoleColor.Yellow);                    
                    query = Console.ReadLine();
                }
            }
            else
            {
                DisplayMessage("ERROR DURING CONNECTION", ConsoleColor.White, ConsoleColor.Red);                
            }

        }

        static void DisplayMessage(string message, ConsoleColor foregroundColor = ConsoleColor.White, ConsoleColor backgroundColor = ConsoleColor.Black)
        {
            Console.BackgroundColor = backgroundColor;
            Console.ForegroundColor = foregroundColor;
            Console.WriteLine(message);
            Console.ResetColor();
        }


        /// <summary>
        /// Gets the access token to invoke Azure Analysis Services
        /// </summary>
        /// <returns></returns>
        static async Task<string> GetBearerToken()
        {
            var authority = ConfigurationManager.AppSettings["aad:Authority"];
            var resourceUri = ConfigurationManager.AppSettings["aad:ResourceUri"];
            var clientId = ConfigurationManager.AppSettings["aad:ClientId"];
            var redirectUri = new Uri(ConfigurationManager.AppSettings["aad:RedirectUri"]);

            var ctx = new AuthenticationContext(authority);
            var token = await ctx.AcquireTokenAsync(resourceUri, clientId, redirectUri, new PlatformParameters(PromptBehavior.Auto));

            return token.AccessToken;
        }

    }
}
