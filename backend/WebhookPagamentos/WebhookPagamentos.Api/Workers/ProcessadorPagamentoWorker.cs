using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;
using Unity;
using WebhookPagamentos.Core.Interfaces;
using WebhookPagamentos.Core.Models;

namespace WebhookPagamentos.Api.Workers
{
    public static class ProcessadorPagamentoWorker
    {
        public static void IniciarLoopDeProcessamento()
        {
            // Avisa o IIS: "Tenho um trabalho em background. Me dê um Token para eu saber se você for desligar."
            HostingEnvironment.QueueBackgroundWorkItem(async token =>
            {
                // Pega a nossa fila (Channel) lá do Unity
                var canal = UnityConfig.Container.Resolve<Channel<PagamentoPayload>>();

                try
                {
                    // WaitToReadAsync fica aguardando silenciosamente até que um novo item chegue na fila
                    while (await canal.Reader.WaitToReadAsync(token))
                    {
                        while (canal.Reader.TryRead(out var payload))
                        {
                            // A cada item, pegamos uma instância nova do repositório
                            var repository = UnityConfig.Container.Resolve<IPagamentoRepository>();

                            // SIMULA O PROCESSAMENTO PESADO (Ex: validações complexas, chamadas a outros sistemas)
                            await Task.Delay(2000, token);

                            // Finaliza a transação salvando o Status do Contrato e marcando o Log como 'Concluido'
                            await repository.AtualizarStatusContratoAsync(payload);
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    // O IIS mandou a aplicação desligar/reiniciar. O token foi cancelado.
                    // A thread encerra graciosamente aqui sem quebrar nada.
                }
                catch (Exception ex)
                {
                    // Em um cenário real, aqui você usaria um Serilog/NLog para gravar o erro
                    Console.WriteLine($"Erro crítico no worker: {ex.Message}");
                }
            });
        }
    }
}