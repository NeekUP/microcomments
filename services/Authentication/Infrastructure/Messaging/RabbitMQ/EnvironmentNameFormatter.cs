using MassTransit.Topology;
using System;

namespace Authentication.Infrastructure.Messaging.RabbitMQ
{
    public class EnvironmentNameFormatter : IEntityNameFormatter
    {
        private IEntityNameFormatter _original;
        public EnvironmentNameFormatter( IEntityNameFormatter original )
        {
            _original = original;
        }

        public string FormatEntityName<T>()
        {
            var environment = Environment.GetEnvironmentVariable( "ASPNETCORE_ENVIRONMENT" ) ?? "Development";
            return $"{environment}:{_original.FormatEntityName<T>()}";
        }
    }
}
