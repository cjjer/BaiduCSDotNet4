using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using CloudAPI.Deserializer;
using CloudAPI.Serializer;
using System.Text.RegularExpressions;

namespace CloudAPI
{
    public abstract class CommonClient
    {
        internal readonly Uri RootUri;
        readonly IHttpClient httpClient;
        bool jsonStreamingAvailable = false;
        readonly string userAgent;


        public bool UseJsonStreamingIfAvailable { get; set; }

        public CommonClient(Uri rootUri)
            : this(rootUri, new HttpClientWrapper())
        {
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.UseNagleAlgorithm = false;
        }

        public CommonClient(Uri rootUri, bool expect100Continue, bool useNagleAlgorithm)
            : this(rootUri, new HttpClientWrapper())
        {
            ServicePointManager.Expect100Continue = expect100Continue;
            ServicePointManager.UseNagleAlgorithm = useNagleAlgorithm;
        }

        public CommonClient(Uri rootUri, IHttpClient httpClient)
        {
            RootUri = rootUri;
            this.httpClient = httpClient;
            UseJsonStreamingIfAvailable = true;

            var assemblyVersion = GetType().Assembly.GetName().Version;
            userAgent = string.Format("CloudeAPI/{0}", assemblyVersion);
        }

        internal string UserAgent { get { return userAgent; } }

        Uri BuildUri(string relativeUri)
        {
            var baseUri = RootUri;
            if (!RootUri.AbsoluteUri.EndsWith("/"))
                baseUri = new Uri(RootUri.AbsoluteUri + "/");

            if (relativeUri.StartsWith("/"))
                relativeUri = relativeUri.Substring(1);
            return AddToken(new Uri(baseUri, relativeUri));

        }
        public abstract Uri AddToken(Uri url);
        protected HttpRequestMessage HttpDelete(string relativeUri)
        {
            var absoluteUri = BuildUri(relativeUri);
            return new HttpRequestMessage(HttpMethod.Delete, absoluteUri);
        }

        protected HttpRequestMessage HttpGet(string relativeUri)
        {
            var absoluteUri = BuildUri(relativeUri);
            return new HttpRequestMessage(HttpMethod.Get, absoluteUri);
        }

        protected HttpRequestMessage HttpPostAsJson(string relativeUri, object postBody)
        {
            var absoluteUri = BuildUri(relativeUri);
            var postBodyJson = BuildSerializer().Serialize(postBody);
            var request = new HttpRequestMessage(HttpMethod.Post, absoluteUri)
            {
                Content = new StringContent(postBodyJson, Encoding.UTF8, "application/json")
            };
            return request;
        }

        protected HttpRequestMessage HttpPutAsJson(string relativeUri, object putBody)
        {
            var absoluteUri = BuildUri(relativeUri);
            var postBodyJson = BuildSerializer().Serialize(putBody);
            var request = new HttpRequestMessage(HttpMethod.Put, absoluteUri)
            {
                Content = new StringContent(postBodyJson, Encoding.UTF8, "application/json")
            };
            return request;
        }
        protected HttpRequestMessage HttpPutAsContent(string relativeUri, HttpContent putBody)
        {
            var absoluteUri = BuildUri(relativeUri);
            var request = new HttpRequestMessage(HttpMethod.Put, absoluteUri)
            {
                Content = putBody
            };
            return request;
        }

        protected HttpResponseMessage SendHttpRequest(HttpRequestMessage request, params HttpStatusCode[] expectedStatusCodes)
        {
            return SendHttpRequest(request, null, expectedStatusCodes);
        }

        protected Task<HttpResponseMessage> SendHttpRequestAsync(HttpRequestMessage request, params HttpStatusCode[] expectedStatusCodes)
        {
            return SendHttpRequestAsync(request, null, expectedStatusCodes);
        }

        protected HttpResponseMessage SendHttpRequest(HttpRequestMessage request, string commandDescription, params HttpStatusCode[] expectedStatusCodes)
        {
            var task = SendHttpRequestAsync(request, commandDescription, expectedStatusCodes);
            try
            {
                Task.WaitAll(task);
            }
            catch (AggregateException ex)
            {
                if (ex.InnerExceptions.Count() == 1)
                    throw ex.InnerExceptions.Single();
                throw;
            }
            return task.Result;
        }

        protected Task<HttpResponseMessage> SendHttpRequestAsync(HttpRequestMessage request, string commandDescription, params HttpStatusCode[] expectedStatusCodes)
        {
            if (UseJsonStreamingIfAvailable && jsonStreamingAvailable)
            {
                request.Headers.Accept.Clear();
                request.Headers.Remove("Accept");
                request.Headers.Add("Accept", "application/json;stream=true");
            }

            request.Headers.Add("User-Agent", userAgent);

            var baseTask = httpClient.SendAsync(request);
            var continuationTask = baseTask.ContinueWith(requestTask =>
            {
                var response = requestTask.Result;
                response.EnsureExpectedStatusCode(commandDescription, expectedStatusCodes);
                return response;
            });
            return continuationTask;
        }

        protected T SendHttpRequestAndParseResultAs<T>(HttpRequestMessage request, params HttpStatusCode[] expectedStatusCodes) where T : new()
        {
            return SendHttpRequestAndParseResultAs<T>(request, null, expectedStatusCodes);
        }

        protected T SendHttpRequestAndParseResultAs<T>(HttpRequestMessage request, string commandDescription, params HttpStatusCode[] expectedStatusCodes) where T : new()
        {
            var response = SendHttpRequest(request, commandDescription, expectedStatusCodes);
            return response.Content == null ? default(T) : response.Content.ReadAsJson<T>();
        }
        public event OperationCompletedEventHandler OperationCompleted;

        protected void OnOperationCompleted(OperationCompletedEventArgs args)
        {
            var eventInstance = OperationCompleted;
            if (eventInstance != null)
                eventInstance(this, args);
        }
        static CustomJsonSerializer BuildSerializer()
        {
            return new CustomJsonSerializer();
        }
    }
}
