using System.ComponentModel.DataAnnotations;

namespace MedIoTHubCoreAPI3.API.DataContracts.IoT.Requests
{
    public class SearchDevicesRequest
    {
        [Required]
        public string Query { get; set; }

        public int MaxCount { get; set; }
    }
}
