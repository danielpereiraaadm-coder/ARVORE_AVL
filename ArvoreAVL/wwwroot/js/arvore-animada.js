(function () {
    'use strict';

    // ------------------------------------------------------------
    // Configuração visual
    // ------------------------------------------------------------
    const LARG = 76;      // largura do nó
    const ALT = 62;       // altura do nó
    const DX = 92;        // distância horizontal entre nós (ordem em-ordem)
    const DY = 96;        // distância vertical entre níveis
    const MARGEM = 24;
    const DURACAO = 1300; // ms por transição na velocidade "Normal"
    const NS = 'http://www.w3.org/2000/svg';

    const reduzirMovimento = window.matchMedia
        && window.matchMedia('(prefers-reduced-motion: reduce)').matches;

    // ------------------------------------------------------------
    // Elementos e dados
    // ------------------------------------------------------------
    const dadosEl = document.getElementById('dados-arvore');
    const palco = document.getElementById('palco-arvore');
    const lona = document.getElementById('lona-arvore');
    const svg = document.getElementById('arestas-arvore');
    if (!dadosEl || !palco || !lona || !svg) return;

    const elTitulo = document.getElementById('anim-titulo');
    const elDescricao = document.getElementById('anim-descricao');
    const elPasso = document.getElementById('anim-passo');
    const elVazio = document.getElementById('palco-vazio');
    const elControles = document.getElementById('anim-controles');
    const btnAnterior = document.getElementById('anim-anterior');
    const btnProximo = document.getElementById('anim-proximo');
    const btnRepetir = document.getElementById('anim-repetir');
    const selVelocidade = document.getElementById('anim-velocidade');

    let quadros = [];
    try {
        quadros = JSON.parse(dadosEl.textContent).quadros || [];
    } catch (e) {
        return;
    }
    if (!quadros.length) return;

    function contar(no) { return no ? 1 + contar(no.e) + contar(no.d) : 0; }
    function niveis(no) { return no ? 1 + Math.max(niveis(no.e), niveis(no.d)) : 0; }

    // Tamanho máximo entre todos os quadros (a área não "pula" entre passos)
    let maxNos = 1;
    let maxNiveis = 1;
    quadros.forEach(function (q) {
        maxNos = Math.max(maxNos, contar(q.raiz));
        maxNiveis = Math.max(maxNiveis, niveis(q.raiz));
    });
    const alturaLona = (maxNiveis - 1) * DY + ALT + MARGEM * 2;

    // ------------------------------------------------------------
    // Estado
    // ------------------------------------------------------------
    const estado = { nos: new Map(), calc: null, indice: -1 };
    let execucao = 0;   // invalida transições em andamento
    let sequencia = 0;  // invalida a reprodução automática
    let animando = false;

    // ------------------------------------------------------------
    // Layout: x pela ordem em-ordem, y pela profundidade
    // ------------------------------------------------------------
    function larguraLona() {
        return Math.max(palco.clientWidth, (maxNos - 1) * DX + LARG + MARGEM * 2);
    }

    function ajustarLona() {
        const w = larguraLona();
        lona.style.width = w + 'px';
        lona.style.height = alturaLona + 'px';
        svg.setAttribute('width', w);
        svg.setAttribute('height', alturaLona);
    }

    function calcular(raiz) {
        const n = contar(raiz);
        const larguraConteudo = n > 0 ? (n - 1) * DX + LARG : 0;
        const desloc = (larguraLona() - larguraConteudo) / 2;

        const mapa = new Map();
        const arestas = new Set();
        let i = 0;

        (function visitar(no, prof, pai) {
            if (!no) return;
            visitar(no.e, prof + 1, no);
            mapa.set(no.v, {
                x: desloc + i * DX,
                y: MARGEM + prof * DY,
                h: no.h,
                fb: no.fb,
                pai: pai ? pai.v : null
            });
            i++;
            visitar(no.d, prof + 1, no);
        })(raiz, 0, null);

        mapa.forEach(function (info, v) {
            if (info.pai !== null) arestas.add(info.pai + '>' + v);
        });

        return { mapa: mapa, arestas: arestas };
    }

    // ------------------------------------------------------------
    // Elementos dos nós
    // ------------------------------------------------------------
    function criarNo(v) {
        const el = document.createElement('div');
        el.className = 'no-anim';

        const forte = document.createElement('strong');
        forte.textContent = v;
        const altura = document.createElement('small');
        const fb = document.createElement('small');

        el.append(forte, altura, fb);
        return { el: el, altura: altura, fb: fb, entrando: false, saindo: false };
    }

    function definirTextos(no, info) {
        no.altura.textContent = 'Altura: ' + info.h;
        no.fb.textContent = 'FB: ' + info.fb;
    }

    // ------------------------------------------------------------
    // Interface
    // ------------------------------------------------------------
    function mostrarLegenda(indice, q) {
        elTitulo.textContent = q.titulo || '';
        elDescricao.textContent = q.descricao || '';
        elPasso.textContent = quadros.length > 1
            ? 'Passo ' + (indice + 1) + ' de ' + quadros.length
            : '';
    }

    function atualizarBotoes() {
        btnAnterior.disabled = animando || estado.indice <= 0;
        btnProximo.disabled = animando || estado.indice >= quadros.length - 1;
    }

    function resetar() {
        execucao++;
        animando = false;
        estado.nos.forEach(function (no) { no.el.remove(); });
        estado.nos.clear();
        estado.calc = null;
        estado.indice = -1;
        svg.innerHTML = '';
        ajustarLona();
    }

    // ------------------------------------------------------------
    // Transição entre dois quadros
    // ------------------------------------------------------------
    function irPara(indice, instantaneo) {
        return new Promise(function (resolve) {
            const minhaVez = ++execucao;
            const q = quadros[indice];
            const fim = calcular(q.raiz);
            const ini = estado.calc;
            const arestasIni = ini ? ini.arestas : new Set();
            const desbalanceados = q.desbalanceados || [];
            const dur = (instantaneo || reduzirMovimento)
                ? 0
                : DURACAO * Number(selVelocidade.value);

            animando = true;
            mostrarLegenda(indice, q);
            atualizarBotoes();

            // Posições inicial e final de cada nó
            const todos = new Set([...estado.nos.keys(), ...fim.mapa.keys()]);
            const posIni = new Map();
            const posFim = new Map();

            todos.forEach(function (v) {
                const antes = ini ? ini.mapa.get(v) : undefined;
                const depois = fim.mapa.get(v);
                const a = antes || depois;
                const b = depois || antes;

                posIni.set(v, { x: a.x, y: a.y });
                posFim.set(v, { x: b.x, y: b.y });

                let no = estado.nos.get(v);
                if (!no) {
                    no = criarNo(v);
                    estado.nos.set(v, no);
                    lona.appendChild(no.el);
                    definirTextos(no, depois);
                }

                no.entrando = !antes;
                no.saindo = !depois;

                const moveu = !!(antes && depois && antes.pai !== depois.pai);
                no.el.classList.toggle('novo', q.novo === v);
                no.el.classList.toggle('desbalanceado', desbalanceados.indexOf(v) >= 0);
                no.el.classList.toggle('movido', moveu);
                no.el.classList.toggle('removido', !depois);
            });

            // Arestas (linhas): as de antes e as de depois, com fade
            svg.innerHTML = '';
            const linhas = new Map();
            new Set([...arestasIni, ...fim.arestas]).forEach(function (chave) {
                const ln = document.createElementNS(NS, 'line');
                ln.setAttribute('class', 'aresta');
                svg.appendChild(ln);
                linhas.set(chave, ln);
            });

            function desenhar(e, t) {
                const atual = new Map();

                todos.forEach(function (v) {
                    const a = posIni.get(v);
                    const b = posFim.get(v);
                    const x = a.x + (b.x - a.x) * e;
                    const y = a.y + (b.y - a.y) * e;
                    const no = estado.nos.get(v);

                    atual.set(v, { x: x, y: y });
                    no.el.style.transform = 'translate(' + x + 'px,' + y + 'px)';
                    no.el.style.opacity = no.entrando ? t : (no.saindo ? 1 - t : 1);
                });

                linhas.forEach(function (ln, chave) {
                    const partes = chave.split('>');
                    const p = atual.get(Number(partes[0]));
                    const c = atual.get(Number(partes[1]));
                    if (!p || !c) {
                        ln.style.display = 'none';
                        return;
                    }
                    ln.setAttribute('x1', p.x + LARG / 2);
                    ln.setAttribute('y1', p.y + ALT);
                    ln.setAttribute('x2', c.x + LARG / 2);
                    ln.setAttribute('y2', c.y);

                    const emIni = arestasIni.has(chave);
                    const emFim = fim.arestas.has(chave);
                    ln.style.opacity = (emIni && emFim) ? 1 : (emFim ? t : 1 - t);
                });
            }

            function concluir() {
                Array.from(estado.nos.entries()).forEach(function (par) {
                    const v = par[0];
                    const no = par[1];
                    if (no.saindo) {
                        no.el.remove();
                        estado.nos.delete(v);
                    } else {
                        no.el.style.opacity = 1;
                        definirTextos(no, fim.mapa.get(v));
                    }
                });

                linhas.forEach(function (ln, chave) {
                    if (!fim.arestas.has(chave)) ln.remove();
                    else ln.style.opacity = 1;
                });

                estado.calc = fim;
                estado.indice = indice;
                elVazio.style.display = fim.mapa.size ? 'none' : 'flex';
                animando = false;
                atualizarBotoes();
            }

            desenhar(0, 0);

            if (dur === 0) {
                desenhar(1, 1);
                concluir();
                resolve(true);
                return;
            }

            const t0 = performance.now();
            function passo(agora) {
                if (minhaVez !== execucao) {
                    resolve(false); // cancelada
                    return;
                }
                const t = Math.min(1, (agora - t0) / dur);
                const e = t < 0.5 ? 4 * t * t * t : 1 - Math.pow(-2 * t + 2, 3) / 2;
                desenhar(e, t);

                if (t < 1) {
                    requestAnimationFrame(passo);
                } else {
                    concluir();
                    resolve(true);
                }
            }
            requestAnimationFrame(passo);
        });
    }

    // ------------------------------------------------------------
    // Reprodução automática e controles
    // ------------------------------------------------------------
    function pausa(ms) {
        return new Promise(function (r) { setTimeout(r, ms); });
    }

    async function reproduzir() {
        const token = ++sequencia;
        resetar();
        await irPara(0, true);
        if (quadros.length < 2) return;

        await pausa(700);

        for (let i = 1; i < quadros.length; i++) {
            if (token !== sequencia) return;
            const ok = await irPara(i, false);
            if (!ok || token !== sequencia) return;
            await pausa(800 * Number(selVelocidade.value));
        }
    }

    btnRepetir.addEventListener('click', function () {
        reproduzir();
    });

    btnProximo.addEventListener('click', function () {
        if (animando || estado.indice >= quadros.length - 1) return;
        sequencia++; // interrompe a reprodução automática
        irPara(estado.indice + 1, false);
    });

    btnAnterior.addEventListener('click', function () {
        if (animando || estado.indice <= 0) return;
        sequencia++;
        irPara(estado.indice - 1, false);
    });

    let temporizadorRedim;
    window.addEventListener('resize', function () {
        clearTimeout(temporizadorRedim);
        temporizadorRedim = setTimeout(function () {
            if (animando) return;
            const i = Math.max(estado.indice, 0);
            sequencia++;
            resetar();
            irPara(i, true);
        }, 200);
    });

    // ------------------------------------------------------------
    // Início
    // ------------------------------------------------------------
    if (quadros.length < 2) elControles.style.display = 'none';
    reproduzir();
})();