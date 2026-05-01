using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Cms;
using Nop.Plugin.Widgets.CampaignPopup.Models;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.Widgets.CampaignPopup.Controllers
{
    [AuthorizeAdmin]
    [Area(AreaNames.Admin)]
    [AutoValidateAntiforgeryToken]
    public class CampaignPopupController : BasePluginController
    {
        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;
        private readonly IPermissionService _permissionService;
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;

        public CampaignPopupController(
            ILocalizationService localizationService,
            INotificationService notificationService,
            IPermissionService permissionService,
            ISettingService settingService,
            IStoreContext storeContext)
        {
            _localizationService = localizationService;
            _notificationService = notificationService;
            _permissionService = permissionService;
            _settingService = settingService;
            _storeContext = storeContext;
        }

        public async Task<IActionResult> Configure()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageWidgets))
                return AccessDeniedView();

            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var settings = await _settingService.LoadSettingAsync<CampaignPopupSettings>(storeScope);
            var widgetSettings = await _settingService.LoadSettingAsync<WidgetSettings>(storeScope);

            var model = new ConfigurationModel
            {
                ActiveStoreScopeConfiguration = storeScope,
                Enabled = settings.Enabled && widgetSettings.ActiveWidgetSystemNames.Contains(CampaignPopupDefaults.SystemName),
                PictureId = settings.PictureId,
                RedirectUrl = settings.RedirectUrl
            };

            if (storeScope > 0)
            {
                model.Enabled_OverrideForStore =
                    await _settingService.SettingExistsAsync(settings, x => x.Enabled, storeScope) ||
                    await _settingService.SettingExistsAsync(widgetSettings, x => x.ActiveWidgetSystemNames, storeScope);
                model.PictureId_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.PictureId, storeScope);
                model.RedirectUrl_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.RedirectUrl, storeScope);
            }

            return View("~/Plugins/Widgets.CampaignPopup/Views/CampaignPopup/Configure.cshtml", model);
        }

        [HttpPost]
        public async Task<IActionResult> Configure(ConfigurationModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageWidgets))
                return AccessDeniedView();

            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var settings = await _settingService.LoadSettingAsync<CampaignPopupSettings>(storeScope);
            var widgetSettings = await _settingService.LoadSettingAsync<WidgetSettings>(storeScope);

            settings.Enabled = model.Enabled;
            settings.PictureId = model.PictureId;
            settings.RedirectUrl = model.RedirectUrl?.Trim();

            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.Enabled, model.Enabled_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.PictureId, model.PictureId_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.RedirectUrl, model.RedirectUrl_OverrideForStore, storeScope, false);

            if (model.Enabled && !widgetSettings.ActiveWidgetSystemNames.Contains(CampaignPopupDefaults.SystemName))
                widgetSettings.ActiveWidgetSystemNames.Add(CampaignPopupDefaults.SystemName);
            if (!model.Enabled && widgetSettings.ActiveWidgetSystemNames.Contains(CampaignPopupDefaults.SystemName))
                widgetSettings.ActiveWidgetSystemNames.Remove(CampaignPopupDefaults.SystemName);

            await _settingService.SaveSettingOverridablePerStoreAsync(widgetSettings, x => x.ActiveWidgetSystemNames, model.Enabled_OverrideForStore, storeScope, false);
            await _settingService.ClearCacheAsync();

            _notificationService.SuccessNotification(
                await _localizationService.GetResourceAsync("Plugins.Widgets.CampaignPopup.Messages.SettingsSaved"));

            return await Configure();
        }
    }
}
