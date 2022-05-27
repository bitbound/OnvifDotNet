using OnvifDeviceV10;
using OnvifMediaV10;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;
using LibVLCSharp.Shared;

namespace OnvifDotNet
{
    public static class OnvifHelper
    {
        static OnvifHelper()
        {
            LibVLCSharp.Shared.Core.Initialize();
        }


        public static Binding CreateBinding()
        {
            var binding = new CustomBinding();
            var textBindingElement = new TextMessageEncodingBindingElement
            {
                MessageVersion = MessageVersion.CreateVersion(EnvelopeVersion.Soap12, AddressingVersion.WSAddressing10)
            };
            var httpBindingElement = new HttpTransportBindingElement
            {
                AllowCookies = true,
                MaxBufferSize = int.MaxValue,
                MaxReceivedMessageSize = int.MaxValue
            };

            binding.Elements.Add(textBindingElement);
            binding.Elements.Add(httpBindingElement);

            return binding;
        }

        public static DeviceClient GetDeviceClient(string cameraIp)
        {
            return new DeviceClient(
                CreateBinding(),
                new EndpointAddress($"http://{cameraIp}/onvif/device_service"));
        }

        public static async Task<MediaClient> GetMediaClient(string cameraIp)
        {
            var deviceClient = GetDeviceClient(cameraIp);

            var capabilities = await deviceClient.GetCapabilitiesAsync(new[] { CapabilityCategory.All });

            return new MediaClient(
                CreateBinding(),
                new EndpointAddress(capabilities.Capabilities.Media.XAddr));
        }

        public static async Task<MediaUri> GetStreamingUri(
            string cameraIp, 
            string profileToken, 
            StreamType streamType = StreamType.RTPUnicast,
            TransportProtocol transportProtocol = TransportProtocol.RTSP)
        {
            var streamSetup = new StreamSetup()
            {
                Stream = streamType,
                Transport = new Transport() 
                { 
                    Protocol = transportProtocol 
                }
            };

            var mediaClient = await GetMediaClient(cameraIp);

            return await mediaClient.GetStreamUriAsync(streamSetup, profileToken);
        }

        public static async Task RecordStream(
            string ipAddress, 
            string profileToken, 
            StreamType streamType, 
            TransportProtocol transport, 
            string outputPath,
            CancellationToken cancellationToken = default)
        {

            using var libvlc = new LibVLC();
            using var mediaPlayer = new MediaPlayer(libvlc);
            
            libvlc.Log += (sender, e) =>
            {
                Console.WriteLine($"[{e.Level}] {e.Module}:{e.Message}");
            };

            var mediaUri = await GetStreamingUri(ipAddress, profileToken, streamType, transport);

            var media = new LibVLCSharp.Shared.Media(
                libvlc,
                mediaUri.Uri,
                FromType.FromLocation);
            
            media.AddOption(":sout=#file{dst=" + outputPath + "}");
            //media.AddOption(":sout-keep");
            mediaPlayer.Play(media);

            
            await Task.Run(cancellationToken.WaitHandle.WaitOne);
            
            mediaPlayer.Stop();
        }
    }
}
