using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace WebhookPagamentos.Api.Filters
{
    // Herdamos de AuthorizationFilterAttribute para barrar a requisição o mais cedo possível
    public class ApiKeyAuthAttribute : AuthorizationFilterAttribute
    {
        private const string ApiKeyHeaderName = "X-Api-Key";

        public override void OnAuthorization(HttpActionContext actionContext)
        {
            // 1. Tenta achar o cabeçalho 'X-Api-Key' na requisição
            if (actionContext.Request.Headers.TryGetValues(ApiKeyHeaderName, out var extractedApiKeys))
            {
                var providedApiKey = extractedApiKeys.FirstOrDefault();

                // 2. Busca a chave verdadeira lá do Web.config
                var expectedApiKey = ConfigurationManager.AppSettings["SecretApiKey"];

                // 3. Compara as duas. Se bater, libera a catraca (return)!
                if (providedApiKey == expectedApiKey)
                {
                    return;
                }
            }

            // Se chegou aqui, é porque não mandou a chave ou mandou a chave errada.
            // Retorna um Erro 401 (Não Autorizado) e encerra a conversa.
            actionContext.Response = actionContext.Request.CreateResponse(
                HttpStatusCode.Unauthorized,
                new { Mensagem = "Acesso Negado. API Key ausente ou inválida." }
            );
        }
    }
}