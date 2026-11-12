namespace ArvoreAVL.Models
{
    public class NoAVL
    {
        public int Valor { get; set; }
        public int Altura { get; set; } = 1;

        public NoAVL? Esquerda { get; set; }
        public NoAVL? Direita { get; set; }

        public NoAVL(int valor)
        {
            Valor = valor;
        }
    }
}