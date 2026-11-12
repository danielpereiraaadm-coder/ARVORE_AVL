namespace ArvoreAVL.Models
{
    public class RegistroRotacao
    {
        public int Numero { get; set; }
        public DateTime DataHora { get; set; }
        public string Operacao { get; set; } = "";   // "Inserir" ou "Remover"
        public int Valor { get; set; }                // valor da operação que causou a rotação
        public string Descricao { get; set; } = "";  // mensagem da rotação
    }
}