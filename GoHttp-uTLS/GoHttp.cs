using Newtonsoft.Json;
using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace GoHttp_uTLS
{
    public class GoHttp
    {
        static GoHttp()
        {
            // Environment.SetEnvironmentVariable("GODEBUG", "cgocheck=0");
        }

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        delegate void Callback(IntPtr str);

        [DllImport("GoHttpLib.dll", EntryPoint = "HttpGet", CallingConvention = CallingConvention.StdCall)]
        extern static void _HttpGet(byte[] url, byte[] header,
            [MarshalAs(UnmanagedType.FunctionPtr)] Callback callback);

        public static Task<string> GetAsync(string url, string header = "", bool respWithHeader = false)
        {
            var tsc = new TaskCompletionSource<string>();
            Task.Run(() =>
            {
                try
                {
                    var json = "";
                    GoHttp._HttpGet(
                        Encoding.UTF8.GetBytes(url),
                        Encoding.UTF8.GetBytes(header),
                        (str) =>
                        {
                            json = Utils.GoStringToCSharpString(str);
                        });
                    var result = JsonConvert.DeserializeObject<Result>(json);
                    if (result == null)
                        throw new Exception("Result cannot be null.");
                    if (result.Success)
                    {
                        var body = result.Data;
                        if (!respWithHeader)
                        {
                            var split = result.Data.Split(
                                new string[] { "\r\n\r\n" }, 2, StringSplitOptions.None);
                            body = split.Length < 2 ? "" : split[1];
                        }
                        tsc.SetResult(body);
                    }
                    else
                        throw new Exception(result.Error);
                }
                catch (Exception ex)
                {
                    tsc.SetException(ex);
                }
            });
            return tsc.Task;
        }

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        delegate void BytesCallback(IntPtr bytes, int n);

        [DllImport("GoHttpLib.dll", EntryPoint = "HttpGetBytes", CallingConvention = CallingConvention.StdCall)]
        extern static void _HttpGetBytes(byte[] url, byte[] header,
            [MarshalAs(UnmanagedType.FunctionPtr)] BytesCallback bcallback,
            [MarshalAs(UnmanagedType.FunctionPtr)] Callback callback);

        public static Task GetBytesAsync(string url, Action<byte[]> callback, bool respWithHeader = false)
            => GetBytesAsync(url, "", callback, respWithHeader);

        public static Task GetBytesAsync(string url, 
            string header, Action<byte[]> callback, bool respWithHeader = false)
        {
            if (string.IsNullOrEmpty(url))
                throw new Exception("url cannot be empty.");
            if (callback == null)
                throw new Exception("callback cannot be null.");
            header = header ?? "";

            var tsc = new TaskCompletionSource<bool>();
            Task.Run(() =>
            {
                try
                {
                    var json = "";
                    var isFirst = true;
                    GoHttp._HttpGetBytes(
                        Encoding.UTF8.GetBytes(url),
                        Encoding.UTF8.GetBytes(header),
                        (ptr, n) =>
                        {
                            if (!respWithHeader && isFirst)
                            {
                                isFirst = false;
                                return;
                            }
                            var buffer = new byte[n];
                            Marshal.Copy(ptr, buffer, 0, n);
                            callback?.Invoke(buffer);
                        },
                        (str) =>
                        {
                            json = Utils.GoStringToCSharpString(str);
                        });
                    var result = JsonConvert.DeserializeObject<Result>(json);
                    if (result == null)
                        throw new Exception("Result cannot be null.");
                    if (result.Success)
                        tsc.SetResult(true);
                    else
                        throw new Exception(result.Error);
                }
                catch (Exception ex)
                {
                    tsc.SetException(ex);
                }
            });
            return tsc.Task;
        }
    }
}