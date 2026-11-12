namespace ArvoreAVL.Models
{
    // Nó "achatado" para ser enviado ao JavaScript (JSON)
    public class NoDto
    {
        public int V { get; set; }      // valor
        public int H { get; set; }      // altura
        public int Fb { get; set; }     // fator de balanceamento
        public NoDto? E { get; set; }   // filho esquerdo
        public NoDto? D { get; set; }   // filho direito
    }

    // Um "quadro" da animação: uma foto da árvore em um momento
    public class QuadroAnimacao
    {
        public string Titulo { get; set; } = "";
        public string Descricao { get; set; } = "";
        public NoDto? Raiz { get; set; }

        // Valor do nó recém-inserido (destaque verde)
        public int? Novo { get; set; }

        // Valores dos nós desbalanceados (destaque vermelho)
        public List<int> Desbalanceados { get; set; } = new();
    }

    public class AnimacaoArvore
    {
        public List<QuadroAnimacao> Quadros { get; set; } = new();
    }
}