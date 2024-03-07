using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using ww.Utilities.Extensions;

namespace ww.Utilities
{
    public static class HTTP
    {
        #region GET
        /// <param name="acceptInvalidCertificate">WARNING: Setting this to true is a major security risk when calling a server not in our control.</param>
        public static string Get(string url, string username, string password, IDictionary<string, string> additionalHeaders = null, DataType? dataType = null, int? timeout = null, string userAgentString = null, WebProxy proxy = null, bool acceptInvalidCertificate = false)
        {
            if (additionalHeaders == null)
            {
                additionalHeaders = new Dictionary<string, string>();
            }
            additionalHeaders.Add("Authorization", "Basic " + (username + ":" + password).ToBase64String());
            return Get(url, additionalHeaders, dataType, timeout, userAgentString, proxy, acceptInvalidCertificate);
        }
        /// <param name="acceptInvalidCertificate">WARNING: Setting this to true is a major security risk when calling a server not in our control.</param>
        public static string Get(string url, IDictionary<string, string> additionalHeaders = null, DataType? dataType = null, int? timeout = null, string userAgentString = null, WebProxy proxy = null, bool acceptInvalidCertificate = false)
        {
            using (WebResponse response = GetWebResponse(url, additionalHeaders, dataType, timeout, userAgentString, proxy, acceptInvalidCertificate))
            {
                using (Stream stream = response.GetResponseStream())
                {
                    using (var streamReader = new StreamReader(stream))
                    {
                        return streamReader.ReadToEnd();
                    }
                }
            }
        }

        /// <param name="acceptInvalidCertificate">WARNING: Setting this to true is a major security risk when calling a server not in our control.</param>
        public static HttpStatusCode? GetResponseStatusCode(string url, IDictionary<string, string> additionalHeaders = null, DataType? dataType = null, int? timeout = null, string userAgentString = null, WebProxy proxy = null, bool acceptInvalidCertificate = false)
        {
            try
            {
                using (WebResponse response = GetWebResponse(url, additionalHeaders, dataType, timeout, userAgentString, proxy, acceptInvalidCertificate))
                {
                    return ((HttpWebResponse)response).StatusCode;
                }
            }
            catch (WebException ex)
            {
                using (var response = (HttpWebResponse)ex.Response)
                {
                    return response?.StatusCode;
                }
            }
        }

        /// <summary>
        /// Reads the web page only up to the title tag, returning the title itself or empty string when none exists.<para/>
        /// Taken from https://stackoverflow.com/questions/11652883/how-to-get-webpage-title-without-downloading-all-the-page-source
        /// </summary>
        /// <param name="acceptInvalidCertificate">WARNING: Setting this to true is a major security risk when calling a server not in our control.</param>
        public static string GetTitle(string url, IDictionary<string, string> additionalHeaders = null, DataType? dataType = null, int? timeout = null, string userAgentString = null, WebProxy proxy = null, bool acceptInvalidCertificate = false)
        {
            using (WebResponse response = GetWebResponse(url, additionalHeaders, dataType, timeout, userAgentString, proxy, acceptInvalidCertificate))
            {
                using (Stream stream = response.GetResponseStream())
                {
                    string title = "";
                    // Compiled regex to check for <title></title> block.
                    var titleCheck = new Regex(@"<title.*>\s*(.+?)\s*</title>", RegexOptions.Compiled | RegexOptions.IgnoreCase);
                    const int bytesToRead = 8092;
                    var buffer = new byte[bytesToRead];
                    string contents = "";
                    int length;
                    while ((length = stream.Read(buffer, 0, bytesToRead)) > 0)
                    {
                        // Convert the byte-array to a string and add it to the rest of the
                        // contents that have been downloaded so far.
                        contents += Encoding.UTF8.GetString(buffer, 0, length);

                        Match m = titleCheck.Match(contents);
                        if (m.Success)
                        {
                            // We found a <title></title> match =].
                            title = m.Groups[1].Value;
                            break;
                        }
                        else if (contents.Contains("</head>"))
                        {
                            // Reached end of head-block; no title found =[.
                            break;
                        }
                    }
                    return title;
                }
            }
        }

        private static WebResponse GetWebResponse(string url, IDictionary<string, string> additionalHeaders, DataType? dataType, int? timeout, string userAgentString, WebProxy proxy, bool acceptInvalidCertificate)
        {
            HttpWebRequest objRequest = (HttpWebRequest)WebRequest.Create(url);
            objRequest.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            if (additionalHeaders != null && additionalHeaders.Count > 0)
            {
                foreach (KeyValuePair<string, string> header in additionalHeaders)
                {
                    objRequest.Headers.Add(header.Key, header.Value);
                }
            }
            if (dataType != null)
            {
                objRequest.Accept = dataType.Value.GetLabel();
            }
            if (timeout != null)
            {
                objRequest.Timeout = timeout.Value;
            }
            if (userAgentString != null)
            {
                objRequest.UserAgent = userAgentString;
            }
            if (proxy != null)
            {
                objRequest.Proxy = proxy;
            }
            if (acceptInvalidCertificate)
            {
                objRequest.ServerCertificateValidationCallback = delegate { return true; };
            }

            return objRequest.GetResponse();
        }
        #endregion

