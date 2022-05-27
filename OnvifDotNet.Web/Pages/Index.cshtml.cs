using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace OnvifDotNet.Web.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {

        }

        public void OnPost(string cameraIp, string profileToken)
        {
            CameraIp = cameraIp;
            ProfileToken = profileToken;
        }
        public string CameraIp { get; set; } = "";
        public string ProfileToken { get; set; } = "";

        public bool HasData => !string.IsNullOrWhiteSpace(CameraIp) && !string.IsNullOrWhiteSpace(ProfileToken);
    }
}