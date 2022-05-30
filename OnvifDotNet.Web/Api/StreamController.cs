using Microsoft.AspNetCore.Mvc;
using OnvifDotNet.Core.Helpers;
using System.Diagnostics;

namespace OnvifDotNet.Web.Api
{
    [ApiController]
    [Route("api/[controller]")]
    public class StreamController : ControllerBase  
    {
        [HttpGet("{ipAddress}/{profileToken}")]
        public async Task Get(string ipAddress, string profileToken)
        {
            var mediaUri = await OnvifHelper.GetStreamingUri(ipAddress, profileToken);
            var psi = new ProcessStartInfo()
            {
                FileName = "ffmpeg.exe",
                Arguments = $"-i {mediaUri.Uri} -r 10 -f ogg pipe:1",
                RedirectStandardOutput = true
            };

            using var proc = Process.Start(psi);

            if (proc is null)
            {
                return;
            }

            Response.Headers.ContentType = "video/ogg";

            await proc.StandardOutput.BaseStream.CopyToAsync(Response.Body);
        }
    }
}
