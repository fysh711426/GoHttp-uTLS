package main

import (
	"C"
	"bufio"
	"net"
	"net/http"
	"net/http/httputil"
	"time"

	tls "github.com/refraction-networking/utls"
	"golang.org/x/net/http2"
)

//export HttpGet
func HttpGet(curl *C.char) string {
	url := C.GoString(curl)
	req, _ := http.NewRequest("GET", url, nil)
	req.Header.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/92.0.4515.131 Safari/537.36")
	hostname := req.Host
	addr := hostname + ":443"

	config := tls.Config{ServerName: hostname}
	dialConn, err := net.DialTimeout("tcp", addr, time.Duration(15)*time.Second)
	if err != nil {
		return ""
	}
	uTlsConn := tls.UClient(dialConn, &config, tls.HelloChrome_62)
	err = uTlsConn.Handshake()
	if err != nil {
		return ""
	}

	var resp *http.Response
	alpn := uTlsConn.HandshakeState.ServerHello.AlpnProtocol
	switch alpn {
	case "h2":
		req.Proto = "HTTP/2.0"
		req.ProtoMajor = 2
		req.ProtoMinor = 0

		tr := http2.Transport{}
		cConn, err := tr.NewClientConn(uTlsConn)
		if err != nil {
			return ""
		}
		resp, err = cConn.RoundTrip(req)
		if err != nil {
			return ""
		}
	case "http/1.1", "":
		req.Proto = "HTTP/1.1"
		req.ProtoMajor = 1
		req.ProtoMinor = 1

		err := req.Write(uTlsConn)
		if err != nil {
			return ""
		}
		resp, err = http.ReadResponse(bufio.NewReader(uTlsConn), req)
		if err != nil {
			return ""
		}
	default:
		return ""
	}

	bytes, _err := httputil.DumpResponse(resp, true)
	if _err != nil {
		return ""
	}
	return string(bytes)
}

func main() {}
