using Grpc.Net.Client;
using Performance;
using Monitor = Performance.Monitor;

namespace ApiGateway
{
    public interface IGrpcPerformanceClient
    {
        Task<ResponseModel.PerformanceStatusModel> GetPerformanceStatus(string clientName);
    }

    internal class GrpcPerformanceClient(string serverUrl) : IGrpcPerformanceClient, IDisposable
    {
        private readonly GrpcChannel _channel = GrpcChannel.ForAddress(serverUrl);

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            _channel.Dispose();
        }

        public async Task<ResponseModel.PerformanceStatusModel> GetPerformanceStatus(string clientName)
        {
            var client = new Monitor.MonitorClient(_channel);

            var response = await client.GetPerformanceAsync(new PerformanceStatusRequest
            {
                ClientName = clientName
            });

            return new ResponseModel.PerformanceStatusModel
            {
                CpuPercentageUsage = response.CpuPercentageUsage,
                MemoryUsage = response.MemoryUsage,
                ProcessesRunning = response.ProcessesRunning,
                ActiveConnections = response.ActiveConnections
            };
        }
    }
}