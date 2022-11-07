using GoHttp_uTLS;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Example
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var http = new GoHttp();

            // Set request timeout 
            http.Timeout = TimeSpan.FromSeconds(30);

            // Set the browser fingerprint used by uTLS
            http.ClientHello = ClientHello.HelloChrome_Auto;

            var url = "";

            // Get response with header and body
            var httpContent = await http.GetAsync(url,
                HttpBody.HeaderAndBody);

            // Download file
            var saveName = @"D:\xxx\file.jpg";
            using (var fs = new FileStream(saveName, 
                FileMode.Create, FileAccess.Write))
            {
                await http.GetBytesAsync(url, (bytes) =>
                {
                    fs.Write(bytes, 0, bytes.Length);
                });
            }

            // Set request header
            var referer = "";
            var userAgent = "";
            var cookie = "aaa=1; bbb=2";
            var header = $"Referer:{referer}|User-Agent:{userAgent}|Cookie:{cookie}";
            var html = await http.GetAsync(url, header);
        }
    }
}