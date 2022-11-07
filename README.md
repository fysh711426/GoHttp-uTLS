# GoHttp-uTLS  

This repo is a C# wrapper for [utls](https://github.com/refraction-networking/utls).  

It supports ClientHello fingerprint resistance, which can simulate browser handshake content.  

However, it does not currently support custom ClientHello. If you need this function, you can use golang to implement it, and then package it to C# library with reference to the method used in this repo.  

---  

### What problems can this library solve ?  

* Avoid detection by anti-bot services  
* Support TLS 1.3 on win10  

---  

> **Note**  

* Custom ClientHello is not supported  
* Only support http GET method  
* Only support window system  

---  

### Nuget install  

```
PM> Install-Package GoHttp-uTLS
```  

---  

### Example  

```C#
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
```
