﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Runtime.Serialization;

namespace UTH.Infrastructure.Utility
{
    /// <summary>
    /// Http 辅助类
    /// </summary>
    public class HttpHelper
    {
        #region HTTP Code

        //HTTP: Status 1xx(临时响应)
        //->表示临时响应并需要请求者继续执行操作的状态代码。

        //详细代码及说明:
        //HTTP: Status 100 (继续)
        //-> 请求者应当继续提出请求。 服务器返回此代码表示已收到请求的第一部分，正在等待其余部分。
        //HTTP: Status 101 (切换协议)
        //-> 请求者已要求服务器切换协议，服务器已确认并准备切换。


        //HTTP Status 2xx(成功)
        //->表示成功处理了请求的状态代码;

        //HTTP Status 200 (成功)
        //-> 服务器已成功处理了请求。 通常，这表示服务器提供了请求的网页。
        //HTTP Status 201 (已创建)
        //-> 请求成功并且服务器创建了新的资源。
        //HTTP Status 202 (已接受)
        //-> 服务器已接受请求，但尚未处理。
        //HTTP Status 203 (非授权信息)
        //-> 服务器已成功处理了请求，但返回的信息可能来自另一来源。
        //HTTP Status 204 (无内容)
        //-> 服务器成功处理了请求，但没有返回任何内容。
        //HTTP Status 205 (重置内容)
        //-> 服务器成功处理了请求，但没有返回任何内容。
        //HTTP Status 206 (部分内容)
        //-> 服务器成功处理了部分 GET 请求。

        //HTTP Status 4xx(请求错误)
        //->这些状态代码表示请求可能出错，妨碍了服务器的处理。

        //HTTP Status 400 （错误请求） 
        //->服务器不理解请求的语法。
        //HTTP Status 401 （未授权） 
        //->请求要求身份验证。 对于需要登录的网页，服务器可能返回此响应。
        //HTTP Status 403 （禁止）
        //-> 服务器拒绝请求。
        //HTTP Status 404 （未找到） 
        //->服务器找不到请求的网页。
        //HTTP Status 405 （方法禁用） 
        //->禁用请求中指定的方法。
        //HTTP Status 406 （不接受） 
        //->无法使用请求的内容特性响应请求的网页。
        //HTTP Status 407 （需要代理授权） 
        //->此状态代码与 401（未授权）类似，但指定请求者应当授权使用代理。
        //HTTP Status 408 （请求超时） 
        //->服务器等候请求时发生超时。
        //HTTP Status 409 （冲突） 
        //->服务器在完成请求时发生冲突。 服务器必须在响应中包含有关冲突的信息。
        //HTTP Status 410 （已删除）
        //-> 如果请求的资源已永久删除，服务器就会返回此响应。
        //HTTP Status 411 （需要有效长度） 
        //->服务器不接受不含有效内容长度标头字段的请求。
        //HTTP Status 412 （未满足前提条件） 
        //->服务器未满足请求者在请求中设置的其中一个前提条件。
        //HTTP Status 413 （请求实体过大） 
        //->服务器无法处理请求，因为请求实体过大，超出服务器的处理能力。
        /*HTTP Status 414 （请求的 URI 过长） 请求的 URI（通常为网址）过长，服务器*/

        //HTTP Status 415 （不支持的媒体类型） 
        //->请求的格式不受请求页面的支持。
        //HTTP Status 416 （请求范围不符合要求） 
        //->如果页面无法提供请求的范围，则服务器会返回此状态代码。
        //HTTP Status 417 （未满足期望值） 
        //->服务器未满足”期望”请求标头字段的要求。

        //HTTP Status 5xx （服务器错误）
        /*->这些状态代码表示服务器在尝试处理请求时发生内部错误。 这些错误可能是服务器 本身的错误，而不是请求出错。*/

        //HTTP Status 500 （服务器内部错误） 
        //->服务器遇到错误，无法完成请求。
        //HTTP Status 501 （尚未实施） 
        /*->服务器不具备完成请求的功能。 例如，服务器无法识别请求方法时可能会返回此代码。*/

        //HTTP Status 502 （错误网关） 
        //->服务器作为网关或代理，从上游服务器收到无效响应。
        //HTTP Status 503 （服务不可用）
        //-> 服务器目前无法使用（由于超载或停机维护）。 通常，这只是暂时状态。
        //HTTP Status 504 （网关超时） 
        //->服务器作为网关或代理，但是没有及时从上游服务器收到请求。
        //HTTP Status 505 （HTTP 版本不受支持）
        //-> 服务器不支持请求中所用的 HTTP 协议版本。

        #endregion

        /// <summary>
        /// 获取请求结果对象
        /// </summary>
        /// <typeparam name="TModel">返回对象</typeparam>
        /// <typeparam name="TParam">请求对象</typeparam>
        /// <param name="url">请求地址</param>
        /// <param name="post">请求对象</param>
        /// <param name="method">请求方式</param>
        /// <param name="contentType"></param>
        /// <returns></returns>
        public static TModel Request<TModel, TParam>(string url, TParam post = default(TParam), string method = null, string contentType = "application/json", Action<HttpClientOptions> action = null)
        {
            return Request<TModel>(url, (post.IsNull() ? null : JsonHelper.Serialize(post)), method: method, contentType: contentType, action: action);
        }

