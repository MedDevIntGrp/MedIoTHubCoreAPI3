using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using AutoMapper;
using MedIoTHubCoreAPI3.API.Common.Settings;
using MedIoTHubCoreAPI3.Services.Contracts;
using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Common.Exceptions;
using Microsoft.Azure.Devices.Shared;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Device = Microsoft.Azure.Devices.Device;
using SIoT = MedIoTHubCoreAPI3.Services.Model.IoT;

namespace MedIoTHubCoreAPI3.Services.Services
{
    public class DeviceManagementService : IDeviceManagementService
    {
        private readonly IMapper _mapper;
        private readonly ILogger<DeviceManagementService> _logger;
        private readonly RegistryManager _registryManager;

        public DeviceManagementService(IOptions<AppSettings> settings, IMapper mapper,
            ILogger<DeviceManagementService> logger)
        {
            var settings1 = settings?.Value;

            if (settings1 != null && settings1.IsValid())
                _registryManager = RegistryManager.CreateFromConnectionString(settings1.IoTHub?.ConnectionString);
            else
                throw new Exception("AppSettings need to be reviewed");

            _mapper = mapper;
            _logger = logger;
        }

        ~DeviceManagementService()
        {
            _registryManager?.CloseAsync();
        }

        #region Public methods

        public async Task<SIoT.Device> GetDeviceAsync(string deviceId)
        {
            var data = await GetIoTHubDeviceAsync(deviceId).ConfigureAwait(false);

            return data != null ? _mapper.Map<SIoT.Device>(data) : null;
        }

        public async Task<SIoT.Device> AddDeviceAsync(string deviceId)
        {
            var iotDevice = await AddDeviceToIoTHubAsync(deviceId).ConfigureAwait(false);

            if (iotDevice != null)
                return _mapper.Map<SIoT.Device>(iotDevice);
            return null;
        }

        public async Task<SIoT.Device> AddDeviceAsync(string deviceId, SIoT.DeviceIoTSettings settings)
        {
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));

            var device = await AddDeviceToIoTHubAsync(deviceId).ConfigureAwait(false);

            var twinUpdated = await UpdateDeviceSettingsAsync(deviceId, settings).ConfigureAwait(false);

