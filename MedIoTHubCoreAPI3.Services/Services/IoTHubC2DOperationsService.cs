using System;
using System.Threading.Tasks;
using AutoMapper;
using MedIoTHubCoreAPI3.API.Common.Settings;
using MedIoTHubCoreAPI3.Services.Contracts;
using MedIoTHubCoreAPI3.Services.Model.IoT;
using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Shared;
using Microsoft.Extensions.Options;
using S = MedIoTHubCoreAPI3.Services.Model;

namespace MedIoTHubCoreAPI3.Services
{
    public class IoTHubC2DOperationsService : IIoTHubC2DOperationsService
    {
        private readonly IMapper _mapper;
        private readonly RegistryManager _registryManager;
        private readonly ServiceClient _serviceClient;
        private readonly AppSettings _settings;

        public IoTHubC2DOperationsService(IOptions<AppSettings> settings, IMapper mapper)
        {
            _settings = settings?.Value;
            _mapper = mapper;

            if (_settings != null && _settings.IsValid())
            {
                _registryManager = RegistryManager.CreateFromConnectionString(_settings.IoTHub.ConnectionString);
                _serviceClient = ServiceClient.CreateFromConnectionString(_settings.IoTHub.ConnectionString);
            }
            else
            {
                throw new Exception("AppSettings need to be reviewed");
            }
        }

        #region Direct methods

        public async Task<string> InvokeDirectMethodAsync(string deviceId, string methodName, string payload)
        {
            try
            {
                if (_settings.IoTHub?.DirectMethodTimeOut != null)
                {
                    var methodInvocation = new CloudToDeviceMethod(methodName)
                        {ResponseTimeout = TimeSpan.FromSeconds((double) _settings.IoTHub?.DirectMethodTimeOut)};
                    methodInvocation.SetPayloadJson(payload);

                    // Invoke the direct method asynchronously and get the response from the simulated device.
                    var response = await _serviceClient.InvokeDeviceMethodAsync(deviceId, methodInvocation).ConfigureAwait(false);

                    if (response != null)
                        return response.GetPayloadAsJson();
                }

                return null;
            }
            catch (Exception ex)
            {
                //TODO: add trace
                return null;
            }
        }

        #endregion

        ~IoTHubC2DOperationsService()
        {
            _registryManager?.CloseAsync();

            _serviceClient?.CloseAsync();
        }

        #region Twin

        public async Task<Twins> UpdateTwinsAsync(string deviceId, string patch)
        {
            var twin = await _registryManager.GetTwinAsync(deviceId).ConfigureAwait(false);
            if (twin != null)
                return await UpdateTwinsAsync(deviceId, twin.ETag, patch).ConfigureAwait(false);
            return null;
        }

        public async Task<Twins> UpdateTwinsAsync(string deviceId, string etag, string patch)
        {
            var data = await _registryManager.UpdateTwinAsync(deviceId, patch, etag).ConfigureAwait(false);

            if (data != null)
                return _mapper.Map<Twins>(data);
            return null;
        }

        public async Task UpdateTwinsAsync(TwinsSearchRequest request, string patch)
        {
            var query = _registryManager.CreateQuery($"SELECT * FROM devices {request.WhereCondition}");

            if (query != null)
            {
                var twinsInScope = await query.GetNextAsTwinAsync().ConfigureAwait(false);

                if (twinsInScope != null)
                    foreach (var item in twinsInScope)
#pragma warning disable 4014
                        UpdateTwinsAsync(item.DeviceId, item.ETag, patch);
#pragma warning restore 4014
            }
        }

        public async Task<Twins> GetTwinsAsync(string deviceId)
        {
            var twin = await _registryManager.GetTwinAsync(deviceId).ConfigureAwait(false);

            if (twin != null)
                return _mapper.Map<Twins>(twin);
            return null;
        }

        #endregion

        #region Jobs

        public async Task RunTwinUpdateJobAsync(string jobId, string queryCondition, Twins twin, DateTime startTime,
            long timeOut)
        {
            if (string.IsNullOrEmpty(jobId))
                throw new ArgumentNullException("jobId");

            if (string.IsNullOrEmpty(queryCondition))
                throw new ArgumentNullException("queryCondition");

            if (twin == null)
                throw new ArgumentNullException("twin");

            var iotHubTwin = _mapper.Map<Twin>(twin);

            if (iotHubTwin == null)
                throw new Exception("Twin mapping error.");


            using (var jobClient = JobClient.CreateFromConnectionString(_settings.IoTHub.ConnectionString))
            {
                if (jobClient != null)
                {
                    var createJobResponse = await jobClient.ScheduleTwinUpdateAsync(
                        jobId,
                        queryCondition,
                        iotHubTwin,
                        startTime,
                        timeOut);
                }
            }
        }

        public async Task RunTwinUpdateJobAsync(string jobId, string queryCondition, Twins twin)
        {
            await RunTwinUpdateJobAsync(jobId, queryCondition, twin, DateTime.UtcNow,
                (long) TimeSpan.FromMinutes(2).TotalSeconds);
        }

        public async Task<JobResponse> MonitorJobWithDetailsAsync(string jobId)
        {
            if (string.IsNullOrEmpty(jobId))
                throw new ArgumentNullException("jobId");

            using (var jobClient = JobClient.CreateFromConnectionString(_settings.IoTHub.ConnectionString))
            {
                if (jobClient != null)
                    return await jobClient.GetJobAsync(jobId);
                return null;
            }
        }

        #endregion
    }
}