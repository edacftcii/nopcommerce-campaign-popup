using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Routing;
using Nop.Core;
using Nop.Core.Domain.Cms;
using Nop.Services.Cms;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Plugins;
using Nop.Web.Framework;
using Nop.Web.Framework.Infrastructure;
using Nop.Web.Framework.Menu;

namespace Nop.Plugin.Widgets.CampaignPopup
{
    public class CampaignPopupPlugin : BasePlugin, IWidgetPlugin, IAdminMenuPlugin
    {
        private readonly IWebHelper _webHelper;
        private readonly ILocalizationService _localizationService;
        private readonly ISettingService _settingService;
        private readonly WidgetSettings _widgetSettings;

        public CampaignPopupPlugin(
            IWebHelper webHelper,
            ILocalizationService localizationService,
            ISettingService settingService,
            WidgetSettings widgetSettings)
        {
            _webHelper = webHelper;
            _localizationService = localizationService;
            _settingService = settingService;
            _widgetSettings = widgetSettings;
        }

        public bool HideInWidgetList => false;

        public override async Task InstallAsync()
        {
            await _settingService.SaveSettingAsync(new CampaignPopupSettings
            {
                Enabled = true,
                PictureId = 0,
                RedirectUrl = string.Empty
            });

            if (!_widgetSettings.ActiveWidgetSystemNames.Contains(CampaignPopupDefaults.SystemName))
            {
                _widgetSettings.ActiveWidgetSystemNames.Add(CampaignPopupDefaults.SystemName);
                await _settingService.SaveSettingAsync(_widgetSettings);
            }

            await _localizationService.AddOrUpdateLocaleResourceAsync(new Dictionary<string, string>
            {
                ["Plugins.Widgets.CampaignPopup.Menu.Main"] = "Campaign Popup",
                ["Plugins.Widgets.CampaignPopup.Menu.Settings"] = "Settings",
                ["Plugins.Widgets.CampaignPopup.Configuration"] = "Campaign popup configuration",
                ["Plugins.Widgets.CampaignPopup.Fields.Enabled"] = "Enabled",
                ["Plugins.Widgets.CampaignPopup.Fields.Enabled.Hint"] = "Enable or disable the campaign popup on the storefront.",
                ["Plugins.Widgets.CampaignPopup.Fields.PictureId"] = "Campaign image",
                ["Plugins.Widgets.CampaignPopup.Fields.PictureId.Hint"] = "Upload the image that will be displayed inside the popup.",
                ["Plugins.Widgets.CampaignPopup.Fields.RedirectUrl"] = "Redirect URL",
                ["Plugins.Widgets.CampaignPopup.Fields.RedirectUrl.Hint"] = "Enter the URL that customers should visit when they click the campaign image.",
                ["Plugins.Widgets.CampaignPopup.Messages.SettingsSaved"] = "Campaign popup settings saved successfully."
            });

            await base.InstallAsync();
        }

        public override async Task UninstallAsync()
        {
            await _settingService.DeleteSettingAsync<CampaignPopupSettings>();

            if (_widgetSettings.ActiveWidgetSystemNames.Contains(CampaignPopupDefaults.SystemName))
            {
                _widgetSettings.ActiveWidgetSystemNames.Remove(CampaignPopupDefaults.SystemName);
                await _settingService.SaveSettingAsync(_widgetSettings);
            }

            await _localizationService.DeleteLocaleResourcesAsync("Plugins.Widgets.CampaignPopup");

            await base.UninstallAsync();
        }

        public override string GetConfigurationPageUrl()
        {
            return $"{_webHelper.GetStoreLocation()}Admin/CampaignPopup/Configure";
        }

        public Task<IList<string>> GetWidgetZonesAsync()
        {
            IList<string> widgetZones = new List<string>
            {
                PublicWidgetZones.BodyEndHtmlTagBefore
            };

            return Task.FromResult(widgetZones);
        }

        public string GetWidgetViewComponentName(string widgetZone)
        {
            return CampaignPopupDefaults.ViewComponentName;
        }

        public async Task ManageSiteMapAsync(SiteMapNode rootNode)
        {
            var mainTitle = await _localizationService.GetResourceAsync("Plugins.Widgets.CampaignPopup.Menu.Main");
            var settingsTitle = await _localizationService.GetResourceAsync("Plugins.Widgets.CampaignPopup.Menu.Settings");

            var menuItem = new SiteMapNode
            {
                SystemName = "CampaignPopup.Main",
                Title = string.IsNullOrWhiteSpace(mainTitle) ? "Campaign Popup" : mainTitle,
                Visible = true,
                IconClass = "far fa-image",
                ChildNodes = new List<SiteMapNode>
                {
                    new SiteMapNode
                    {
                        SystemName = "CampaignPopup.Settings",
                        Title = string.IsNullOrWhiteSpace(settingsTitle) ? "Settings" : settingsTitle,
                        ControllerName = "CampaignPopup",
                        ActionName = "Configure",
                        Visible = true,
                        IconClass = "far fa-circle",
                        RouteValues = new RouteValueDictionary { { "area", AreaNames.Admin } }
                    }
                }
            };

            var pluginNode = rootNode.ChildNodes.FirstOrDefault(x => x.SystemName == "Third party plugins");
            if (pluginNode != null)
                pluginNode.ChildNodes.Add(menuItem);
            else
                rootNode.ChildNodes.Add(menuItem);
        }
    }
}
