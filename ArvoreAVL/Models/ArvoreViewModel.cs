namespace ArvoreAVL.Models
{
    public class ArvoreViewModel
    {
        public NoAVL? Raiz { get; set; }

        public int Altura { get; set; }
        public int Quantidade { get; set; }

        public string PreOrdem { get; set; } = "";
        public string EmOrdem { get; set; } = "";
        public string PosOrdem { get; set; } = "";

        public string Mensagem { get; set; } = "";
        public string Erro { get; set; } = "";

        // Histórico das rotações realizadas (mais recentes primeiro)
        public IReadOnlyList<RegistroRotacao> Historico { get; set; } = new List<RegistroRotacao>();

        // Quadros da animação em JSON (lidos pelo arvore-animada.js)
        public string AnimacaoJson { get; set; } = "{\"quadros\":[]}";
    }
}