using CommonPlayniteShared;
using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace CommonPluginsShared
{
    // TODO https://stackoverflow.com/questions/62802238/very-slow-httpclient-sendasync-call

    public enum WebUserAgentType
    {
        Request
    }


    public class HttpHeader
    {
        public string Key { get; set; }
        public string Value { get; set; }
    }


    public class Web
    {
        private static ILogger Logger => LogManager.GetLogger();

        public static string UserAgent = $"Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:126.0) Gecko/20100101 Firefox/126.0 Playnite/{API.Instance.ApplicationInfo.ApplicationVersion.ToString(2)}";


        private static string StrWebUserAgentType(WebUserAgentType UserAgentType)
        {
            switch (UserAgentType)
            {
                case WebUserAgentType.Request:
                    return "request";
                default:
                    break;
            }
            return string.Empty;
        }


        /// <summary>
        /// Download file image and resize in icon format (64x64).
        /// </summary>
        /// <param name="ImageFileName"></param>
        /// <param name="url"></param>
        /// <param name="ImagesCachePath"></param>
        /// <param name="PluginName"></param>
        /// <returns></returns>
        public static Task<bool> DownloadFileImage(string ImageFileName, string url, string ImagesCachePath, string PluginName)
        {
            string PathImageFileName = Path.Combine(ImagesCachePath, PluginName.ToLower(), ImageFileName);

            if (!StringExtensions.IsHttpUrl(url))
            {
                return Task.FromResult(false);
            }

            using (var client = new HttpClient())
            {
                try
                {
                    var cachedFile = HttpFileCache.GetWebFile(url);
                    if (string.IsNullOrEmpty(cachedFile))
                    {
                        //logger.Warn("Web file not found: " + url);
                        return Task.FromResult(false);
                    }

                    ImageTools.Resize(cachedFile, 64, 64, PathImageFileName);
                }
                catch (Exception ex)
                {
                    if (!url.Contains("steamcdn-a.akamaihd.net", StringComparison.InvariantCultureIgnoreCase) && !ex.Message.Contains("(403)"))
                    {
                        Common.LogError(ex, false, $"Error on download {url}");
                    }
                    return Task.FromResult(false);
                }
            }

            // Delete file is empty
            try
            {
                if (File.Exists(PathImageFileName + ".png"))
                {
                    FileInfo fi = new FileInfo(PathImageFileName + ".png");
                    if (fi.Length == 0)
                    {
                        File.Delete(PathImageFileName + ".png");
                    }
                }
            }
            catch (Exception ex)
            {
                Common.LogError(ex, false, $"Error on delete file image");
                return Task.FromResult(false);
            }

            return Task.FromResult(true);
        }

        public static async Task<bool> DownloadFileImageTest(string url)
        {
            if (!url.ToLower().Contains("http"))
            {
                return false;
            }

            using (var client = new HttpClient())
            {
                try
                {
                    client.DefaultRequestHeaders.Add("User-Agent", Web.UserAgent);
                    HttpResponseMessage response = await client.GetAsync(url).ConfigureAwait(false);

                    if (response.StatusCode != HttpStatusCode.OK)
                    {
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    Common.LogError(ex, false, $"Error on download {url}");
                    return false;
                }
            }

            return true;
        }


        /// <summary>
        /// Download file stream.
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static async Task<Stream> DownloadFileStream(string url)
        {
            using (var client = new HttpClient())
            {
                try
                {
                    client.DefaultRequestHeaders.Add("User-Agent", Web.UserAgent);
                    HttpResponseMessage response = await client.GetAsync(url).ConfigureAwait(false);
                    return await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    Common.LogError(ex, false, $"Error on download {url}");
                    return null;
                }
            }
        }

        public static async Task<Stream> DownloadFileStream(string url, List<HttpCookie> Cookies)
        {
            HttpClientHandler handler = new HttpClientHandler();
            if (Cookies != null)
            {
                CookieContainer cookieContainer = new CookieContainer();

                foreach (var cookie in Cookies)
                {
                    Cookie c = new Cookie();
                    c.Name = cookie.Name;
                    c.Value = Tools.FixCookieValue(cookie.Value);
                    c.Domain = cookie.Domain;
                    c.Path = cookie.Path;

                    try
                    {
                        cookieContainer.Add(c);
                    }
                    catch (Exception ex)
                    {
                        Common.LogError(ex, true);
                    }
                }

                handler.CookieContainer = cookieContainer;
            }

            using (var client = new HttpClient(handler))
            {
                try
                {
                    client.DefaultRequestHeaders.Add("User-Agent", Web.UserAgent);
                    HttpResponseMessage response = await client.GetAsync(url).ConfigureAwait(false);
                    return await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    Common.LogError(ex, false, $"Error on download {url}");
                    return null;
                }
            }
        }


        /// <summary>
        /// Download string data and keep url parameter when there is a redirection.
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static async Task<string> DownloadStringDataKeepParam(string url)
        {
            using (var client = new HttpClient())
            {
                var request = new HttpRequestMessage()
                {
                    RequestUri = new Uri(url),
                    Method = HttpMethod.Get
                };

                HttpResponseMessage response;
                try
                {
                    client.DefaultRequestHeaders.Add("User-Agent", Web.UserAgent);
                    response = await client.SendAsync(request).ConfigureAwait(false);

                    var uri = response.RequestMessage.RequestUri.ToString();
                    if (uri != url)
                    {
                        var urlParams = url.Split('?').ToList();
                        if (urlParams.Count == 2)
                        {
                            uri += "?" + urlParams[1];
                        }
                        
                        return await DownloadStringDataKeepParam(uri);
                    }
                }
                catch (Exception ex)
                {
                    Common.LogError(ex, false, $"Error on download {url}");
                    return string.Empty;
                }

                if (response == null)
                {
                    return string.Empty;
                }

                int statusCode = (int)response.StatusCode;

                // We want to handle redirects ourselves so that we can determine the final redirect Location (via header)
                if (statusCode >= 300 && statusCode <= 399)
                {
                    var redirectUri = response.Headers.Location;
                    if (!redirectUri.IsAbsoluteUri)
                    {
                        redirectUri = new Uri(request.RequestUri.GetLeftPart(UriPartial.Authority) + redirectUri);
                    }

                    Common.LogDebug(true, string.Format("DownloadStringData() redirecting to {0}", redirectUri));

                    return await DownloadStringDataKeepParam(redirectUri.ToString());
                }
                else
                {
                    return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                }
            }
        }


        /// <summary>
        /// Download compressed string data.
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static async Task<string> DownloadStringDataWithGz(string url)
        {
            HttpClientHandler handler = new HttpClientHandler()
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            };

            using (HttpClient client = new HttpClient(handler))
            {
                client.DefaultRequestHeaders.Add("User-Agent", Web.UserAgent);
                return await client.GetStringAsync(url).ConfigureAwait(false);
            }
        }


        /// <summary>
        /// Download string data with manage redirect url.
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static async Task<string> DownloadStringData(string url)
        {
            using (HttpClient client = new HttpClient())
            {
                HttpRequestMessage request = new HttpRequestMessage()
                {
                    RequestUri = new Uri(url),
                    Method = HttpMethod.Get
                };

                HttpResponseMessage response;
                try
                {
                    client.DefaultRequestHeaders.Add("User-Agent", Web.UserAgent);
                    response = await client.SendAsync(request).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    Common.LogError(ex, false, $"Error on download {url}");
                    return string.Empty;
                }

                if (response == null)
                {
                    return string.Empty;
                }

                int statusCode = (int)response.StatusCode;

                // We want to handle redirects ourselves so that we can determine the final redirect Location (via header)
                if (statusCode >= 300 && statusCode <= 399)
                {
                    var redirectUri = response.Headers.Location;
                    if (!redirectUri.IsAbsoluteUri)
                    {
                        redirectUri = new Uri(request.RequestUri.GetLeftPart(UriPartial.Authority) + redirectUri);
                    }

                    Common.LogDebug(true, string.Format("DownloadStringData() redirecting to {0}", redirectUri));

                    return await DownloadStringData(redirectUri.ToString());
                }
                else
                {
                    return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                }
            }
        }

        /// <summary>
        /// Download string data with a specific UserAgent.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="UserAgentType"></param>
        /// <returns></returns>
        public static async Task<string> DownloadStringData(string url, WebUserAgentType UserAgentType)
        {
            using (var client = new HttpClient())
            {
                var request = new HttpRequestMessage()
                {
                    RequestUri = new Uri(url),
                    Method = HttpMethod.Get
                };

                HttpResponseMessage response;
                try
                {
                    client.DefaultRequestHeaders.Add("User-Agent", Web.UserAgent);
                    client.DefaultRequestHeaders.UserAgent.TryParseAdd(StrWebUserAgentType(UserAgentType));
                    response = await client.SendAsync(request).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    Common.LogError(ex, false, $"Error on download {url}");
                    return string.Empty;
                }

                if (response == null)
                {
                    return string.Empty;
                }

                int statusCode = (int)response.StatusCode;
                if (statusCode == 200)
                {
                    return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                }
                else
                {
                    Logger.Warn($"DownloadStringData() with statuscode {statusCode} for {url}");
                    return string.Empty;
                }
            }
        }

        /// <summary>
        /// Download string data with custom cookies.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="cookies"></param>
        /// <param name="userAgent"></param>
        /// <returns></returns>
        public static async Task<string> DownloadStringData(string url, List<HttpCookie> cookies = null, string userAgent = "", bool keepParam = false)
        {
            HttpClientHandler handler = new HttpClientHandler();
            if (cookies != null)
            {
                CookieContainer cookieContainer = new CookieContainer();

                foreach (var cookie in cookies)
                {
                    Cookie c = new Cookie();
                    c.Name = cookie.Name;
                    c.Value = Tools.FixCookieValue(cookie.Value);
                    c.Domain = cookie.Domain;
                    c.Path = cookie.Path;

                    try
                    {
                        cookieContainer.Add(c);
                    }
                    catch (Exception ex)
                    {
                        Common.LogError(ex, true);
                    }
                }

                handler.CookieContainer = cookieContainer;
            }

            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri(url),
                Method = HttpMethod.Get
            };

            HttpResponseMessage response;
            using (var client = new HttpClient(handler))
            {
                if (userAgent.IsNullOrEmpty())
                {
                    client.DefaultRequestHeaders.Add("User-Agent", Web.UserAgent);
                }
                else
                {
                    client.DefaultRequestHeaders.Add("User-Agent", userAgent);
                }

                try
                {
                    response = await client.SendAsync(request).ConfigureAwait(false);
                    int statusCode = (int)response.StatusCode;
                    bool IsRedirected = (request.RequestUri.ToString() != url) || (statusCode >= 300 && statusCode <= 399);

                    // We want to handle redirects ourselves so that we can determine the final redirect Location (via header)
                    if (IsRedirected)
                    {
                        string urlNew = request.RequestUri.ToString();
                        var redirectUri = response.Headers.Location;
                        if (!redirectUri?.IsAbsoluteUri ?? false)
                        {
                            redirectUri = new Uri(request.RequestUri.GetLeftPart(UriPartial.Authority) + redirectUri);
                            urlNew = redirectUri.ToString();
                        }
                        
                        if (keepParam)
                        {
                            var urlParams = url.Split('?').ToList();
                            if (urlParams.Count == 2)
                            {
                                var urlNewParams = urlNew.Split('?').ToList();
                                if (urlNewParams.Count == 2)
                                {
                                    if (urlParams[1] != urlNewParams[1])
                                    {
                                        urlNew += "&" + urlParams[1];
                                    }
                                }
                                else
                                {
                                    urlNew += "?" + urlParams[1];
                                }
                            }
                        }

                        Common.LogDebug(true, string.Format("DownloadStringData() redirecting to {0}", urlNew));
                        return await DownloadStringData(urlNew, cookies, userAgent);
                    }
                    else
                    {
                        return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    }

                }
                catch (Exception ex)
                {
                    if (ex.Message.Contains("Section=ResponseHeader Detail=CR"))
                    {
                        Logger.Warn($"Used UserAgent: Anything");
                        return DownloadStringData(url, cookies, "Anything").GetAwaiter().GetResult();
                    }
                    else
                    {
                        Common.LogError(ex, false, $"Error on Get {url}");
                    }
                }
            }

            return string.Empty;
        }
        
        public static async Task<string> DownloadStringData(string url, CookieContainer Cookies = null, string UserAgent = "")
        {
            var response = string.Empty;

            HttpClientHandler handler = new HttpClientHandler();
            if (Cookies.Count > 0)
            {
                handler.CookieContainer = Cookies;
            }

            using (var client = new HttpClient(handler))
            {
                if (UserAgent.IsNullOrEmpty())
                {
                    client.DefaultRequestHeaders.Add("User-Agent", Web.UserAgent);
                }
                else
                {
                    client.DefaultRequestHeaders.Add("User-Agent", UserAgent);
                }

                HttpResponseMessage result;
                try
                {
                    result = await client.GetAsync(url).ConfigureAwait(false);
                    if (result.IsSuccessStatusCode)
                    {
                        response = await result.Content.ReadAsStringAsync().ConfigureAwait(false);
                    }
                    else
                    {
                        Logger.Error($"Web error with status code {result.StatusCode.ToString()}");
                    }
                }
                catch (Exception ex)
                {
                    Common.LogError(ex, false, $"Error on Get {url}");
                }
            }

            return response;
        }

        public static async Task<string> DownloadStringData(string url, List<HttpHeader> HttpHeaders = null, List<HttpCookie> Cookies = null)
        {
            HttpClientHandler handler = new HttpClientHandler();
            if (Cookies != null)
            {
                CookieContainer cookieContainer = new CookieContainer();

                foreach (var cookie in Cookies)
                {
                    Cookie c = new Cookie();
                    c.Name = cookie.Name;
                    c.Value = Tools.FixCookieValue(cookie.Value);
                    c.Domain = cookie.Domain;
                    c.Path = cookie.Path;

                    try
                    {
                        cookieContainer.Add(c);
                    }
                    catch (Exception ex)
                    {
                        Common.LogError(ex, true);
                    }
                }

                handler.CookieContainer = cookieContainer;
            }

            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri(url),
                Method = HttpMethod.Get
            };

            string response = string.Empty;
            using (var client = new HttpClient(handler))
            {
                client.DefaultRequestHeaders.Add("User-Agent", Web.UserAgent);

                if (HttpHeaders != null)
                {
                    HttpHeaders.ForEach(x => 
                    {
                        client.DefaultRequestHeaders.Add(x.Key, x.Value);
                    });
                }

                HttpResponseMessage result;
                try
                {
                    result = await client.GetAsync(url).ConfigureAwait(false);
                    if (result.IsSuccessStatusCode)
                    {
                        response = await result.Content.ReadAsStringAsync().ConfigureAwait(false);
                    }
                    else
                    {
                        Logger.Error($"Web error with status code {result.StatusCode.ToString()}");
                    }
                }
                catch (Exception ex)
                {
                    Common.LogError(ex, false, $"Error on Get {url}");
                }
            }

            return response;
        }

        /// <summary>
        /// Download string data with a bearer token.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="token"></param>
        /// <param name="UrlBefore"></param>
        /// <returns></returns>
        public static async Task<string> DownloadStringData(string url, string token, string UrlBefore = "", string LangHeader = "")
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("User-Agent", Web.UserAgent);

                if (!LangHeader.IsNullOrEmpty())
                {
                    client.DefaultRequestHeaders.Add("Accept-Language", LangHeader);
                }

                if (!UrlBefore.IsNullOrEmpty())
                {
                    await client.GetStringAsync(UrlBefore).ConfigureAwait(false);
                }

                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
                string result = await client.GetStringAsync(url).ConfigureAwait(false);

                return result;
            }
        }


        public static async Task<string> DownloadStringDataWithUrlBefore(string url, string UrlBefore = "", string LangHeader = "")
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("User-Agent", Web.UserAgent);

                if (!LangHeader.IsNullOrEmpty())
                {
                    client.DefaultRequestHeaders.Add("Accept-Language", LangHeader);
                }

                if (!UrlBefore.IsNullOrEmpty())
                {
                    await client.GetStringAsync(UrlBefore).ConfigureAwait(false);
                }
                
                string result = await client.GetStringAsync(url).ConfigureAwait(false);
                return result;
            }
        }


        public static async Task<string> DownloadStringDataJson(string url)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("User-Agent", Web.UserAgent);
                client.DefaultRequestHeaders.Add("Accept", "*/*");

                string result = await client.GetStringAsync(url).ConfigureAwait(false);
                return result;
            }
        }


        public static async Task<string> PostStringData(string url, string token, StringContent content)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("User-Agent", Web.UserAgent);
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
                var response = await client.PostAsync(url, content);
                var str = await response.Content.ReadAsStringAsync();
                return str;
            }
        }

        public static async Task<string> PostStringData(string url, StringContent content)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("User-Agent", Web.UserAgent);
                var response = await client.PostAsync(url, content);
                var str = await response.Content.ReadAsStringAsync();
                return str;
            }
        }

        /// <summary>
        /// Post data with a payload.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="payload"></param>
        /// <returns></returns>
        public static async Task<string> PostStringDataPayload(string url, string payload, List<HttpCookie> Cookies = null, List<KeyValuePair<string, string>> moreHeader = null)
        {
            //var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            //var settings = (SettingsSection)config.GetSection("system.net/settings");
            //var defaultValue = settings.HttpWebRequest.UseUnsafeHeaderParsing;
            //settings.HttpWebRequest.UseUnsafeHeaderParsing = true;
            //config.Save(ConfigurationSaveMode.Modified);
            //ConfigurationManager.RefreshSection("system.net/settings");

            var response = string.Empty;

            HttpClientHandler handler = new HttpClientHandler();
            if (Cookies != null)
            {
                CookieContainer cookieContainer = new CookieContainer();

                foreach (HttpCookie cookie in Cookies)
                {
                    Cookie c = new Cookie
                    {
                        Name = cookie.Name,
                        Value = Tools.FixCookieValue(cookie.Value),
                        Domain = cookie.Domain,
                        Path = cookie.Path
                    };

                    try
                    {
                        cookieContainer.Add(c);
                    }
                    catch (Exception ex)
                    {
                        Common.LogError(ex, true);
                    }
                }

                handler.CookieContainer = cookieContainer;
            }

            using (var client = new HttpClient(handler))
            {
                client.DefaultRequestHeaders.Add("User-Agent", Web.UserAgent);
                client.DefaultRequestHeaders.Add("accept", "application/json, text/javascript, */*; q=0.01");
                client.DefaultRequestHeaders.Add("Vary", "Accept-Encoding");

                moreHeader?.ForEach(x =>
                {
                    client.DefaultRequestHeaders.Add(x.Key, x.Value);
                });

                HttpContent c = new StringContent(payload, Encoding.UTF8, "application/json");

                HttpResponseMessage result;
                try
                {
                    result = await client.PostAsync(url, c).ConfigureAwait(false);
                    if (result.IsSuccessStatusCode)
                    {
                        response = await result.Content.ReadAsStringAsync().ConfigureAwait(false);
                    }
                    else
                    {
                        Logger.Error($"Web error with status code {result.StatusCode.ToString()}");
                    }
                }
                catch (Exception ex)
                {
                    Common.LogError(ex, false, $"Error on Post {url}");
                }
            }

            //settings.HttpWebRequest.UseUnsafeHeaderParsing = defaultValue;
            //config.Save(ConfigurationSaveMode.Modified);
            //ConfigurationManager.RefreshSection("system.net/settings");

            return response;
        }

        public static async Task<string> PostStringDataCookies(string url, FormUrlEncodedContent formContent, List<HttpCookie> Cookies = null)
        {
            var response = string.Empty;

            HttpClientHandler handler = new HttpClientHandler();
            if (Cookies != null)
            {
                CookieContainer cookieContainer = new CookieContainer();

                foreach (HttpCookie cookie in Cookies)
                {
                    Cookie c = new Cookie
                    {
                        Name = cookie.Name,
                        Value = Tools.FixCookieValue(cookie.Value),
                        Domain = cookie.Domain,
                        Path = cookie.Path
                    };

                    try
                    {
                        cookieContainer.Add(c);
                    }
                    catch (Exception ex)
                    {
                        Common.LogError(ex, true);
                    }
                }

                handler.CookieContainer = cookieContainer;
            }

            using (var client = new HttpClient(handler))
            {
                var els = url.Split('/');
                string baseUrl = els[0] + "//" + els[2];

                client.DefaultRequestHeaders.Add("User-Agent", Web.UserAgent);
                client.DefaultRequestHeaders.Add("origin", baseUrl);
                client.DefaultRequestHeaders.Add("referer", baseUrl);

                HttpResponseMessage result;
                try
                {
                    result = await client.PostAsync(url, formContent).ConfigureAwait(false);
                    if (result.IsSuccessStatusCode)
                    {
                        response = await result.Content.ReadAsStringAsync().ConfigureAwait(false);
                    }
                    else
                    {
                        Logger.Error($"Web error with status code {result.StatusCode.ToString()}");
                    }
                }
                catch (Exception ex)
                {
                    Common.LogError(ex, false, $"Error on Post {url}");
                }
            }

            return response;
        }
    }
}
