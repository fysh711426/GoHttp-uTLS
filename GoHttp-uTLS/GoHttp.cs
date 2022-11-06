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

        class Result
        {
            public bool Success { get; set; }
            public string Data { get; set; }
            public string Error { get; set; }
        }

        static string GoStringToCSharpString(IntPtr pointer)
        {
            var length = 0;
            while (Marshal.ReadByte(pointer + length) != 0)
                length++;
            var buffer = new byte[length];
            Marshal.Copy(pointer, buffer, 0, length);
            return Encoding.UTF8.GetString(buffer);
        }

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        delegate void Callback(IntPtr str);

        [DllImport("GoHttp.dll", EntryPoint = "HttpGet", CallingConvention = CallingConvention.StdCall)]
        extern static void _HttpGet(byte[] url, byte[] header,
            [MarshalAs(UnmanagedType.FunctionPtr)] Callback callback);

        public static Task<string> GetAsync(string url, string header = "", bool bodyOnly = true)
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
                            json = GoStringToCSharpString(str);
                        });
                    var result = JsonConvert.DeserializeObject<Result>(json);
                    if (result.Success)
                    {
                        var body = result.Data;
                        if (bodyOnly)
                        {
                            var split = result.Data.Split("\r\n\r\n", 2);
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

        [DllImport("GoHttp.dll", EntryPoint = "HttpGetBytes", CallingConvention = CallingConvention.StdCall)]
        extern static void _HttpGetBytes(byte[] url, byte[] header,
            [MarshalAs(UnmanagedType.FunctionPtr)] BytesCallback bcallback,
            [MarshalAs(UnmanagedType.FunctionPtr)] Callback callback);

        public static Task GetBytesAsync(string url, Action<byte[]> callback, bool bodyOnly = true)
            => _GetBytesAsync(url, callback: callback, bodyOnly: bodyOnly);
        public static Task GetBytesAsync(string url, string header, Action<byte[]> callback, bool bodyOnly = true)
            => _GetBytesAsync(url, header: header, callback: callback, bodyOnly: bodyOnly);

        static Task _GetBytesAsync(string url,
            string header = "", Action<byte[]> callback = null, bool bodyOnly = true)
        {
            var tsc = new TaskCompletionSource();
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
                            if (bodyOnly && isFirst)
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
                            json = GoStringToCSharpString(str);
                        });
                    var result = JsonConvert.DeserializeObject<Result>(json);
                    if (result.Success)
                        tsc.SetResult();
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