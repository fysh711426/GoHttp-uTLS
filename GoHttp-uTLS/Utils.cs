using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace GoHttp_uTLS
{
    internal static class Utils
    {
        public static string GoStringToCSharpString(IntPtr pointer)
        {
            var length = 0;
            while (Marshal.ReadByte(pointer + length) != 0)
                length++;
            var buffer = new byte[length];
            Marshal.Copy(pointer, buffer, 0, length);
            return Encoding.UTF8.GetString(buffer);
        }
    }
}
