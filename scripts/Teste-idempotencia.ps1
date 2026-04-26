# Configurações iniciais
$url = "http://localhost:58232/api/webhooks/pagamento"
$apiKey = "TesteOseias!2026"
$headers = @{ "X-Api-Key" = $apiKey }

Write-Host "--- TESTE DE IDEMPOTÊNCIA (DUPLICIDADE) ---" -ForegroundColor Cyan

# Montamos o Payload uma vez só para garantir que os dados são idênticos
$body = @{
    IdTransacao = "TXN-001"
    IdContrato = "CTR-999"
    Valor = 150.50
    DataPagamento = "2026-04-25T10:00:00"
    Status = "Sucesso"
} | ConvertTo-Json

# ---------------------------------------------------------
# 1. PRIMEIRA CHAMADA (Deve ser processada com sucesso)
# ---------------------------------------------------------
Write-Host "`n[Tentativa 1] Enviando transação TXN-001 pela primeira vez..." -ForegroundColor Yellow
try {
    $resposta1 = Invoke-RestMethod -Uri $url -Method Post -Headers $headers -Body $body -ContentType "application/json; charset=utf-8"
    Write-Host "SUCESSO: A API recebeu o evento!" -ForegroundColor Green
    Write-Host "Resposta da API: $($resposta1.Mensagem)" -ForegroundColor White
} 
catch {
    Write-Host "ERRO INESPERADO: $($_.Exception.Message)" -ForegroundColor Red
}

# ---------------------------------------------------------
# 2. SEGUNDA CHAMADA (Deve acionar a trava de Idempotência)
# ---------------------------------------------------------
Write-Host "`n[Tentativa 2] Reenviando a exata mesma transação (TXN-001)..." -ForegroundColor Yellow
try {
    $resposta2 = Invoke-RestMethod -Uri $url -Method Post -Headers $headers -Body $body -ContentType "application/json; charset=utf-8"
    
    # Se o script cair aqui, significa que sua API trata duplicidade de forma silenciosa (Retornando 200 ou 202)
    Write-Host "A API barrou internamente de forma silenciosa!" -ForegroundColor Green
    Write-Host "Resposta da API: $($resposta2.Mensagem)" -ForegroundColor White
} 
catch {
    # Se o script cair aqui, significa que sua API trata duplicidade estourando um Erro HTTP (Ex: 400 ou 409)
    Write-Host "SUCESSO: A API bloqueou a duplicidade com um Erro HTTP!" -ForegroundColor Green
    Write-Host "Detalhe do Bloqueio: $($_.Exception.Message)" -ForegroundColor Gray
}

Write-Host "`n--- FIM DO TESTE ---" -ForegroundColor Cyan
Read-Host "Pressione ENTER para fechar a janela..."