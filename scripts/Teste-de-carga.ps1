## Teste com diversos registros para observar os pendentes e concluídos sendo atualizados.
$url = "http://localhost:58232/api/webhooks/pagamento"
$headers = @{ "X-Api-Key" = "TesteOseias!2026" }

Write-Host "Iniciando disparo em lote..." -ForegroundColor Cyan

for ($i = 1; $i -le 10; $i++) {
    # Gera um valor aleatório para ficar bonito no dashboard
    $valorAleatorio = Get-Random -Minimum 50 -Maximum 1000
    
    $body = @{ 
        IdTransacao = "TXN-LOTE-00$i"
        IdContrato = "CTR-TESTE"
        Valor = $valorAleatorio
        Status = "Sucesso" 
    } | ConvertTo-Json
    
    # Dispara a requisição
    Invoke-RestMethod -Uri $url -Method Post -Headers $headers -Body $body -ContentType "application/json"
    
    Write-Host "Enviado: TXN-LOTE-00$i" -ForegroundColor Green
}

Write-Host "Todos os 10 webhooks foram disparados!" -ForegroundColor Yellow
Read-Host "Pressione ENTER para fechar..."