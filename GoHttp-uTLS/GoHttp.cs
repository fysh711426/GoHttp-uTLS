using Newtonsoft.Json;
using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GoHttp_uTLS
{
    public class GoHttp
    {
        static GoHttp()
        {
            // Environment.SetEnvironmentVariable("GODEBUG", "cgocheck=0");
        }

        /// <summary>
        /// Http request timeout, default is 30 seconds.
        /// </summary>
        public TimeSpan Timeout { get; set; }

        /// <summary>
        /// Browser fingerprint used by uTLS, default is HelloChrome_Auto.
        /// </summary>
        public ClientHello ClientHello { get; set; } = ClientHello.HelloChrome_Auto;

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        delegate void Callback(IntPtr str);

        [DllImport("GoHttpLib.dll", EntryPoint = "HttpGet", CallingConvention = CallingConvention.StdCall)]
        extern static void _HttpGet(byte[] url, byte[] header,
            [MarshalAs(UnmanagedType.FunctionPtr)] Callback callback,
            int httpBody, int timeout, int clientHello);

        /// <summary>
        /// Call the http get method using uTLS.
        /// </summary>
        /// <param name="url">Request url.</param>
        /// <param name="httpBody">Response with header or body.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns></returns>
        public Task<string> GetAsync(string url, HttpBody httpBody = HttpBody.Body, CancellationToken token = default)
            => GetAsync(url, "", httpBody, token);

        /// <summary>
        /// Call the http get method using uTLS.
        /// </summary>
        /// <param name="url">Request url.</param>
        /// <param name="header">Request header, separated by '|'.</param>
        /// <param name="httpBody">Response with header or body.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public Task<string> GetAsync(string url, string header,
            HttpBody httpBody = HttpBody.Body, CancellationToken token = default)
        {
            CheckOSPlatform();

            if (string.IsNullOrEmpty(url))
                throw new Exception("url cannot be empty.");
            header = header ?? "";

            var timeout = Timeout.TotalMilliseconds == 0 ?
                TimeSpan.FromSeconds(30) : Timeout;
            
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
                        },
                        (int)httpBody,
                        (int)timeout.TotalMilliseconds,
                        (int)ClientHello);
                    token.ThrowIfCancellationRequested();
                    var result = JsonConvert.DeserializeObject<Result>(json);
                    if (result == null)
                        throw new Exception("Result cannot be null.");
                    if (result.Success)
                        tsc.SetResult(result.Data);
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
        delegate int BytesCallback(IntPtr bytes, int n);

        [DllImport("GoHttpLib.dll", EntryPoint = "HttpGetBytes", CallingConvention = CallingConvention.StdCall)]
        extern static void _HttpGetBytes(byte[] url, byte[] header,
            [MarshalAs(UnmanagedType.FunctionPtr)] BytesCallback bcallback,
            [MarshalAs(UnmanagedType.FunctionPtr)] Callback callback,
            int httpBody, int timeout, int clientHello);

        /// <summary>
        /// Call the http get method using uTLS.
        /// </summary>
        /// <param name="url">Request url.</param>
        /// <param name="callback">Response bytes callback.</param>
        /// <param name="httpBody">Response with header or body.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns></returns>
        public Task GetBytesAsync(string url, Action<byte[]> callback, HttpBody httpBody = HttpBody.Body, CancellationToken token = default)
            => GetBytesAsync(url, "", callback, httpBody, token);

        /// <summary>
        /// Call the http get method using uTLS.
        /// </summary>
        /// <param name="url">Request url.</param>
        /// <param name="header">Request header, separated by '|'.</param>
        /// <param name="callback">Response bytes callback.</param>
        /// <param name="httpBody">Response with header or body.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public Task GetBytesAsync(string url, 
            string header, Action<byte[]> callback,
            HttpBody httpBody = HttpBody.Body, CancellationToken token = default)
        {
            CheckOSPlatform();

            if (string.IsNullOrEmpty(url))
                throw new Exception("url cannot be empty.");
            if (callback == null)
                throw new Exception("callback cannot be null.");
            header = header ?? "";

            var timeout = Timeout.TotalMilliseconds == 0 ?
                TimeSpan.FromSeconds(30) : Timeout;

            var tsc = new TaskCompletionSource<bool>();
            Task.Run(() =>
            {
                try
                {
                    var json = "";
                    GoHttp._HttpGetBytes(
                        Encoding.UTF8.GetBytes(url),
                        Encoding.UTF8.GetBytes(header),
                        (ptr, n) =>
                        {
                            var buffer = new byte[n];
                            Marshal.Copy(ptr, buffer, 0, n);
                            callback.Invoke(buffer);
                            if (token.IsCancellationRequested)
                                return 1;
                            return 0;
                        },
                        (str) =>
                        {
                            json = Utils.GoStringToCSharpString(str);
                        },
                        (int)httpBody,
                        (int)timeout.TotalMilliseconds,
                        (int)ClientHello);
                    token.ThrowIfCancellationRequested();
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

        private void CheckOSPlatform()
        {
#if (NET48 || NET47 || NET46 || NET45)
#else
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                throw new Exception("Only support window system.");
#endif
        }

        private class Result
        {
            public bool Success { get; set; }
            public string Data { get; set; } = "";
            public string Error { get; set; } = "";
        }
    }
}