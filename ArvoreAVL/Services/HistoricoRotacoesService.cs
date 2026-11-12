using ArvoreAVL.Models;

namespace ArvoreAVL.Services
{
    public class HistoricoRotacoesService
    {
        private const int Limite = 200;

        private readonly List<RegistroRotacao> _registros = new();
        private readonly object _trava = new();
        private int _contador;

        // Animação da última operação, consumida uma única vez pela tela (Index)
        private AnimacaoArvore? _animacaoPendente;

        public void Registrar(string operacao, int valor, IEnumerable<string> mensagens)
        {
            lock (_trava)
            {
                foreach (string mensagem in mensagens)
                {
                    _registros.Add(new RegistroRotacao
                    {
                        Numero = ++_contador,
                        DataHora = DateTime.Now,
                        Operacao = operacao,
                        Valor = valor,
                        Descricao = mensagem
                    });
                }

                if (_registros.Count > Limite)
                    _registros.RemoveRange(0, _registros.Count - Limite);
            }
        }

        // Mais recentes primeiro
        public IReadOnlyList<RegistroRotacao> Obter()
        {
            lock (_trava)
            {
                return _registros.OrderByDescending(r => r.Numero).ToList();
            }
        }

        public void Limpar()
        {
            lock (_trava)
            {
                _registros.Clear();
                _contador = 0;
                _animacaoPendente = null;
            }
        }

        public void DefinirAnimacao(AnimacaoArvore animacao)
        {
            lock (_trava)
            {
                _animacaoPendente = animacao;
            }
        }

        // Entrega a animação pendente e a descarta (atualizar a página não repete a animação)
        public AnimacaoArvore? ConsumirAnimacao()
        {
            lock (_trava)
            {
                AnimacaoArvore? animacao = _animacaoPendente;
                _animacaoPendente = null;
                return animacao;
            }
        }
    }
}