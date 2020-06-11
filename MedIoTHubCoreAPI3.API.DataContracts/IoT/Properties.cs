using Newtonsoft.Json.Linq;

namespace MedIoTHubCoreAPI3.API.DataContracts.IoT
{
    public class Properties
    {
        public JObject Desired { get; set; }

        public JObject Reported { get; set; }
    }
}
