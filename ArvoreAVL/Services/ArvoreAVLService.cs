using ArvoreAVL.Models;

namespace ArvoreAVL.Services
{
    public class ArvoreAVLService
    {
        private NoAVL? raiz;

        private readonly List<string> mensagensRotacao = new();

        public NoAVL? Raiz => raiz;

        private int Altura(NoAVL? no)
        {
            return no?.Altura ?? 0;
        }

        private int FatorBalanceamento(NoAVL? no)
        {
            if (no == null)
                return 0;

            return Altura(no.Esquerda) - Altura(no.Direita);
        }

        private void AtualizarAltura(NoAVL no)
        {
            no.Altura = 1 + Math.Max(
                Altura(no.Esquerda),
                Altura(no.Direita)
            );
        }

        // Rotação simples à direita: LL
        private NoAVL RotacaoDireita(NoAVL y)
        {
            NoAVL x = y.Esquerda!;
            NoAVL? subArvore = x.Direita;

            x.Direita = y;
            y.Esquerda = subArvore;

            AtualizarAltura(y);
            AtualizarAltura(x);

            mensagensRotacao.Add("Rotação LL executada.");

            return x;
        }

        // Rotação simples à esquerda: RR
        private NoAVL RotacaoEsquerda(NoAVL x)
        {
            NoAVL y = x.Direita!;
            NoAVL? subArvore = y.Esquerda;

            y.Esquerda = x;
            x.Direita = subArvore;

            AtualizarAltura(x);
            AtualizarAltura(y);

            mensagensRotacao.Add("Rotação RR executada.");

            return y;
        }

        // Balanceamento dos nós
        private NoAVL Balancear(NoAVL no)
        {
            AtualizarAltura(no);

            int fator = FatorBalanceamento(no);

            // Caso LL
            if (fator > 1 && FatorBalanceamento(no.Esquerda) >= 0)
            {
                return RotacaoDireita(no);
            }

            // Caso LR
            if (fator > 1 && FatorBalanceamento(no.Esquerda) < 0)
            {
                no.Esquerda = RotacaoEsquerda(no.Esquerda!);

                mensagensRotacao.Add("Rotação LR executada.");

                return RotacaoDireitaSemMensagem(no);
            }

            // Caso RR
            if (fator < -1 && FatorBalanceamento(no.Direita) <= 0)
            {
                return RotacaoEsquerda(no);
            }

            // Caso RL
            if (fator < -1 && FatorBalanceamento(no.Direita) > 0)
            {
                no.Direita = RotacaoDireitaSemMensagem(no.Direita!);

                mensagensRotacao.Add("Rotação RL executada.");

                return RotacaoEsquerdaSemMensagem(no);
            }

            return no;
        }

        private NoAVL RotacaoDireitaSemMensagem(NoAVL y)
        {
            NoAVL x = y.Esquerda!;
            NoAVL? subArvore = x.Direita;

            x.Direita = y;
            y.Esquerda = subArvore;

            AtualizarAltura(y);
            AtualizarAltura(x);

            return x;
        }

        private NoAVL RotacaoEsquerdaSemMensagem(NoAVL x)
        {
            NoAVL y = x.Direita!;
            NoAVL? subArvore = y.Esquerda;

            y.Esquerda = x;
            x.Direita = subArvore;

            AtualizarAltura(x);
            AtualizarAltura(y);

            return y;
        }

        // Inserção
        public bool Inserir(int valor)
        {
            mensagensRotacao.Clear();

            if (Buscar(valor))
                return false;

            raiz = InserirNo(raiz, valor);

            return true;
        }

        private NoAVL InserirNo(NoAVL? no, int valor)
        {
            if (no == null)
                return new NoAVL(valor);

            if (valor < no.Valor)
                no.Esquerda = InserirNo(no.Esquerda, valor);
            else
                no.Direita = InserirNo(no.Direita, valor);

            return Balancear(no);
        }

        // Busca
        public bool Buscar(int valor)
        {
            NoAVL? atual = raiz;

            while (atual != null)
            {
                if (valor == atual.Valor)
                    return true;

                atual = valor < atual.Valor
                    ? atual.Esquerda
                    : atual.Direita;
            }

            return false;
        }

        // Remoção
        public bool Remover(int valor)
        {
            mensagensRotacao.Clear();

            if (!Buscar(valor))
                return false;

            raiz = RemoverNo(raiz, valor);

            return true;
        }

        private NoAVL? RemoverNo(NoAVL? no, int valor)
        {
            if (no == null)
                return null;

            if (valor < no.Valor)
            {
                no.Esquerda = RemoverNo(no.Esquerda, valor);
            }
            else if (valor > no.Valor)
            {
                no.Direita = RemoverNo(no.Direita, valor);
            }
            else
            {
                if (no.Esquerda == null)
                    return no.Direita;

                if (no.Direita == null)
                    return no.Esquerda;

                NoAVL sucessor = MenorValor(no.Direita);

                no.Valor = sucessor.Valor;

                no.Direita = RemoverNo(
                    no.Direita,
                    sucessor.Valor
                );
            }

            return Balancear(no);
        }

        private NoAVL MenorValor(NoAVL no)
        {
            NoAVL atual = no;

            while (atual.Esquerda != null)
                atual = atual.Esquerda;

            return atual;
        }

        // Percursos
        public List<int> PercorrerPreOrdem()
        {
            List<int> valores = new();
            PreOrdem(raiz, valores);
            return valores;
        }

        private void PreOrdem(NoAVL? no, List<int> valores)
        {
            if (no == null) return;

            valores.Add(no.Valor);
            PreOrdem(no.Esquerda, valores);
            PreOrdem(no.Direita, valores);
        }

        public List<int> PercorrerEmOrdem()
        {
            List<int> valores = new();
            EmOrdem(raiz, valores);
            return valores;
        }

        private void EmOrdem(NoAVL? no, List<int> valores)
        {
            if (no == null) return;

            EmOrdem(no.Esquerda, valores);
            valores.Add(no.Valor);
            EmOrdem(no.Direita, valores);
        }

        public List<int> PercorrerPosOrdem()
        {
            List<int> valores = new();
            PosOrdem(raiz, valores);
            return valores;
        }

        private void PosOrdem(NoAVL? no, List<int> valores)
        {
            if (no == null) return;

            PosOrdem(no.Esquerda, valores);
            PosOrdem(no.Direita, valores);
            valores.Add(no.Valor);
        }

        // Quantidade de nós
        public int ContarNos(NoAVL? no)
        {
            if (no == null)
                return 0;

            return 1 + ContarNos(no.Esquerda)
                     + ContarNos(no.Direita);
        }

        // Limpar árvore
        public void Limpar()
        {
            raiz = null;
            mensagensRotacao.Clear();
        }

        public List<string> ObterMensagensRotacao()
        {
            return mensagensRotacao.ToList();
        }

        public int ObterAltura()
        {
            return Altura(raiz);
        }

        public int ObterFatorBalanceamento(NoAVL? no)
        {
            return FatorBalanceamento(no);
        }
    }
}
