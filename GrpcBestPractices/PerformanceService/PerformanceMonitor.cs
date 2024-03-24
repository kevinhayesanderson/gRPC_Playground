using Grpc.Core;
using Performance;
using Monitor = Performance.Monitor;

namespace PerformanceService
{
    public class PerformanceMonitor : Monitor.MonitorBase
    {
        public override Task<PerformanceStatusResponse> GetPerformance(
            PerformanceStatusRequest request,
            ServerCallContext context)
        {
            var random = new Random();

            return Task.FromResult(new PerformanceStatusResponse
            {
                CpuPercentageUsage = random.NextDouble() * 100,
                MemoryUsage = random.NextDouble() * 100,
                ProcessesRunning = random.Next(),
                ActiveConnections = random.Next()
            });
        }

        public override async Task GetManyPerformanceStats(
            IAsyncStreamReader<PerformanceStatusRequest> requestStream,
            IServerStreamWriter<PerformanceStatusResponse> responseStream,
            ServerCallContext context)
        {
            while (await requestStream.MoveNext())
            {
                var random = new Random();
                await responseStream.WriteAsync(new PerformanceStatusResponse
                {
                    CpuPercentageUsage = random.NextDouble() * 100,
                    MemoryUsage = random.NextDouble() * 100,
                    ProcessesRunning = random.Next(),
                    ActiveConnections = random.Next()

                });
            }
        }
    }
}