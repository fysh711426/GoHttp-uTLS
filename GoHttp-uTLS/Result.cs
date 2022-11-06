using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoHttp_uTLS
{
    internal class Result
    {
        public bool Success { get; set; }
        public string Data { get; set; } = "";
        public string Error { get; set; } = "";
    }
}
