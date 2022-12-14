package main

// #include <stdlib.h>
// typedef void (*callback)(char*);
// static void helper(callback f, char *str) { f(str); }
// typedef int (*bytesCallback)(char*, int);
// static int bytesHelper(bytesCallback f, char *bytes, int n) { return f(bytes, n); }
import "C"

import (
	"bufio"
	"encoding/json"
	"errors"
	"io"
	"net"
	"net/http"
	"net/http/httputil"
	"strings"
	"time"
	"unsafe"

	tls "github.com/refraction-networking/utls"
	"golang.org/x/net/http2"
)

type BytesStruct struct {
	addr unsafe.Pointer
	len  int
	cap  int
}

type Result struct {
	Success bool
	Error   string
	Data    string
}

//export HttpGet
func HttpGet(url *C.char, header *C.char, f C.callback, httpBody int, timeout int, clientHello int) {
	result := HttpGetWarp(url, header, httpBody, timeout, clientHello)
	ptr := C.CString(result)
	C.helper(f, ptr)
	C.free(unsafe.Pointer(ptr))
}

func HttpGetWarp(url *C.char, header *C.char, httpBody int, timeout int, clientHello int) string {
	_url := C.GoString(url)
	_header := C.GoString(header)

	req, _ := http.NewRequest("GET", _url, nil)
	SetRequestHeader(req, _header)

	resp, err := GetResponse(req, timeout, clientHello)
	if err != nil {
		return GetResult("", err)
	}

	defer resp.Body.Close()

	body := true
	if httpBody == 1 {
		body = false
	}
	bytes, err := httputil.DumpResponse(resp, body)
	if err != nil {
		return GetResult("", err)
	}

	data := string(bytes)
	if httpBody == 0 {
		split := strings.SplitN(data, "\r\n\r\n", 2)
		if len(split) < 2 {
			data = ""
		} else {
			data = split[1]
		}
	}
	return GetResult(data, nil)
}

//export HttpGetBytes
func HttpGetBytes(url *C.char, header *C.char, bfunc C.bytesCallback, f C.callback, httpBody int, timeout int, clientHello int) {
	result := HttpGetBytesWarp(url, header, bfunc, httpBody, timeout, clientHello)
	ptr := C.CString(result)
	C.helper(f, ptr)
	C.free(unsafe.Pointer(ptr))
}

func HttpGetBytesWarp(url *C.char, header *C.char, bfunc C.bytesCallback, httpBody int, timeout int, clientHello int) string {
	_url := C.GoString(url)
	_header := C.GoString(header)

	req, _ := http.NewRequest("GET", _url, nil)
	SetRequestHeader(req, _header)

	resp, err := GetResponse(req, timeout, clientHello)
	if err != nil {
		return GetResult("", err)
	}

	defer resp.Body.Close()

	if httpBody == 1 || httpBody == 2 {
		bytes, err := httputil.DumpResponse(resp, false)
		if err != nil {
			return GetResult("", err)
		}
		n := len(bytes)
		p := C.int(n)
		ptr := (*C.char)((*BytesStruct)(unsafe.Pointer(&bytes)).addr)
		exit := C.bytesHelper(bfunc, ptr, p)
		if int(exit) == 1 {
			return GetResult("", nil)
		}
	}

	if httpBody == 0 || httpBody == 2 {
		buffer := make([]byte, 4096)
		for {
			n, err := resp.Body.Read(buffer)
			if n > 0 {
				p := C.int(n)
				ptr := (*C.char)(unsafe.Pointer(&buffer[0]))
				exit := C.bytesHelper(bfunc, ptr, p)
				if int(exit) == 1 {
					return GetResult("", nil)
				}
			}
			if err != nil {
				if err == io.EOF {
					err = nil
				}
				break
			}
		}
		if err != nil {
			return GetResult("", err)
		}
	}
	return GetResult("", nil)
}

func GetResult(data string, err error) string {
	success := false
	_err := ""
	if err == nil {
		success = true
	} else {
		_err = err.Error()
	}
	result := &Result{Success: success, Data: data, Error: _err}
	bytes, err := json.Marshal(result)
	if err != nil {
		return `{"Success":false, "Data":"", "Error":"Result to json faild."}`
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
		req.Header.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/102.0.5005.63 Safari/537.36")
	}
}

func GetResponse(req *http.Request, timeout int, clientHello int) (*http.Response, error) {
	var resp *http.Response
	uConn, err := HandshakeHandler(req, timeout, clientHello)
	if err != nil {
		return nil, err
	}
	alpn := uConn.HandshakeState.ServerHello.AlpnProtocol
	switch alpn {
	case "http/1.1":
		resp, err = HttpHandler(req, uConn)
		if err != nil {
			return nil, err
		}
	case "h2", "":
		resp, err = Http2Handler(req, uConn)
		if err != nil {
			if strings.HasPrefix(err.Error(), "unexpected EOF") {
				uConn, err = HandshakeHandler(req, timeout, clientHello)
				if err != nil {
					return nil, err
				}
				resp, err = HttpHandler(req, uConn)
				if err != nil {
					return nil, err
				}
			} else {
				return nil, err
			}
		}
	default:
		err = errors.New("server hello alpn protocol error")
		return nil, err
	}
	return resp, nil
}

func HandshakeHandler(req *http.Request, timeout int, clientHello int) (*tls.UConn, error) {
	hostname := req.Host
	addr := hostname + ":443"

	config := tls.Config{ServerName: hostname}
	dialConn, err := net.DialTimeout("tcp", addr, time.Duration(timeout)*time.Millisecond)
	if err != nil {
		return nil, err
	}

	clientHelloID := GetClientHelloID(clientHello)
	if clientHelloID == nil {
		return nil, errors.New("not found ClientHelloID")
	}
	uConn := tls.UClient(dialConn, &config, *clientHelloID)
	err = uConn.Handshake()
	if err != nil {
		return nil, err
	}
	return uConn, nil
}

func HttpHandler(req *http.Request, uConn *tls.UConn) (*http.Response, error) {
	req.Proto = "HTTP/1.1"
	req.ProtoMajor = 1
	req.ProtoMinor = 1

	err := req.Write(uConn)
	if err != nil {
		return nil, err
	}

	resp, err := http.ReadResponse(bufio.NewReader(uConn), req)
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

	resp, err := cConn.RoundTrip(req)
	if err != nil {
		return nil, err
	}
	return resp, nil
}

func main() {}
