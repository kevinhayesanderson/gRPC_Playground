using Google.Protobuf;
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
            return Task.FromResult(GetPerformaceResponse());
        }

        public override async Task GetManyPerformanceStats(
            IAsyncStreamReader<PerformanceStatusRequest> requestStream,
            IServerStreamWriter<PerformanceStatusResponse> responseStream,
            ServerCallContext context)
        {
            while (await requestStream.MoveNext())
            {
                await responseStream.WriteAsync(GetPerformaceResponse());
            }
        }

        private PerformanceStatusResponse GetPerformaceResponse()
        {
            var random = new Random();
            var dataLoad1 = new byte[100];
            var dataLoad2 = new byte[100];
            random.NextBytes(dataLoad1);
            random.NextBytes(dataLoad2);

            return new PerformanceStatusResponse
            {
                CpuPercentageUsage = random.NextDouble() * 100,
                MemoryUsage = random.NextDouble() * 100,
                ProcessesRunning = random.Next(),
                ActiveConnections = random.Next(),
                DataLoad1 = UnsafeByteOperations.UnsafeWrap(dataLoad1),
                DataLoad2 = ByteString.CopyFrom(dataLoad2)
            };
        }
    }
}