        #region PUT
        /// <param name="acceptInvalidCertificate">WARNING: Setting this to true is a major security risk when calling a server not in our control.</param>
        public static string Put(SendType type, string url, string data, DataType datatype = DataType.XML, IDictionary<string, string> additionalHeaders = null, bool acceptInvalidCertificate = false)
        {
            try
            {
                using (WebResponse response = PutWebResponse(type, url, Encoding.UTF8.GetBytes(data), datatype, additionalHeaders, acceptInvalidCertificate))
                {
                    using (Stream stream = response.GetResponseStream())
                    {
                        using (var streamReader = new StreamReader(stream))
                        {
                            return streamReader.ReadToEnd();
                        }
                    }
                }
            }
            catch (WebException ex)
            {
                WebException newEx = PutWebResponseCatch(ex);
                if (newEx != null)
                {
                    throw newEx;
                }
                throw;
            }
        }
        /// <param name="acceptInvalidCertificate">WARNING: Setting this to true is a major security risk when calling a server not in our control.</param>
        public static string Put(SendType type, string url, FileInfo file, DataType datatype = DataType.XML, IDictionary<string, string> additionalHeaders = null, bool acceptInvalidCertificate = false)
        {
            try
            {
                using (var fs = new StreamReader(file.OpenRead()))
                {
                    using (WebResponse response = PutWebResponse(type, url, Encoding.UTF8.GetBytes(fs.ReadToEnd()), datatype, additionalHeaders, acceptInvalidCertificate))
                    {
                        using (Stream stream = response.GetResponseStream())
                        {
                            using (var streamReader = new StreamReader(stream))
                            {
                                return streamReader.ReadToEnd();
                            }
                        }
                    }
                }
            }
            catch (WebException ex)
            {
                WebException newEx = PutWebResponseCatch(ex);
                if (newEx != null)
                {
                    throw newEx;
                }
                throw;
            }
        }

        /// <param name="acceptInvalidCertificate">WARNING: Setting this to true is a major security risk when calling a server not in our control.</param>
        public static string PutGZIP(SendType type, string url, FileInfo file, DataType datatype = DataType.XML, IDictionary<string, string> additionalHeaders = null, bool acceptInvalidCertificate = false)
        {
            byte[] arr;
            using (var fs = new StreamReader(file.OpenRead()))
            {
                arr = Encoding.UTF8.GetBytes(fs.ReadToEnd());
            }

            // Prepare for compress
            using (var ms = new MemoryStream())
            {
                using (var sw = new GZipStream(ms, CompressionMode.Compress))
                {
                    // Compress
                    sw.Write(arr, 0, arr.Length);

                    // Transform byte[] zip data to string
                    arr = ms.ToArray();
                }
            }

            try
            {
                using (WebResponse response = PutWebResponse(type, url, arr, datatype, additionalHeaders, acceptInvalidCertificate))
                {
                    using (Stream stream = response.GetResponseStream())
                    {
                        using (var streamReader = new StreamReader(stream))
                        {
                            return streamReader.ReadToEnd();
                        }
                    }
                }
            }
            catch (WebException ex)
            {
                WebException newEx = PutWebResponseCatch(ex);
                if (newEx != null)
                {
                    throw newEx;
                }
                throw;
            }
        }

        private static WebResponse PutWebResponse(SendType type, string url, byte[] data, DataType dataType, IDictionary<string, string> additionalHeaders, bool acceptInvalidCertificate)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            request.Method = type.ToString();
            request.ContentType = dataType.GetLabel();
            if (additionalHeaders != null && additionalHeaders.Count > 0)
            {
                foreach (KeyValuePair<string, string> header in additionalHeaders)
                {
                    request.Headers.Add(header.Key, header.Value);
                }
            }
            if (acceptInvalidCertificate)
            {
                request.ServerCertificateValidationCallback = delegate { return true; };
            }

            request.ContentLength = data.Length;
            using (Stream dataStream = request.GetRequestStream())
            {
                dataStream.Write(data, 0, data.Length);
            }

            return request.GetResponse();
        }

        private static WebException PutWebResponseCatch(WebException ex)
        {
            using (WebResponse response = ex.Response)
            {
                using (Stream stream = response.GetResponseStream())
                {
                    if (stream != null)
                    {
                        using (var streamReader = new StreamReader(stream))
                        {
                            throw new WebException(streamReader.ReadToEnd(), ex);
                        }
                    }
                }
            }
            return null;
        }
        #endregion

