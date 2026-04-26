import { useState, useEffect } from 'react';
import axios from 'axios';
import './App.css';

function App() {
  const [pagamentos, setPagamentos] = useState([]);
  const [contratos, setContratos] = useState([]); // Novo estado para os contratos
  const [carregando, setCarregando] = useState(false);
  const [abaAtiva, setAbaAtiva] = useState('logs'); // Controle das abas

  const [filtroStatus, setFiltroStatus] = useState('');
  const [filtroContrato, setFiltroContrato] = useState('');

  // Busca TUDO da API ao mesmo tempo 
  const buscarDados = async () => {
    setCarregando(true);
    try {
      const config = { headers: { 'X-Api-Key': 'TesteOseias!2026' } };
      const urlBase = 'http://localhost:58232/api/webhooks';

      const [resLogs, resContratos] = await Promise.all([
        axios.get(`${urlBase}/pagamentos`, config),
        axios.get(`${urlBase}/contratos`, config)
      ]);
      
      const logsFormatados = resLogs.data.map(item => ({
        ...item, DadosOriginais: JSON.parse(item.Payload)
      }));

      setPagamentos(logsFormatados);
      setContratos(resContratos.data);

    } catch (error) {
      console.error("Erro na API:", error);
    }
    setCarregando(false);
  };

  useEffect(() => {
    buscarDados();
    const intervalo = setInterval(buscarDados, 5000);
    return () => clearInterval(intervalo);
  }, []);

  // Filtros da Aba de Logs
  const pagamentosFiltrados = pagamentos.filter(pag => {
    const statusBate = filtroStatus ? pag.StatusProcessamento === filtroStatus : true;
    const contratoBate = filtroContrato ? pag.DadosOriginais.IdContrato.includes(filtroContrato) : true;
    return statusBate && contratoBate;
  });

  // Cálculos para a Aba Financeira (KPIs)
  const totalArrecadado = contratos.reduce((acc, contrato) => acc + contrato.Valor, 0);
  const totalContratos = contratos.length;

  const renderBadge = (status) => {
    if (status === 'Concluido' || status === 'Sucesso') return <span className="status-badge status-sucesso">✅ {status}</span>;
    if (status === 'Pendente') return <span className="status-badge status-pendente">⏳ {status}</span>;
    return <span className="status-badge status-erro">❌ {status}</span>;
  };

  return (
    <div className="dashboard-container">
      <div className="header">
        <h1>📊 Painel de Controle</h1>
        <button className="btn-refresh" onClick={buscarDados} disabled={carregando}>
          {carregando ? "⏳ Atualizando..." : "🔄 Atualizar Agora"}
        </button>
      </div>

      {/* Navegação entre Abas */}
      <div className="tabs-container">
        <button className={`tab-btn ${abaAtiva === 'logs' ? 'active' : ''}`} onClick={() => setAbaAtiva('logs')}>
          Monitoramento (Logs de TI)
        </button>
        <button className={`tab-btn ${abaAtiva === 'financeiro' ? 'active' : ''}`} onClick={() => setAbaAtiva('financeiro')}>
          Visão Financeira (Negócios)
        </button>
      </div>

      {/* CONTEÚDO DA ABA 1: LOGS DA TI */}
      {abaAtiva === 'logs' && (
        <>
          <div className="filters">
            <input type="text" placeholder="🔍 Buscar ID do Contrato..." value={filtroContrato} onChange={(e) => setFiltroContrato(e.target.value)} />
            <select value={filtroStatus} onChange={(e) => setFiltroStatus(e.target.value)}>
              <option value="">Todos os Status</option>
              <option value="Concluido">Concluídos</option>
              <option value="Pendente">Pendentes</option>
              <option value="Erro">Com Erro</option>
            </select>
          </div>

          <div className="tabela-container">
            <table>
              <thead>
                <tr>
                  <th>Transação (Webhooks Brutos)</th>
                  <th>Contrato</th>
                  <th>Valor Recebido</th>
                  <th>Processamento</th>
                  <th>Horário</th>
                </tr>
              </thead>
              <tbody>
                {pagamentosFiltrados.map((pag, idx) => (
                  <tr key={idx} style={pag.StatusProcessamento === 'Erro' ? {backgroundColor: '#fef2f2'} : {}}>
                    <td><strong>{pag.IdTransacao}</strong></td>
                    <td>{pag.DadosOriginais.IdContrato}</td>
                    <td>R$ {pag.DadosOriginais.Valor?.toFixed(2)}</td>
                    <td>{renderBadge(pag.StatusProcessamento)}</td>
                    <td>{new Date(pag.DataRecebimento).toLocaleString()}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </>
      )}

      {/* CONTEÚDO DA ABA 2: VISÃO FINANCEIRA */}
      {abaAtiva === 'financeiro' && (
        <>
          {/* Cartões de KPI no Topo */}
          <div className="kpi-row">
            <div className="kpi-card">
              <h3>Volume Total Arrecadado</h3>
              <p className="valor">
                {totalArrecadado.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' })}
              </p>
            </div>
            <div className="kpi-card" style={{borderLeftColor: '#059669'}}>
              <h3>Contratos Ativos na Base</h3>
              <p className="valor">{totalContratos} contratos</p>
            </div>
          </div>

          {/* Grid de Contratos Individuais */}
          <div className="cards-grid">
            {contratos.length === 0 ? <p>Nenhum contrato processado ainda.</p> : null}
            
            {contratos.map((contrato, idx) => (
              <div className="contrato-card" key={idx}>
                <div className="contrato-header">
                  <strong>{contrato.IdContrato}</strong>
                  {renderBadge(contrato.Status)}
                </div>
                <div className="contrato-valor">
                  {contrato.Valor.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' })}
                </div>
                <div className="contrato-data">
                  Última movimentação: {new Date(contrato.UltimaAtualizacao).toLocaleString()}
                </div>
              </div>
            ))}
          </div>
        </>
      )}

    </div>
  );
}

export default App;