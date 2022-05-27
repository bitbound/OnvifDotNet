using OnvifDeviceV10;
using OnvifDotNet.Core.Helpers;
using OnvifMediaV10;
using System.CommandLine;
using System.Text.Json;

var jsonOptions = new JsonSerializerOptions() { WriteIndented = true };

var rootCommand = new RootCommand("Get info about and record video from ONVIF-compliant cameras.");

var ipOption = new Option<string>(
    new[] { "-i", "--ip" },
    "The IP address of the ONVIF-compliant camera.");
ipOption.IsRequired = true;

var profileTokenOption = new Option<string>(
    new[] { "-p", "--profile-token" },
    "The token string for a media profile.  This can be found in the profiles command output.");
profileTokenOption.IsRequired = true;

var streamTypeOption = new Option<StreamType>(
    new[] { "-s", "--stream-type" },
    () => StreamType.RTPUnicast,
    "The stream type to request when creating a new streaming URI.");

var transportOption = new Option<TransportProtocol>(
    new[] { "-t", "--transport" },
     () => TransportProtocol.RTSP,
    "The transport protocol to request when creating a new streaming URI.");

var outputPathOption = new Option<string>(
    new[] { "-o", "--output-dir" },
    "The output directory where recordings will be saved, organized by date.");
outputPathOption.IsRequired = true;


var capabilitiesCommand = new Command("capabilities", "Output the capabilities of a camera in JSON format.");
capabilitiesCommand.AddOption(ipOption);
capabilitiesCommand.SetHandler(async (string ipAddress) =>
{
    var client = OnvifHelper.GetDeviceClient(ipAddress);
    var capabilities = await client.GetCapabilitiesAsync(new[] { CapabilityCategory.All });

    Console.WriteLine("###### Device Capabilities ######");
    Console.WriteLine(JsonSerializer.Serialize(capabilities.Capabilities, jsonOptions));
    Console.WriteLine("#### End Device Capabilities ####");
}, ipOption);



var profilesCommand = new Command("profiles", "Output the media profiles of a camera in JSON format.");
profilesCommand.AddOption(ipOption);
profilesCommand.SetHandler(async (string ipAddress) =>
{
    var client = await OnvifHelper.GetMediaClient(ipAddress);
    var profiles = await client.GetProfilesAsync();
    Console.WriteLine("###### Media Profiles ######");
    Console.WriteLine(JsonSerializer.Serialize(profiles.Profiles, jsonOptions));
    Console.WriteLine("#### End Media Profiles ####");
}, ipOption);


var mediaUriCommand = new Command("uri", "Retrieve and output a new media URI.");
mediaUriCommand.AddOption(ipOption);
mediaUriCommand.AddOption(profileTokenOption);
mediaUriCommand.AddOption(streamTypeOption);
mediaUriCommand.AddOption(transportOption);
mediaUriCommand.SetHandler(async (string ipAddress, string profileToken, StreamType streamType, TransportProtocol transport) =>
{
    var mediaUri = await OnvifHelper.GetStreamingUri(ipAddress, profileToken, streamType, transport);

    Console.WriteLine("###### Streaming Uri ######");
    Console.WriteLine(mediaUri.Uri);
    Console.WriteLine("#### End Streaming Uri ####");
}, ipOption, profileTokenOption, streamTypeOption, transportOption);


var recordCommand = new Command("record", "Record from a streaming URI to the file system.");
recordCommand.AddOption(ipOption);
recordCommand.AddOption(profileTokenOption);
recordCommand.AddOption(streamTypeOption);
recordCommand.AddOption(transportOption);
recordCommand.AddOption(outputPathOption);
recordCommand.SetHandler(async (string ipAddress, string profileToken, StreamType streamType, TransportProtocol transport, string outputDir) =>
{
    var cancelRequested = false;
    Console.CancelKeyPress += (s, e) =>
    {
        cancelRequested = true;
    };

    var cts = new CancellationTokenSource();

    Console.WriteLine("###### Recording Stream ######");
    while (!cancelRequested)
    {
        var now = DateTimeOffset.Now;
        var fileDir = Path.Combine(outputDir, $"{now:yyyy-MM}");
        Directory.CreateDirectory(fileDir);
        var filePath = Path.Combine(
            fileDir, 
            $"{now:yyyy-MM-dd HH.mm.ss.fff}.mp4");

        cts = new CancellationTokenSource();
        cts.CancelAfter(TimeSpan.FromMinutes(15));

        await OnvifHelper.RecordStream(ipAddress, profileToken, streamType, transport, filePath, cts.Token);
    }

    cts.Cancel();

    Console.WriteLine("#### End Recording Stream ####");
}, ipOption, profileTokenOption, streamTypeOption, transportOption, outputPathOption);



rootCommand.AddCommand(capabilitiesCommand);
rootCommand.AddCommand(profilesCommand);
rootCommand.AddCommand(mediaUriCommand);
rootCommand.AddCommand(recordCommand);

await rootCommand.InvokeAsync(args);