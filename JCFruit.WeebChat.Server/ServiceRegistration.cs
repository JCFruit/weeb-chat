using Autofac;
using JCFruit.WeebChat.Server.Services;
using JCFruit.WeebChat.Server.Tcp;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Net;

namespace JCFruit.WeebChat.Server
{
    public static class ServiceRegistration
    {
        public static void Register(HostBuilderContext host, IServiceCollection services)
        {
            var configuration = host.Configuration;

            var tcpOptions = configuration.GetSection("Server");
            services.AddSingleton(new TcpOptions
            {
                IPAddress = IPAddress.Parse(tcpOptions.GetValue<string>("IPAddress")),
                Port = tcpOptions.GetValue<int>("Port")
            });

            services.AddSingleton<IConnectionHandler, MediatedConnectionHandler>();
            services.AddSingleton<TcpServer>();
            services.AddHostedService<ServerHost>();

            services.AddMediatR(new[] { typeof(ServiceRegistration) }, options => options.AsSingleton());

            services.AddSingleton<MessageSerializer>();
            services.AddSingleton<UserStorage>();
        }

        public static void ConfigureContainer(HostBuilderContext host, ContainerBuilder container)
        {
            container.RegisterDecorator<FragmentationHandler, IConnectionHandler>();
        }
    }
}
