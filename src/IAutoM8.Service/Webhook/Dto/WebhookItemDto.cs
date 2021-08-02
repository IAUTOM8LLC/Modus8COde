using Newtonsoft.Json;
using System;

namespace IAutoM8.Service.Webhook.Dto
{
    public class WebhookItemDto
    {
        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("timestamp")]
        public DateTime Timestamp { get; set; }
        [JsonProperty("apiUrl")]
        public string ApiUrl { get; set; }
    }
}
