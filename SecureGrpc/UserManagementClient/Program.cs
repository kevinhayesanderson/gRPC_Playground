﻿using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Net.Client;
using System.Security.Cryptography.X509Certificates;
using Users;

Console.WriteLine("Hello, World!");

Console.WriteLine("Please enter the gRPC service URL.");
var url = Console.ReadLine();

//var handler = new HttpClientHandler
//{
//    ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
//};

var certificate = new X509Certificate2("UserManagementClient.pfx", "password");

var handler = new HttpClientHandler();
handler.ClientCertificates.Add(certificate);

using var channel = GrpcChannel.ForAddress(url, new GrpcChannelOptions { HttpHandler = handler });

var client = new UserManager.UserManagerClient(channel);

using var call = client.GetAllUsers(new Empty());

while (await call.ResponseStream.MoveNext())
{
    var user = call.ResponseStream.Current;
    Console.WriteLine("User details extracted");
    Console.WriteLine($"First name: {user.FirstName}");
    Console.WriteLine($"Surname: {user.Surname}");
    Console.WriteLine($"Gender: {user.Gender}");
    Console.WriteLine($"Date of birth: {user.DateOfBirth.ToDateTime():yyyy - MM - dd} ");
    Console.WriteLine($"Nationality: {user.Nationality}");
    Console.WriteLine($"Address: {user.Address.FirstLine}");
    Console.WriteLine($"Postcode or Zip code: {user.Address.PostcodeOrZipCode}");
    Console.WriteLine($"Town: {user.Address.Town}");
    Console.WriteLine($"Country: {user.Address.Country}");
    Console.WriteLine(string.Empty);
}

Console.ReadKey();