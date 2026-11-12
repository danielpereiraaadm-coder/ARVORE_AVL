using System.Text.Json;
using ArvoreAVL.Models;

namespace ArvoreAVL.Services
{
    /// <summary>
    /// Monta os quadros da animação (antes, desbalanceada, depois) sem alterar o ArvoreAVLService.
    /// O quadro intermediário é simulado: inserção/remoção de árvore binária de busca comum,
    /// feita em uma CÓPIA da árvore, sem rebalancear.
    /// </summary>
    public static class ConstrutorAnimacao
    {
        private static readonly JsonSerializerOptions Opcoes = new(JsonSerializerDefaults.Web);

        // ---------- API pública ----------

        public static NoAVL? Clonar(NoAVL? no)
        {
            if (no == null)
                return null;

            return new NoAVL(no.Valor)
            {
                Altura = no.Altura,
                Esquerda = Clonar(no.Esquerda),
                Direita = Clonar(no.Direita)
            };
        }

        public static AnimacaoArvore QuadroUnico(NoAVL? raiz)
        {
            return new AnimacaoArvore
            {
                Quadros =
                {
                    new QuadroAnimacao
                    {
                        Titulo = "Árvore atual",
                        Descricao = raiz == null ? "A árvore está vazia." : "",
                        Raiz = ParaDto(raiz)
                    }
                }
            };
        }

        public static AnimacaoArvore Criar(
            string operacao,
            int valor,
            NoAVL? antes,
            NoAVL? depois,
            IReadOnlyList<string> mensagens)
        {
            bool inserir = operacao == "Inserir";
            string verbo = inserir ? "Inserindo" : "Removendo";

            var animacao = new AnimacaoArvore();

            // 1) Estado anterior
            animacao.Quadros.Add(new QuadroAnimacao
            {
                Titulo = "Estado anterior",
                Descricao = antes == null ? "A árvore estava vazia." : "Árvore antes da operação.",
                Raiz = ParaDto(antes)
            });

            // 2) Operação aplicada, ainda sem rebalancear
            NoAVL? intermediaria = inserir
                ? InserirSemBalancear(Clonar(antes), valor)
                : RemoverSemBalancear(Clonar(antes), valor);

            NoDto? dtoIntermediario = ParaDto(intermediaria);
            NoDto? dtoDepois = ParaDto(depois);

            var desbalanceados = new List<int>();
            ColetarDesbalanceados(dtoIntermediario, desbalanceados);

            bool mudou = JsonSerializer.Serialize(dtoIntermediario, Opcoes)
                         != JsonSerializer.Serialize(dtoDepois, Opcoes);

            string descricao;
            if (desbalanceados.Count > 0)
                descricao = $"Desbalanceamento detectado no nó {string.Join(", ", desbalanceados)}: o fator de balanceamento saiu do intervalo de -1 a 1.";
            else if (!mudou)
                descricao = "Nenhuma rotação necessária: a árvore continua balanceada.";
            else
                descricao = "Árvore ajustada após a operação.";

            animacao.Quadros.Add(new QuadroAnimacao
            {
                Titulo = $"{verbo} {valor}",
                Descricao = descricao,
                Raiz = dtoIntermediario,
                Novo = inserir ? (int?)valor : null,
                Desbalanceados = desbalanceados
            });

            // 3) Resultado final, depois da rotação
            if (desbalanceados.Count > 0 || mudou)
            {
                animacao.Quadros.Add(new QuadroAnimacao
                {
                    Titulo = "Rotação realizada",
                    Descricao = mensagens.Count > 0
                        ? string.Join(" ", mensagens)
                        : "A árvore foi rebalanceada.",
                    Raiz = dtoDepois
                });
            }

            return animacao;
        }

        public static string Serializar(AnimacaoArvore animacao)
        {
            return JsonSerializer.Serialize(animacao, Opcoes);
        }

        // ---------- Conversão para DTO (altura e FB recalculados pela estrutura) ----------

        private static NoDto? ParaDto(NoAVL? no)
        {
            return Converter(no).dto;
        }

        private static (NoDto? dto, int altura) Converter(NoAVL? no)
        {
            if (no == null)
                return (null, 0);

            var (esquerda, alturaEsquerda) = Converter(no.Esquerda);
            var (direita, alturaDireita) = Converter(no.Direita);

            int altura = 1 + Math.Max(alturaEsquerda, alturaDireita);

            var dto = new NoDto
            {
                V = no.Valor,
                H = altura,
                Fb = alturaEsquerda - alturaDireita,
                E = esquerda,
                D = direita
            };

            return (dto, altura);
        }

        private static void ColetarDesbalanceados(NoDto? no, List<int> lista)
        {
            if (no == null)
                return;

            if (Math.Abs(no.Fb) > 1)
                lista.Add(no.V);

            ColetarDesbalanceados(no.E, lista);
            ColetarDesbalanceados(no.D, lista);
        }

        // ---------- Operações de árvore binária de busca (sem balanceamento) ----------

        private static NoAVL? InserirSemBalancear(NoAVL? raiz, int valor)
        {
            if (raiz == null)
                return new NoAVL(valor);

            if (valor < raiz.Valor)
                raiz.Esquerda = InserirSemBalancear(raiz.Esquerda, valor);
            else if (valor > raiz.Valor)
                raiz.Direita = InserirSemBalancear(raiz.Direita, valor);

            return raiz;
        }

        private static NoAVL? RemoverSemBalancear(NoAVL? raiz, int valor)
        {
            if (raiz == null)
                return null;

            if (valor < raiz.Valor)
            {
                raiz.Esquerda = RemoverSemBalancear(raiz.Esquerda, valor);
            }
            else if (valor > raiz.Valor)
            {
                raiz.Direita = RemoverSemBalancear(raiz.Direita, valor);
            }
            else
            {
                if (raiz.Esquerda == null)
                    return raiz.Direita;

                if (raiz.Direita == null)
                    return raiz.Esquerda;

                // Dois filhos: substitui pelo sucessor em ordem
                NoAVL sucessor = raiz.Direita;
                while (sucessor.Esquerda != null)
                    sucessor = sucessor.Esquerda;

                raiz.Valor = sucessor.Valor;
                raiz.Direita = RemoverSemBalancear(raiz.Direita, sucessor.Valor);
            }

            return raiz;
        }
    }
}