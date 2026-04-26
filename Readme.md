# Avaliação de habilidades técnicas SABEMI TEC

Este projeto é uma solução Fullstack desenvolvida para receber, processar e exibir notificações de pagamentos (Webhooks) de forma segura, assíncrona e totalmente idempotente.

## Arquitetura e Tecnologias

A solução foi dividida em um monólito modular no Backend e uma Single Page Application no Frontend:

### Backend (.NET Framework 4.7.2)
* **ASP.NET Web API:** Porta de entrada para recebimento dos POSTs.
* **SQL Server & Dapper:** Persistência de dados rápida e leve via Micro-ORM.
* **System.Threading.Channels:** Fila em memória para suportar picos de requisições (Buffer) sem travar a API.
* **HostingEnvironment.QueueBackgroundWorkItem:** Processamento assíncrono em background (Worker) para simulação de regras de negócio pesadas.
* **Unity Container:** Injeção de Dependência.
* **Segurança:** Proteção de endpoint customizada via `ActionFilterAttribute` exigindo **API Key**.

### Frontend (React + Vite)
* **React.js:** Interface reativa para acompanhamento em tempo real.
* **Axios:** Cliente HTTP para consumo da API.
* **CSS Puro & Flexbox:** Estilização limpa, sem dependência de frameworks pesados.
* **Polling (setInterval):** Atualização automática da tabela a cada 5 segundos para acompanhar o processamento em background.

---

## Como executar o projeto localmente

### 1. Banco de Dados (SQL Server)
1. Abra o SQL Server Management Studio (SSMS).
2. Execute o script de criação localizado em `scripts/criacao_BD.sql`.
3. Este script criará o banco `WebhookDB` e as tabelas necessárias com as travas de **Idempotência** (`UNIQUE INDEX`).

### 2. Backend (API)
1. Abra a solução `WebhookPagamentos.sln` no Visual Studio.
2. Verifique o arquivo `Web.config` no projeto da API. A `ConnectionString` está configurada para acessar o `localhost\SQLEXPRESS` com `TrustServerCertificate=True`. Ajuste o servidor se necessário.
3. No `Web.config`, note a chave de segurança `<add key="SecretApiKey" value="TesteOseias!2026" />`.
4. Compile a solução e execute (F5) usando o IIS Express.

### 3. Frontend (Dashboard)
1. Certifique-se de ter o Node.js instalado.
2. Abra um terminal na pasta `frontend/dashboard-pagamentos`.
3. Execute `npm install` para baixar as dependências.
4. Execute `npm run dev` para iniciar o servidor web.
5. Acesse a URL gerada (geralmente `http://localhost:5173/`).

---

## Como testar a Resiliência e Idempotência

Para demonstrar o resultado da arquitetura, preparei um script de teste de carga em PowerShell.

1. Com o Backend e o Frontend rodando, abra o **PowerShell**.
2. Execute os scripts localizados em `scripts/*`.
3. **O que observar:**
   * A API retornará `202 Accepted` quase instantaneamente para todos os disparos.
   * No Dashboard, você verá os 10 eventos aparecerem com status **⏳ Pendente** ao executar o script `Teste-de-carga.ps1`.
   * Ao longo de 20 segundos, observe a tela: os status mudarão sozinhos para **✅ Concluído** em cascata, provando que o processamento em background está funcionando sem bloquear novas requisições.
   * Ao executar o script `Teste-segurança.ps1` devemos observar um erro informando falta de API Key e na segunda vez o sucesso passando na validação de segurança. 
   * Ao executar o script `Teste-idempotencia.ps1` devemos observar um sucesso no primeiro processamento e um erro de ID já processado na segunda tentativa. 

> **Nota sobre Idempotência:** Tente disparar uma requisição manualmente com um `IdTransacao` que já exista. O banco de dados barrará a inserção e a API tratará o erro silenciosamente, sem duplicar o processamento ou quebrar o fluxo.

## 🌟 Diferenciais da Versão Atual (V2)

Nesta versão, o projeto evoluiu de um simples monitor de logs para uma ferramenta de **Gestão Financeira**:

* **Visão Multicamadas:** Separação clara entre monitoramento técnico (Logs para TI) e visão de negócios (Cards para o Financeiro).
* **Lógica de Acumulador Financeiro:** Ajuste na persistência para que os webhooks somem valores ao saldo do contrato, em vez de apenas sobrescrever.
* **Alta Performance no Frontend:** Implementação de `Promise.all` no React, garantindo que as requisições de Logs e Contratos aconteçam em paralelo, reduzindo o tempo de carregamento.
* **Dashboard Moderno:** Uso de KPIs (indicadores chave de performance) para exibir o Total Arrecadado e Contratos Ativos.

---

## 🚀 Como testar a Nova Visão Financeira

1. No Dashboard, mude para a aba **"Visão Financeira (Negócios)"**.
2. Observe os cards de contrato sendo atualizados e os valores sendo somados conforme os webhooks são processados em background.