using AutoMapper;

using MedIoTHubCoreAPI3.IoC.Configuration.AutoMapper.Profiles;

namespace MedIoTHubCoreAPI3.IoC.Configuration.AutoMapper
{
    public static class MappingConfigurationsHelper
    {
        public static void ConfigureMapper()
        {
            var configuration = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(typeof(APIMappingProfile));
                cfg.AddProfile(typeof(ServicesMappingProfile));
            });
        }
    }
}
