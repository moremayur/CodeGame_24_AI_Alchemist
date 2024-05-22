using Google.Cloud.Dialogflow.V2;
using Google.Protobuf;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace AI_Alchemist_WebApi.Controllers
{
    [Route("webhook")]
    public class WebhookController : Controller
    {
        private static readonly JsonParser jsonParser = new JsonParser(JsonParser.Settings.Default.WithIgnoreUnknownFields(true));

        [HttpPost]
        public async Task<JsonResult> GetWebhookResponse()
        {
            WebhookRequest request;

            using (var reader = new StreamReader(Request.Body))
            {
                request = jsonParser.Parse<WebhookRequest>(reader);
            }

            var intent = request.QueryResult.Intent;
            var param = request.QueryResult.Parameters;
            
            var response = new WebhookResponse();

            //var isIntentDisplayName = pas.Fields.ContainsKey("displayName") && pas.Fields["displayName"].ToString().Replace('\"', ' ').Trim().Length > 0;
            //var displayName = pas.Fields["displayName"].ToString().Replace('\"', ' ').Trim();


            StringBuilder sb = new StringBuilder();

            if (!string.IsNullOrEmpty(intent.DisplayName))
            {
                var displayName = intent.DisplayName?.Replace('\"', ' ').Trim();

                JObject mockDocInfoResponse = JObject.Load(new JsonTextReader(System.IO.File.OpenText("documentInfoResponse.json")));
                JArray mockDocs = (JArray)mockDocInfoResponse["documents"];

                JObject mockReleaseInfo = JObject.Load(new JsonTextReader(System.IO.File.OpenText("releaseFeaturesInfo.json")));
                JArray mockReleases = (JArray)mockReleaseInfo["releases"];


                if (displayName.Equals("document.history", StringComparison.InvariantCultureIgnoreCase))
                {
                    var documentCount = param.Fields["documentcount"]?.ToString().Replace('\"', ' ').Trim();

                    if (!string.IsNullOrEmpty(documentCount))
                    {
                        List<JToken> documents = mockDocs
                            .OrderByDescending(x => x["DateCreated"].Value<DateTime>())
                            .Select(x => x)
                            .Take(Convert.ToInt16(documentCount))
                            .ToList();

                        int cnt = 1;
                        foreach (JToken doc in documents)
                        {
                            sb.Append(cnt + ".\n" + "DocumentName: " + doc["Name"].ToString() + "\nStatus: " + doc["Status"].ToString() + "\n");
                            cnt++;
                        }
                    }
                }
                else if (displayName.Equals("document.status", StringComparison.InvariantCultureIgnoreCase))
                {
                    var documentName = param.Fields["documentname"].ToString().Replace('\"', ' ').Trim();

                    if (!string.IsNullOrEmpty(documentName))
                    {
                        JToken resObj = mockDocs.Where(obj => obj["Name"].Value<string>().ToLower() == documentName.ToLower()).SingleOrDefault();

                        if (resObj != null)
                        {
                            sb.Append($"The status of { documentName} is {resObj["Status"].ToString()}.");
                        }
                        else
                        {
                            sb.Clear();
                            sb.Append("The " + documentName + " is not found!");
                        }
                    }
                }
                else if (displayName.Equals("application.featues", StringComparison.InvariantCultureIgnoreCase))
                {
                    var versioinNo = param.Fields["version"].ToString().Replace('\"', ' ').Trim();

                    if (!string.IsNullOrEmpty(versioinNo))
                    {
                        sb.Append($"The {versioinNo} release contains features, ");

                        JToken resObj = mockReleases.Where(obj => obj["number"].Value<string>().ToLower() == versioinNo.ToLower()).SingleOrDefault();

                        if (resObj != null)
                        {
                            var features = resObj["features"];
                            sb.Append("\n" + string.Join(System.Environment.NewLine, features));
                        }
                        else
                        {
                            sb.Clear();
                            sb.Append($"Information about this version {versioinNo} is not available!");
                        }
                    }
                }
            }

            if (sb.Length == 0)
            {
                sb.Append("Not understand, please check request..");
            }

            response.FulfillmentText = sb.ToString();

            return Json(response);
        }
    }
}
