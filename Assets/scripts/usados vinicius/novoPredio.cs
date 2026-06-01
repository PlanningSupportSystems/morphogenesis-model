using System;
using System.Collections.Generic;
using System.Collections;
//using System.Drawing;

//using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.UI;
using static IsovistaP;

public class novoPredio : MonoBehaviour, ISelecionavel
{
    //    ControleAglomeracao GerenteAmbiente = Terrain.activeTerrain.GetComponent<ControleAglomeracao>();
    ControleAglomeracao GerenteAmbiente;// = GameObject.Find("ambiente").GetComponent<ControleAglomeracao>();
    public int indiceCriacao;
    public int indiceNovoPredio;


    [SerializeField] public string enderecoCelula;
//    [SerializeField] public Vector2Int endCelula = new Vector2Int(0, 0);
    public Celula minhaCelula;
    public OcupacaoCelula usoCelula;
    public TipoEspacoConstruido np_tipo;

    public List<lugar> lugaresPossiveis = new List<lugar>();


    public bool criarVizinhos = false;
    /// <summary>
    /// sistema de celulas para vizinhanca, substituiu os raycast e posicao xyz
    /// 
    /// checar quais precisam ficar. tvz so os dictionary e os list<tipoEspacoConstruido>. 
    /// </summary>
    public Dictionary<Vector2Int, Celula> celulasVonNeumann = new Dictionary<Vector2Int, Celula>();
    public Dictionary<Vector2Int, Celula> celulasVonNeumann_Comp = new Dictionary<Vector2Int, Celula>();
    public Dictionary<Vector2Int, Celula> celulasMoore = new Dictionary<Vector2Int, Celula>();

    public List<Celula> vizinhos_celulas_VN;

    public List<TipoEspacoConstruido> vizinhos_originais_tipos_VN;
    public List<novoPredio> vizinhos_originais_predio_VN;
    /*
    public List<novoPredio> vizinhos_originais_rua_VN;
    public List<lugar> vizinhos_originais_lugar_VN;

    public List<TipoEspacoConstruido> vizinhos_originais_tipos_VNC;
    public List<novoPredio> vizinhos_originais_predio_VNC;
    public List<novoPredio> vizinhos_originais_rua_VNC;
    public List<lugar> vizinhos_originais_lugar_VNC;
    */

    //propriedades relativas interface ISelecionavel
    public bool EstaSelecionado { get; set; }
//    public bool souVizinhoSelecionado{ get; set; }
//    public Color corOriginal;
    IsovistaP _iso_display;
//    public static bool click = false; //FAZER ALGO com os enderecoCelular, analisar

    public Renderer np_meuRenderer;

    bool debug_novoPredio = false;
    bool checagem_vizinhos_trancados = false;
    bool checagem_vizinhos_quina = false;


    public static Vector3 np_half = default;
    public string np_nome;
    public Vector3 np_endereco = default;
    private lugar lugartemp;

    ValoresReferenciaNormalizacao referencia_normalizacao;
    bool _isovista_calculada;

    /// <summary>
    /// ////////daki pra baixo td pode sair. substituido pelo sistema de celulas.
    /// revisar, por la, quais precisam ficar.
    /// </summary>
    /*
    public int np_saldoVizinhos;
    public List<novoPredio> np_meus_vizinhos_predio;
    public List<novoPredio> np_meus_vizinhos_rua;
    public List<lugar> np_meus_vizinhos_lugar;

    public List<novoPredio> np_meus_vizinhos_click_predio;
    public List<novoPredio> np_meus_vizinhos_click_rua;
    public List<lugar> np_meus_vizinhos_click_lugar;

    public List<novoPredio> np_click_quinas;
    public List<novoPredio> np_click_vizinhanca_quina;
    public List<novoPredio> np_click_vizinhanca_quina_orto;
    */
    //revisar para classificar de ortogonais(von neuman) ou diagonais (moore)

    public delegate void MetodoEscolha();
    MetodoEscolha metodo_escolha;

    public SO_EspacoConstruido tipoEspaco;


