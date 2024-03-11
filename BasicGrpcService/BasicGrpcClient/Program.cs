// See https://aka.ms/new-console-template for more information

using Grpc.Net.Client;
using BasicGrpcService;

using var channel = GrpcChannel.ForAddress("https://localhost:5001"); // The port number(5001) must match the port of the gRPC server.
var client = new GreetingsManager.GreetingsManagerClient(channel);
var reply = await client.GenerateGreetingAsync(new GreetingRequest { Name = "BasicGrpcClient" });
Console.WriteLine("Greeting: " + reply.GreetingMessage);

Console.WriteLine("Press any key to exit...");
Console.ReadKey();