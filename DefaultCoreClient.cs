using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace ApiCore
{
    public class DefaultCoreClient : ICoreClient
    {
        public string Endpoint { get; }
        public string ContentType { get; set; }
        public string UserAgent { get; set; }
        public bool KeepAlive { get; set; }
        public int Timeout { get; set; }
        public DefaultCoreClient(string endpoint)
        {
            KeepAlive = true;
            Timeout = 10000;
            ContentType = "application/json;charset=utf-8";
            UserAgent = "Mozilla/5.0 (Linux; Android 10; WLZ-AL10 Build/HUAWEIWLZ-AL10; wv) AppleWebKit/537.36 (KHTML, like Gecko) Version/4.0 Chrome/77.0.3865.120 MQQBrowser/6.2 TBS/045513 Mobile Safari/537.36 MMWEBID/4985 MicroMessenger/7.0.22.1820(0x2700163B) Process/tools WeChat/arm64 Weixin NetType/WIFI Language/zh_CN ABI/arm64";

            Endpoint = endpoint;
        }

        protected virtual byte[] BuildBody(IDictionary<string, object> paras)
        {
            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
            string s = JsonConvert.SerializeObject(paras, settings);
            return Encoding.UTF8.GetBytes(s);
        }
        protected virtual T ReadBody<T>(HttpWebResponse rsp)
        {
            T t;
            using (Stream responseStream = rsp.GetResponseStream())
            {
                using (StreamReader streamReader = new StreamReader(responseStream, Encoding.UTF8))
                {
                    string text = streamReader.ReadToEnd();
                    if (string.IsNullOrEmpty(text))
                    {
                        t = default(T);
                    }
                    else
                    {
                        Type typeFromHandle = typeof(T);
                        string name = typeFromHandle.Name;
                        if (name != null)
                        {
                            if (name == "String")
                            {
                                return (T)((object)Convert.ChangeType(text, typeFromHandle));
                            }
                            if (name == "Int32")
                            {
                                return (T)((object)Convert.ChangeType(int.Parse(text), typeFromHandle));
                            }
                            if (name == "Double")
                            {
                                return (T)((object)Convert.ChangeType(double.Parse(text), typeFromHandle));
                            }
                            if (name == "DateTime")
                            {
                                return (T)((object)Convert.ChangeType(DateTime.Parse(text), typeFromHandle));
                            }
                        }
                        t = JsonConvert.DeserializeObject<T>(text);
                    }
                }
            }
            return t;
        }
        protected virtual string ReadError(HttpWebResponse rsp)
        {
            string result;
            using (Stream responseStream = rsp.GetResponseStream())
            {
                using (StreamReader streamReader = new StreamReader(responseStream, Encoding.UTF8))
                {
                    result = streamReader.ReadToEnd();
                }
            }
            return result;
        }

        HttpWebRequest BuildRequest(IRequest req)
        {
            Type attribute = typeof(HttpPropertyAttribute);
            Dictionary<HttpMember, Dictionary<string, object>> dictionary = req.GetType().GetProperties()
                .Where(p => p.IsDefined(attribute, false))                
                .Select(p => new HttpParameter(
                    ((HttpPropertyAttribute)p.GetCustomAttributes(attribute, false).First()).Member,
                    p.Name,
                    p.GetValue(req)
                 ))
                .GroupBy(p => p.Member)
                .ToDictionary(g => g.Key, g => g.ToDictionary(p => p.Name, p => p.Value));
            string path = req.GetPath();
            string text = string.IsNullOrEmpty(path) ? Endpoint : (Endpoint + "/" + path);
            HttpWebRequest httpWebRequest = WebRequest.Create(text) as HttpWebRequest;
            httpWebRequest.Method = req.GetMethod().ToString();
            httpWebRequest.KeepAlive = KeepAlive;
            httpWebRequest.Timeout = Timeout;
            httpWebRequest.ContentType = ContentType;
            httpWebRequest.Accept = "*/*";
            httpWebRequest.UserAgent = UserAgent;
            if (dictionary.TryGetValue(HttpMember.Header, out Dictionary<string, object> dictionary2))
            {
                foreach (KeyValuePair<string, object> keyValuePair in dictionary2)
                {
                    httpWebRequest.Headers.Add(keyValuePair.Key, keyValuePair.Value.ToString());
                }
            }
            if (dictionary.TryGetValue(HttpMember.Body, out Dictionary<string, object> paras))
            {
                byte[] array = BuildBody(paras);
                httpWebRequest.ContentLength = array.Length;
                using (Stream requestStream = httpWebRequest.GetRequestStream())
                {
                    requestStream.Write(array, 0, array.Length);
                    return httpWebRequest;
                }
            }
            httpWebRequest.ContentLength = 0L;
            return httpWebRequest;
        }
        public virtual Response Execute(IRequest req)
        {
            Response result;
            try
            {
                using (HttpWebResponse httpWebResponse = BuildRequest(req).GetResponse() as HttpWebResponse)
                {
                    result = new Response(httpWebResponse.StatusCode, null);
                }
            }
            catch (Exception ex)
            {
                if (ex is WebException && (ex as WebException).Response != null)
                {
                    using (HttpWebResponse httpWebResponse2 = (ex as WebException).Response as HttpWebResponse)
                    {
                        return new Response(httpWebResponse2.StatusCode, ReadError(httpWebResponse2));
                    }
                }
                result = new Response(HttpStatusCode.InternalServerError, ex.Message);
            }
            return result;
        }
        public virtual Response<T> Execute<T>(IRequest<T> req)
        {
            Response<T> result;
            try
            {
                using (HttpWebResponse httpWebResponse = BuildRequest(req).GetResponse() as HttpWebResponse)
                {
                    result = new Response<T>(httpWebResponse.StatusCode, ReadBody<T>(httpWebResponse), null);
                }
            }
            catch (Exception ex)
            {
                if (ex is WebException && (ex as WebException).Response != null)
                {
                    using (HttpWebResponse httpWebResponse2 = (ex as WebException).Response as HttpWebResponse)
                    {
                        return new Response<T>(httpWebResponse2.StatusCode, default(T), ReadError(httpWebResponse2));
                    }
                }
                result = new Response<T>(HttpStatusCode.InternalServerError, default(T), ex.Message);
            }
            return result;
        }
        public IAsyncResult BeginExecute(IRequest req, AsyncCallback callback)
        {
            return BuildRequest(req).BeginGetResponse(callback, req);
        }
    }
}
