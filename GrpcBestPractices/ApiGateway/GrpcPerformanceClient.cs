using Grpc.Core;
using Grpc.Net.Client;
using Performance;
using Monitor = Performance.Monitor;

namespace ApiGateway
{
    public interface IGrpcPerformanceClient
    {
        Task<ResponseModel.PerformanceStatusModel> GetPerformanceStatus(string clientName);

        Task<IEnumerable<ResponseModel.PerformanceStatusModel>> GetPerformanceStatuses(IEnumerable<string> clientNames);
    }

    internal class GrpcPerformanceClient(string serverUrl) : IGrpcPerformanceClient, IDisposable
    {
        private readonly GrpcChannel _channel = GrpcChannel.ForAddress(serverUrl, new GrpcChannelOptions
        {
            HttpHandler = new SocketsHttpHandler()
            {
                //Setting up keep-alive pings
                PooledConnectionIdleTimeout = Timeout.InfiniteTimeSpan,
                KeepAlivePingDelay = TimeSpan.FromSeconds(60),
                KeepAlivePingTimeout = TimeSpan.FromSeconds(30),
                //for multiple connections for concurrent calls
                EnableMultipleHttp2Connections = true,
            }
        });

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

        public async Task<IEnumerable<ResponseModel.PerformanceStatusModel>> GetPerformanceStatuses(IEnumerable<string> clientNames)
        {
            var client = new Monitor.MonitorClient(_channel);

            using var call = client.GetManyPerformanceStats();

            var responses = new List<ResponseModel.PerformanceStatusModel>();

            var readTask = Task.Run(async () =>
            {
                await foreach (var response in call.ResponseStream.ReadAllAsync())
                {
                    responses.Add(new ResponseModel.PerformanceStatusModel
                    {
                        CpuPercentageUsage = response.CpuPercentageUsage,
                        MemoryUsage = response.MemoryUsage,
                        ProcessesRunning = response.ProcessesRunning,
                        ActiveConnections = response.ActiveConnections
                    });
                }
            });

            foreach (var clientName in clientNames)
            {
                await call.RequestStream.WriteAsync(new PerformanceStatusRequest
                {
                    ClientName = clientName
                });
            }

            await call.RequestStream.CompleteAsync();

            await readTask;

            return responses;
        }
    }
}