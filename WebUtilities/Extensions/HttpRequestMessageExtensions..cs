using System.Diagnostics;
using System.Net.Http;
using System.ServiceModel.Channels;
using System.Web;

namespace ww.WebUtilities.Extensions
{
    [DebuggerStepThrough]
    public static class HttpRequestMessageExtensions
    {
        public static string GetIPAddress(this HttpRequestMessage request)
        {
            if (request.Properties.ContainsKey("MS_HttpContext"))
            {
                return ((HttpContextWrapper)request.Properties["MS_HttpContext"]).Request.UserHostAddress;
            }
            if (request.Properties.ContainsKey(RemoteEndpointMessageProperty.Name))
            {
                return ((RemoteEndpointMessageProperty)request.Properties[RemoteEndpointMessageProperty.Name]).Address;
            }
            if (HttpContext.Current != null)
            {
                return HttpContext.Current.Request.UserHostAddress;
            }
            return null;
        }
    }
}
