namespace Home.Test
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Newtonsoft.Json.Linq;

    public static class ODataClientHelper
    {
        //// Server paging

        public static IList<JToken> GetServerPaging(string url, Action<HttpClient> before = null, Func<HttpResponseMessage, bool> after = null)
        {
            return GetServerPagingAsync(url, before, after).GetAwaiter().GetResult();
        }

        public static async Task<IList<JToken>> GetServerPagingAsync(string url, Action<HttpClient> before = null, Func<HttpResponseMessage, bool> after = null)
        {
            Assert.IsNotNull(url);
            Assert.IsTrue(url.Length > 0);

            var result = new List<JToken>();

            do
            {
                using (var client = new HttpClient() { Timeout = Timeout.InfiniteTimeSpan })
                {
                    if (before != null)
                    {
                        before(client);
                    }

                    var response = await client.GetAsync(url);
                    var process = true;

                    if (after != null)
                    {
                        process = after(response);
                    }

                    if (process)
                    {
                        response.EnsureSuccessStatusCode();
                    }

                    var text = await response.Content.ReadAsStringAsync();
                    if (string.IsNullOrEmpty(text))
                    {
                        url = null;
                    }
                    else
                    {
                        var obj = JObject.Parse(text);
                        if (obj == null)
                        {
                            break;
                        }

                        var array = (JArray)obj["value"];
                        if (array != null)
                        {
                            result.AddRange(array.ToArray());
                        }

                        url = (string)obj["@odata.nextLink"];
                    }
                }
            } while (url != null);

            return result;
        }

        //// GET

        public static JToken InvokeGet(string url, Action<HttpClient> before = null, Func<HttpResponseMessage, bool> after = null)
        {
            return InvokeGetAsync(url, before, after).GetAwaiter().GetResult();
        }

        public static async Task<JToken> InvokeGetAsync(string url, Action<HttpClient> before = null, Func<HttpResponseMessage, bool> after = null)
        {
            Assert.IsNotNull(url);
            Assert.IsTrue(url.Length > 0);

            using (var client = new HttpClient() { Timeout = Timeout.InfiniteTimeSpan })
            {
                if (before != null)
                {
                    before(client);
                }

                var response = await client.GetAsync(url);
                var process = true;

                if (after != null)
                {
                    process = after(response);
                }

                if (process)
                {
                    response.EnsureSuccessStatusCode();
                    var result = await response.Content.ReadAsStringAsync();
                    return GetValue(result);
                }
            }

            return null;
        }

        //// POST

        public static JToken InvokePost(string url, string json, Action<HttpClient> before = null, Func<HttpResponseMessage, bool> after = null)
        {
            return InvokePostAsync(url, json, before, after).GetAwaiter().GetResult();
        }

        public static async Task<JToken> InvokePostAsync(string url, string json, Action<HttpClient> before = null, Func<HttpResponseMessage, bool> after = null)
        {
            Assert.IsNotNull(url);
            Assert.IsTrue(url.Length > 0);
            Assert.IsNotNull(json);
            Assert.IsTrue(json.Length > 0);

            using (var client = new HttpClient() { Timeout = Timeout.InfiniteTimeSpan })
            {
                if (before != null)
                {
                    before(client);
                }

                var response = await client.PostAsync(url, new StringContent(json, Encoding.UTF8, "application/json"));
                var process = true;

                if (after != null)
                {
                    process = after(response);
                }

                if (process)
                {
                    response.EnsureSuccessStatusCode();
                    var result = await response.Content.ReadAsStringAsync();
                    return GetValue(result);
                }
            }

            return null;
        }

        //// PUT

        public static JToken InvokePut(string url, string json, Action<HttpClient> before = null, Func<HttpResponseMessage, bool> after = null)
        {
            return InvokePutAsync(url, json, before, after).GetAwaiter().GetResult();
        }

        public static async Task<JToken> InvokePutAsync(string url, string json, Action<HttpClient> before = null, Func<HttpResponseMessage, bool> after = null)
        {
            Assert.IsNotNull(url);
            Assert.IsTrue(url.Length > 0);
            Assert.IsNotNull(json);
            Assert.IsTrue(json.Length > 0);

            using (var client = new HttpClient() { Timeout = Timeout.InfiniteTimeSpan })
            {
                if (before != null)
                {
                    before(client);
                }

                var response = await client.PutAsync(url, new StringContent(json, Encoding.UTF8, "application/json"));
                var process = true;

                if (after != null)
                {
                    process = after(response);
                }

                if (process)
                {
                    response.EnsureSuccessStatusCode();
                    var result = await response.Content.ReadAsStringAsync();
                    return GetValue(result);
                }
            }

            return null;
        }

        //// PATCH

        public static JToken InvokePatch(string url, string json, Action<HttpClient> before = null, Func<HttpResponseMessage, bool> after = null)
        {
            return InvokePatchAsync(url, json, before, after).GetAwaiter().GetResult();
        }

        public static async Task<JToken> InvokePatchAsync(string url, string json, Action<HttpClient> before = null, Func<HttpResponseMessage, bool> after = null)
        {
            Assert.IsNotNull(url);
            Assert.IsTrue(url.Length > 0);
            Assert.IsNotNull(json);
            Assert.IsTrue(json.Length > 0);

            using (var client = new HttpClient() { Timeout = Timeout.InfiniteTimeSpan })
            {
                if (before != null)
                {
                    before(client);
                }

                var request = new HttpRequestMessage(new HttpMethod("PATCH"), url)
                {
                    Content = new StringContent(json, Encoding.UTF8, "application/json")
                };
                var response = await client.SendAsync(request);
                var process = true;

                if (after != null)
                {
                    process = after(response);
                }

                if (process)
                {
                    response.EnsureSuccessStatusCode();
                    var result = await response.Content.ReadAsStringAsync();
                    return GetValue(result);
                }
            }

            return null;
        }

        //// DELETE

        public static JToken InvokeDelete(string url, Action<HttpClient> before = null, Func<HttpResponseMessage, bool> after = null)
        {
            return InvokeDeleteAsync(url, before, after).GetAwaiter().GetResult();
        }

        public static async Task<JToken> InvokeDeleteAsync(string url, Action<HttpClient> before = null, Func<HttpResponseMessage, bool> after = null)
        {
            Assert.IsNotNull(url);
            Assert.IsTrue(url.Length > 0);

            using (var client = new HttpClient() { Timeout = Timeout.InfiniteTimeSpan })
            {
                if (before != null)
                {
                    before(client);
                }

                var response = await client.DeleteAsync(url);
                var process = true;

                if (after != null)
                {
                    process = after(response);
                }

                if (process)
                {
                    response.EnsureSuccessStatusCode();
                    var result = await response.Content.ReadAsStringAsync();
                    return GetValue(result);
                }
            }

            return null;
        }

        //// Helpers

        private static JToken GetValue(string json)
        {
            if (string.IsNullOrEmpty(json))
            {
                return null;
            }

            var obj = JObject.Parse(json);
            var value = obj["value"];

            return value == null ? obj : value;
        }
    }
}
