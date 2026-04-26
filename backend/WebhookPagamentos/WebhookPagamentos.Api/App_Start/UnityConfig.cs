using System;
using System.Threading.Channels;
using System.Web.Http;
using Unity;
using Unity.AspNet.WebApi;
using Unity.Injection;
using Unity.Lifetime;
using WebhookPagamentos.Core.Interfaces;
using WebhookPagamentos.Core.Models;
using WebhookPagamentos.Infrastructure.Repositories;

namespace WebhookPagamentos.Api
{
    /// <summary>
    /// Specifies the Unity configuration for the main container.
    /// </summary>
    public static class UnityConfig
    {
        #region Unity Container
        private static Lazy<IUnityContainer> container =
          new Lazy<IUnityContainer>(() =>
          {
              var container = new UnityContainer();
              RegisterTypes(container);
              return container;
          });

        /// <summary>
        /// Configured Unity Container.
        /// </summary>
        public static IUnityContainer Container => container.Value;
        #endregion

        /// <summary>
        /// Registers the type mappings with the Unity container.
        /// </summary>
        public static void RegisterTypes(IUnityContainer container)
        {
            // 1. Canal de comunicação
            var canal = Channel.CreateUnbounded<PagamentoPayload>();
            container.RegisterInstance(canal, new ContainerControlledLifetimeManager());

            // 2. Lê a string de conexão direto do Web.config
            var connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

            // 3. Registra o Repositório avisando para ele usar essa string de conexão no construtor!
            container.RegisterType<IPagamentoRepository, PagamentoRepository>(
                new HierarchicalLifetimeManager(),
                new InjectionConstructor(connectionString)
            );
        }
    }
}