import { useState, useEffect } from 'react';
import axios from 'axios';
import './App.css';

function App() {
  const [pagamentos, setPagamentos] = useState([]);
  const [filtroStatus, setFiltroStatus] = useState('');
  const [filtroContrato, setFiltroContrato] = useState('');
  const [carregando, setCarregando] = useState(false);

  // Função que vai na API C# buscar os dados
  const buscarDados = async () => {
    setCarregando(true);
    try {
      const response = await axios.get('http://localhost:58232/api/webhooks/pagamentos', {
        headers: {
          'X-Api-Key': 'TesteOseias!2026'
        }
      });
      
      const dadosFormatados = response.data.map(item => ({
        ...item,
        DadosOriginais: JSON.parse(item.Payload)
      }));

      setPagamentos(dadosFormatados);
    } catch (error) {
      console.error("Erro ao buscar dados:", error);
      alert("Falha ao conectar com a API. Verifique se o IIS Express está rodando no Visual Studio e se a porta está correta.");
    }
    setCarregando(false);
  };

  useEffect(() => {
    buscarDados();
    const intervalo = setInterval(buscarDados, 5000);
    return () => clearInterval(intervalo);
  }, []);

  const pagamentosFiltrados = pagamentos.filter(pag => {
    const statusBate = filtroStatus ? pag.StatusProcessamento === filtroStatus : true;
    const contratoBate = filtroContrato ? pag.DadosOriginais.IdContrato.includes(filtroContrato) : true;
    return statusBate && contratoBate;
  });

  const renderStatus = (status) => {
    if (status === 'Concluido' || status === 'Sucesso') return <span className="status-badge status-sucesso">✅ Concluído</span>;
    if (status === 'Pendente') return <span className="status-badge status-pendente">⏳ Pendente</span>;
    return <span className="status-badge status-erro">❌ Erro</span>;
  };

  return (
    <div className="dashboard-container">
      <div className="header">
        <h1>📊 Dashboard de Webhooks</h1>
        <button className="btn-refresh" onClick={buscarDados} disabled={carregando}>
          {carregando ? "⏳ Carregando..." : "🔄 Atualizar Agora"}
        </button>
      </div>

      <div className="filters">
        <input 
          type="text" 
          placeholder="🔍 Filtrar por ID do Contrato..." 
          value={filtroContrato}
          onChange={(e) => setFiltroContrato(e.target.value)}
        />
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
              <th>ID Transação</th>
              <th>Contrato</th>
              <th>Valor (R$)</th>
              <th>Status do Banco</th>
              <th>Data Recebimento</th>
            </tr>
          </thead>
          <tbody>
            {pagamentosFiltrados.length === 0 ? (
              <tr><td colSpan="5" style={{textAlign: 'center'}}>Nenhum pagamento encontrado.</td></tr>
            ) : (
              pagamentosFiltrados.map((pag, index) => (
                <tr key={index} style={pag.StatusProcessamento === 'Erro' ? {backgroundColor: '#fef2f2'} : {}}>
                  <td><strong>{pag.IdTransacao}</strong></td>
                  <td>{pag.DadosOriginais.IdContrato}</td>
                  <td>{pag.DadosOriginais.Valor?.toFixed(2)}</td>
                  <td>{renderStatus(pag.StatusProcessamento)}</td>
                  <td>{new Date(pag.DataRecebimento).toLocaleString()}</td>
                </tr>
              ))
            )}
          </tbody>
        </table>
      </div>
    </div>
  );
}

export default App;