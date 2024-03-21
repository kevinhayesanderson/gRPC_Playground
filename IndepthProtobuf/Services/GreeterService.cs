using Grpc.Core;

namespace IndepthProtobuf.Services;

public class GreeterService(ILogger<GreeterService> logger) : Greeter.GreeterBase
{
    public override Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
    {
        var message = new HelloReply
        {
            Message = "Hello" + request.Name,
            NestedMessageField = new HelloReply.Types.NestedMessage()
        };

        message.NestedMessageField.StringCollection.Add("entry 1");
        message.NestedMessageField.StringCollection.Add(new List<string>
        {
            "entry 2",
            "entry 3"
        });

        message.NestedMessageField.StringToStringMap.Add("entry 1", "value 1");
        message.NestedMessageField.StringToStringMap.Add(new Dictionary<string, string>
        {
            { "entry 2", "value 2" },
            { "entry 3", "value 3"}
        });
        message.NestedMessageField.StringToStringMap["entry 4"] = "value 4";

        message.BasicTypesField = new BasicTypes
        {
            IntField = 1,
            LongField = 2
        };

        return Task.FromResult(message);
    }
}