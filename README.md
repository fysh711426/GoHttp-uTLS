# GoHttp-uTLS  

This repo is a C# wrapper for [uTLS](https://github.com/refraction-networking/utls).  
It provides ClientHello fingerprint resistance, which can simulate browser TLS handshake.  

---  

### What problems can this repo solve ?  

* Simulate browser TLS handshake  
* Support TLS 1.3 on win10  

---  

### Unsupported  

* Custom ClientHello is not supported  
* Only support http GET method  
* Only support window system  

This repo does not support custom ClientHello. If you need this function, you can use uTLS to implement it, and then package it to C# library with reference to the method used in this repo.  

---  

### Nuget install  

```
PM> Install-Package GoHttp-uTLS
```  

---  

### Example  

```C#
var url = "";
var http = new GoHttp();
var html = await http.GetAsync(url);
```

#### Get response with header and body  

```C#
var httpContent = await http.GetAsync(url,
    HttpBody.HeaderAndBody);
```

#### Download file  

```C#
var saveName = @"D:\xxx\file.jpg";
using (var fs = new FileStream(saveName, 
    FileMode.Create, FileAccess.Write))
{
    await http.GetBytesAsync(url, (bytes) =>
    {
        fs.Write(bytes, 0, bytes.Length);
    });
}
```

#### Set request header  

```C#
var userAgent = "xxx";
var cookie = "aaa=1; bbb=2";
var header = $"User-Agent:{userAgent}|Cookie:{cookie}";
var html = await http.GetAsync(url, header);
```

#### Set parameters  

```C#
var http = new GoHttp();

// Set request timeout 
http.Timeout = TimeSpan.FromSeconds(30);

// Set the browser fingerprint used by uTLS
http.ClientHello = ClientHello.HelloChrome_Auto;
```

---  

### Declare  

This repo is developed by fysh711426, not an official uTLS product.  