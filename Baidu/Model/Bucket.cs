using System.Diagnostics;
using System.Net.Http;
using Newtonsoft.Json;

namespace CloudAPI.Baidu.Model
{
    [DebuggerDisplay("Baidu.Bucket: {Name}")]
   public  class Bucket
    {
        //[JsonIgnore]
        //public HttpMethod Method { get; set; }

        //[JsonProperty("bucket_name")]
        //public string MethodAsString
        //{
        //    get { return Method.Method; }
        //}

        [JsonProperty("bucket_name")]
        public string Name { get; set; }

        [JsonProperty("cdatetime")]
        public long CreateTime { get; set; }

        [JsonProperty("status")]
        public int Status { get; set; }

        [JsonProperty("used_capacity")]
        public long UsedCapacity { get; set; }

        [JsonProperty("total_capacity")]
        public long TotalCapacity { get; set; }

        [JsonProperty("region")]
        public string Region { get; set; }

        [JsonProperty("x-bs-acl")]
        public string Acl { get; set; }
        
         
    }
}
