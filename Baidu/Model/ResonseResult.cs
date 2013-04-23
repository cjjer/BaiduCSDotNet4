using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace CloudAPI.Baidu.Model
{
    public class ResonseResult
    {
        [JsonProperty("Error")]
        public ResonseResultItem Error { get; set; }


    }
    public class ResonseResultItem
    {
        [JsonProperty("code")]
        public string Code { get; set; }

        [JsonProperty("Message")]
        public string Message { get; set; }

        [JsonProperty("RequestId")]
        public string RequestId { get; set; }


    }
}
