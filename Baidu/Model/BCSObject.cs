using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using System.Diagnostics;

namespace CloudAPI.Baidu.Model
{
    public class BCSObjectResult
    {
        [JsonProperty("object_list")]
        public List<BCSObject> Items { get; set; }
        [JsonProperty("bucket")]
        public string Bucket { get; set; }
        [JsonProperty("start")]
        public int Start { get; set; }

        [JsonProperty("limit")]
        public int Limit { get; set; }
    }
    [DebuggerDisplay("Baidu.BCSObject: {Id}")]
    public class BCSObject
    {
        [JsonProperty("version_key")]
        public string VersionKey { get; set; }

        [JsonProperty("object")]
        public string Id { get; set; }

        [JsonProperty("requester")]
        public string Requester { get; set; }
        [JsonProperty("request_ip")]
        public string RequestIp { get; set; }
        [JsonProperty("superfile")]
        public string Superfile { get; set; }
        [JsonProperty("parent_dir")]
        public string ParentDir { get; set; }
        [JsonProperty("size")]
        public long Size { get; set; }
        [JsonProperty("is_dir")]
        public int IsDir { get; set; }

        /// <summary>
        /// 暂时没有增加 Superfile  和 IsDir 判断
        /// </summary>
        public string Path
        {
            get
            {
                return String.Format("{0}{1}", (ParentDir??"/").TrimEnd('/'), Id);
            }
        }
    }
}
