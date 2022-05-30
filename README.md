# OnvifDotNet
Sample library and projects for interacting with ONVIF-compliant IP cameras.

Example Usages:

```
// List the device capabilities.
onvif-dotnet capabilities --ip 192.168.0.138

// List the media profiles.
onvif-dotnet profiles --ip 192.168.0.138

// Generate a new streaming URI.
onvif-dotnet uri -i 192.168.0.138 -p MainProfileToken

// Record the stream to files, organized by date/time.
onvif-dotnet record -i 192.168.0.138 -p MainProfileToken -o \path\to\some\folder\
```
