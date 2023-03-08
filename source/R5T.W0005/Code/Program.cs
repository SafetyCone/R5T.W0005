using System;

using Microsoft.AspNetCore.Connections;


namespace R5T.W0005
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Configure ports.
            builder.WebHost.UseUrls("http://localhost:5002", "https://localhost:5003");

            var app = builder.Build();

            app.MapGet("/", () => "Hello World!");

            app.Run();
        }
    }
}