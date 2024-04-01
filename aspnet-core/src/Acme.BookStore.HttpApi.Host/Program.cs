using System;
using System.Threading.Tasks;
using Amazon;
using Amazon.Extensions.Configuration.SystemsManager;
using Amazon.SimpleSystemsManagement.Model;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;

namespace Acme.BookStore;

public class Program
{
    public async static Task<int> Main(string[] args)
    {
        Log.Debug("Starting Acme.BookStore.HttpApi.Host. Program.Main.");

        try
        {
            var builder = WebApplication.CreateBuilder(args);

            Log.Debug("WebApplication.CreateBuilder done.");

            builder.Configuration.AddSystemsManager(configureSource => {
                configureSource.Path = "/acme.bookstore/";
                configureSource.ReloadAfter = TimeSpan.FromSeconds(20);
            });

            Log.Debug("builder.Configuration.AddSystemsManager done.");

            Log.Debug("Configuration: {Configuration}", builder.Configuration.AsEnumerable());

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(builder.Configuration)
                .Enrich.FromLogContext()
                .WriteTo.Async(c => c.File("Logs/logs.txt"))
                .WriteTo.Async(c => c.Console())
                .CreateLogger();

            Log.Debug("Logger configured.");

            Log.Debug("Starting Acme.BookStore.HttpApi.Host.");

            builder.Host.AddAppSettingsSecretsJson()
                .UseAutofac()
                .UseSerilog();

            Log.Debug("Host configured.");

            await builder.AddApplicationAsync<BookStoreHttpApiHostModule>();

            Log.Debug("Application added.");

            var app = builder.Build();

            Log.Debug("WebApplication built.");

            await app.InitializeApplicationAsync();

            Log.Debug("Application initialized.");

            await app.RunAsync();

            Log.Debug("Application started.");

            return 0;
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Host terminated unexpectedly!");

            if (ex is HostAbortedException)
            {
                throw;
            }

            return 1;
        }
        finally
        {
            Log.CloseAndFlush();

            Log.Debug("Logger closed.");
        }
    }
}
