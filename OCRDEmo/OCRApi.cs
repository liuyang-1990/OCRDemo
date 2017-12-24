using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace OCRDemo
{
    public class OCRApi
    {
        /// <summary>
        /// 生成随机串，随机串包含字母或数字
        /// </summary>
        /// <returns> @return 随机串</returns>
        public static string GenerateNonceStr()
        {
            return DateTime.Now.ToString("yyMMddHHmm");
        }
        /// <summary>
        /// 生成当前时间戳
        /// 标准北京时间，时区为东八区，自1970年1月1日 0点0分0秒以来的秒数
        /// </summary>
        /// <returns>@return 时间戳</returns>
        public static long GenerateCurrentTimeStamp()
        {
            TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return Convert.ToInt64(ts.TotalSeconds);
        }
        /// <summary>
        /// 获取要加密的字符串
        /// </summary>
        /// <returns></returns>
        private static string GetOrignal()
        {
            var current = GenerateCurrentTimeStamp();
            var expired = GenerateCurrentTimeStamp() + 2592000;
            return $"a={OCRConfig.appid}&b={OCRConfig.bucket}&k={OCRConfig.secret_id}&e={expired}&t={current}&r={GenerateNonceStr()}&u=0&f=";
        }

        /// <summary>
        /// HMAC-SHA1加密算法
        /// </summary>
        /// <param name="secret">密钥</param>
        /// <returns></returns>
        public static string HmacSha1Sign()
        {
            var hmacsha1 = new HMACSHA1(Encoding.UTF8.GetBytes(OCRConfig.secret_key));
            var dataBuffer = Encoding.UTF8.GetBytes(GetOrignal());
            var hashBytes = hmacsha1.ComputeHash(dataBuffer);
            List<byte> bytes = new List<byte>();
            bytes.AddRange(hashBytes);
            bytes.AddRange(dataBuffer);
            return Convert.ToBase64String(bytes.ToArray());
        }

    }
}