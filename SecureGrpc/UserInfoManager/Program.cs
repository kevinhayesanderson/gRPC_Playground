using Microsoft.AspNetCore.Server.Kestrel.Core;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using UserInfoManager;
using UserInfoManager.Services;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.WebHost.ConfigureKestrel(options =>
        {
            //we need to tell our application to use a specific .pfx file as the HTTPS certificate.
            options.ConfigureHttpsDefaults(o =>
            {
                o.ServerCertificate = new X509Certificate2("UserInfoManager.pfx", "password");
            });

            options.ListenLocalhost(5002, o => o.Protocols = HttpProtocols.Http1);
            options.ListenLocalhost(5000, o => o.Protocols = HttpProtocols.Http2);

            //for redirect requests, we should listen
            options.ListenAnyIP(5001, o => o.UseHttps());
        });

        //tell the application which port to redirect the http request to.
        builder.Services.AddHttpsRedirection(options =>
        {
            options.RedirectStatusCode = (int)HttpStatusCode.PermanentRedirect;
            options.HttpsPort = 5001;
        });

        // Add services to the container.
        builder.Services.AddGrpc();
        builder.Services.AddControllers();
        builder.Services.AddSingleton<UserDataCache>();

        var app = builder.Build();

        //This statement will ensure that whenever a call is made from a client to an
        //unencrypted HTTP endpoint, it will be redirected to an encrypted HTTPS port.
        app.UseHttpsRedirection();

        // Configure the HTTP request pipeline.
        app.MapGrpcService<GreeterService>();
        app.MapGrpcService<UserInfoService>();

        app.MapControllers();

        app.Run();
    }
}