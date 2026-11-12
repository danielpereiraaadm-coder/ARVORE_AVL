using ArvoreAVL.Models;
using ArvoreAVL.Services;
using Microsoft.AspNetCore.Mvc;

namespace ArvoreAVL.Controllers;

public class HomeController : Controller
{
    private readonly ArvoreAVLService _arvore;
    private readonly HistoricoRotacoesService _historico;

    public HomeController(ArvoreAVLService arvore, HistoricoRotacoesService historico)
    {
        _arvore = arvore;
        _historico = historico;
    }

    public IActionResult Index()
    {
        return View(CriarViewModel());
    }

    [HttpPost]
    public IActionResult Inserir(int valor)
    {
        // Foto da árvore ANTES da operação (usada na animação)
        NoAVL? antes = ConstrutorAnimacao.Clonar(_arvore.Raiz);

        bool inserido = _arvore.Inserir(valor);

        TempData["Mensagem"] = inserido
            ? $"Valor {valor} inserido com sucesso."
            : $"O valor {valor} já existe na árvore.";

        if (inserido)
            RegistrarRotacoes("Inserir", valor, antes);

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public IActionResult Remover(int valor)
    {
        NoAVL? antes = ConstrutorAnimacao.Clonar(_arvore.Raiz);

        bool removido = _arvore.Remover(valor);

        TempData["Mensagem"] = removido
            ? $"Valor {valor} removido com sucesso."
            : $"O valor {valor} não foi encontrado.";

        if (removido)
            RegistrarRotacoes("Remover", valor, antes);

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public IActionResult Buscar(int valor)
    {
        bool encontrado = _arvore.Buscar(valor);

        TempData["Mensagem"] = encontrado
            ? $"O valor {valor} foi encontrado na árvore."
            : $"O valor {valor} não foi encontrado.";

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public IActionResult Limpar()
    {
        _arvore.Limpar();
        _historico.Limpar(); // remova esta linha se quiser manter o histórico após limpar a árvore

        TempData["Mensagem"] = "Árvore limpa com sucesso.";

        return RedirectToAction(nameof(Index));
    }

    private void RegistrarRotacoes(string operacao, int valor, NoAVL? antes)
    {
        var mensagens = _arvore.ObterMensagensRotacao().ToList();

        foreach (string mensagem in mensagens)
        {
            Console.WriteLine(mensagem);
        }

        _historico.Registrar(operacao, valor, mensagens);

        _historico.DefinirAnimacao(
            ConstrutorAnimacao.Criar(operacao, valor, antes, _arvore.Raiz, mensagens));
    }

    private ArvoreViewModel CriarViewModel()
    {
        AnimacaoArvore animacao =
            _historico.ConsumirAnimacao() ?? ConstrutorAnimacao.QuadroUnico(_arvore.Raiz);

        return new ArvoreViewModel
        {
            Raiz = _arvore.Raiz,
            Altura = _arvore.ObterAltura(),
            Quantidade = _arvore.ContarNos(_arvore.Raiz),

            PreOrdem = string.Join(" → ", _arvore.PercorrerPreOrdem()),
            EmOrdem = string.Join(" → ", _arvore.PercorrerEmOrdem()),
            PosOrdem = string.Join(" → ", _arvore.PercorrerPosOrdem()),

            Mensagem = TempData["Mensagem"]?.ToString() ?? "",

            Historico = _historico.Obter(),
            AnimacaoJson = ConstrutorAnimacao.Serializar(animacao)
        };
    }
}