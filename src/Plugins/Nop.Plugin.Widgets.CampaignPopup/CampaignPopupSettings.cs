using Nop.Core.Configuration;

namespace Nop.Plugin.Widgets.CampaignPopup
{
    public class CampaignPopupSettings : ISettings
    {
        public bool Enabled { get; set; }

        public int PictureId { get; set; }

        public string RedirectUrl { get; set; }
    }
}
