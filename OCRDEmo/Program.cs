using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;


namespace OCRDemo
{
    class Program
    {
        static void Main(string[] args)
        {

            string postUrl = "http://recognition.image.myqcloud.com/ocr/general";
            UseUrl(postUrl);
            UseImage(postUrl);
        }
        /// <summary>
        /// 使用Url传递
        /// </summary>
        /// <param name="postUrl"></param>
        static void UseUrl(string postUrl)
        {
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", OCRApi.HmacSha1Sign());
            client.DefaultRequestHeaders.Host = OCRConfig.Host;
            var task = client.PostAsJsonAsync(postUrl, new
            {
                appid = OCRConfig.appid,
                bucket = OCRConfig.bucket,
                url = "http://test-123456.image.myqcloud.com/test.jpg"
            });
            task.Wait();
            var responseTask = task.Result.Content.ReadAsStringAsync();
            responseTask.Wait();
            var result = responseTask.Result;
        }

        static void UseImage(string url)
        {
            HttpWebRequest webReq = (HttpWebRequest)WebRequest.Create(new Uri(url));
            Stream memStream = new MemoryStream();
            webReq.Method = "POST";
            string boundary = "--------------" + DateTime.Now.Ticks.ToString("x");// 边界符  
            webReq.ContentType = "multipart/form-data; boundary=" + boundary;
            byte[] enter = Encoding.ASCII.GetBytes("\r\n");  //换行
            memStream.Write(enter, 0, enter.Length);
            Dictionary<string, string> dic = new Dictionary<string, string>()
            {
                {"appid",OCRConfig.appid} ,
                {"bucket",OCRConfig.bucket}
            };
            //写入文本字段
            string inputPartHeaderFormat = "--" + boundary + "\r\n" + "Content-Disposition:form-data;name=\"{0}\";" + "\r\n\r\n{1}\r\n";
            foreach (var kv in dic)
            {
                string inputPartHeader = string.Format(inputPartHeaderFormat, kv.Key, kv.Value);
                var inputPartHeaderBytes = Encoding.ASCII.GetBytes(inputPartHeader);
                memStream.Write(inputPartHeaderBytes, 0, inputPartHeaderBytes.Length);
            }
            var fileStream = new FileStream("1.jpg", FileMode.Open, FileAccess.Read);
            // 写入文件  
            string imagePartHeader = "--" + boundary + "\r\n" +
                                     "Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"\r\n" +
                                     "Content-Type: image/jpeg\r\n\r\n";
            var header = string.Format(imagePartHeader, "image", "1.jpg");
            var headerbytes = Encoding.UTF8.GetBytes(header);
            memStream.Write(headerbytes, 0, headerbytes.Length);
            var buffer = new byte[1024];
            int bytesRead;
            while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) != 0)
            {
                memStream.Write(buffer, 0, bytesRead);
            }
            // 最后的结束符  
            byte[] endBoundary = Encoding.ASCII.GetBytes("xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx" + "\r\n" + boundary + "--\r\n");
            memStream.Write(endBoundary, 0, endBoundary.Length);
            webReq.ContentLength = memStream.Length;
            webReq.Headers.Add(HttpRequestHeader.Authorization, OCRApi.HmacSha1Sign());
            webReq.Host = OCRConfig.Host;
            var requestStream = webReq.GetRequestStream();
            memStream.Position = 0;
            memStream.CopyTo(requestStream);
            HttpWebResponse response = (HttpWebResponse)webReq.GetResponse();
            StreamReader sr = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
            var ret = sr.ReadToEnd();
            sr.Close();
            response.Close();
            requestStream.Close();
            memStream.Close();
        }

    }
}
