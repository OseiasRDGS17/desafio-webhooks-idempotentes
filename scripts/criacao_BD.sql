-- 1. Cria o banco de dados
CREATE DATABASE WebhookDB;
GO

-- 2. Entra no banco de dados que acabamos de criar
USE WebhookDB;
GO

-- 3. Cria a tabela de Logs Brutos (Com a restrição de unicidade para a Idempotência)
CREATE TABLE LogEventosBrutos (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    IdTransacao VARCHAR(100) NOT NULL,
    Payload NVARCHAR(MAX) NOT NULL, -- JSON completo
    StatusProcessamento VARCHAR(50) NOT NULL,
    DataRecebimento DATETIME NOT NULL DEFAULT GETDATE(),
    
    -- Além da PK temos a UC de ID transação
    CONSTRAINT UQ_IdTransacao UNIQUE (IdTransacao) 
);
GO

-- 4. Cria a tabela de Status do Contrato
CREATE TABLE StatusContrato (
    IdContrato VARCHAR(100) PRIMARY KEY,
    Valor DECIMAL(18,2) NOT NULL,
    Status VARCHAR(50) NOT NULL,
    UltimaAtualizacao DATETIME NOT NULL
);
GO

USE WebhookDB;

-- Inserindo um evento simulando que falhou na validação interna
INSERT INTO LogEventosBrutos (IdTransacao, Payload, StatusProcessamento, DataRecebimento)
VALUES (
    'TXN-ERRO-VALIDACAO', 
    '{"IdTransacao": "TXN-ERRO-VALIDACAO", "IdContrato": "CONTRATO-INVALIDO", "Valor": 0, "Status": "Falha"}', 
    'Erro', 
    GETDATE()
);