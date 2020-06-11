using System.ComponentModel.DataAnnotations;

namespace MedIoTHubCoreAPI3.API.DataContracts.IoT.Requests
{
    public class ProvisionDeviceRequest
    {
        [Required]
        public string DeviceId { get; set; }

        public DeviceIoTSettings DeviceIoTSettings { get; set; }
    }
}
