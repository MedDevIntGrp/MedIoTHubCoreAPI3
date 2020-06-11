using AutoMapper;

using DCIoT = MedIoTHubCoreAPI3.API.DataContracts.IoT;
using SIoT = MedIoTHubCoreAPI3.Services.Model.IoT;

namespace MedIoTHubCoreAPI3.IoC.Configuration.AutoMapper.Profiles
{
    public class APIMappingProfile : Profile
    {
        public APIMappingProfile()
        {
            CreateMap<SIoT.Device, DCIoT.Device>().ReverseMap();
            CreateMap<SIoT.DeviceIoTSettings, DCIoT.DeviceIoTSettings>().ReverseMap();
            CreateMap<SIoT.Location, DCIoT.Location>().ReverseMap();
            CreateMap<SIoT.Tags, DCIoT.Tags>().ReverseMap();
            CreateMap<SIoT.Twins, DCIoT.Twins>().ReverseMap();
            CreateMap<SIoT.Properties, DCIoT.Properties>().ReverseMap();
        }
    }
}
