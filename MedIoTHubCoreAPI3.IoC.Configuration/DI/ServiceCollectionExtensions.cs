using System;
using System.Reflection;

using AutoMapper;

using MedIoTHubCoreAPI3.API.Common.Settings;
using MedIoTHubCoreAPI3.IoC.Configuration.AutoMapper;
using MedIoTHubCoreAPI3.Services;
using MedIoTHubCoreAPI3.Services.Contracts;
using MedIoTHubCoreAPI3.Services.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;


namespace MedIoTHubCoreAPI3.IoC.Configuration.DI
{
    public static class ServiceCollectionExtensions
    {
        public static void ConfigureBusinessServices(this IServiceCollection services, IConfiguration configuration)
        {
            if (services != null)
            {
                var appSettingsSection = configuration.GetSection(nameof(AppSettings));
                if (appSettingsSection == null)
                    throw new System.Exception("No appsettings section has been found");

                var appSettings = appSettingsSection.Get<AppSettings>();

                if (!appSettings.IsValid())
                    throw new Exception("No valid settings.");

                services.Configure<AppSettings>(appSettingsSection);

                services.AddTransient<IDeviceManagementService, DeviceManagementService>();
                services.AddTransient<IIoTHubC2DOperationsService, IoTHubC2DOperationsService>();
            }
        }

        public static void ConfigureMappings(this IServiceCollection services)
        {
            if (services != null)
            {
                //Automap settings
                services.AddAutoMapper(Assembly.GetExecutingAssembly());
                MappingConfigurationsHelper.ConfigureMapper();
            }
        }
    }
}
