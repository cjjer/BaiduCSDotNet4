using System.Diagnostics;
using System.Net.Http;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace CloudAPI.Baidu.Model
{
    [DebuggerDisplay("Baidu.AclModel")]
    class AclModel
    {
        [JsonProperty("statements")]
        public List<AclQuery> Text { get; set; }
    }
    [DebuggerDisplay("Baidu.AclQuery")]
    class AclQuery
    {
        /// <summary>
        /// 取值为 * allow 
        /// </summary>
         //没有做成 Enum ，取值参见 Varss.Action
       [JsonProperty("action")]
        public string[] ActionText { get; set; }
        [JsonProperty("user")]
        public string[] UserList { get; set; }
        [JsonProperty("resource")]
        public string[] ResourceList { get; set; }
        //没有做成 Enum ，取值参见 Varss.EFFECT
         [JsonProperty("effect")]
        public string Effect { get; set; }
       
    }
}
