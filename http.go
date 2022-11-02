package main

// #include <stdlib.h>
// typedef void (*callback)(char*);
// static void helper(callback f, char *str) { f(str); }
import "C"

import (
	"bufio"
	"net"
	"net/http"
	"net/http/httputil"
	"strings"
	"time"
	"unsafe"

	tls "github.com/refraction-networking/utls"
	"golang.org/x/net/http2"
)

//export HttpGet
func HttpGet(url *C.char, header *C.char, f C.callback) {
	html := HttpGetWarp(url, header)
	ptr := C.CString(html)
	C.helper(f, ptr)
	C.free(unsafe.Pointer(ptr))
}

func HttpGetWarp(url *C.char, header *C.char) string {
	_url := C.GoString(url)
	_header := C.GoString(header)

	req, _ := http.NewRequest("GET", _url, nil)
	SetRequestHeader(req, _header)

	uTlsConn, err := HandshakeHandler(req)
	if err != nil {
		return ""
	}

	var resp *http.Response
	alpn := uTlsConn.HandshakeState.ServerHello.AlpnProtocol
	switch alpn {
	case "http/1.1":
		resp, err = HttpHandler(req, uTlsConn)
		if err != nil {
			return ""
		}
	case "h2", "":
		resp, err = Http2Handler(req, uTlsConn)
		if err != nil {
			if strings.HasPrefix(err.Error(), "unexpected EOF") {
				uTlsConn, err = HandshakeHandler(req)
				if err != nil {
					return ""
				}
				resp, err = HttpHandler(req, uTlsConn)
				if err != nil {
					return ""
				}
			} else {
				return ""
			}
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

func SetRequestHeader(req *http.Request, header string) {
	if header != "" {
		attrs := strings.Split(header, "|")
		for _, attr := range attrs {
			split := strings.SplitN(attr, ":", 2)
			key := strings.Trim(split[0], " ")
			val := strings.Trim(split[1], " ")
			req.Header.Add(key, val)
		}
	}
	if _, ok := req.Header["User-Agent"]; !ok {
		req.Header.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/92.0.4515.131 Safari/537.36")
	}
}

func HandshakeHandler(req *http.Request) (*tls.UConn, error) {
	hostname := req.Host
	addr := hostname + ":443"

	config := tls.Config{ServerName: hostname}
	dialConn, err := net.DialTimeout("tcp", addr, time.Duration(15)*time.Second)
	if err != nil {
		return nil, err
	}

	uTlsConn := tls.UClient(dialConn, &config, tls.HelloChrome_102)
	err = uTlsConn.Handshake()
	if err != nil {
		return nil, err
	}
	return uTlsConn, nil
}

func HttpHandler(req *http.Request, uConn *tls.UConn) (*http.Response, error) {
	req.Proto = "HTTP/1.1"
	req.ProtoMajor = 1
	req.ProtoMinor = 1

	err := req.Write(uConn)
	if err != nil {
		return nil, err
	}

	var resp *http.Response
	resp, err = http.ReadResponse(bufio.NewReader(uConn), req)
	if err != nil {
		return nil, err
	}
	return resp, nil
}

func Http2Handler(req *http.Request, uConn *tls.UConn) (*http.Response, error) {
	req.Proto = "HTTP/2.0"
	req.ProtoMajor = 2
	req.ProtoMinor = 0

	tr := http2.Transport{}
	cConn, err := tr.NewClientConn(uConn)
	if err != nil {
		return nil, err
	}

	var resp *http.Response
	resp, err = cConn.RoundTrip(req)
	if err != nil {
		return nil, err
	}
	return resp, nil
}

func main() {
	url := ""
	header := ""
	resp := HttpGetWarp(C.CString(url), C.CString(header))
	print(resp)
}
