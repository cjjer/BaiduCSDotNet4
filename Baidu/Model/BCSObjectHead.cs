using System.Diagnostics;
using System.Net.Http;
using Newtonsoft.Json;
using System;
using System.Net.Http.Headers;

namespace CloudAPI.Baidu.Model
{
    public class BCSObjectHead
    {
        public CacheControlHeaderValue CacheControl { get; set; }
        public MediaTypeHeaderValue ContentType { get; set; }
        public DateTimeOffset? Expires { get; set; }
        public string Acl { get; set; }
        public string MetaKey { get; set; }
        public string FilePath { get; set; }
        public string FileName { get; set; }
        
     }
  
}
