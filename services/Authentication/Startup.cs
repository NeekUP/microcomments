using Authentication.Domain;
using Authentication.Events;
using Authentication.Infrastructure;
using Authentication.Infrastructure.DataAccess;
using Authentication.Infrastructure.Messaging.RabbitMQ;
using Authentication.Usecases;
using MassTransit;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Linq;

namespace Authentication
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            var connString = Configuration.GetConnectionString( "UserManagment" );
            ConfigureOptions( services );

            // Database
            services.AddDbContext<UserManagementDBContext>( options => options.UseNpgsql( connString ) );
            services.AddTransient<IUnitOfWork, UnitOfWork>();

            // Infrastructure
            services.AddControllers().AddNewtonsoftJson();
            services.AddScoped<ITokenProvider, JwtTokenProvider>();
            services.AddScoped<ITokenService, TokenService>();
            services.AddTransient<IDnsLookup, DnsLookup>();
            services.AddSingleton<IHashProvider, HashProvider>();

            // Usecases
            services.AddTransient<IRegisterHandler, Register>();
            services.AddTransient<ILoginHandler, Login>();
            services.AddTransient<IRefreshTokenHandler, Refresh>();
            services.AddTransient<IConfirmationHandler, ConfirmEmail>();

            // Messaging
            var rabbitMQOptions = new RabbitMQOptions();
            Configuration.GetSection( "Messaging:RabbitMQ" ).Bind( rabbitMQOptions );
            services.AddMassTransit( x =>
            {
                x.SetKebabCaseEndpointNameFormatter();
                x.UsingRabbitMq( ( context, cfg ) =>
                {
                    cfg.ConfigureEndpoints( context );

                    cfg.Host( rabbitMQOptions.Host, rabbitMQOptions.VirtualHost, h =>
                    {
                        h.Username( rabbitMQOptions.User);
                        h.Password( rabbitMQOptions.Password );
                    } );

                    cfg.Publish<UserRegistered>( x =>
                    {
                        x.Durable = true;
                        x.AutoDelete = false; 
                        x.ExchangeType = RabbitMQExchangeType.Fanout;
                    } );

                    cfg.MessageTopology.SetEntityNameFormatter( new EnvironmentNameFormatter( cfg.MessageTopology.EntityNameFormatter ) );
                } );
            } );

            services.AddMassTransitHostedService();

            // Swagger
            services.AddSwaggerGen( c =>
            {
                c.SwaggerDoc( "v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "Users API",
                    Description = "TODO",
                    Contact = new OpenApiContact
                    {
                        Name = "Nikita Popovsky",
                        Email = "nikita@popovsky.pro",
                        Url = new Uri( "http://nikita.popovsky.pro" ),
                    },
                    License = new OpenApiLicense
                    {
                        Name = "Use under MIT",
                        Url = new Uri( "https://opensource.org/licenses/MIT" ),
                    }
                } );

                c.OperationFilter<RemoveVersionFromParameter>();
                c.DocumentFilter<ReplaceVersionWithExactValueInPath>();
            } );

            services.AddApiVersioning( o =>
            {
                o.AssumeDefaultVersionWhenUnspecified = true;
                o.DefaultApiVersion = new ApiVersion( 1, 0 );
            } );
        }

        private void ConfigureOptions( IServiceCollection services )
        {
            services.Configure<TokenServiceOptions>( Configuration.GetSection( "TokenService" ) );
            services.Configure<JwtTokenProviderOptions>( Configuration.GetSection( "JwtTokenProvider" ) );
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();
            app.UseSwagger();
            app.UseSwaggerUI( c =>
            {
                c.SwaggerEndpoint( "/swagger/v1/swagger.json", "v1" );
            } );

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }

    public class RemoveVersionFromParameter : IOperationFilter
    {
        public void Apply( OpenApiOperation operation, OperationFilterContext context )
        {
            var versionParameter = operation.Parameters.Single( p => p.Name == "version" );
            operation.Parameters.Remove( versionParameter );
        }
    }

    public class ReplaceVersionWithExactValueInPath : IDocumentFilter
    {
        public void Apply( OpenApiDocument swaggerDoc, DocumentFilterContext context )
        {
            var paths = new OpenApiPaths();
            foreach ( var path in swaggerDoc.Paths )
            {
                paths.Add( path.Key.Replace( "v{version}", swaggerDoc.Info.Version ), path.Value );
            }
            swaggerDoc.Paths = paths;
        }
    }
}
