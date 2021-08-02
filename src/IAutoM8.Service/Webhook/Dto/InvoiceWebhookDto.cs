using Newtonsoft.Json;
using System.Collections.Generic;

namespace IAutoM8.Service.Webhook.Dto
{
    public class InvoiceWebhookDto
    {
        [JsonProperty("event_key")]
        public string EventKey { get; set; }
        [JsonProperty("object_type")]
        public string ObjectType { get; set; }
        [JsonProperty("object_keys")]
        public List<WebhookItemDto> ObjectKeys { get; set; }
        [JsonProperty("api_url")]
        public string ApiUrl { get; set; }
    }
}
