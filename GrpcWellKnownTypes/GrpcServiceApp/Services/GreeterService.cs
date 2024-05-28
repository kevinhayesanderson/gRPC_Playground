using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using System.Text;

namespace GrpcServiceApp.Services;

public class GreeterService : Greeter.GreeterBase
{
    private readonly ILogger<GreeterService> _logger;
    private readonly MessageCounter _messageCounter;

    public GreeterService(ILogger<GreeterService> logger, MessageCounter messageCounter)
    {
        _logger = logger;
        _messageCounter = messageCounter;
    }

    public override Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
    {
        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            var message = "Hello " + request.Name;
            var currentTime = DateTime.UtcNow;
            var timeToDeadline = context.Deadline - currentTime;
            var messageBytes = Encoding.ASCII.GetBytes(message);

            return Task.FromResult(new HelloReply
            {
                Message = "Hello " + request.Name,
                MessageProcessedCount = _messageCounter.IncrementCount(),
                MessageLengthInBytes = (ulong)messageBytes.Length,
                MessageLengthInLetters = message.Length,
                MillisecondsToDeadline = timeToDeadline.Milliseconds,
                SecondsToDeadline = (float)timeToDeadline.TotalSeconds,
                MinutesToDeadline = timeToDeadline.TotalMinutes,
                LastNamePresent = request.Name.Split(' ').Length > 1,
                MessageBytes = Google.Protobuf.ByteString.CopyFrom(messageBytes),
                ResponseTimeUtc = Timestamp.FromDateTime(currentTime),
                CallProcessingDuration = Timestamp.FromDateTime(currentTime) - request.RequestTimeUtc
            });
        }
        return Task.FromResult(new HelloReply());
    }
}