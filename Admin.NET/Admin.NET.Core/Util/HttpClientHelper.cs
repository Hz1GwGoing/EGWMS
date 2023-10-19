using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;

namespace Furion.Extras.Admin.NET.Util
{
    public class HttpClientHelper
    {
        /// <summary>
        /// Post请求
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <param name="jsonString">请求数据，格式Json</param>
        /// <returns></returns>
        public async System.Threading.Tasks.Task<HttpResponseMessage> PostToolAsync(string url,string jsonString)
        {
            try
            {
                using var httpClient = new HttpClient();
                var requestContent = new StringContent(jsonString, Encoding.UTF8, "application/json");
                var result = await httpClient.PostAsync(url, requestContent);
                if (!result.IsSuccessStatusCode)
                {
                    //throw new Exception("网络请求失败！");
                    throw Oops.Oh($"网络请求失败！");
                }
                return result;
            }
            catch(Exception ex)
            {
                //throw new Exception($"底层接口请求失败：{ex.Message}");
                throw Oops.Oh($"底层接口请求失败：{ex.Message}");
            }
        }
        
        /// <summary>
        /// Get请求
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <returns></returns>
        public async System.Threading.Tasks.Task<HttpResponseMessage> GetToolAsync(string url)
        {
            try
            {
                using var httpClient = new HttpClient();
                var result = await httpClient.GetAsync(url);
                if (!result.IsSuccessStatusCode)
                {
                    //throw new Exception("网络请求失败！");
                    throw Oops.Oh($"网络请求失败！");
                }
                return result;
            }
            catch(Exception ex)
            {
                //throw new Exception($"底层接口请求失败：{ex.Message}");
                throw Oops.Oh($"底层接口请求失败：{ex.Message}");
            }
        }
    }
}
