using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebhookPagamentos.Core.Models;

namespace WebhookPagamentos.Core.Interfaces
{
    public interface IPagamentoRepository
    {
        Task<bool> SalvarEventoBrutoAsync(PagamentoPayload payload);
        Task AtualizarStatusContratoAsync(PagamentoPayload payload);
        Task<System.Collections.Generic.IEnumerable<dynamic>> ListarEventosAsync();
    }
}
