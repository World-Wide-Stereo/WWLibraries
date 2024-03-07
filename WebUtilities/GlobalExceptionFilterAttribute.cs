using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Reflection;
using System.Web;
using System.Web.Http.Filters;
using ww.Utilities;
using ww.Utilities.Extensions;
using ww.WebUtilities.Extensions;

namespace ww.WebUtilities
{
    public class SkipGlobalExceptionFilterAttribute : Attribute { }
    public class GlobalExceptionFilterAttribute : ExceptionFilterAttribute
    {
        public static void GlobalExceptionHandler(Exception ex, HttpRequestMessage requestMessage = null, HttpRequest request = null)
        {
            string ipAddress = requestMessage.GetIPAddress();
            HttpException httpEx = ex as HttpException;
            if (httpEx != null && ((HttpStatusCode)httpEx.GetHttpCode()).EqualsAnyOf(HttpStatusCode.NotFound, HttpStatusCode.BadRequest))
            {
                //SuspiciousAPIUser.LogBadRequest(ipAddress);
                return;
            }

            string body;
            if (requestMessage != null)
            {
                body = $"URL: {requestMessage.RequestUri}{Environment.NewLine}Client IP: {ipAddress}{Environment.NewLine}Content Type: {requestMessage.Content.Headers.ContentType}{Environment.NewLine}Content Length: {requestMessage.Content.Headers.ContentLength}{Environment.NewLine}{Environment.NewLine}Headers:{Environment.NewLine}{requestMessage.Headers.ToString().Trim()}{Environment.NewLine}{Environment.NewLine}Request:{Environment.NewLine}{requestMessage.Content.ReadAsStringAsync().Result}{Environment.NewLine}{Environment.NewLine}Exception:{Environment.NewLine}{ex}";
            }
            else if (request != null)
            {
                string rawRequest;
                using (var streamReader = new StreamReader(request.InputStream))
                {
                    rawRequest = streamReader.ReadToEnd();
                }
                body = $"URL: {request.Url}{Environment.NewLine}Client IP: {request.UserHostAddress}{Environment.NewLine}Content Type: {request.Headers["Content-Type"]}{Environment.NewLine}Content Length: {request.Headers["Content-Length"]}{Environment.NewLine}{Environment.NewLine}Headers:{Environment.NewLine}{request.Headers.AllKeys.Except(new[] { "Content-Type", "Content-Length" }).Select(x => $"{x}: {request.Headers[x]}").Join(Environment.NewLine)}{Environment.NewLine}{Environment.NewLine}Request:{Environment.NewLine}{rawRequest}{Environment.NewLine}{Environment.NewLine}Exception:{Environment.NewLine}{ex}";
            }
            else
            {
                body = ex.ToString();
            }
            Email.sendAlertEmail($"{WebUtilityFunctions.GetEntryAssembly()?.GetName().Name ?? Assembly.GetExecutingAssembly().GetName().Name} Exception", body, priority: MailPriority.High);
        }

        public override void OnException(HttpActionExecutedContext context)
        {
            if (context.ActionContext.ControllerContext.Controller.GetType().GetCustomAttribute(typeof(SkipGlobalExceptionFilterAttribute)) == null)
            {
                GlobalExceptionHandler(context.Exception, requestMessage: context.Request);
            }
        }
    }
}
