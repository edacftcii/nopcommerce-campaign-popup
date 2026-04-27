using System.ComponentModel.DataAnnotations;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Widgets.CampaignPopup.Models
{
    public record ConfigurationModel : BaseNopModel
    {
        public int ActiveStoreScopeConfiguration { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.CampaignPopup.Fields.Enabled")]
        public bool Enabled { get; set; }
        public bool Enabled_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.CampaignPopup.Fields.PictureId")]
        [UIHint("Picture")]
        public int PictureId { get; set; }
        public bool PictureId_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.CampaignPopup.Fields.RedirectUrl")]
        public string RedirectUrl { get; set; }
        public bool RedirectUrl_OverrideForStore { get; set; }
    }
}
