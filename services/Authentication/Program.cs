using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Web;
using System;

namespace Authentication
{
    public class Program
    {
        public static void Main( string[] args )
        {
            var logger = NLogBuilder.ConfigureNLog( "nlog.config" ).GetCurrentClassLogger();
            try
            {
                logger.Debug( "Start..." );
                CreateHostBuilder( args ).Build().Run();
            }
            catch ( Exception exception )
            {
                logger.Error( exception, "Stopped program because of exception" );
                throw;
            }
            finally
            {
                NLog.LogManager.Shutdown();
            }
        }

        public static IHostBuilder CreateHostBuilder( string[] args )
        {
            return Host.CreateDefaultBuilder( args )
                .ConfigureWebHostDefaults( webBuilder =>
                 {
                     webBuilder.UseStartup<Startup>();
                 } ).ConfigureLogging( logging =>
                 {
                     logging.ClearProviders();
                     logging.SetMinimumLevel( LogLevel.Trace );
                 } ).ConfigureAppConfiguration( ( hostContext, builder ) =>
                 {
                     if ( hostContext.HostingEnvironment.IsDevelopment() )
                     {
                         builder.AddUserSecrets<Program>();
                     }
                 } )
                .UseNLog();
        }
    }
}
