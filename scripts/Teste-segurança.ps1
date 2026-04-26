# Configurações iniciais
$url = "http://localhost:58232/api/webhooks/pagamento"
$apiKey = "TesteOseias!2026"

Write-Host "--- INICIANDO BATERIA DE TESTES ---" -ForegroundColor Cyan

# 1. TESTE ERRADO (SEM API KEY)
Write-Host "`n[Teste 1] Tentando enviar sem API Key..." -ForegroundColor Yellow
try {
    $bodyInvasor = @{ IdTransacao = "TXN-INVASOR-$(Get-Random)"; IdContrato = "CTR-123"; Valor = 50.00; Status = "Sucesso" } | ConvertTo-Json
    Invoke-RestMethod -Uri $url -Method Post -Body $bodyInvasor -ContentType "application/json"
    Write-Host "ERRO: A API aceitou a requisição sem chave!" -ForegroundColor Red
} 
catch {
    # Aqui capturamos o erro 401 e mostramos que o sistema funcionou ao barrar
    Write-Host "SUCESSO: A API barrou o invasor corretamente!" -ForegroundColor Green
    Write-Host "Detalhe do Erro: $($_.Exception.Message)" -ForegroundColor Gray
}

# 2. TESTE CORRETO (COM API KEY)
Write-Host "`n[Teste 2] Enviando com API Key correta..." -ForegroundColor Yellow
try {
    $bodySeguro = @{ IdTransacao = "TXN-SEGURA-$(Get-Random)"; IdContrato = "CTR-123"; Valor = 50.00; Status = "Sucesso" } | ConvertTo-Json
    $headers = @{ "X-Api-Key" = $apiKey }
    
    $resposta = Invoke-RestMethod -Uri $url -Method Post -Headers $headers -Body $bodySeguro -ContentType "application/json"
    
    Write-Host "SUCESSO: Webhook aceito pela API!" -ForegroundColor Green
    Write-Host "Resposta: $($resposta.Mensagem)" -ForegroundColor White
} 
catch {
    Write-Host "ERRO: A API rejeitou uma chave que deveria ser válida!" -ForegroundColor Red
    Write-Host "Detalhe: $($_.Exception.Message)"
}

Write-Host "`n--- TESTES FINALIZADOS ---" -ForegroundColor Cyan
Read-Host "Pressione ENTER para fechar..."