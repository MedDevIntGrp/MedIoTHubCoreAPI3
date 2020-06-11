using Newtonsoft.Json;

namespace MedIoTHubCoreAPI3.Services.Model.IoT
{
    public partial class Location
    {
        [JsonProperty("city")]
        public string City { get; set; }

        [JsonProperty("country")]
        public string Country { get; set; }
    }
}