        /// <summary>
        /// 获取请求结果对象
        /// </summary>
        /// <typeparam name="TModel">返回对象</typeparam>
        /// <param name="url">请求地址</param>
        /// <param name="post">请求对象</param>
        /// <param name="method">请求方式</param>
        /// <param name="contentType"></param>
        /// <returns></returns>
        public static TModel Request<TModel>(string url, string post = null, string method = null, string contentType = "application/json", Action<HttpClientOptions> action = null)
        {
            Action<HttpClientOptions> callback = (option) =>
            {
                option.URL = url;
                option.ContentType = contentType;
                option.Method = "GET";
                if (!post.IsEmpty())
                {
                    option.Method = "POST";
                    option.PostDataType = EnumHttpData.String;
                    option.PostString = post;
                }
                if (!method.IsEmpty())
                {
                    option.Method = method;
                }
                action?.Invoke(option);
            };
            return Request<TModel>(callback);
        }

        /// <summary>
        /// 获取请求结果对象
        /// </summary>
        /// <param name="option">请求配置</param>
        /// <returns></returns>
        public static TModel Request<TModel>(Action<HttpClientOptions> action)
        {
            var model = default(TModel);
            var result = Request(action);
            if (result != null && !result.Content.IsEmpty())
            {
                model = JsonHelper.Deserialize<TModel>(result.Content);
            }
            return model;
        }

        /// <summary>
        /// 获取请求结果对象
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <param name="post">post数据</param>
        /// <param name="method">请求方式</param>
        /// <param name="contentType"></param>
        /// <returns></returns>
        public static HttpClientResult Request(string url, string post = null, string method = null, string contentType = "application/json", Action<HttpClientOptions> action = null)
        {
            Action<HttpClientOptions> callback = (option) =>
            {
                option.URL = url;
                option.ContentType = contentType;
                option.Method = "GET";
                if (!post.IsEmpty())
                {
                    option.Method = "POST";
                    option.PostDataType = EnumHttpData.String;
                    option.PostString = post;
                }
                if (!method.IsEmpty())
                {
                    option.Method = method;
                }
                action?.Invoke(option);
            };
            return Request(callback);
        }


        /// <summary>
        /// 上传文件
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="url"></param>
        /// <param name="file"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public static TModel Request<TModel>(string url, FileUploadModel file, Action<HttpClientOptions> action)
        {
            var model = default(TModel);
            var result = Request(url, file, action);
            if (result != null && !result.Content.IsEmpty())
            {
                model = JsonHelper.Deserialize<TModel>(result.Content);
            }
            return model;
        }

        /// <summary>
        /// 上传文件
        /// </summary>
        /// <param name="url"></param>
        /// <param name="file"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public static HttpClientResult Request(string url, FileUploadModel file, Action<HttpClientOptions> action)
        {
            #region demo

            //FileUploadModel model = new FileUploadModel();
            //model.Id = StringHelper.Get(files["id"]);
            //model.Name = StringHelper.Get(files["name"]);
            //model.Type = StringHelper.Get(files["type"]);
            //model.Size = LongHelper.Get(files["size"]);
            //model.Chunk = LongHelper.Get(files["chunk"]);
            //model.Chunks = LongHelper.Get(files["chunks"]);
            //model.LastModifiedDate = DateTimeHelper.Get(files["lastModifiedDate"]);

            //IFormFile file = files.Files.FirstOrDefault();
            //using (Stream stream = file.OpenReadStream())
            //{
            //    model.Bytes = new byte[stream.Length];
            //    stream.Read(model.Bytes, 0, (int)stream.Length);
            //    stream.Close();
            //}

            //HttpHelper.PostFile("http://localhost:50768/common/Upload2", model);

            #endregion

            Action<HttpClientOptions> callback = (option) =>
            {
                action?.Invoke(option);

                FileUploadData data = new FileUploadData();
                foreach (var key in file.Items.Keys)
                {
                    data.AddField(StringHelper.Get(key), StringHelper.Get(file.Items[key]));
                }
                data.AddFile("file", file.Name, file.Bytes);
                data.PrepareFormData();

                option.URL = url;
                option.Method = "POST";
                option.PostDataType = EnumHttpData.File;
                option.PostBytes = data.GetFormData().ToArray();
            };
            return Request(callback);
        }


        /// <summary>
        /// 获取请求结果对象
        /// </summary>
        /// <param name="action">参数配置</param>
        /// <returns></returns>
        public static HttpClientResult Request(Action<HttpClientOptions> action)
        {
            action.CheckNull();
            HttpClientOptions option = new HttpClientOptions();
            action(option);
            var response = new HttpWebClientUtility().Request(option);

            #region check request & response 

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                throw new AuthException(string.Format("[{0}]{1}", response.StatusCode, response.Content));
            }

            if (response.StatusCode.GetValue() >= HttpStatusCode.InternalServerError.GetValue())
            {
                throw new ServiceException(string.Format("[{0}]{1}", response.StatusCode, response.Content));
            }

            if (response.StatusCode.GetValue() >= HttpStatusCode.BadRequest.GetValue())
            {
                throw new NetworkException(string.Format("[{0}]{1}", response.StatusCode, response.Content));
            }

            if (response.Content == "无法连接到远程服务器")
            {
                throw new NetworkException(string.Format("[{0}]{1}", HttpStatusCode.NotFound, response.Content));
            }

            #endregion

            return response;
        }

    }
}
