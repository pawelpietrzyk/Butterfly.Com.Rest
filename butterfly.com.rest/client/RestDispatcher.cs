using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace butterfly.com.rest.client
{
    public abstract class RestDispatcher
    {        
        public void DispatchRequest(HttpContext context)
        {
            switch (context.Request.HttpMethod)
            {
                case "GET":
                    this.GET(context);
                    break;
                case "POST":
                    this.POST(context);
                    break;
                case "DELETE":
                    this.DELETE(context);
                    break;
                default: break;
            }
        }
        public void ResultOK(HttpContext context)
        {
            if (context != null)
            {
                context.Response.Status = "200 OK";
                context.Response.StatusCode = 200;
            }
        }
        public static RestMethod DecodeRestMethod(HttpContext context)
        {
            RestMethod method = Serializer.Deserialize(context.Request.InputStream, typeof(RestMethod)) as RestMethod;
            
            return method;
        }
        public abstract void DispatchRestMethod(RestMethod method);
        public abstract void GET(HttpContext context);
        public abstract void POST(HttpContext context);
        public abstract void DELETE(HttpContext context);
    }
}