        /// <summary>
        /// Makes a web request to a URL. It can return deserialized data, the raw response when the type is set to string or byte[], or a temp file containing the raw response when the type is set to FileInfo.
        /// </summary>
        /// <param name="acceptInvalidCertificate">WARNING: Setting this to true is a major security risk when calling a server not in our control.</param>
        public static TReturnType SendWebRequest<TReturnType>(SendType eSendType, string strUrl, WebHeaderCollection objHeaders = null, string strPostData = null, byte[] bytPostData = null, string strContentType = null, string strAccept = null, Encoding encoding = null, DataType responseDataType = DataType.JSON, bool acceptInvalidCertificate = false)
        {
            return SendWebRequest<TReturnType>(out _, eSendType, strUrl, objHeaders: objHeaders, strPostData: strPostData, bytPostData: bytPostData, strContentType: strContentType, strAccept: strAccept, encoding: encoding, responseDataType: responseDataType, acceptInvalidCertificate: acceptInvalidCertificate);
        }
        /// <summary>
        /// Makes a web request to a URL. It can return deserialized data, the raw response when the type is set to string or byte[], or a temp file containing the raw response when the type is set to FileInfo.
        /// </summary>
        /// <param name="acceptInvalidCertificate">WARNING: Setting this to true is a major security risk when calling a server not in our control.</param>
        public static TReturnType SendWebRequest<TReturnType>(out string rawResponse, SendType eSendType, string strUrl, WebHeaderCollection objHeaders = null, string strPostData = null, byte[] bytPostData = null, string strContentType = null, string strAccept = null, Encoding encoding = null, DataType responseDataType = DataType.JSON, bool acceptInvalidCertificate = false)
        {
            HttpWebRequest objRequest = (HttpWebRequest)WebRequest.Create(strUrl);
            objRequest.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            objRequest.Method = eSendType.ToString();

            if (encoding == null)
                encoding = Encoding.UTF8;
            if (objHeaders != null)
                objRequest.Headers = objHeaders;
            if (!strAccept.IsNullOrBlank())
                objRequest.Accept = strAccept;
            if (acceptInvalidCertificate)
                objRequest.ServerCertificateValidationCallback = delegate { return true; };
            // Create the post data for the JSON object.
            objRequest.ContentLength = 0;
            if ((!strPostData.IsNullOrBlank() || bytPostData != null) && !strContentType.IsNullOrBlank())
            {
                if (bytPostData == null)
                    bytPostData = encoding.GetBytes(strPostData);

                objRequest.ContentType = strContentType;
                objRequest.ContentLength = bytPostData.Length;

                using (var objStream = objRequest.GetRequestStream())
                    objStream.Write(bytPostData, 0, bytPostData.Length);
            }

            try
            {
                using (var objResponse = (HttpWebResponse)objRequest.GetResponse())
                {
                    using (var objResponseStream = objResponse.GetResponseStream())
                    {
                        if (objResponseStream == null)
                            throw new WebException("No response to web request.", WebExceptionStatus.UnknownError);

                        byte[] bytes;
                        using (var memoryStream = new MemoryStream())
                        {
                            objResponseStream.CopyTo(memoryStream);
                            bytes = memoryStream.ToArray();
                            rawResponse = encoding.GetString(bytes);
                        }

                        Type type = typeof(TReturnType);
                        if (type == typeof(string))
                        {
                            return (TReturnType)(object)rawResponse;
                        }
                        if (type == typeof(byte[]) || type == typeof(IEnumerable<byte>))
                        {
                            return (TReturnType)(object)bytes;
                        }
                        if (type == typeof(FileInfo))
                        {
                            var strFileName = Path.GetTempPath() + Path.GetRandomFileName();
                            File.WriteAllBytes(strFileName, bytes);
                            return (TReturnType)(object)new FileInfo(strFileName);
                        }

                        switch (responseDataType)
                        {
                            case DataType.XML:
                            case DataType.Text_XML:
                                return XMLSerializer.Deserialize<TReturnType>(rawResponse);
                            case DataType.JSON:
                            default:
                                return JsonConvert.DeserializeObject<TReturnType>(rawResponse);
                        }
                    }
                }
            }
            catch (WebException ex)
            {
                using (var response = ex.Response)
                {
                    if (response == null)
                        throw;

                    using (var errorStream = response.GetResponseStream())
                    {
                        if (errorStream == null)
                            throw;

                        using (var errorReader = new StreamReader(errorStream))
                            throw new WebException(errorReader.ReadToEnd(), ex, ex.Status, ex.Response);
                    }
                }
            }
        }

        #region Enums
        public enum DataType
        {
            [EnumLabel("application/text")]
            Text,
            [EnumLabel("application/xml")]
            XML,
            [EnumLabel("application/json")]
            JSON,
            [EnumLabel("application/graphql")]
            GraphQL,
            [EnumLabel("text/xml")]
            Text_XML,
            [EnumLabel("text/html")]
            Text_HTML,
            None,
        }

        public enum SendType
        {
            POST,
            PUT,
            PATCH,
            GET,
            DELETE,
        }
        #endregion
    }
}