    public void atribuirDelegate(string nomeMetodo_T)
    {
        if (string.IsNullOrEmpty(nomeMetodo_T))
        {
            Debug.LogWarning("atribuirDelegate: nome do método vazio");
            metodo_escolha = null;
            return;
        }

        MethodInfo methodInfo = this.GetType().GetMethod(nomeMetodo_T, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        if (methodInfo == null)
        {
            Debug.LogError($"atribuirDelegate: Método '{nomeMetodo_T}' não encontrado em {this.GetType().Name}");
            metodo_escolha = null;
            return;
        }

        try //if (methodInfo != null)
        {
            // Cria o delegate a partir do MethodInfo
            metodo_escolha = (MetodoEscolha)Delegate.CreateDelegate(typeof(MetodoEscolha), this, methodInfo);
            return;
        }
        catch (ArgumentException ex)
        {
            Debug.LogError($"atribuirDelegate: assinatura incompatível para '{nomeMetodo_T}': {ex.Message}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"atribuirDelegate: erro criando delegate para '{nomeMetodo_T}': {ex.GetType().Name}: {ex.Message}");
        }

        metodo_escolha = null;
        return;
    }

    public void Inicializar(ControleAglomeracao gerente)
    {
        GerenteAmbiente = gerente;
        indiceCriacao = GerenteAmbiente.ContadorRodadas;

        _isovista_calculada = false;

        np_half = GerenteAmbiente.espacoConstruido.transform.localScale / 2.1f;

        np_meuRenderer = gameObject.GetComponent<Renderer>();

        EstaSelecionado = false;

        lugaresPossiveis ??= new List<lugar>();
        celulasVonNeumann ??= new Dictionary<Vector2Int, Celula>();
        vizinhos_celulas_VN ??= new List<Celula>();
    }

    public void Start()
    {
    }
    public IEnumerator ExecutarGeracao()
    {
        if (InputsMorfo.tracking)
        {
            Debug.Log("tracking novoPredio");
        }

        atribuirDelegate(GerenteAmbiente?.Tipo_Localizacao);
        if (metodo_escolha != null)
        {
            try { metodo_escolha(); }
            catch (Exception ex)
            { Debug.LogError($"metodo_escolha invocação falhou: {ex.GetType().Name}: {ex.Message}"); }
        }
        else
        { Debug.LogWarning("metodo_escolha não atribuído em novoPredio.Start()"); }

        yield return StartCoroutine(GravarImagens(" antes "));
        //        NP_ChecarSeEhRua();
        TipoEspacoConstruido tipoTemp = DecidirEstadoCelula();
        AplicarEstadoCelula(tipoTemp);

        //////////USO DAS CELULAS PARA CHECAR VIZINHOS
        //        celulasVonNeumann = new Dictionary<Vector2Int, Celula>();  //foi para inicializar()
        //        vizinhos_celulas_VN = new List<Celula>();                  //foi para inicializar()
        //        Legacy_NP_Celulas_PegarVizinhas(minhaCelula, celulasVonNeumann, Celula.offsetsVonNeumann, criarVizinhos);//, enderecos_da_vizinhanca);
        criarVizinhos = true; //virou parametro base
//        NP_iterarVizinhanca(minhaCelula, Celula.offsetsVonNeumann, Action_AtualizaVizinhanca);
        NP_iterarVizinhanca(lugartemp.minhaCelula, Celula.offsetsVonNeumann, Action_AtualizaVizinhanca);

//        yield return StartCoroutine(GravarImagens(" depois "));

        NP_CalcularIsovista();

        //        celulasVonNeumann_Comp = new Dictionary<Vector2Int, Celula>();
        //        celulasMoore = new Dictionary<Vector2Int, Celula>();
        //        Legacy_NP_Celulas_PegarVizinhas(minhaCelula, celulasVonNeumann_Comp, Celula.offsetsVonNeumannComplemento, criarVizinhos);//, enderecos_da_vizinhanca_VNcomp);
        //        Legacy_NP_Celulas_PegarVizinhas(minhaCelula, celulasMoore, Celula.offsetsMoore, criarVizinhos);//, enderecos_da_vizinhanca_Moore);
        //        Legacy_NP_GuardarVizinhosOriginais();
        criarVizinhos = false;
        vizinhos_originais_predio_VN.Clear();
//        NP_iterarVizinhanca(minhaCelula, Celula.offsetsVonNeumann, Action_GuardarVizinhosOriginais);
        NP_iterarVizinhanca(minhaCelula, Celula.offsetsVonNeumann, Action_GuardarVizinhosOriginais);

        debug_novoPredio = false;
        if (debug_novoPredio)
        {
            Debug.Log(celulasVonNeumann == null
                ? "dictionary = NULL"
                : $"dictionary OK | count_todos = {celulasVonNeumann.Count}");
            Debug.Log(celulasVonNeumann_Comp == null
                ? "dictionary = NULL"
                : $"dictionary OK | count_todos = {celulasVonNeumann_Comp.Count}");
            Debug.Log(celulasMoore == null
                ? "dictionary = NULL"
                : $"dictionary OK | count_todos = {celulasMoore.Count}");

                string texto = "celulas von nueman: " + celulasVonNeumann.Count + "\n";
                foreach (var kvp in celulasVonNeumann)
                {
                    texto += $"{kvp.Key} -> {kvp.Value} : {kvp.Value.novoPredio} : {kvp.Value.lugar}\n";
                }
                Debug.Log(texto);

                texto = "celulas von nueman complementar: " + celulasVonNeumann_Comp.Count + "\n";
                foreach (var kvp in celulasVonNeumann_Comp)
                {
                    texto += $"{kvp.Key} -> {kvp.Value} : {kvp.Value.novoPredio} : {kvp.Value.lugar}\n";
                }
                Debug.Log(texto);

                texto = "celulas moore: " + celulasMoore.Count + "\n";
                foreach (var kvp in celulasMoore)
                {
                    texto += $"{kvp.Key} -> {kvp.Value} : {kvp.Value.novoPredio} : {kvp.Value.lugar}\n";
                }
                Debug.Log(texto);
        }
        debug_novoPredio = false;

        //////////USO DE POSICAO VECTOR3 E RAYCAST PARA CHECAR VIZINHOS
//        NP_MeusVizinhos();
//        Legacy_NP_ChecaVizinhoTrancado();
//        NP_ChecarSeEhRua();

        //colocado para, dps de todos os checks, tentar preservar a maior isovista.
//        if (InputsMorfo.boolModoPreservaIso) NP_PreservarMaiorIsovista();
    }

    void isoplace()
    {
        //        NP_CalcularIsovista();

        foreach (var l in GerenteAmbiente.lugaresAtivos)
        {
            Debug.Log($"{l._nome} media={l.medida_geral_ponderada:R}");
        }

        float toleranciaMediaPonderada = 0.001f;

        float ref_medida_geral_ponderada = GerenteAmbiente.lugaresAtivos
            .Max(casa => casa.medida_geral_ponderada);

        lugaresPossiveis = GerenteAmbiente.lugaresAtivos
            .Where(casa => Mathf.Abs(casa.medida_geral_ponderada - ref_medida_geral_ponderada) <= toleranciaMediaPonderada)
            .ToList();

        //        lugaresPossiveis = GerenteAmbiente.lugaresAtivos.Where(casa => casa.medida_geral_ponderada == ref_medida_geral_ponderada).ToList();
        if (lugaresPossiveis == null || lugaresPossiveis.Count == 0) return;    // if (lista_medida_geral_ponderada == null || lista_medida_geral_ponderada.Count == 0) return;

        aleatorio();
    }

    void aleatorio()
    {
        if (GerenteAmbiente.lugaresAtivos == null || GerenteAmbiente.lugaresAtivos.Count == 0)
            return;

        if (lugaresPossiveis == null || lugaresPossiveis.Count == 0)
        {
            Debug.LogWarning("aleatorio: lugaresPossiveis vazio ou nulo. usando GerenteAmbiente.lugaresAtivos");
            lugaresPossiveis = new List<lugar>(GerenteAmbiente.lugaresAtivos);
        }

//        LugaresCorNormal(GerenteAmbiente.ContadorRodadas);
        LugaresCorElegivel(GerenteAmbiente.ContadorRodadas);

        int index = UnityEngine.Random.Range(0, lugaresPossiveis.Count);
        lugartemp = lugaresPossiveis[index];

        if (lugartemp == null || lugartemp.minhaCelula == null)
        {
            Debug.LogWarning("aleatorio: lugar inválido sorteado.");
            return;
        }

        if (lugartemp.minhaCelula.novoPredio != null)
        {
            Debug.LogWarning("aleatorio: célula já ocupada (estado inconsistente).");
            return;
        }
        //        aceitarcelula();
    }

    void LugaresCorNormal(int rodadaVisual)
    {
        foreach (lugar l in GerenteAmbiente.lugaresAtivos)
        {
            l.estadoSelecao = lugar.EstadoCorLugar.Normal;
            l.AplicarCorTempo(rodadaVisual);
        }

    }

    void LugaresCorElegivel(int rodadaVisual)
    {
        foreach (lugar l in lugaresPossiveis)
        {
            l.rodadasElegivel.Add(rodadaVisual);
            l.estadoSelecao = lugar.EstadoCorLugar.Elegivel;
            l.AplicarCorTempo(rodadaVisual);
            //            l.AtualizarCor();
            Debug.Log($"REG ELEGIVEL rodada {rodadaVisual}: {l._nome}");

        }

    }

    IEnumerator GravarImagens(string antesdps)
    {
        if (InputsMorfo.boolGravarImagens)
        {
            yield return StartCoroutine(GerenteAmbiente.salvarImagens.FotoTela("zena" + antesdps + GerenteAmbiente.ContadorRodadas));
        }

    }

    public enum OcupacaoCelula
    {
        predio,
        rua,
        bloqueado
    }

    public TipoEspacoConstruido DecidirEstadoCelula()
    {
        bool trancado = Action_ChecaVizinhoTrancado(lugartemp.minhaCelula);
        TipoEspacoConstruido usoTeste = trancado ? TipoEspacoConstruido.Rua : TipoEspacoConstruido.Predio;
        //checa se tranca sendo predio.
        //se trancado, deve retornar rua,
        //se nao, retorna predio

        bool temQuina = Action_ChecaQuina(lugartemp.minhaCelula, usoTeste); 
        //usando rua ou predio do trancado
        //se voltar q tem quina, deve ser bloqueado
        //se nao tiver quina, retorna ok para a ocupacao q tinha sido sugerida

        bool rua_por_maior_isovista = false;
        if (InputsMorfo.boolModoPreservaIso)
        {
            if (_isovista_calculada == false) NP_CalcularIsovista();
            rua_por_maior_isovista = (lugartemp.iso.medidasNormalizadas.distanciaMaxima == 1); ///pode ser substituido por uma comparacao medidasnormalizadas.maxdist ==1
            rua_por_maior_isovista = NP_PreservarMaiorIsovista(); ///pode ser substituido por uma comparacao medidasnormalizadas.maxdist ==1
            Debug.Log("maior isovista: ");// + lugartemp.distanciaMaxima + "valor referencia: " + referencia_normalizacao.distanciaMaxima_Max);
        }

        bool profundidade = false;
        if (InputsMorfo.boolModoPreservaProfundidade)
        {
            if (_isovista_calculada == false) NP_CalcularIsovista();
            profundidade = (lugartemp.iso.medidasNormalizadas.ProfundidadeRua == 1);
            Debug.Log("profundidade rua: ");// + lugartemp.total_profundidade_Rua + "valor referencia: " + referencia_normalizacao.ProfundidadeRua_Min);
        }
        if (_isovista_calculada && InputsMorfo.boolModoPreservaProfundidade || InputsMorfo.boolModoPreservaIso)
        {
            Debug.Log(referencia_normalizacao.Publicar());
            Debug.Log(lugartemp.iso.medidasBrutas.Publicar());
            Debug.Log(lugartemp.iso.medidasNormalizadas.Publicar());
        }

        if (profundidade || rua_por_maior_isovista)
            usoTeste = TipoEspacoConstruido.Rua;

        usoTeste = temQuina ? TipoEspacoConstruido.Bloqueado : usoTeste;

        return usoTeste;
    }

    public void AplicarEstadoCelula(TipoEspacoConstruido usoTeste)
    {
        switch (usoTeste)
        {
            case TipoEspacoConstruido.Predio:
                tipoEspaco = GerenteAmbiente.PegarSO(TipoEspacoConstruido.Predio);// ._so_construir[0];
                indiceNovoPredio = GerenteAmbiente.Geral_novosPrediosConstruidos.Count();
                np_nome = tipoEspaco.nome + " " + indiceCriacao.ToString() + "_" + indiceNovoPredio.ToString();

                if (!GerenteAmbiente.Geral_novosPrediosConstruidos.Contains(this))
                    GerenteAmbiente.Geral_novosPrediosConstruidos.Add(this);
                break;

            case TipoEspacoConstruido.Rua:
                tipoEspaco = GerenteAmbiente.PegarSO(TipoEspacoConstruido.Rua);// ._so_construir[0];
                indiceNovoPredio = GerenteAmbiente.Geral_novosPrediosRuas.Count();
                np_nome = tipoEspaco.nome + " " + indiceCriacao.ToString() + "_" + indiceNovoPredio.ToString();

                if (!GerenteAmbiente.Geral_novosPrediosRuas.Contains(this))
                    GerenteAmbiente.Geral_novosPrediosRuas.Add(this);

                break;
            
            case TipoEspacoConstruido.Bloqueado:
                lugartemp.indiceBloqueio = GerenteAmbiente.ContadorRodadas;
                lugartemp.Bloquear();
                lugartemp.AtualizarCor();
                Debug.LogWarning("bloqueado");
                break;
        }
        Debug.LogWarning("bloqueado fora do case");


        aceitarcelula(usoTeste);
        if (usoTeste == TipoEspacoConstruido.Bloqueado)
            return;

        this.name = np_nome;
        np_tipo = tipoEspaco.tipo;

//        if (np_tipo == TipoEspacoConstruido.Rua)
//            debug_novoPredio = true;
//        if (debug_novoPredio) Debug.Log("pos checar se eh rua valor de trancado: " + rua + " nome obj: " + np_nome);
//        debug_novoPredio = false;

        gameObject.transform.localScale = tipoEspaco.escala;
        var rend = gameObject.GetComponent<Renderer>();
        if (rend != null)
        {
            rend.material.color = tipoEspaco.cor;
        }
        gameObject.layer = LayerMask.NameToLayer(tipoEspaco.layer);

        if (!GerenteAmbiente.Geral_novosPrediosTotal.Contains(this))
            GerenteAmbiente.Geral_novosPrediosTotal.Add(this);

        LugaresCorNormal(GerenteAmbiente.ContadorRodadas + 1);
    }

    void aceitarcelula(TipoEspacoConstruido usoTeste)
    {
        if (usoTeste == TipoEspacoConstruido.Bloqueado)
        {
            Debug.LogWarning("aceitarcelula: usoTeste é Bloqueado, trancado e quina estão verdadeiros. impossivel ocupar.");
            seDestruir();
            return;
        }

        int rodadaVisual = GerenteAmbiente.ContadorRodadas;

        lugartemp.rodadasEleito.Add(rodadaVisual);
        lugartemp.estadoSelecao = lugar.EstadoCorLugar.Eleito;
        lugartemp.AtualizarCor();

        np_endereco = lugartemp._endereco;

        setMinhaCelula(lugartemp.minhaCelula);
        minhaCelula.addnovoPredio(this);

        transform.position = np_endereco;
    }

    public bool NP_PreservarMaiorIsovista()
    {
        //PRESERVA QUANDO FOR A SELECAO FOR EQUIVALENTE A MAIOR RUA. DAI ELA SE MANTEM COMO RUA
        //EH UM EXTRA, SER RUA DE MODO A NAO TRANCAR OS PREDIOS, E MANTER QD FOR A MAIOR.
        //        Debug.Log("maior distancia desse lugar: " + lugartemp.distanciaMaxima);
        //        Debug.Log("maior distancia da rodada de medida: " + GerenteAmbiente.lugaresAtivos.Max(casa => casa.distanciaMaxima));

        // defesa: garantir que temos dados para comparar
        if (GerenteAmbiente == null)
        {
            if (debug_novoPredio) Debug.LogWarning("NP_PreservarMaiorIsovista: GerenteAmbiente == null para " + np_nome);
            return false;
        }

        if (GerenteAmbiente.lugaresAtivos == null || GerenteAmbiente.lugaresAtivos.Count == 0)
        {
            if (debug_novoPredio) Debug.LogWarning("NP_PreservarMaiorIsovista: lugaresAtivos vazio para " + np_nome);
            return false;
        }

        if (lugartemp == null)
        {
            if (debug_novoPredio) Debug.LogWarning("NP_PreservarMaiorIsovista: lugartemp == null para " + np_nome);
            return false;
        }

        if (lugartemp.iso == null)
        {
            if (debug_novoPredio) Debug.LogWarning("NP_PreservarMaiorIsovista: lugartemp sem iso valida para " + np_nome);
            return false;
        }

        // proteger contra elementos nulos e lugares ainda sem isovista calculada
        float maxDistancia = GerenteAmbiente.lugaresAtivos
            .Where(c => c != null)
            .Where(c => c.gameObject.activeSelf)
            .Where(c => c.iso != null)
            .Select(c => c.iso.medidasBrutas.distanciaMaxima)
            .DefaultIfEmpty(float.MinValue)
            .Max();

        if (debug_novoPredio) Debug.Log($"NP_PreservarMaiorIsovista: lugartemp.distanciaMaxima={lugartemp.iso.medidasBrutas.distanciaMaxima}, maxDist={maxDistancia}");
      
        if (Mathf.Approximately(lugartemp.iso.medidasBrutas.distanciaMaxima, maxDistancia))
        {
            if (debug_novoPredio) Debug.Log("vai ser rua pra preservar a maior isovista");
            return true; // eh a maior distancia vista, preserva como rua
        }
        return false;

    }

    public void setMinhaCelula(Celula c)
    {
        minhaCelula = c;
        enderecoCelula = c.endereco.ToString();
//        Debug.Log("NOVO_PREDIO: predio " + this.name + " recebeu celula " + enderecoCelula);
        if (minhaCelula.lugar == null) Debug.LogWarning("celula sem LUGAR");
        if (minhaCelula.lugar != null) Debug.LogWarning("celula com LUGAR: " + minhaCelula.lugar._nome);
        if (minhaCelula.novoPredio == null) Debug.LogWarning("celula sem NOVO_PREDIO");
        if (minhaCelula.novoPredio != null) Debug.LogWarning("celula com NOVO_PREDIO: " + minhaCelula.novoPredio.np_nome);
    }
//    public void NP_CriarCelulas(Vector2Int[] enderecosCelulas, Vector3[] enderecosMundo)
        
    public void NP_ColocarLugarNaCelula(Celula c)
    {
            if (c.novoPredio == null && c.lugar == null)
            {
                GameObject go = Instantiate(GerenteAmbiente.vizinhoPossivel, c.posicaoMundo, Quaternion.identity);
                lugar porta_lugar = go.GetComponent<lugar>();
                porta_lugar.Inicializar(GerenteAmbiente);
                porta_lugar.addCelula(c);
                c.addLugar(porta_lugar);// cel_vizinha.lugar = porta_lugar;
            }
    }


    public void NP_CalcularIsovista()
    {
        if (GerenteAmbiente.lugaresAtivos == null || GerenteAmbiente.lugaresAtivos.Count == 0)
        {
            Debug.LogWarning("Nao ha lugares candidatos para avaliar.");
            return;
        }
        //seta a layer onde vai fazer as medidas de isovista, pegar so predios e ruas, e nao pegar lugares
//        int _templayer = (1 << LayerMask.NameToLayer("layer_predios"));
        LayerMask _templayer = LayerMask.GetMask("layer_predios", "layer_ruas");//, "layer_lugares");


        /// ===== PRIMEIRO RODADA ===== 
        ///busca quais lugares foram afetados pelo novoPredio. 
        ///faz L_CALCULEISOVISTAS apenas para eles. 
        ///referencia_normalizacao 
        ///
        
        LayerMask bloqueadores = LayerMask.GetMask("layer_predios");
        LayerMask transparentes = LayerMask.GetMask("layer_lugares", "layer_ruas");

        Vector3 pos_mundo = new Vector3();
        if (minhaCelula.posicaoMundo == null)
        {
            pos_mundo = lugartemp.minhaCelula.posicaoMundo;
        }
        else if (minhaCelula.posicaoMundo != null)
        {
            pos_mundo = minhaCelula.posicaoMundo;
        }

        IsovistaP iso_temp = new IsovistaP(pos_mundo);
        List<ResultadoRaioVisao> resultados = iso_temp.VarrerCampoVisao(
            minhaCelula.posicaoMundo,
            InputsMorfo.input_distanciaCampoVisao,
            CalcularQtdRaios(),
            LayerMask.GetMask("layer_predios"),
            LayerMask.GetMask("layer_lugares", "layer_ruas")
        );

//        HashSet<lugar> lugaresVisiveis = new HashSet<lugar>(iso_temp.LerLugaresVisiveis(resultados));
        HashSet<lugar> lugaresVisiveis = new HashSet<lugar>();

        foreach (Vector3 origem in PegarPontosVisibilidadeNovoPredio())
        {
            IsovistaP iso_temp2 = new IsovistaP(origem);

            List<ResultadoRaioVisao> resultados2 = iso_temp2.VarrerCampoVisao(
                origem,
                InputsMorfo.input_distanciaCampoVisao,
                CalcularQtdRaios(),
                LayerMask.GetMask("layer_predios"),
                LayerMask.GetMask("layer_lugares", "layer_ruas")
            );

            foreach (lugar l in iso_temp2.LerLugaresVisiveis(resultados2))
            {
                if (l != null)
                    lugaresVisiveis.Add(l);
            }
        }

        //        foreach (var l in lugaresVisiveis)
        //        {
        //            NP_iterarVizinhanca(l.minhaCelula, Celula.offsetsMoore, Action_AtualizaVizinhanca);
        //        }

        
        foreach (var lugar in lugaresVisiveis)
        {
            if (lugar != null)
            {
                lugar.L_CalculeIsovistas(_templayer);
            }
        }
      

        foreach (var lugarAtivo in GerenteAmbiente.lugaresAtivos)
        {
            if (lugarAtivo == null) continue;
            if (!lugarAtivo.gameObject.activeSelf) continue;

            if (lugarAtivo.iso == null || lugarAtivo.indiceCriacao == GerenteAmbiente.ContadorRodadas)
                lugarAtivo.L_CalculeIsovistas(_templayer);

        }

        List<lugar> lugaresAtivosComIso = GerenteAmbiente.lugaresAtivos
            .Where(l => l != null)
            .Where(l => l.gameObject.activeSelf)
            .Where(l => l.iso != null)
            .ToList();

        if (lugaresAtivosComIso.Count == 0)
        {
            Debug.LogWarning("Nao ha lugares ativos com isovista valida para normalizar.");
            return;
        }
        

        // ===== RESTANTE DA PRIMEIRA VARREDURA =====
        //        referencia_normalizacao = Normalizador.BuscarReferenciaNormalizacao(GerenteAmbiente.lugaresAtivos, _templayer);
        referencia_normalizacao = Normalizador.BuscarReferenciaNormalizacao(lugaresAtivosComIso, _templayer);

        foreach (var l in GerenteAmbiente.lugaresAtivos)
        {
            Debug.Log($"ANTES NORMALIZAR rodada {GerenteAmbiente.ContadorRodadas}: {l._nome} ativo={l.gameObject.activeSelf} iso={l.iso != null} media={l.medida_geral_ponderada}");
        }

        foreach (var l in lugaresAtivosComIso)
        {
            Debug.Log($"VAI NORMALIZAR rodada {GerenteAmbiente.ContadorRodadas}: {l._nome} , max: {l.iso.medidasBrutas.distanciaMaxima}");
        }

        // ===== SEGUNDA VARREDURA: NORMALIZAR =====
        Normalizador.NomalizarLista(lugaresAtivosComIso, referencia_normalizacao);

        _isovista_calculada = true;

    }
    


    int CalcularQtdRaios()
    {
        float raioVisao = InputsMorfo.input_distanciaCampoVisao;
        Vector3 tamanho_EspacoConstruido_Vector;

        Renderer rend = GerenteAmbiente.espacoConstruido.GetComponent<Renderer>();
        if (rend == null)
        {
            Debug.LogWarning("espacoConstruido sem Renderer! Usando padrão.");
            tamanho_EspacoConstruido_Vector = new Vector3(1f, 1f, 1f);
        }
        else
        {
            // CORREÇÃO: Só atribui se rend não for null
            tamanho_EspacoConstruido_Vector = rend.bounds.size;
        }
        float tamanhoCelula = Mathf.Min(tamanho_EspacoConstruido_Vector.x, tamanho_EspacoConstruido_Vector.z);

        float espacamentoDesejado = tamanhoCelula;// * 0.5f;
        int qtd = Mathf.CeilToInt((2f * Mathf.PI * raioVisao) / espacamentoDesejado);

        debug_novoPredio = true;
        if (debug_novoPredio)
        {
            Debug.Log($"NOVOPREDIO: CalcularQtdRaios: raioVisao={raioVisao:F2}, " +
                      $"tamanhoVec={tamanho_EspacoConstruido_Vector}, " +
                      $"tamanhoCelula={tamanhoCelula:F2}, " +
                      $"espacamentoDesejado={espacamentoDesejado:F2}, " +
                      $"qtdCalculada={qtd}, qtdClampada={Mathf.Clamp(qtd, 36, 720)}");
        }
        debug_novoPredio = false;

        return Mathf.Clamp(qtd, 36, 720);
    }


    private List<Vector3> PegarPontosVisibilidadeNovoPredio()
    {
        List<Vector3> pontos = new List<Vector3>();

        Renderer rend = GetComponent<Renderer>();

        if (rend != null)
        {
            Bounds b = rend.bounds;

            float y = minhaCelula.posicaoMundo.y;

            pontos.Add(new Vector3(b.min.x, y, b.min.z));
            pontos.Add(new Vector3(b.min.x, y, b.max.z));
            pontos.Add(new Vector3(b.max.x, y, b.min.z));
            pontos.Add(new Vector3(b.max.x, y, b.max.z));

            return pontos;
        }

        // fallback: usa tamanho da célula a partir do prefab espacoConstruido
        Renderer rendCelula = GerenteAmbiente.espacoConstruido.GetComponent<Renderer>();

        float tamanhoCelula = 1f;

        if (rendCelula != null)
        {
            Vector3 tam = rendCelula.bounds.size;
            tamanhoCelula = Mathf.Min(tam.x, tam.z);
        }

        Vector3 c = minhaCelula.posicaoMundo;
        float h = tamanhoCelula * 0.5f;

        pontos.Add(c + new Vector3(-h, 0f, -h));
        pontos.Add(c + new Vector3(-h, 0f, h));
        pontos.Add(c + new Vector3(h, 0f, -h));
        pontos.Add(c + new Vector3(h, 0f, h));

        return pontos;
    }




    public struct DebugVizinhoQuina
    {
        public Vector2Int enderecoDiagonal;
        public string nomeDiagonal;
        public TipoEspacoConstruido tipoDiagonal;

        public Vector2Int orto1;
        public string nomeOrto1;
        public TipoEspacoConstruido? tipoOrto1;

        public Vector2Int orto2;
        public string nomeOrto2;
        public TipoEspacoConstruido? tipoOrto2;

        public bool orto1EhRua;
        public bool orto2EhRua;

        public string conclusao;
    }

    private void NP_Debug_LogRelatorio(List<DebugVizinhoQuina> dados, bool resultadoFinal)
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();

        sb.AppendLine("===== Legacy_NP_ChecaVizinhoQuina =====");
        sb.AppendLine($"Objeto: {gameObject.name}");
        sb.AppendLine($"Endereco: {minhaCelula?.endereco}");
        sb.AppendLine("--------------------------------");

        foreach (var d in dados)
        {
            sb.AppendLine($"Diagonal: {d.nomeDiagonal} @ {d.enderecoDiagonal} ({d.tipoDiagonal})");

            sb.AppendLine($"  Orto1: {d.orto1} -> {d.nomeOrto1} ({d.tipoOrto1}) | ehPredio: {d.orto1EhRua}");
            sb.AppendLine($"  Orto2: {d.orto2} -> {d.nomeOrto2} ({d.tipoOrto2}) | ehPredio: {d.orto2EhRua}");

            sb.AppendLine($"  => {d.conclusao}");
            sb.AppendLine("--------------------------------");
        }

        sb.AppendLine($"RESULTADO FINAL: {resultadoFinal}");

        if (debug_novoPredio) Debug.Log(sb.ToString());
    }

  


    public void Select()
    {
        //Debug.Log("novoPredio clicado: " + this.name +", estado: "+ EstaSelecionado);

        EstaSelecionado = true;
        np_meuRenderer.material.color = tipoEspaco.cor_selecao; //Legacy_AtualizaCor();
        
        InputsMorfo.IM_propriedades_E_C.SetActive(true); // _propriedades_E_C.SetActive(true);
        InputsMorfo.IM_propriedades_L.SetActive(false); ;// _propriedades_L.SetActive(false);

//        if (celulasMoore == null) Debug.Log("dictionary Moore null");
//        Legacy_AtualizaCor(celulasMoore, true);
        NP_iterarVizinhanca(celula_T: minhaCelula, offset_T: Celula.offsetsMoore, operacao_T: Action_AtualizaCor);

        InputsMorfo.IM_obj_nome.text = np_nome; // IM_nome_obj.text = np_nome; 

        ///contagem de qd criado
        int count_original_todos = vizinhos_originais_tipos_VN.Count();
        int count_original_predios = vizinhos_originais_tipos_VN.Count(p =>
            p == TipoEspacoConstruido.Predio);

        int count_original_ruas = vizinhos_originais_tipos_VN.Count(p =>
            p == TipoEspacoConstruido.Rua);

        // .Count(cel_vizinha => cel_vizinha.novoPredio is TipoEspacoConstruido.Predio);
        InputsMorfo.IM_predios_texto_VizinhosInicial_Total.text = count_original_todos.ToString();// np_meus_vizinhos_predio.Count.ToString();
        InputsMorfo.IM_predios_texto_VizinhosInicial_Predios.text = count_original_predios.ToString();// np_meus_vizinhos_predio.Count(en => en.np_nome.Contains("predio")).ToString();
        InputsMorfo.IM_predios_texto_VizinhosInicial_Ruas.text = count_original_ruas.ToString();// np_meus_vizinhos_rua.Count(en => en.np_nome.Contains("rua")).ToString();

        ///enderecoCelular qd clickado
//        int count_todos = minhaCelula.vizinhosVN.Count(t => t != null && t.novoPredio != null);
        int count_todos = Celula.offsetsVonNeumann.Count
            (offset => minhaCelula.vizinhosPorOffset.TryGetValue(offset, out Celula c) 
            && c.novoPredio != null);
        //        int count_predios = minhaCelula.vizinhosVN.Count
        //            (p => p != null
        //             && p.novoPredio != null
        //             && p.novoPredio.np_tipo == TipoEspacoConstruido.Predio);
        int count_predios = Celula.offsetsVonNeumann.Count
            (offset => minhaCelula.vizinhosPorOffset.TryGetValue(offset, out Celula c)
             && c.novoPredio != null
             && c.novoPredio.np_tipo == TipoEspacoConstruido.Predio);

//        int count_ruas = minhaCelula.vizinhosVN.Count(p =>
//            p?.novoPredio?.np_tipo == TipoEspacoConstruido.Rua);
        int count_ruas = Celula.offsetsVonNeumann.Count
            (offset => minhaCelula.vizinhosPorOffset.TryGetValue(offset, out Celula c)
             && c?.novoPredio?.np_tipo == TipoEspacoConstruido.Rua);

        InputsMorfo.IM_predios_texto_VizinhosClick_Total.text = count_todos.ToString();// np_meus_vizinhos_click_predio.Count.ToString();
        InputsMorfo.IM_predios_texto_VizinhosClick_Predios.text = count_predios.ToString();// np_meus_vizinhos_click_predio.Count(en => en.np_nome.Contains("predio")).ToString();
        InputsMorfo.IM_predios_texto_VizinhosClick_Ruas.text = count_ruas.ToString();// np_meus_vizinhos_click_rua.Count(en => en.np_nome.Contains("rua")).ToString();

        LayerMask _templayer = LayerMask.GetMask("layer_predios", "layer_ruas");//, "layer_lugares");
//        int _templayer = (1 << LayerMask.NameToLayer("layer_predios"))
//                       | (1 << LayerMask.NameToLayer("layer_ruas"))
//                       //                         | (1 << LayerMask.NameToLayer("layer_lugares"))
//                       ;

        Debug.Log("click campo visao qual a layer: " + _templayer.ToString());
        Debug.Log($"meu tipo: {np_tipo}, trancado: {checagem_vizinhos_trancados}, quina: {checagem_vizinhos_quina}");

        _iso_display = new IsovistaP(np_endereco, 360, InputsMorfo.input_distanciaCampoVisao, _templayer);
        _iso_display.CampoVisao(360, InputsMorfo.input_distanciaCampoVisao);
        _iso_display.isoMesh(_iso_display.pontosContorno, np_nome + "mesh");

    }

    public void Deselect()
    {
        EstaSelecionado = false;
        np_meuRenderer.material.color = tipoEspaco.cor;// Legacy_AtualizaCor();
        //Debug.Log("novoPredio clicado: " + this.name + "estado: " + EstaSelecionado);

        InputsMorfo.IM_obj_nome.text = "no selection";
        
        InputsMorfo.IM_propriedades_E_C.SetActive(false); // _propriedades_E_C.SetActive(false);
        InputsMorfo.IM_propriedades_L.SetActive(false); ;// _propriedades_L.SetActive(false);

        // destruir iso mesh se existir
        if (_iso_display != null)
        {
            _iso_display.destruirMesh();
            _iso_display = null;
        }

//        Legacy_AtualizaCor(celulasMoore, false);
        NP_iterarVizinhanca(celula_T: minhaCelula, offset_T: Celula.offsetsMoore, operacao_T: Action_AtualizaCor);

    }

    public void seDestruir()
    {
        if (GerenteAmbiente == null) GerenteAmbiente = GameObject.Find("ambiente").GetComponent<ControleAglomeracao>();

        // Remover de todas as listas onde possa estar registrado
        GerenteAmbiente.Geral_novosPrediosConstruidos?.Remove(this);
        GerenteAmbiente.Geral_novosPrediosRuas?.Remove(this);
        GerenteAmbiente.Geral_novosPrediosTotal?.Remove(this);

        // Finalmente destruir o GameObject
        Destroy(this.gameObject);
    }

    public void AtivarTempo(bool ativoNesseTempo)
    {
        gameObject.SetActive(ativoNesseTempo);
    }


    public void NP_iterarVizinhanca
    (Celula celula_T,
     Vector2Int[] offset_T,
     System.Action<Celula, Celula, Vector2Int> operacao_T)
    {
        //exemplo de iteracao, pode ser adaptado para outros usos
        foreach (var offset_t in offset_T)
        {
            GerenteAmbiente.livroCelulas.TryGetValue(
                celula_T.endereco + offset_t,
                out Celula vizinha_t
            );

            operacao_T(celula_T, vizinha_t, offset_t);
        }
    }

    public void Action_AtualizaVizinhanca(Celula celulaBase_T, Celula vizinha_T, Vector2Int offset_T)
    {
        ///atualiza lista de celulas vizinhas. se criar = true, cria as q faltam
        if (criarVizinhos && vizinha_T == null)
        {
            Vector2Int enderecoCelular = celulaBase_T.endereco + offset_T;
            Vector3 enderecoReal = celulaBase_T.posicaoMundo +
                new Vector3
                (
                    offset_T.x * InputsMorfo.input_distanciaAdjacencia,
                    0,
                    offset_T.y * InputsMorfo.input_distanciaAdjacencia
                );

            Celula nova = GerenteAmbiente.CriarCelula(enderecoCelular, enderecoReal);
            if (nova != null)
                NP_ColocarLugarNaCelula(nova);

            vizinha_T = nova;
        }
//        celulaBase_T.novoPredio.vizinhos_celulas_VN.Add(vizinha_T);
//        celulaBase_T.vizinhosVN.Add(vizinha_T);
        celulaBase_T.vizinhosPorOffset[offset_T] = vizinha_T;
    }
    public void Action_GuardarVizinhosOriginais(Celula c_sem_uso, Celula celula_T, Vector2Int o_sem_uso)
    {
        if (celula_T != null && celula_T.novoPredio != null)
            vizinhos_originais_tipos_VN.Add(celula_T.novoPredio.np_tipo);
    }
    public void Action_AtualizaCor(Celula c_sem_uso, Celula celula_T, Vector2Int o_sem_uso)
    {
        if (celula_T == null)
        {
            Debug.Log("Action_AtualizaCor com celula null");
            return;
        }
                
        if (celula_T.novoPredio != null && celula_T.novoPredio.np_meuRenderer != null && celula_T.novoPredio.tipoEspaco != null)
        {
            //Debug.Log("atualizando vizinho. selecionado: " + selecionado +
            //    "novopredio: " + vz.novoPredio + "novopredio.tipo: " + vz.novoPredio.tipoEspaco);
            if (EstaSelecionado) celula_T.novoPredio.np_meuRenderer.material.color = celula_T.novoPredio.tipoEspaco.cor_selecao;
            if (!EstaSelecionado) celula_T.novoPredio.np_meuRenderer.material.color = celula_T.novoPredio.tipoEspaco.cor;
        }
    }
    public bool Action_ChecaVizinhoTrancado(Celula celula_T)//, Dictionary<Vector2Int, Celula> _vizinhanca)
    {
        bool trancado = false;

        Action<Celula, Celula, Vector2Int> actionChecarTrancado =
            (celulaBase, celulaVizinha, offset) =>
            {
                if (trancado) return;

                if (celulaVizinha?.novoPredio?.np_tipo != TipoEspacoConstruido.Predio)
                    return;

                criarVizinhos = false; 
//                Debug.Log($"Checando vizinho {celulaVizinha.endereco} para trancamento: encontrou {countPredios} prédios adjacentes.");

                int count_predios = Celula.offsetsVonNeumann.Count
                    (offset => celulaVizinha.vizinhosPorOffset.TryGetValue(offset, out Celula c)
                     && c.novoPredio != null
                     && c.novoPredio.np_tipo == TipoEspacoConstruido.Predio);

                if (count_predios >= 3)
                    trancado = true;
            };

        NP_iterarVizinhanca
            (celula_T, Celula.offsetsVonNeumann, actionChecarTrancado);

        return trancado;
    }
    public bool Action_ChecaVizinhoQuina(Celula celula_T)//, Dictionary<Vector2Int,Celula> _vizinhanca)
    {
        bool vizinhoEHquina = false;
        List<DebugVizinhoQuina> relatorio = new List<DebugVizinhoQuina>();

        Action<Celula, Celula, Vector2Int> actionChecarQuina =
            (celulaBase, celulaDiagonal, offsetDiagonal) =>
            {
                if (vizinhoEHquina)
                    return;

                // se NAO tem celula, ou NAO tem um novoPredio, ou EH uma rua, nao precisa checar se eh quina, pule pro proximo
                if (celulaDiagonal?.novoPredio == null ||
                    celulaDiagonal.novoPredio.np_tipo == TipoEspacoConstruido.Rua)
                    return;

                //so vai checar se a quina C for um predio
                DebugVizinhoQuina d = new DebugVizinhoQuina();

                d.enderecoDiagonal = celulaDiagonal.endereco;
                d.nomeDiagonal = celulaDiagonal.novoPredio.name;
                d.tipoDiagonal = celulaDiagonal.novoPredio.np_tipo;

                // A partir do offset diagonal, deriva os dois ortogonais intermediários.
                d.orto1 = celulaBase.endereco + new Vector2Int(offsetDiagonal.x, 0);
                d.orto2 = celulaBase.endereco + new Vector2Int(0, offsetDiagonal.y);

                // --- ORTO 1 ---
                if (GerenteAmbiente.livroCelulas.TryGetValue(d.orto1, out Celula c1))
                {
                    if (c1?.novoPredio != null)
                    {
                        d.nomeOrto1 = c1.novoPredio.name;
                        d.tipoOrto1 = c1.novoPredio.np_tipo;
                        d.orto1EhRua = c1.novoPredio.np_tipo == TipoEspacoConstruido.Rua;
                    }
                    else if (c1?.lugar != null)
                    {
                        d.nomeOrto1 = c1.lugar.name;
                        d.tipoOrto1 = null;
                        d.orto1EhRua = Action_ChecaVizinhoTrancado(c1);
                    }
                }

                // --- ORTO 2 ---
                if (GerenteAmbiente.livroCelulas.TryGetValue(d.orto2, out Celula c2))
                {
                    if (c2?.novoPredio != null)
                    {
                        d.nomeOrto2 = c2.novoPredio.name;
                        d.tipoOrto2 = c2.novoPredio.np_tipo;
                        d.orto2EhRua = c2.novoPredio.np_tipo == TipoEspacoConstruido.Rua;
                    }
                    else if (c2?.lugar != null)
                    {
                        d.nomeOrto2 = c2.lugar.name;
                        d.tipoOrto2 = null;
                        d.orto2EhRua = Action_ChecaVizinhoTrancado(c2);//, vizinhos_VN); ;
                    }
                }

                // --- REGRA ---
                if ((d.orto1EhRua && d.orto2EhRua))
                {
                    d.conclusao = "EH UMA QUINA";
                    vizinhoEHquina = true;

                    relatorio.Add(d);
                    return;
                }
                else
                {
                    d.conclusao = "NAO EH QUINA (lugar ou predio adjacente)";
                    vizinhoEHquina = false;
                    relatorio.Add(d);
                }
            };

        NP_iterarVizinhanca(celula_T, Celula.offsetsVonNeumannComplemento, actionChecarQuina);

        //        NP_Debug_LogRelatorio(relatorio, vizinhoEHquina);
        return vizinhoEHquina;
    }

    public bool Action_ChecaQuina(Celula celula_T, TipoEspacoConstruido usoTeste)//, Dictionary<Vector2Int,Celula> _vizinhanca)
    {
        bool vizinhoEHquina = false;
        List<DebugVizinhoQuina> relatorio = new List<DebugVizinhoQuina>();

        Action<Celula, Celula, Vector2Int> actionChecarQuina =
            (celulaBase, celulaDiagonal, offsetDiagonal) =>
            {
                if (vizinhoEHquina)
                    return;

                // se NAO tem celula, ou NAO tem um novoPredio, nao precisa checar se eh quina, pule pro proximo
                if (celulaDiagonal?.novoPredio == null)
                    return;

                //so vai checar se a quina C for um predio
                DebugVizinhoQuina d = new DebugVizinhoQuina();

                d.enderecoDiagonal = celulaDiagonal.endereco;
                d.nomeDiagonal = celulaDiagonal.novoPredio.name;
                d.tipoDiagonal = celulaDiagonal.novoPredio.np_tipo;

                // A partir do offset diagonal, deriva os dois ortogonais intermediários.
                d.orto1 = celulaBase.endereco + new Vector2Int(offsetDiagonal.x, 0);
                d.orto2 = celulaBase.endereco + new Vector2Int(0, offsetDiagonal.y);

                // --- ORTO 1 ---
                if (!GerenteAmbiente.livroCelulas.TryGetValue(d.orto1, out Celula c1))
                    return;
                if (c1?.novoPredio == null)
                    return;

                    d.nomeOrto1 = c1.novoPredio.name;
                    d.tipoOrto1 = c1.novoPredio.np_tipo;
//                    d.orto1EhRua = c1.novoPredio.np_tipo == TipoEspacoConstruido.Rua;

                // --- ORTO 2 ---
                if (!GerenteAmbiente.livroCelulas.TryGetValue(d.orto2, out Celula c2))
                    return;
                if (c2?.novoPredio == null)
                    return;
                        d.nomeOrto2 = c2.novoPredio.name;
                        d.tipoOrto2 = c2.novoPredio.np_tipo;
                //                      d.orto2EhRua = c2.novoPredio.np_tipo == TipoEspacoConstruido.Rua;

                // --- REGRA ---
                if (d.tipoOrto1 == d.tipoOrto2 
                 && d.tipoDiagonal == usoTeste 
                 && usoTeste != d.tipoOrto1)
                    vizinhoEHquina = true;
                relatorio.Add(d);
            };

        NP_iterarVizinhanca(celula_T, Celula.offsetsVonNeumannComplemento, actionChecarQuina);

        //        NP_Debug_LogRelatorio(relatorio, vizinhoEHquina);
        return vizinhoEHquina;
    }


    public void NP_ChecarSeEhRua() //LEGACY
    {
        Celula celula_T = lugartemp.minhaCelula;
        bool deveSerRua_T = false;
        if (debug_novoPredio) Debug.Log("checar se eh rua valor de trancado: " + deveSerRua_T);

        // remove qualquer referência antiga para evitar duplicatas/contadores incorretos
        GerenteAmbiente.Geral_novosPrediosConstruidos?.Remove(this);
        GerenteAmbiente.Geral_novosPrediosRuas?.Remove(this);
        // NÃO removemos ainda de Geral_novosPrediosTotal aqui (mantemos registro geral), ou remova se preferir.

        //VIZINHO TRANCADO TEM Q CHECAR Q ELE PROPRIO NAO VAI FICAR TRANCADO
        checagem_vizinhos_trancados = Action_ChecaVizinhoTrancado(celula_T);// minhaCelula); // = Legacy_NP_ChecaVizinhoTrancado(minhaCelula);
        checagem_vizinhos_quina = Action_ChecaVizinhoQuina(celula_T); // minhaCelula);//= Legacy_NP_ChecaVizinhoQuina(minhaCelula);

        bool rua_por_maior_isovista = false;
        if (InputsMorfo.boolModoPreservaIso)
        {
            if (_isovista_calculada == false) NP_CalcularIsovista();
            //            NP_CalcularIsovista(); //criar_T um check de q ja foi calculado pra evitar recalcular
            rua_por_maior_isovista = NP_PreservarMaiorIsovista(); ///pode ser substituido por uma comparacao medidasnormalizadas.maxdist ==1
            Debug.Log("maior isovista: ");// + lugartemp.distanciaMaxima + "valor referencia: " + referencia_normalizacao.distanciaMaxima_Max);
            Debug.Log(referencia_normalizacao.Publicar());
            Debug.Log(lugartemp.iso.medidasBrutas.Publicar());
            Debug.Log(lugartemp.iso.medidasNormalizadas.Publicar());
        }

        bool profundidade = false;
        if (InputsMorfo.boolModoPreservaProfundidade)
        {
            if (_isovista_calculada == false) NP_CalcularIsovista();
            //            NP_CalcularIsovista(); //criar_T um check de q ja foi calculado pra evitar recalcular
            profundidade = lugartemp.iso.medidasNormalizadas.ProfundidadeRua == 1;
            Debug.Log("profundidade rua: ");// + lugartemp.total_profundidade_Rua + "valor referencia: " + referencia_normalizacao.ProfundidadeRua_Min);
            Debug.Log(referencia_normalizacao.Publicar());
            Debug.Log(lugartemp.iso.medidasBrutas.Publicar());
            Debug.Log(lugartemp.iso.medidasNormalizadas.Publicar());
        }

        //        aceitarcelula();

        deveSerRua_T = checagem_vizinhos_trancados || checagem_vizinhos_quina || rua_por_maior_isovista || profundidade;

        if (deveSerRua_T)
            Debug.Log($"deve_ser_rua: {deveSerRua_T} = trancado: {checagem_vizinhos_trancados} + quina: {checagem_vizinhos_quina} + vista: {rua_por_maior_isovista} + profundidade: {profundidade}");

        string rua = deveSerRua_T ? "sim" : "nao";

        if (rua == "nao")
        {

            np_nome = "predio " + indiceCriacao.ToString() + "_";// + indiceNovoPredio.ToString();
            gameObject.name = np_nome;

            tipoEspaco = GerenteAmbiente.PegarSO(TipoEspacoConstruido.Predio);// ._so_construir[0];
            indiceNovoPredio = GerenteAmbiente.Geral_novosPrediosConstruidos.Count();
            np_nome = tipoEspaco.nome + " " + indiceCriacao.ToString() + "_" + indiceNovoPredio.ToString();

            if (!GerenteAmbiente.Geral_novosPrediosConstruidos.Contains(this))
                GerenteAmbiente.Geral_novosPrediosConstruidos.Add(this);
        }
        else if (rua == "sim")
        {
            tipoEspaco = GerenteAmbiente.PegarSO(TipoEspacoConstruido.Rua);// ._so_construir[0];
            indiceNovoPredio = GerenteAmbiente.Geral_novosPrediosRuas.Count();
            np_nome = tipoEspaco.nome + " " + indiceCriacao.ToString() + "_" + indiceNovoPredio.ToString();
            //            np_nome = tipoEspaco.nome + GerenteAmbiente.Geral_novosPrediosRuas.Count.ToString();

            if (!GerenteAmbiente.Geral_novosPrediosRuas.Contains(this))
                GerenteAmbiente.Geral_novosPrediosRuas.Add(this);
        }

        this.name = np_nome;
        np_tipo = tipoEspaco.tipo;

        //        aceitarcelula();

        if (np_tipo == TipoEspacoConstruido.Rua)
            debug_novoPredio = true;
        if (debug_novoPredio) Debug.Log("pos checar se eh rua valor de trancado: " + rua + " nome obj: " + np_nome);
        debug_novoPredio = false;

        gameObject.transform.localScale = tipoEspaco.escala;
        var rend = gameObject.GetComponent<Renderer>();
        if (rend != null)
        {
            rend.material.color = tipoEspaco.cor;
        }
        gameObject.layer = LayerMask.NameToLayer(tipoEspaco.layer);

        if (!GerenteAmbiente.Geral_novosPrediosTotal.Contains(this))
            GerenteAmbiente.Geral_novosPrediosTotal.Add(this);
    }

    public void Legacy_NP_Celulas_PegarVizinhas(Celula celula_T, Dictionary<Vector2Int, Celula> dicionario_T, Vector2Int[] matrizVizinhanca_T, bool criar_T)//, List<Vector3> lista_endereco_real_trabalho = null)
    {
        //Dictionary<Vector2Int, Celula> dicionario_T,          -> aonde ficam anotadas as celulas vizinhas
        //Vector2Int[] matrizVizinhanca_T,                      -> referencia para encontrar a vizinhanca
        //bool criar_T,                                         -> define se vai criar_T celulas vizinhas (ou so anotar quais sao)
        //List<Vector3> lista_endereco_real_trabalho = null     -> aonde ficam os enderecos de mundo real das celulas vizinhas || DESNECESSARIOA como entrada, DELETADA
        //minhaCelula (trocada por celula_T)                    -> celula de referencia para pegar vizinhanca

        if (celula_T == null || dicionario_T == null || matrizVizinhanca_T == null)
        {
            Debug.LogWarning("falta parametro de entrada, abortando Legacy_NP_Celulas_PegarVizinhas\n" +
                "celula_T: " + celula_T + ", dicionario_T: " + dicionario_T + ", matrizVizinhanca_T:" + matrizVizinhanca_T);
            return;
        }
        if (debug_novoPredio) Debug.Log(
            "nome do predio: " + np_nome + "\n" +
            "minhaCelula endereco: " + celula_T.endereco.ToString() + "\n" +
            "minhaCelula lugar no mundo: " + celula_T.posicaoMundo.ToString());

        ////////////SE FOR CRIAR TEM Q ZERAR a lista de celulas vizinhas
        if (criar_T)// && dicionario_T != null)
        {
            if (debug_novoPredio) Debug.Log("NP CRIAR CELULAS: total dictionary celulasvizinhas: " + dicionario_T.Count);
            dicionario_T.Clear();
        }

        for (int i = 0; i < matrizVizinhanca_T.Length; i++)
        {
            Vector2Int enderecoCelular = celula_T.endereco + matrizVizinhanca_T[i];
            //enderecoReal[i] = enderecoReal;
            //            NP_CriarCelulas(enderecoCelular[i], enderecoReal[i], criar_T);

            if (!GerenteAmbiente.livroCelulas.TryGetValue(enderecoCelular, out Celula celula) && criar_T)
            {
                float x = celula_T.posicaoMundo.x + matrizVizinhanca_T[i].x * InputsMorfo.input_distanciaAdjacencia;
                float z = celula_T.posicaoMundo.z + matrizVizinhanca_T[i].y * InputsMorfo.input_distanciaAdjacencia;
                Vector3 enderecoReal = new Vector3(x, celula_T.posicaoMundo.y, z);// celulasVizinhas[Celula.offsetsVonNeumann[i]] = new Celula(Celula.offsetsVonNeumann[i], enderecoReal);

                celula = GerenteAmbiente.CriarCelula(enderecoCelular, enderecoReal);
                if (celula != null)
                    NP_ColocarLugarNaCelula(celula);
            }
            if (celula == null)
                continue;
            dicionario_T.Add(enderecoCelular, celula);
        }

        debug_novoPredio = false;
        if (debug_novoPredio)
        {
            string texto = "dicionario_T tamanho: " + dicionario_T.Count + ", estado criar_T: " + criar_T + "\n";

            foreach (var kvp in dicionario_T)
            {
                texto += $"{kvp.Key} -> {kvp.Value} : {kvp.Value.novoPredio} : {kvp.Value.lugar}\n";
            }

            Debug.Log(texto);

        }
        debug_novoPredio = false;
    }

    public bool Legacy_NP_ChecaVizinhoQuina(Celula celula_T)//, Dictionary<Vector2Int,Celula> _vizinhanca)
    {
        bool vizinhoEHquina = false;
        List<DebugVizinhoQuina> relatorio = new List<DebugVizinhoQuina>();

        foreach (Vector2Int offsetDiagonal in Celula.offsetsVonNeumannComplemento)
        //         foreach (Celula celulaDiagonal in _vizinhanca.Values)
        //            foreach (Celula cel_vizinha in celulasVonNeumann_Comp.Values)
        {

            Vector2Int enderecoDiagonal = celula_T.endereco + offsetDiagonal;
            if (!GerenteAmbiente.livroCelulas.TryGetValue(enderecoDiagonal, out Celula celulaDiagonal))
                continue;

            // se NAO tem celula, ou NAO tem um novoPredio, ou EH uma rua, nao precisa checar se eh quina, pule pro proximo
            if (celulaDiagonal.novoPredio == null || celulaDiagonal.novoPredio.np_tipo == TipoEspacoConstruido.Rua)
                continue;
            //so vai checar se a quina C for um predio
            DebugVizinhoQuina d = new DebugVizinhoQuina();

            d.enderecoDiagonal = celulaDiagonal.endereco;
            d.nomeDiagonal = celulaDiagonal.novoPredio.name;
            d.tipoDiagonal = celulaDiagonal.novoPredio.np_tipo;

            Vector2Int para_orto = celulaDiagonal.endereco;
            d.orto1 = new Vector2Int(para_orto.x, celula_T.endereco.y); //new Vector2Int(para_orto.x, minhaCelula.endereco.y);
            d.orto2 = new Vector2Int(celula_T.endereco.x, para_orto.y); //new Vector2Int(minhaCelula.endereco.x, para_orto.y);

            // --- ORTO 1 ---
            if (GerenteAmbiente.livroCelulas.TryGetValue(d.orto1, out Celula c1))
            {
                d.nomeOrto1 = "null";
                d.tipoOrto1 = null;
                d.orto1EhRua = false;

                if (c1?.novoPredio != null)
                //                if (celulasVonNeumann.TryGetValue(d.orto1, out Celula c1) && c1?.novoPredio != null)
                {
                    d.nomeOrto1 = c1.novoPredio.name;
                    d.tipoOrto1 = c1.novoPredio.np_tipo;
                    d.orto1EhRua = c1.novoPredio.np_tipo == TipoEspacoConstruido.Rua;
                }

                else if (c1?.lugar != null)
                {
                    Dictionary<Vector2Int, Celula> vizinhos_VN = new Dictionary<Vector2Int, Celula>();
                    //                    bool criarVizinhos = false;

                    ///////REFAZENDO NESSE PONTO
                    //                   Legacy_NP_Celulas_PegarVizinhas(c1, vizinhos_VN, Celula.offsetsVonNeumann, criarVizinhos);
                    d.nomeOrto1 = c1.lugar.name;
                    d.tipoOrto1 = null;
                    //                    d.orto1EhRua = Legacy_NP_ChecaVizinhoTrancado(c1);//, vizinhos_VN);
                    d.orto1EhRua = Action_ChecaVizinhoTrancado(c1);//, vizinhos_VN);

                }
            }

            // --- ORTO 2 ---
            if (GerenteAmbiente.livroCelulas.TryGetValue(d.orto2, out Celula c2))
            {
                d.nomeOrto2 = "null";
                d.tipoOrto2 = null;
                d.orto2EhRua = false;

                if (c2?.novoPredio != null)
                //                if (celulasVonNeumann.TryGetValue(d.orto2, out Celula c2) && c2?.novoPredio != null)
                {
                    d.nomeOrto2 = c2.novoPredio.name;
                    d.tipoOrto2 = c2.novoPredio.np_tipo;
                    d.orto2EhRua = c2.novoPredio.np_tipo == TipoEspacoConstruido.Rua;
                }
                else if (c2?.lugar != null)
                {
                    Dictionary<Vector2Int, Celula> vizinhos_VN = new Dictionary<Vector2Int, Celula>();
                    //  bool criarVizinhos = false;

                    ///////REFAZENDO NESSE PONTO
                    //                    Legacy_NP_Celulas_PegarVizinhas(c2, vizinhos_VN, Celula.offsetsVonNeumann, criarVizinhos);
                    d.nomeOrto2 = c2.lugar.name;
                    d.tipoOrto2 = null;
                    d.orto2EhRua = Action_ChecaVizinhoTrancado(c2);//, vizinhos_VN); ;
                }

            }

            // --- REGRA ---
            if ((d.orto1EhRua && d.orto2EhRua))
            {
                d.conclusao = "EH UMA QUINA";
                vizinhoEHquina = true;

                relatorio.Add(d);
                break;
            }
            else
            {
                d.conclusao = "NAO EH QUINA (lugar ou predio adjacente)";
                vizinhoEHquina = false;
                relatorio.Add(d);
            }
        }

        //        NP_Debug_LogRelatorio(relatorio, vizinhoEHquina);
        return vizinhoEHquina;
    }
    public bool Legacy_NP_ChecaVizinhoTrancado(Celula celula_T)//, Dictionary<Vector2Int, Celula> _vizinhanca)
    {
        debug_novoPredio = false;
        bool trancado = false;
        //        bool[] vizinhos_trancados = { false, false, false, false };
        bool[] vizinhos_trancados = new bool[Celula.offsetsVonNeumann.Length];

        int i = 0;
        int vizinhospredios = 0;
        foreach (Vector2Int offset in Celula.offsetsVonNeumann)
        //            foreach (Celula cel_vizinha in _vizinhanca.Values)
        {
            if (!GerenteAmbiente.livroCelulas.TryGetValue(celula_T.endereco + offset, out Celula cel_vizinha))
                continue;


            if (cel_vizinha.novoPredio == null || cel_vizinha.novoPredio.np_tipo != TipoEspacoConstruido.Predio)
                continue;
            //          if (cel_vizinha != null && cel_vizinha.novoPredio != null && cel_vizinha.novoPredio.np_tipo == TipoEspacoConstruido.Predio)
            //            {
            vizinhospredios++;
            int countPredios = 0;

            foreach (Vector2Int offset_vizinho in Celula.offsetsVonNeumann)
            //                foreach (Celula vizinho_vizinho in cel_vizinha.novoPredio.celulasVonNeumann.Values) 
            {
                if (!GerenteAmbiente.livroCelulas.TryGetValue(cel_vizinha.endereco + offset_vizinho, out Celula vizinho_vizinho))
                    continue;

                if (vizinho_vizinho.novoPredio != null
                 && vizinho_vizinho.novoPredio.np_tipo == TipoEspacoConstruido.Predio
                 && vizinho_vizinho.endereco != celula_T.endereco)
                {
                    countPredios++;
                }
            }

            if (countPredios >= 3)
            {
                vizinhos_trancados[i] = true;
                /*if (debug_novoPredio)*/
                Debug.Log($"Vizinho trancado detectado em {cel_vizinha.endereco} com {countPredios} prédios checagem_vizinhos_trancados.");
            }
            //            }
            i++;
        }
        // CORREÇÃO: Verifica se algum vizinho está trancado usando LINQ ao invés de acessar índices fixos

        trancado = vizinhos_trancados.Any(v => v);
        //        trancado = (vizinhospredios == 4) || vizinhos_trancados.Any(v => v);

        //        trancado = (vizinhospredios == 4) || vizinhos_trancados[0] || vizinhos_trancados[1] || vizinhos_trancados[2] || vizinhos_trancados[3];
        if (vizinhospredios == 4) debug_novoPredio = true;
        if (debug_novoPredio) Debug.Log($"celula trancada: {trancado}, #vizinhos predios: {vizinhospredios}, v0.{vizinhos_trancados[0]}, v1.{vizinhos_trancados[1]}, " +
            $"  v.2{vizinhos_trancados[2]}, v3.{vizinhos_trancados[3]}");
        debug_novoPredio = false;

        return trancado;
    }
    public void Legacy_AtualizaCor(Dictionary<Vector2Int, Celula> celulasSelecionadas, bool selecionado)
    {
        if (celulasSelecionadas == null)
        {
            //Debug.Log("dictionary checagem_vizinhos_trancados null");
            return;
        }
        if (celulasSelecionadas != null)
        {
            //Debug.Log("select click checagem_vizinhos_trancados Von Neumann, contados: " + celulasSelecionadas.Count);
            foreach (Celula vz in celulasSelecionadas.Values)
            {
                if (vz != null && vz.novoPredio != null && vz.novoPredio.np_meuRenderer != null && vz.novoPredio.tipoEspaco != null)
                {
                    //Debug.Log("atualizando vizinho. selecionado: " + selecionado +
                    //    "novopredio: " + vz.novoPredio + "novopredio.tipo: " + vz.novoPredio.tipoEspaco);
                    if (selecionado) vz.novoPredio.np_meuRenderer.material.color = vz.novoPredio.tipoEspaco.cor_selecao;
                    if (!selecionado) vz.novoPredio.np_meuRenderer.material.color = vz.novoPredio.tipoEspaco.cor;
                }
            }
        }
    }
    private void Legacy_NP_GuardarVizinhosOriginais()
    {
        //////      VIZINHOS VON NEUMANN
        vizinhos_originais_tipos_VN = celulasVonNeumann.Values
            .Where(t => t != null && t.novoPredio != null)
            .Select(t => t.novoPredio.np_tipo).ToList();


        //////      VIZINHOS complementar VON NEUMANN
        //        vizinhos_originais_tipos_VNC = celulasVonNeumann_Comp.Values
        //            .Where(t => t != null && t.novoPredio != null)
        //            .Select(t => t.novoPredio.np_tipo).ToList();
    }


    void Legacy_aleatorio_Collider()
    {

        bool collisionChecker = true;
        int contador = 0;

        ///verificar se o endereco eh espacoConstruido:
        ///1. nao sobrepoe
        if (GerenteAmbiente.lugaresAtivos.Count <= 0) { return; }

        int _np_index = 0;

        //        lugar lugartemp;
        while (collisionChecker == true)
        {
            _np_index = UnityEngine.Random.Range(0, GerenteAmbiente.lugaresAtivos.Count);
            lugartemp = GerenteAmbiente.lugaresAtivos[_np_index];
            np_endereco = lugartemp._endereco;

            // checagem local: permite substituir o proprio lugar
            Collider[] hits = Physics.OverlapBox(np_endereco, np_half, Quaternion.identity, ~0, QueryTriggerInteraction.Collide);
            if (hits == null || hits.Length == 0)
            {
                collisionChecker = false;
            }
            else
            {
                // busca colliders pertencentes ao lugartemp
                var lugarCols = lugartemp != null ? lugartemp.GetComponentsInChildren<Collider>() : new Collider[0];
                bool onlyLugar = true;
                foreach (var h in hits)
                {
                    if (h == null) continue;
                    // se o collider não pertence ao lugar escolhido, marca ocupado
                    if (lugartemp == null) { onlyLugar = false; break; }
                    if (h.gameObject != lugartemp.gameObject && !lugarCols.Contains(h))
                    {
                        onlyLugar = false;
                        break;
                    }
                }
                collisionChecker = !onlyLugar;
            }

            contador++;
            if (contador > GerenteAmbiente.lugaresAtivos.Count)
            {
                break;
            }
        }

        if (lugartemp != null)
        {
            // desativa colliders imediatamente para evitar detecção física enquanto substituímos
            var cols = lugartemp.GetComponentsInChildren<Collider>();
            foreach (var c in cols) if (c != null) c.enabled = false;

            if (lugartemp.minhaCelula != null)
            {
                // centraliza destruição no próprio lugar
                setMinhaCelula(lugartemp.minhaCelula);// minhaCelula = lugartemp.minhaCelula;
                minhaCelula.addnovoPredio(this);
            }


            if (debug_novoPredio) Debug.Log("minhaCelula endereco: " + minhaCelula.ToString());
            //            lugartemp.seDestruir();
        }

        this.transform.position = np_endereco;

    }

    void Legacy_isoplace_semUsarAleatorio()
    {
        NP_CalcularIsovista();

        float ref_medida_geral_ponderada = GerenteAmbiente.lugaresAtivos.Max(casa => casa.medida_geral_ponderada);
        List<lugar> lista_medida_geral_ponderada = GerenteAmbiente.lugaresAtivos.Where(casa => casa.medida_geral_ponderada == ref_medida_geral_ponderada).ToList();
        if (lista_medida_geral_ponderada == null || lista_medida_geral_ponderada.Count == 0) return;
        
        int _index_medida_geral_ponderada = UnityEngine.Random.Range(0, lista_medida_geral_ponderada.Count);

        lugartemp = lista_medida_geral_ponderada[_index_medida_geral_ponderada];
        Debug.Log("lugares pra escolha: " + lista_medida_geral_ponderada.Count + ", escolhido: " + lugartemp._nome + ", indice " + _index_medida_geral_ponderada);

        np_endereco = lugartemp._endereco;

        if (lugartemp != null)
        {
            var cols = lugartemp.GetComponentsInChildren<Collider>();
            foreach (var c in cols) if (c != null) c.enabled = false;
            if (lugartemp.minhaCelula != null)
            {
                // centraliza destruição no próprio lugar
                setMinhaCelula(lugartemp.minhaCelula);// minhaCelula = lugartemp.minhaCelula;
                minhaCelula.addnovoPredio(this);
            }
        }
        this.transform.position = np_endereco;
        
    }
}
