using System.Net;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Web.Http;
using WebhookPagamentos.Core.Interfaces;
using WebhookPagamentos.Core.Models;
using WebhookPagamentos.Api.Filters;

namespace WebhookPagamentos.Api.Controllers
{
    [RoutePrefix("api/webhooks")]
    public class PagamentoController : ApiController
    {
        private readonly IPagamentoRepository _repository;
        private readonly Channel<PagamentoPayload> _canal;

        // O Unity vai injetar o repositório e a fila aqui automaticamente
        public PagamentoController(IPagamentoRepository repository, Channel<PagamentoPayload> canal)
        {
            _repository = repository;
            _canal = canal;
        }

        [HttpPost]
        [Route("pagamento")]
        [ApiKeyAuth]
        public async Task<IHttpActionResult> ReceberWebhook([FromBody] PagamentoPayload payload)
        {
            // Validação simples
            if (payload == null || string.IsNullOrEmpty(payload.IdTransacao))
            {
                return BadRequest("Payload inválido ou IdTransacao ausente.");
            }

            // 1. Tenta salvar o evento bruto (Garante Idempotência no banco)
            var inseridoComSucesso = await _repository.SalvarEventoBrutoAsync(payload);

            if (!inseridoComSucesso)
            {
                // Se retornou false, já existe no banco (Duplicado). Retorna OK silencioso.
                return Ok(new { Mensagem = "Evento já recebido anteriormente." });
            }

            // 2. Joga para a fila em memória
            await _canal.Writer.WriteAsync(payload);

            // 3. Responde imediatamente 202 Accepted
            return Content(HttpStatusCode.Accepted, new { Mensagem = "Webhook recebido e em processamento." });
        }

        [HttpGet]
        [Route("pagamentos")]
        [ApiKeyAuth] // Usa a mesma segurança!
        public async Task<IHttpActionResult> ListarPagamentos()
        {
            var eventos = await _repository.ListarEventosAsync();
            return Ok(eventos);
        }
    }
}