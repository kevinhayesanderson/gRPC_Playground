using BasicGrpcService.Services;
using Microsoft.AspNetCore.Server.Kestrel.Core;

var builder = WebApplication.CreateBuilder(args);

//builder.WebHost.ConfigureKestrel(serverOptions =>
//{
//    // Setup a HTTP/2 endpoint without TLS.
//    serverOptions.ListenLocalhost(5000, o => o.Protocols = HttpProtocols.Http2);
//    //your application will be ready to accept insecure HTTP/2 requests on 5000.
//});

builder.Services.AddGrpc();

// Add services to the container.
builder.Services.AddRazorPages();

var app = builder.Build();

app.MapGrpcService<GreetingsManagerService>();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();
