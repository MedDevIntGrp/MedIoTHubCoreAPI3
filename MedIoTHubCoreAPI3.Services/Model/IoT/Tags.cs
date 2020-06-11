using Newtonsoft.Json;

namespace MedIoTHubCoreAPI3.Services.Model.IoT
{
    public class Tags
    {
        #region Message and settings oriented

        [JsonProperty("measuredDataSchemaVersion", NullValueHandling = NullValueHandling.Ignore)]
        public string MeasuredDataSchemaVersion { get; set; }

        #endregion

        [JsonProperty("patient", NullValueHandling = NullValueHandling.Ignore)]
        public Patient Patient { get; set; }

        #region System and manufacturer oriented

        [JsonProperty("deviceType", NullValueHandling = NullValueHandling.Ignore)]
        public string DeviceType { get; set; }

        [JsonProperty("serialNumber", NullValueHandling = NullValueHandling.Ignore)]
        public string SerialNumber { get; set; }

        [JsonProperty("manufacturerCode", NullValueHandling = NullValueHandling.Ignore)]
        public string ManufacturerCode { get; set; }

        [JsonProperty("firmwareVersion", NullValueHandling = NullValueHandling.Ignore)]
        public string FirmwareVersion { get; set; }

        [JsonProperty("lastFirmwareUpdate", NullValueHandling = NullValueHandling.Ignore)]
        public string LastFirmwareUpdate { get; set; }

        #endregion

        #region Business oriented

        [JsonProperty("environment", NullValueHandling = NullValueHandling.Ignore)]
        public string Environment { get; set; }

        [JsonProperty("status", NullValueHandling = NullValueHandling.Ignore)]
        public string Status { get; set; }

        [JsonProperty("location", NullValueHandling = NullValueHandling.Ignore)]
        public Location Location { get; set; }

        [JsonProperty("roomId", NullValueHandling = NullValueHandling.Ignore)]
        public string RoomId { get; set; }


        [JsonProperty("latitude", NullValueHandling = NullValueHandling.Ignore)]
        public double Latitude { get; set; }


        [JsonProperty("longitude", NullValueHandling = NullValueHandling.Ignore)]
        public double Longitude { get; set; }

        #endregion
    }
}