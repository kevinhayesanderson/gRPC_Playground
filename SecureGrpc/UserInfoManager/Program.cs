using Microsoft.AspNetCore.Authentication.Certificate;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using System.Net;
using System.Security.Claims;
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
                o.ClientCertificateMode = ClientCertificateMode.RequireCertificate;
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

        builder.Services.AddAuthentication(CertificateAuthenticationDefaults.AuthenticationScheme)
            .AddCertificate(options =>
            {
                options.AllowedCertificateTypes = CertificateTypes.All;
                options.Events = new CertificateAuthenticationEvents
                {
                    //triggered when the client certificate has passed validation
                    OnCertificateValidated = context =>
                    {
                        //data extracted from the certificate
                        // as a claim principle, which we will need for authentication
                        var claim = new[]
                        {
                            new Claim(ClaimTypes.Name,
                                      context.ClientCertificate.Subject,
                                      ClaimValueTypes.String,
                                      context.Options.ClaimsIssuer)
                        };

                        context.Principal = new ClaimsPrincipal(new ClaimsIdentity(claim, context.Scheme.Name));

                        Console.WriteLine($"Client certificate thumbprint {context.ClientCertificate.Thumbprint}");
                        Console.WriteLine($"Client certificate subject {context.ClientCertificate.Subject}");

                        context.Success();
                        return Task.CompletedTask;
                    },
                    OnAuthenticationFailed = context =>
                    {
                        context.NoResult();
                        context.Response.StatusCode = 403;
                        context.Response.ContentType = "text/plain";
                        context.Response.WriteAsync(context.Exception.ToString()).Wait();
                        return Task.CompletedTask;
                    }
                };
            })
            .AddCertificateCache();

        var app = builder.Build();

        //This statement will ensure that whenever a call is made from a client to an
        //unencrypted HTTP endpoint, it will be redirected to an encrypted HTTPS port.
        app.UseHttpsRedirection();

        app.UseAuthorization();

        // Configure the HTTP request pipeline.
        app.MapGrpcService<GreeterService>();
        app.MapGrpcService<UserInfoService>();

        app.MapControllers();

        app.Run();
    }
}