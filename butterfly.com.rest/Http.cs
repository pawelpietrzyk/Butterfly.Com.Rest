using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace butterfly.com.rest
{
    public enum HttpMethod
    {
        GET,
        POST,
        PUT,
        DELETE
    }
    
    public class Http
    {
        public const string DefaultContentType = "text/xml";
        public static Http get(string url, HttpParams parameters, string contentType = Http.DefaultContentType, int timeout = 5000)
        {
            return Http.method(HttpMethod.GET, url, String.Empty, parameters, contentType, timeout);            
        }
        public static Http post(string url, string data, HttpParams parameters, string contentType = Http.DefaultContentType)
        {
            return Http.method(HttpMethod.POST, url, data, parameters, contentType);            
        }
        public static Http put(string url, string data, HttpParams parameters, string contentType = Http.DefaultContentType)
        {
            return Http.method(HttpMethod.PUT, url, data, parameters, contentType);            
        }
        public static Http delete(string url, HttpParams parameters, string contentType = Http.DefaultContentType)
        {
            return Http.method(HttpMethod.DELETE, url, String.Empty, parameters, contentType);            
        }
        public static Http method(HttpMethod method, string url, string data, HttpParams parameters, string contentType = Http.DefaultContentType, int timeout = 5000)
        {
            Http http = new Http();
            http.Method = method;
            http.Url = url;
            http.Data = data;
            http.Params = parameters;
            http.ContentType = contentType;
            http.Timeout = timeout;
            return http;
        }
        public Http()
        {
            this.Params = new HttpParams();
        }
        public Http(HttpResponseEventHandler responseCallback) : this()
        {
            this.responseCallback = responseCallback;
        }
        public HttpMethod Method { get; set; }
        public string ContentType { get; set; }
        public string Data { get; set; }
        public string Url { get; set; }
        public int Timeout { get; set; }
        public HttpParams Params { get; set; }

        public Http then(HttpResponseEventHandler callback)
        {
            this.responseCallback = callback;            
            return this;
        }
        public void Async()
        {
            this.executeAsync();
        }
        protected WebRequest createRequest()
        {
            HttpWebRequest request = HttpWebRequest.Create(this.RequestUri) as HttpWebRequest;
            if (request != null)
            {
                request.Timeout = this.Timeout;
                request.ContentType = this.ContentType;
                request.Method = this.Method.ToString();
                request.ContentLength = 0;
                if (!String.IsNullOrEmpty(this.Data))
                {
                    byte[] bytes = ASCIIEncoding.ASCII.GetBytes(this.Data);
                    request.ContentLength = bytes.Length;
                    using (var writeStream = request.GetRequestStream())
                    {
                        writeStream.Write(bytes, 0, bytes.Length);
                    }
                }
            }
            return request;
        }
        protected string readResponseContent(HttpWebResponse response)
        {
            string responseValue = String.Empty;
            using (var responseStream = response.GetResponseStream())
            {
                if (responseStream != null)
                {
                    using (var reader = new StreamReader(responseStream))
                    {
                        responseValue = reader.ReadToEnd();
                        reader.Close();
                    }
                }
            }
            return responseValue;
        }
        protected WebResponse execute()
        {
            WebRequest request = this.createRequest();            
            return request.GetResponse();
        }
        protected void executeAsync()
        {
            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += worker_DoWork;
            worker.RunWorkerCompleted += worker_RunWorkerCompleted;
            worker.RunWorkerAsync(this.createRequest());
        }

        void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (!e.Cancelled)
            {
                if (e.Error == null)
                {
                    HttpWebResponse response = e.Result as HttpWebResponse;
                    if (response != null)
                    {
                        this.callResponseCallback(response.StatusCode, this.readResponseContent(response));
                    }
                }
                else
                {
                    Exception ex = e.Error as Exception;
                    if (ex != null)
                    {
                        this.callResponseCallback(HttpStatusCode.InternalServerError, ex.Message);
                    }
                }
            }
        }

        void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            WebRequest request = e.Argument as WebRequest;
            if (request != null)
            {
                //try
                {
                    e.Result = request.GetResponse();
                    
                }
                //catch (Exception ex)
                {
                    //e.Result = ex;
                }
            }
        }
        public string RequestUri
        {
            get
            {
                if (this.Params.Count > 0)
                {
                    return String.Format("{0}?{1}", this.Url, this.Params.ToString());
                }
                else
                {
                    return this.Url;
                }                
            }            
        }
        public void AddParam(HttpParam param)
        {
            if (!this.Params.Contains(param))
            {
                this.Params.Add(param);
            }            
        }
        public void AddParam(string name, string value)
        {
            this.AddParam(new HttpParam(name, value));
        }
        private void callResponseCallback(HttpStatusCode code, string content)
        {
            if (responseCallback != null)
            {
                HttpResponseEventArgs args = new HttpResponseEventArgs();
                args.StatusCode = code;
                args.Content = content;
                responseCallback(this, args);
            }
        }
        protected HttpResponseEventHandler responseCallback;
    }
    public class HttpResponseEventArgs
    {
        public HttpStatusCode StatusCode { get; set; }
        public string Content { get; set; }
    }
    public delegate void HttpResponseEventHandler(object sender, HttpResponseEventArgs e);
    public class HttpParam
    {
        public string Name { get; set; }
        public string Value { get; set; }

        public HttpParam()
        {
            this.Name = String.Empty;
            this.Value = String.Empty;
        }
        public HttpParam(string name, string value)
        {
            this.Name = name;
            this.Value = value;
        }
        public override string ToString()
        {
            return String.Format("{0}={1}", new object[] { this.Name, this.Value} );
        }
        public bool Equals(HttpParam obj)
        {
            bool ret = false;
            if (obj != null)
            {
                if (this.Name.Equals(obj.Name) && this.Value.Equals(obj.Value))
                {
                    ret = true;
                }
            }
            return ret;
        }
        public override bool Equals(object obj)
        {
            return this.Equals(obj as HttpParam);
        }
        public override int GetHashCode()
        {
            int hashCodeName = (this.Name != null ? this.Name.GetHashCode() : 0);
            int hashCodeValue = (this.Value != null ? this.Value.GetHashCode() : 0);
            return hashCodeName ^ hashCodeValue;
        }
    }
    public class HttpParams : List<HttpParam>
    {
        public HttpParams() : base()
        {
            this.Separator = "&";
        }
        public HttpParams(string separator) : base()
        {
            this.Separator = separator;
        }
        public string Separator { get; set; }
        public override string ToString()
        {
            List<string> list = new List<string>();
            foreach (HttpParam param in this)
            {
                list.Add(param.ToString());
            }
            string[] tmp = list.ToArray();
            return String.Join(this.Separator, tmp);
        }
        public void Add(string name, string value)
        {
            this.Add(new HttpParam() { Name = name, Value = value });
        }

    }
    

    
}
