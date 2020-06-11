using System.ComponentModel.DataAnnotations;

namespace MedIoTHubCoreAPI3.API.DataContracts.IoT.Requests
{
    public class DirectMethodInvoqueRequest
    {
        [Required]
        public string DeviceId { get; set; }

        [Required]
        public string MethodName { get; set; }

        public string Payload { get; set; }
    }
}