            if (device != null && twinUpdated)
                return _mapper.Map<SIoT.Device>(device);
            throw new Exception("An error has occurred during the device creation or the twin updates.");
        }

        public async Task<SIoT.Device> AddDeviceWithTagsAsync(string deviceId, string jsonTwin)
        {
            var twin = new Twin(deviceId);
            twin.Tags = new TwinCollection(jsonTwin);

            var result = await _registryManager.AddDeviceWithTwinAsync(new Device(deviceId), twin).ConfigureAwait(false);

            if (result != null && result.IsSuccessful)
                return await GetDeviceAsync(deviceId).ConfigureAwait(false);
            throw new Exception("An error has occurred during the device creation or the twin updates.");
        }


        /// <summary>
        ///     Add a device with strongly typed settings (could be usefull in some scenarios)
        /// </summary>
        /// <param name="deviceId"></param>
        /// <param name="settings"></param>
        /// <returns></returns>
        public async Task<SIoT.Device> AddDeviceWithTagsAsync(string deviceId, SIoT.DeviceIoTSettings settings)
        {
            var converter = new TwinJsonConverter();
            Twin twin = null;

            if (settings != null && settings.Twins != null)
            {
                twin = new Twin(deviceId);

                var jsonTags = JsonConvert.SerializeObject(settings.Twins,
                    new JsonSerializerSettings {NullValueHandling = NullValueHandling.Ignore});
                twin.Tags = new TwinCollection(jsonTags);
            }

            var result = await _registryManager.AddDeviceWithTwinAsync(new Device(deviceId), twin).ConfigureAwait(false);

            if (result != null && result.IsSuccessful)
                return await GetDeviceAsync(deviceId).ConfigureAwait(false);
            throw new Exception("An error has occurred during the device creation or the twin updates.");
        }

        public async Task<SIoT.Device> AddDeviceWithTwinAsync(string deviceId, Twin twin)
        {
            var result = await _registryManager.AddDeviceWithTwinAsync(new Device(deviceId), twin).ConfigureAwait(false);

            if (result != null && result.IsSuccessful)
                return await GetDeviceAsync(deviceId).ConfigureAwait(false);
            throw new Exception("An error has occurred during the device creation or the twin updates.");
        }

        public async Task<string> GetPrimaryKeyOrThumbprintFromDeviceAsync(string deviceId)
        {
            if (string.IsNullOrEmpty(deviceId))
                throw new ArgumentNullException(nameof(deviceId));

            var result = string.Empty;

            var device = await _registryManager.GetDeviceAsync(deviceId).ConfigureAwait(false);

            if (device != null)
                switch (device.Authentication.Type)
                {
                    case AuthenticationType.Sas:
                        result = device.Authentication.SymmetricKey.PrimaryKey;
                        break;
                    case AuthenticationType.SelfSigned:
                        result = device.Authentication.X509Thumbprint.PrimaryThumbprint;
                        break;
                    case AuthenticationType.CertificateAuthority:
                        result = device.Authentication.X509Thumbprint.PrimaryThumbprint;
                        break;
                }
            else
                return string.Empty;

            return result;
        }

        public async Task<string> GetSecondaryKeyOrThumbprintFromDeviceAsync(string deviceId)
        {
            if (string.IsNullOrEmpty(deviceId))
                throw new ArgumentNullException(nameof(deviceId));

            var result = string.Empty;

            var device = await _registryManager.GetDeviceAsync(deviceId).ConfigureAwait(false);

            if (device != null)
                switch (device.Authentication.Type)
                {
                    case AuthenticationType.Sas:
                        result = device.Authentication.SymmetricKey.SecondaryKey;
                        break;
                    case AuthenticationType.SelfSigned:
                        result = device.Authentication.X509Thumbprint.SecondaryThumbprint;
                        break;
                    case AuthenticationType.CertificateAuthority:
                        result = device.Authentication.X509Thumbprint.SecondaryThumbprint;
                        break;
                }
            else
                return string.Empty;

            return result;
        }

        public async Task<bool> RemoveDeviceAsync(string deviceId)
        {
            try
            {
                await _registryManager.RemoveDeviceAsync(deviceId).ConfigureAwait(false);

                var device = await _registryManager.GetDeviceAsync(deviceId).ConfigureAwait(false);
                if (device != null)
                    return false;
                return true;
            }
            catch (DeviceNotFoundException ex)
            {
                _logger.LogWarning(ex, ex.Message);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return false;
            }
        }

        public async Task<SIoT.Device> DisableDeviceAsync(string deviceId)
        {
            var device = await _registryManager.GetDeviceAsync(deviceId).ConfigureAwait(false);

            if (device != null)
            {
                device.Status = DeviceStatus.Disabled;
                var data = await _registryManager.UpdateDeviceAsync(device).ConfigureAwait(false);
                if (data != null)
                    return _mapper.Map<SIoT.Device>(data);
                return null;
            }

            return null;
        }

        public async Task<SIoT.Device> EnableDeviceAsync(string deviceId)
        {
            var device = await _registryManager.GetDeviceAsync(deviceId).ConfigureAwait(false);

            if (device != null)
            {
                device.Status = DeviceStatus.Enabled;
                var data = await _registryManager.UpdateDeviceAsync(device).ConfigureAwait(false);
                if (data != null)
                    return _mapper.Map<SIoT.Device>(data);
                return null;
            }

            return null;
        }

        /// <summary>
        ///     Usefull for updating Twins properties (either tags or desired/reported properties).
        ///     This first version does not include desired/reported properties serialization/deserialization but it could be added
        ///     easily (additional properties in settings classes)
        /// </summary>
        /// <param name="deviceId"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public async Task<bool> UpdateDeviceSettingsAsync(string deviceId, SIoT.DeviceIoTSettings options)
        {
            if (string.IsNullOrEmpty(deviceId))
                throw new ArgumentNullException(nameof(deviceId));

            if (options == null)
                throw new ArgumentNullException(nameof(options));

            if (options.Twins != null && options.Twins.Tags != null)
                try
                {
                    var twin = await _registryManager.GetTwinAsync(deviceId).ConfigureAwait(false);

                    if (twin != null)
                    {
                        var jsonTags = JsonConvert.SerializeObject(options.Twins,
                            new JsonSerializerSettings {NullValueHandling = NullValueHandling.Ignore});
                        var updatedTwin = await _registryManager.UpdateTwinAsync(deviceId, jsonTags, twin.ETag).ConfigureAwait(false);

                        return updatedTwin != null;
                    }

                    return false;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, ex.Message);
                    throw;
                }

            return false;
        }


        #region Get devices

        //JSON Improrvements:https://devblogs.microsoft.com/dotnet/try-the-new-system-text-json-apis/
        public Task<IEnumerable<JsonDocument>> GetDevicesAsync(int maxCount = 100)
        {
            return GetDevicesAsync("select * from devices", maxCount);
        }

        public async Task<IEnumerable<JsonDocument>> GetDevicesAsync(string query, int maxCount = 100)
        {
            var iotQuery = _registryManager.CreateQuery(query, maxCount);
            var data = new List<string>();

            while (iotQuery.HasMoreResults) data.AddRange(await iotQuery.GetNextAsJsonAsync().ConfigureAwait(false));

            return data.Select(i => JsonDocument.Parse(i));
        }

        #endregion

        #endregion

        #region Private methods

        private Task<Device> GetIoTHubDeviceAsync(string deviceId)
        {
            if (string.IsNullOrEmpty(deviceId))
                throw new ArgumentNullException(nameof(deviceId));

            return _registryManager.GetDeviceAsync(deviceId);
        }

        private async Task<Device> AddDeviceToIoTHubAsync(string deviceId)
        {
            if (string.IsNullOrEmpty(deviceId))
                throw new ArgumentNullException(nameof(deviceId));

            Device device;
            try
            {
                device = await _registryManager.AddDeviceAsync(new Device(deviceId)).ConfigureAwait(false);
            }
            catch (DeviceAlreadyExistsException ex)
            {
                _logger.LogWarning(ex, ex.Message);
                device = await _registryManager.GetDeviceAsync(deviceId).ConfigureAwait(false);
            }

            return device;
        }

        #endregion
    }
}