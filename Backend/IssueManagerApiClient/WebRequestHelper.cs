using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace IssueManagerApiClient
{
    internal enum HttpVerb
    {
        Get,
        Post,
        Put,
        Delete
    }

    internal enum ContentFormat
    {
        Text,
        Json,
        XWwwFormUrlencoded
    }

    internal class RequestContent
    {
        public RequestContent(byte[] data, ContentFormat format)
        {
            Data = data;
            Format = format;
        }

        public byte[] Data { get; }
        public ContentFormat Format { get; }
    }

    internal class WebRequestHelper
    {
        public WebRequestHelper(string host, string storedAccessToken)
        {
            Host = host;
            AccessToken = storedAccessToken;
        }

        public string Host { get; }
        public string AccessToken { get; set; }

        public HttpWebRequest CreateRequest(HttpVerb verb, string action, bool needsAuthentication = true)
        {
            HttpWebRequest request = (HttpWebRequest) WebRequest.Create(Host + action);
            request.Method = verb.ToString().ToUpper();
            if (needsAuthentication)
                request.Headers[HttpRequestHeader.Authorization] = "Bearer " + AccessToken;

            return request;
        }

        public dynamic GetDynamicResponse(HttpWebRequest request, RequestContent content = null)
        {
            try
            {
                string json = Task.Run(async () =>
                {
                    string j = await GetResponseAsync(request, content);
                    return j;
                }).Result;

                if (json == null)
                    return null;

                return DecodeJsonDynamic(json);
            }
            catch (AggregateException e)
            {
                Exception ex = e.InnerExceptions.FirstOrDefault();
                if (ex != null)
                    throw ex;
                throw;
            }
        }

        public dynamic DecodeJsonDynamic(string json)
        {
            try
            {
                object obj = JsonConvert.DeserializeObject(json);
                if (obj == null)
                    throw new Exception();
                return obj;
            }
            catch (Exception)
            {
                throw new BadResponseFormatException();
            }
        }

        public T GetTypedResponse<T>(HttpWebRequest request, RequestContent content = null) where T : class
        {
            try
            {
                string json = Task.Run(async () =>
                {
                    string j = await GetResponseAsync(request, content);
                    return j;
                }).Result;

                if (json == null)
                    return null;

                return DecodeJsonTyped<T>(json);
            }
            catch (AggregateException e)
            {
                Exception ex = e.InnerExceptions.FirstOrDefault();
                if (ex != null)
                    throw ex;
                throw;
            }
        }

        public T DecodeJsonTyped<T>(string json)
        {
            try
            {
                object obj = JsonConvert.DeserializeObject(json, typeof(T));
                if (obj == null)
                    throw new Exception();
                return (T) obj;
            }
            catch (Exception)
            {
                throw new BadResponseFormatException();
            }
        }


        public async Task<Tuple<byte[], string>> GetResponseBytesAsync(HttpWebRequest request)
        {
            HttpWebResponse response;
            try
            {
                response = (HttpWebResponse) await request.GetResponseAsync();
                using (response)
                {
                    switch (response.StatusCode)
                    {
                        case HttpStatusCode.OK: break;
                        case HttpStatusCode.NoContent: return null;
                        default:
                            ThrowException(response.StatusCode);
                            break;
                    }

                    Stream responseDataStream = response.GetResponseStream();
                    using (MemoryStream result = new MemoryStream())
                    {
                        if (responseDataStream != null) await responseDataStream.CopyToAsync(result);

                        return new Tuple<byte[], string>(result.ToArray(), response.ContentType);
                    }
                }
            }
            catch (WebException e)
            {
                response = e.Response as HttpWebResponse;
                if (response != null)
                    ThrowException(response.StatusCode);
                throw;
            }
        }

        public async Task<string> GetResponseAsync(HttpWebRequest request, RequestContent content = null)
        {
            if (content != null)
            {
                switch (content.Format)
                {
                    case ContentFormat.Text:
                        request.ContentType = "text/plain";
                        break;
                    case ContentFormat.Json:
                        request.ContentType = "application/json; charset=utf-8";
                        break;
                    case ContentFormat.XWwwFormUrlencoded:
                        request.ContentType = "application/x-www-form-urlencoded";
                        break;
                    default: throw new ArgumentOutOfRangeException("Invalid content format: " + content.Format);
                }


                using (Stream stream = await request.GetRequestStreamAsync())
                {
                    stream.Write(content.Data, 0, content.Data.Length);
                }
            }

            HttpWebResponse response;
            try
            {
                response = (HttpWebResponse) await request.GetResponseAsync();
                using (response)
                {
                    switch (response.StatusCode)
                    {
                        case HttpStatusCode.OK: break;
                        case HttpStatusCode.NoContent: return null;
                        default:
                            ThrowException(response.StatusCode);
                            break;
                    }

                    Stream responseDataStream = response.GetResponseStream();
                    using (StreamReader reader = new StreamReader(responseDataStream))
                    {
                        return reader.ReadToEnd();
                    }
                }
            }
            catch (WebException e)
            {
                response = e.Response as HttpWebResponse;
                if (response != null)
                    ThrowException(response.StatusCode);
                throw;
            }
        }

        public void ThrowException(HttpStatusCode statusCode)
        {
            switch (statusCode)
            {
                case HttpStatusCode.Unauthorized: throw new UnauthorizedException();
                case HttpStatusCode.BadGateway:
                case HttpStatusCode.ServiceUnavailable:
                case HttpStatusCode.NotFound: throw new ServerNotFoundException();
                default: throw new UnexpectedHttpErrorException(statusCode);
            }
        }
    }
}