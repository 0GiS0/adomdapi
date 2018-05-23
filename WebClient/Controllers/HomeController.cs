using Microsoft.AnalysisServices.AdomdClient;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace WebClient.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public async Task<ActionResult> GetToken(string username, string password)
        {
            var token = await GetBearerToken(username, password);

            return Content(token);
        }

        public async Task<ActionResult> GetQuery(string username, string password, string query)
        {
            var token = await GetBearerToken(username, password);

            using (var ssasConnection = GetConnection(token)) //It closes Azure Analysis Services Connection when it finishes
            {
                var cmd = new AdomdCommand(query)
                {
                    Connection = ssasConnection
                };

                var json = CreateJsonFromDataReader(cmd);

                return Json(json, JsonRequestBehavior.AllowGet);
            }
        }

        public async Task<ActionResult> GetQueryFromAPI(string username, string password, string query)
        {
            var ApiUrl = ConfigurationManager.AppSettings["apiUrl"];
            var token = await GetBearerToken(username, password);

            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = client.GetAsync(new Uri($"{ApiUrl}{query}"));

            string content = response.Result.Content.ReadAsStringAsync().Result;

            return Content(content);
        }

        private object CreateJsonFromDataReader(AdomdCommand cmd)
        {
            var sb = new StringBuilder();
            var sw = new StringWriter(sb);
            var fieldVal = string.Empty;
            var prevFieldVal = string.Empty;
            var columnName = string.Empty;
            var curColumn = new List<string>();

            using (JsonWriter json = new JsonTextWriter(sw))
            {
                using (var reader = cmd.ExecuteReader())
                {
                    json.WriteStartArray();

                    while (reader.Read())
                    {
                        json.WriteStartObject();

                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            if (reader[i] != null)
                            {
                                fieldVal = reader[i].ToString();
                                if (i != 0 && reader[i - 1] != null)
                                    prevFieldVal = reader[i - 1].ToString();
                                else prevFieldVal = "First";
                                if ((fieldVal == null || fieldVal.ToLower().Trim() == "undefined" || fieldVal.ToLower().Trim() == "unknown") && (prevFieldVal == null || prevFieldVal.ToLower().Trim() == "undefined" || prevFieldVal.ToLower().Trim() == "unknown"))
                                {
                                    continue;
                                }
                                else
                                {
                                    columnName = reader.GetName(i).Replace(".[MEMBER_CAPTION]", "").Trim();
                                    curColumn = columnName.Split(new string[] { "." }, StringSplitOptions.None).ToList();
                                    columnName = curColumn[curColumn.Count - 1].Replace("[", "").Replace("]", "");

                                    json.WritePropertyName(columnName);
                                    json.WriteValue(reader[i]);


                                }
                            }
                        }
                        json.WriteEndObject();
                    }
                    json.WriteEndArray();

                }


                var json_serializer = new JavaScriptSerializer();
                return json_serializer.DeserializeObject(sw.ToString());
            }
        }


        private AdomdConnection GetConnection(string token)
        {
            var asDataSource = ConfigurationManager.AppSettings["asDataSource"];
            var connectionString = $"Provider=MSOLAP;Data Source={asDataSource};Initial Catalog=adventureworks;User ID=;Password={token};Persist Security Info=True;Impersonation Level=Impersonate";

            var ssasConnection = new AdomdConnection(connectionString);
            ssasConnection.Open();

            return ssasConnection;
        }

        private async Task<string> GetBearerToken(string username, string password)
        {
            var resourceId = "https://*.asazure.windows.net";
            var authority = "https://login.windows.net/common";
            var clientId = ConfigurationManager.AppSettings["aad:ClientId"];
            var ctx = new AuthenticationContext(authority);
            var token = await ctx.AcquireTokenAsync(resourceId, clientId, new UserPasswordCredential(username, password));

            return token.AccessToken;
        }


        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}