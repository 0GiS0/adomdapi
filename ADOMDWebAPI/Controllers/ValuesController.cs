using Microsoft.AnalysisServices.AdomdClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Http;
using System.Web.Script.Serialization;

namespace ADOMDWebAPI.Controllers
{

    public class ValuesController : ApiController
    {
        readonly string asDataSource = ConfigurationManager.AppSettings["asDataSource"];

        // GET api/values
        public object Get(string query)
        {
            var ssasConnection = GetConnection();

            var cmd = new AdomdCommand(query)
            {
                Connection = ssasConnection
            };


            var json = CreateJsonFromDataReader(cmd);

            return (json);
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

        private AdomdConnection GetConnection()
        {
            /*User account token*/
            var header = Request.Headers.GetValues("Authorization").First();
            var token = header.ToString().Substring(header.ToString().LastIndexOf(' ')).Trim();

            var connectionString = $"Provider=MSOLAP;Data Source={asDataSource};Initial Catalog=adventureworks;User ID=;Password={token};Persist Security Info=True;Impersonation Level=Impersonate";

            var ssasConnection = new AdomdConnection(connectionString);
            ssasConnection.Open();

            return ssasConnection;
        }
    }
}
