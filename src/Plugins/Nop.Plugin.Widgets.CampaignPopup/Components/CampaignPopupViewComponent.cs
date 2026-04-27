using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Plugin.Widgets.CampaignPopup.Models;
using Nop.Services.Configuration;
using Nop.Services.Media;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.Widgets.CampaignPopup.Components
{
    [ViewComponent(Name = CampaignPopupDefaults.ViewComponentName)]
    public class CampaignPopupViewComponent : NopViewComponent
    {
        private readonly IStoreContext _storeContext;
        private readonly ISettingService _settingService;
        private readonly IPictureService _pictureService;

        public CampaignPopupViewComponent(
            IStoreContext storeContext,
            ISettingService settingService,
            IPictureService pictureService)
        {
            _storeContext = storeContext;
            _settingService = settingService;
            _pictureService = pictureService;
        }

        public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
        {
            var store = await _storeContext.GetCurrentStoreAsync();
            var settings = await _settingService.LoadSettingAsync<CampaignPopupSettings>(store.Id);

            if (!settings.Enabled || settings.PictureId <= 0)
                return Content(string.Empty);

            var imageUrl = await _pictureService.GetPictureUrlAsync(settings.PictureId, showDefaultPicture: false);
            if (string.IsNullOrWhiteSpace(imageUrl))
                return Content(string.Empty);

            var model = new PublicInfoModel
            {
                ImageUrl = imageUrl,
                RedirectUrl = settings.RedirectUrl
            };

            return View("~/Plugins/Widgets.CampaignPopup/Views/CampaignPopup/PublicInfo.cshtml", model);
        }
    }
}
