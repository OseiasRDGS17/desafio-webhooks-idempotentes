using Dapper;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using WebhookPagamentos.Core.Interfaces;
using WebhookPagamentos.Core.Models;

namespace WebhookPagamentos.Infrastructure.Repositories
{
    public class PagamentoRepository : IPagamentoRepository
    {
        private readonly string _connectionString;

        // No 4.72 usamos string de conexão e não o IConfiguration
        public PagamentoRepository(string connectionString)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        }

        public async Task<bool> SalvarEventoBrutoAsync(PagamentoPayload payload)
        {
            using (var conexao = new SqlConnection(_connectionString))
            {
                var sql = @"
            BEGIN TRY
                INSERT INTO LogEventosBrutos (IdTransacao, Payload, StatusProcessamento, DataRecebimento)
                VALUES (@IdTransacao, @Payload, 'Pendente', GETDATE());
                
                SELECT 1;
            END TRY
            BEGIN CATCH
                -- 2601: Cannot insert duplicate key row in object with unique index
                -- 2627: Violation of UNIQUE KEY constraint
                IF ERROR_NUMBER() IN (2601, 2627)
                    SELECT 0;
                ELSE
                    THROW;
            END CATCH";

                var payloadJson = JsonConvert.SerializeObject(payload);

                var resultado = await conexao.ExecuteScalarAsync<int>(sql, new
                {
                    payload.IdTransacao,
                    Payload = payloadJson
                });

                return resultado == 1;
            }


        }

        public async Task AtualizarStatusContratoAsync(PagamentoPayload payload)
        {
            using (var conexao = new SqlConnection(_connectionString))
            {
                // Abre a transação para garantir que a atualização do Log e do Contrato ocorram juntas
                await conexao.OpenAsync();

                using (var transacao = conexao.BeginTransaction())
                {
                    try
                    {
                        // 1. Atualiza ou Insere o status do Contrato (UPSERT via MERGE)
                        var sqlMergeContrato = @"
                MERGE INTO StatusContrato AS target
                USING (SELECT @IdContrato AS IdContrato, @Valor AS Valor, @Status AS Status) AS source
                ON target.IdContrato = source.IdContrato
                WHEN MATCHED THEN 
                    UPDATE SET Valor = target.Valor + source.Valor, Status = source.Status, UltimaAtualizacao = GETDATE()
                WHEN NOT MATCHED THEN
                    INSERT (IdContrato, Valor, Status, UltimaAtualizacao)
                    VALUES (source.IdContrato, source.Valor, source.Status, GETDATE());";

                        await conexao.ExecuteAsync(sqlMergeContrato, payload, transacao);

                        // 2. Marca o evento bruto como concluído
                        var sqlAtualizaLog = @"
                UPDATE LogEventosBrutos 
                SET StatusProcessamento = 'Concluido' 
                WHERE IdTransacao = @IdTransacao;";

                        await conexao.ExecuteAsync(sqlAtualizaLog, new { payload.IdTransacao }, transacao);

                        // Commita as duas operações
                        transacao.Commit();
                    }
                    catch
                    {
                        transacao.Rollback();
                        throw;
                    }
                }
            }
        }
        public async Task<System.Collections.Generic.IEnumerable<dynamic>> ListarEventosAsync()
        {
            using (var db = new System.Data.SqlClient.SqlConnection(_connectionString))
            {
                string sql = "SELECT TOP 50 IdTransacao, Payload, StatusProcessamento, DataRecebimento FROM LogEventosBrutos ORDER BY DataRecebimento DESC";
                return await Dapper.SqlMapper.QueryAsync(db, sql);
            }
        }

        public async Task<System.Collections.Generic.IEnumerable<dynamic>> ListarContratosAsync()
        {
            using (var db = new System.Data.SqlClient.SqlConnection(_connectionString))
            {
                string sql = "SELECT IdContrato, Valor, Status, UltimaAtualizacao FROM StatusContrato ORDER BY UltimaAtualizacao DESC";
                return await Dapper.SqlMapper.QueryAsync(db, sql);
            }
        }
    }
}
