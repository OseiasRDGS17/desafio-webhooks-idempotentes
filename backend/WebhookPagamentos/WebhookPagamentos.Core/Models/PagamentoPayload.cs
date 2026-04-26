using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebhookPagamentos.Core.Models
{
    public class PagamentoPayload
    {
        public string IdTransacao { get; set; } = string.Empty;
        public string IdContrato { get; set; } = string.Empty;
        public decimal Valor { get; set; }
        public DateTime DataPagamento { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
