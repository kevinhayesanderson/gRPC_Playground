using Grpc.Core;
using Stats.V1;

namespace GrpcServer
{
    public class StatusServiceV1 : Stats.V1.Status.StatusBase
    {
        public override Task<StatusResponse> GetStatus(StatusRequest request, ServerCallContext context)
        {
            Console.WriteLine($"Client name is {request.ClientName}");
            Console.WriteLine($"Client description is {request.ClientDescription}");
            Console.WriteLine($"Is client ready? {request.Ready}");
            Console.WriteLine($"Is client authorized? {request.Authorized}");

            var random = new Random();
            return Task.FromResult(new StatusResponse
            {
                ServerName = "TestServer",
                ServerDescription = "This is a test server that is used for generating status metrics",
                NumberOfConnections = random.Next(),
                CpuUsage = random.NextDouble() * 100,
                MemoryUsage = random.NextDouble() * 100,
                ErrorsLogged = (ulong)random.Next(),
                CatastrophicFailuresLogged = (uint)random.Next(),
                Active = true
            });
        }
    }
}