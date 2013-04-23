using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Specialized;
using System.Web;
using CloudAPI.Baidu.Model;
using System.IO;
using System.Diagnostics;
using System.Net;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace CloudAPI.Baidu
{
    class CoreClient : CommonClient
    {
        static string GetLastPathSegment(string uri)
        {
            var path = new Uri(uri).AbsolutePath;
            return path
                .Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                .LastOrDefault();
        }
        readonly string _AccessKey;
        readonly string _SecureKey;
        public CoreClient(Uri rootUri, string _ak, string _sk)
            : base(rootUri)
        {
            _AccessKey = _ak;
            _SecureKey = _sk;
        }
        #region Bucket 相关方法
        /// <summary>
        /// API本身不完整 , 无容量，无。。。。。
        /// </summary>
        /// <param name="key"></param>
        /// <param name="acl"></param>
        /// <returns></returns>
        public bool CreateBucket(string key, string acl = Vars.BCS_SDK_ACL_TYPE_PRIVATE)
        {
            Bucket buck = new Bucket
            {
                Name = key,
                Region = key,
                TotalCapacity = 2000
            };
            RequestModel request = new RequestModel(key, HttpMethod.Put.ToString(), "/");
            string query = request.Sign(_SecureKey, _AccessKey);
            var quest = HttpPutAsJson(query, buck);
            quest.Content.Headers.Add(Vars.Acl, acl);
            var response = SendHttpRequest(quest,
                HttpStatusCode.OK, HttpStatusCode.Forbidden, HttpStatusCode.BadRequest);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                var log = response.Content.ReadAsJson<ResonseResult>();
                Console.WriteLine("创建{2}失败，参考{0} (错误码：{1})", log.Error.Message, log.Error.Code, key);
                return false;
            }

            return true;

        }
        public bool DeleteBucket(string key)
        {
            RequestModel request = new RequestModel(key, HttpMethod.Delete.ToString(), "/");
            string query = request.Sign(_SecureKey, _AccessKey);
            var response = SendHttpRequest(HttpDelete(query),
                HttpStatusCode.OK, HttpStatusCode.Forbidden, HttpStatusCode.BadRequest, HttpStatusCode.NotFound);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                var log = response.Content.ReadAsJson<ResonseResult>();
                Console.WriteLine("删除失败，参考{0} (错误码：{1})", log.Error.Message, log.Error.Code);
                return false;
            }
            return true;

        }
        public List<Bucket> GetBucketList()
        {
            RequestModel request = new RequestModel("", HttpMethod.Get.ToString(), "/");
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            string query = request.Sign(_SecureKey, _AccessKey);
            var result = SendHttpRequestAndParseResultAs<List<Bucket>>(
                HttpGet(query),
                HttpStatusCode.OK);
            result = result ?? new List<Bucket> { };

            stopwatch.Stop();
            OnOperationCompleted(new OperationCompletedEventArgs
            {
                QueryText = query,
                ResourcesReturned = result.Count,
                TimeTaken = stopwatch.Elapsed
            });
            return result;

        }
        #endregion
        #region Object 相关方法
        public BCSObjectResult GetObjectList(string bucket, int start = 0, int limit = 10)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            NameValueCollection nc = new NameValueCollection();
            if (start > 0) nc.Add("start", start.ToString());
            if (limit > 0) nc.Add("limit", limit.ToString());
            RequestModel request = new RequestModel(bucket, HttpMethod.Get.ToString(), "/");
            request.QueryValues = nc;
            string query = request.Sign(_SecureKey, _AccessKey);
            var response = SendHttpRequest(
                HttpGet(query),
                HttpStatusCode.OK, HttpStatusCode.Forbidden);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                var log = response.Content.ReadAsJson<ResonseResult>();
                Console.WriteLine("GetObjectList失败，参考{0} (错误码：{1})", log.Error.Message, log.Error.Code);
                return null;
            }
            var rls = response.Content.ReadAsJson<BCSObjectResult>();
            stopwatch.Stop();
            OnOperationCompleted(new OperationCompletedEventArgs
            {
                QueryText = query,
                ResourcesReturned = rls.Items == null ? 0 : rls.Items.Count,
                TimeTaken = stopwatch.Elapsed
            });
            return rls;

        }
        public bool CreateObject(string bucket, string objectid, BCSObjectHead head)
        {
            if (head == null) return false;
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            NameValueCollection nc = new NameValueCollection();

            RequestModel request = new RequestModel(bucket, HttpMethod.Put.ToString(), objectid);
            request.QueryValues = nc;
            string query = request.Sign(_SecureKey, _AccessKey);
            /*
            var quest = HttpPutAsContent(query, new FormUrlEncodedContent(
                new List<KeyValuePair<string, string>>  
             {  
                  new KeyValuePair<string, string>("name", "agree"),  
             }));
            */
            //var content = new MultipartFormDataContent();
            ////var values = new[]
            ////{
            ////    new KeyValuePair<string, string>("Foo", "Bar"),
            ////    new KeyValuePair<string, string>("More", "Less"),
            ////};
            ////foreach (var keyValuePair in values)
            ////{
            ////    content.Add(new StringContent(keyValuePair.Value), keyValuePair.Key);
            ////}
            //if (!String.IsNullOrWhiteSpace(head.FilePath) && System.IO.File.Exists(head.FilePath))
            //{
            //    var fileContent = new ByteArrayContent(System.IO.File.ReadAllBytes(head.FilePath));
            //    //fileContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            //    //{
            //    //    FileName = head.FileName
            //    //};
            //    content.Add(fileContent);
            //}
            StreamContent fileContent = null;
            ContentDispositionHeaderValue dh = null;
            if (!String.IsNullOrWhiteSpace(head.FilePath) && System.IO.File.Exists(head.FilePath))
            {
                fileContent = new StreamContent(File.OpenRead(head.FilePath));
                if (head.FileName != null)
                {
                    dh = new ContentDispositionHeaderValue("attachment")
                           {
                               FileName = head.FileName ?? (Path.GetFileName(head.FilePath))
                           };
                }
            }
            var quest = HttpPutAsContent(query, fileContent);



            if (head.CacheControl != null)
            {
                quest.Headers.CacheControl = head.CacheControl;
            }
            if (quest.Content != null)
            {
                var hr = quest.Content.Headers;

                if (dh != null)
                {
                    hr.ContentDisposition = dh;
                }
                hr.ContentType = head.ContentType;
                hr.Expires = head.Expires;
                if (!String.IsNullOrWhiteSpace(head.Acl))
                {
                    hr.Add(Vars.Acl, head.Acl);
                }
                if (!String.IsNullOrWhiteSpace(head.MetaKey))
                {
                    hr.Add(Vars.MetaKey, head.MetaKey);
                }
            }

            var response = SendHttpRequest(
         quest,
          HttpStatusCode.OK, HttpStatusCode.Forbidden);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                var log = response.Content.ReadAsJson<ResonseResult>();
                Console.WriteLine("CreateObject失败，参考{0} (错误码：{1})", log.Error.Message, log.Error.Code);
                return false;
            }
            stopwatch.Stop();
            OnOperationCompleted(new OperationCompletedEventArgs
         {
             QueryText = query,
             ResourcesReturned = 0,
             TimeTaken = stopwatch.Elapsed
         });
            return true;
        }
        public bool DeleteObject(string bucket, string objectid)
        {
            RequestModel request = new RequestModel(bucket, HttpMethod.Delete.ToString(), objectid);
            string query = request.Sign(_SecureKey, _AccessKey);
            var response = SendHttpRequest(HttpDelete(query),
                HttpStatusCode.OK, HttpStatusCode.Forbidden, HttpStatusCode.BadRequest, HttpStatusCode.NotFound);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                var log = response.Content.ReadAsJson<ResonseResult>();
                Console.WriteLine("DeleteObject失败，参考{0} (错误码：{1})", log.Error.Message, log.Error.Code);
                return false;
            }
            return true;

        }
        public bool GetObject(string bucket, string objectid, string localname)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            NameValueCollection nc = new NameValueCollection();

            RequestModel request = new RequestModel(bucket, HttpMethod.Get.ToString(), objectid);
            request.QueryValues = nc;
            string query = request.Sign(_SecureKey, _AccessKey);

            var quest = HttpGet(query);

            var response = SendHttpRequest(
         quest,
          HttpStatusCode.OK, HttpStatusCode.Forbidden, HttpStatusCode.NotFound);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                var log = response.Content.ReadAsJson<ResonseResult>();
                Console.WriteLine("GetObject失败，参考{0} (错误码：{1})", log.Error.Message, log.Error.Code);
                return false;
            }
            Task task = response.Content.ReadAsStreamAsync().ContinueWith(t =>
            {
                var stream = t.Result;
                using (FileStream fileStream = File.Create(localname, (int)stream.Length))
                {
                    byte[] bytesInStream = new byte[stream.Length];
                    stream.Read(bytesInStream, 0, (int)bytesInStream.Length);
                    fileStream.Write(bytesInStream, 0, bytesInStream.Length);
                }
            });
            task.Wait();
            stopwatch.Stop();
            OnOperationCompleted(new OperationCompletedEventArgs
            {
                QueryText = query,
                ResourcesReturned = 1,
                TimeTaken = stopwatch.Elapsed
            });
            return true;
        }
        #endregion
        #region 权限控制方法
        /// <summary>
        /// 如果smartacl 不为空 ，直接使用 smartacl 做权限
        /// </summary>
        /// <param name="bucket"></param>
        /// <param name="objectid"></param>
        /// <param name="acl"></param>
        /// <param name="smartacl"></param>
        /// <returns></returns>
        public bool PutAcl(string bucket, string objectid, AclModel acl)
        {
            if (acl == null) return false;
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            NameValueCollection nc = new NameValueCollection();
            nc.Add("acl", "1");
            RequestModel request = new RequestModel(bucket, HttpMethod.Put.ToString(), objectid);
            request.QueryValues = nc;
            string query = request.Sign(_SecureKey, _AccessKey);

            HttpRequestMessage quest = HttpPutAsJson(query, acl);
            var response = SendHttpRequest(
         quest,
          HttpStatusCode.OK, HttpStatusCode.Forbidden);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                var log = response.Content.ReadAsJson<ResonseResult>();
                Console.WriteLine("PutAcl 失败，参考{0} (错误码：{1})", log.Error.Message, log.Error.Code);
                return false;
            }
            stopwatch.Stop();
            OnOperationCompleted(new OperationCompletedEventArgs
            {
                QueryText = query,
                ResourcesReturned = 1,
                TimeTaken = stopwatch.Elapsed
            });
            return true;
        }
        public AclModel GetAcl(string bucket, string objectid)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            NameValueCollection nc = new NameValueCollection();
            nc.Add("acl", "1");
            RequestModel request = new RequestModel(bucket, HttpMethod.Get.ToString(), objectid);
            request.QueryValues = nc;
            string query = request.Sign(_SecureKey, _AccessKey);
            var result = SendHttpRequestAndParseResultAs<AclModel>(
         HttpGet(query),
          HttpStatusCode.OK);

            stopwatch.Stop();
            OnOperationCompleted(new OperationCompletedEventArgs
            {
                QueryText = query,
                ResourcesReturned = 1,
                TimeTaken = stopwatch.Elapsed
            });
            return result;
        }

        #endregion
        public override Uri AddToken(Uri url)
        {
            Dictionary<string, string> tokens = new Dictionary<string, string>
            {
                //{"sign","MBO:BEfd4eac2da6f443f033c3d02e366adc:SPpeWJQIELCwEpgykJ5u4JxdG54%3D"}
            };
            return Utilities.AddQueryString(url, tokens);
        }

    }
    public class RequestModel
    {
        public string BUCKET { get; set; }
        public string METHOD { get; set; }
        public string OBJECT { get; set; }
        public string ip { get; set; }
        public string time { get; set; }
        public string size { get; set; }
        public NameValueCollection QueryValues { get; set; }
        /// <summary>
        /// 依次初始化 BUCKET METHOD OBJECT
        /// </summary>
        /// <param name="b"></param>
        /// <param name="m"></param>
        /// <param name="o"></param>
        public RequestModel(string b, string m, string o)
        {
            BUCKET = b;
            METHOD = m;
            OBJECT = o;
        }
        public string Url()
        {
            string url = String.Format("{0}{1}?", BUCKET, HttpUtility.UrlEncodeUnicode(OBJECT));
            if (QueryValues != null && QueryValues.Count > 0)
            {
                url += Utilities.ToQueryString(QueryValues);
            }
            return url;
        }

        public string Sign(string sk, string ak)
        {
            if (String.IsNullOrWhiteSpace(sk) || String.IsNullOrWhiteSpace(ak)) return null;
            if (null == this.BUCKET || null == this.METHOD || null == this.OBJECT) return null;
            string flags = String.Empty, content = String.Empty;
            flags += "MBO";
            content += "Method=" + this.METHOD + "\n"; //method
            content += "Bucket=" + this.BUCKET + "\n"; //bucket
            content += "Object=" + (this.OBJECT.KeepFineUrl()) + "\n"; //object
            if (null != this.ip)
            {
                flags += "I";
                content += "Ip=" + this.ip + "\n";
            }
            if (null != this.time)
            {
                flags += "T";
                content += "Time=" + this.time + "\n";
            }
            if (null != this.ip)
            {
                flags += "S";
                content += "Size=" + this.size + "\n";
            }
            content = flags + "\n" + content;
            string sign = Utilities.Hash(content, System.Text.Encoding.UTF8.GetBytes(sk), true);
            sign = String.Format("sign={0}:{1}:{2}", flags, ak, sign);
            string baseurl = this.Url();
            if (!baseurl.Contains("?"))
            {
                baseurl += "?";
            }
            if (!baseurl.EndsWith("?"))
            {
                baseurl = baseurl.TrimEnd('&') + "&";
            }
            return String.Format("{0}{1}", baseurl, sign);
        }

    }
}
