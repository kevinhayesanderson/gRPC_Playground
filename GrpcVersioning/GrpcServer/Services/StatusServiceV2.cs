﻿using Grpc.Core;
using Stats.V2;

namespace GrpcServer
{
    public class StatusServiceV2 : Stats.V2.Status.StatusBase
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
                NumberOfConnections = random.Next(),
                CpuUsage = random.NextDouble() * 100,
                MemoryUsage = random.NextDouble() * 100,
                CatastrophicFailuresLogged = (uint)random.Next(),
            });
        }
    }
}