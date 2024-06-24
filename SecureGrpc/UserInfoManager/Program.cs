using Microsoft.AspNetCore.Server.Kestrel.Core;
using UserInfoManager;
using UserInfoManager.Services;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.WebHost.ConfigureKestrel(options =>
        {
            options.ListenLocalhost(5002, o => o.Protocols = HttpProtocols.Http1);
            options.ListenLocalhost(5000, o => o.Protocols = HttpProtocols.Http2);
        });

        // Add services to the container.
        builder.Services.AddGrpc();
        builder.Services.AddControllers();
        builder.Services.AddSingleton<UserDataCache>();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        app.MapGrpcService<GreeterService>();
        app.MapGrpcService<UserInfoService>();

        app.MapControllers();

        app.Run();
    }
}