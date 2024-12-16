using CertificateManager;
using CertificateManager.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace settl.identityserver.API
{
    public class Program
    {
        private static CreateCertificates _cc;

        public static void Main(string[] args)
        {
            var sp = new ServiceCollection()
               .AddCertificateManager().BuildServiceProvider();

            _cc = sp.GetService<CreateCertificates>();

            var rsaCert = CreateRsaCertificate("localhost", 10);

            string password = "3*J6S=$GzN";
            var iec = sp.GetService<ImportExportCertificate>();

            var rsaCertPfxBytes = iec.ExportSelfSignedCertificatePfx(password, rsaCert);
            File.WriteAllBytes("settlsecurecertificate.pfx", rsaCertPfxBytes);

            Log.Logger = new LoggerConfiguration().Enrich.FromLogContext()
              .WriteTo.File(
              path: "Logs\\log-.txt",
              outputTemplate: "{Timestamp: yyyy-MM-dd HH:mm:ss.fff} Identity Server Microservice [{Level:u3}] {Message: lj}{NewLine}{Exception}",
              rollingInterval: RollingInterval.Day,
              restrictedToMinimumLevel: LogEventLevel.Information
              ).WriteTo.Console().CreateLogger();

            try
            {
                Log.Information("Application Is Starting");
                CreateHostBuilder(args).Build().Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Application failed to start");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
            .UseSerilog()
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    config.AddEnvironmentVariables();
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });//.UseAllElasticApm();

        public static X509Certificate2 CreateRsaCertificate(string dnsName, int validityPeriodInYears)
        {
            var basicConstraints = new BasicConstraints
            {
                CertificateAuthority = false,
                HasPathLengthConstraint = false,
                PathLengthConstraint = 0,
                Critical = false
            };

            var subjectAlternativeName = new SubjectAlternativeName
            {
                DnsName = new List<string>
                {
                    dnsName,
                }
            };

            var x509KeyUsageFlags = X509KeyUsageFlags.DigitalSignature;

            // only if certification authentication is used
            var enhancedKeyUsages = new OidCollection
            {
                OidLookup.ClientAuthentication,
                OidLookup.ServerAuthentication
            };

            var certificate = _cc.NewRsaSelfSignedCertificate(
                new DistinguishedName { CommonName = dnsName },
                basicConstraints,
                new ValidityPeriod
                {
                    ValidFrom = DateTimeOffset.UtcNow,
                    ValidTo = DateTimeOffset.UtcNow.AddYears(validityPeriodInYears)
                },
                subjectAlternativeName,
                enhancedKeyUsages,
                x509KeyUsageFlags,
                new RsaConfiguration
                {
                    KeySize = 2048,
                    HashAlgorithmName = HashAlgorithmName.SHA512
                });

            return certificate;
        }
    }
}