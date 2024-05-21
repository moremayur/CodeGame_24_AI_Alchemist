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

            var pas = request.QueryResult.Parameters;
            var askingDocumentName = pas.Fields.ContainsKey("document-name") && pas.Fields["document-name"].ToString().Replace('\"', ' ').Trim().Length > 0;
            var documentName = pas.Fields["document-name"].ToString().Replace('\"', ' ').Trim();

            var response = new WebhookResponse();

            StringBuilder sb = new StringBuilder();

            if (askingDocumentName)
            {
                sb.Append("The document name is: " + documentName + "; ");

                JObject jObject = JObject.Load(new JsonTextReader(System.IO.File.OpenText("documentInfoResponse.json")));

                JArray resources = (JArray)jObject["documents"];

                JToken resObj = resources.Where(obj => obj["Name"].Value<string>() == documentName).SingleOrDefault();

                if (resObj != null)
                {
                    sb.Append("and status is " + resObj["Status"].ToString());
                }
                else
                {
                    sb.Clear();
                    sb.Append("This document:" + documentName + " is not found!");
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
