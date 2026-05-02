using System;
using System.Collections.Generic;
//using System.Drawing;

//using System.Diagnostics;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

public class novoPredio : MonoBehaviour, ISelecionavel
{
    //    ControleAglomeracao gerenteAmbiente = Terrain.activeTerrain.GetComponent<ControleAglomeracao>();
    ControleAglomeracao gerenteAmbiente;// = GameObject.Find("ambiente").GetComponent<ControleAglomeracao>();

    /// <summary>
    /// sistema de celulas para vizinhanca, substituiu os raycast e posicao xyz
    /// 
    /// checar quais precisam ficar. tvz so os dictionary e os list<tipoEspacoConstruido>. 
    /// </summary>
    public Dictionary<Vector2Int, Celula> celulasVonNeumann = new Dictionary<Vector2Int, Celula>();
    public Dictionary<Vector2Int, Celula> celulasVonNeumann_Comp = new Dictionary<Vector2Int, Celula>();
    public Dictionary<Vector2Int, Celula> celulasMoore = new Dictionary<Vector2Int, Celula>();

    public List<TipoEspacoConstruido> vizinhos_originais_tipos_VN;
    public List<novoPredio> vizinhos_originais_predio_VN;
    public List<novoPredio> vizinhos_originais_rua_VN;
    public List<lugar> vizinhos_originais_lugar_VN;

    public List<TipoEspacoConstruido> vizinhos_originais_tipos_VNC;
    public List<novoPredio> vizinhos_originais_predio_VNC;
    public List<novoPredio> vizinhos_originais_rua_VNC;
    public List<lugar> vizinhos_originais_lugar_VNC;

    [SerializeField] InputsMorfo valoresEntrada;
    public InputsMorfo entradas;

    Celula minhaCelula;
    [SerializeField] public string enderecoCelula;
    [SerializeField] public Vector2Int endCelula = new Vector2Int (0,0);

    public TipoEspacoConstruido np_tipo;


    //propriedades relativas interface ISelecionavel
    public bool EstaSelecionado { get; set; }
    public bool souVizinhoSelecionado{ get; set; }
    public Color corOriginal;
    IsovistaP _iso_display;
    public static bool click = false; //FAZER ALGO com os enderecoCelular, analisar

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


    void Start()
    {
        if (InputsMorfo.tracking)
        {
            Debug.Log("tracking novoPredio");
        }

        gerenteAmbiente = ControleAglomeracao.Instance; // = GameObject.Find("ambiente").GetComponent<ControleAglomeracao>();
        if (gerenteAmbiente == null)
        {
            // tentativa de fallback (caso o singleton não tenha sido inicializado por alguma razão)
            gerenteAmbiente = FindObjectOfType<ControleAglomeracao>();
        }

        if (gerenteAmbiente == null)
        {
            Debug.LogError("novoPredio.Start: 'ambiente' com ControleAglomeracao não encontrado. Desativando componente.");
            enabled = false; // desativa este MonoBehaviour para evitar chamadas subsequentes
            return;
        }

        _isovista_calculada = false;

        np_half = gerenteAmbiente.espacoConstruido.transform.localScale / 2.1f;
        np_nome = "predio" + gerenteAmbiente.Geral_novosPrediosConstruidos.Count;
        this.gameObject.name = np_nome;
        np_meuRenderer = this.gameObject.GetComponent<Renderer>();

        //configurando sobre interface ISelecionavel
        EstaSelecionado = false;

        atribuirDelegate(gerenteAmbiente?.Tipo_Localizacao);
        if (metodo_escolha != null)
        {
            try { metodo_escolha(); }
            catch (Exception ex)
            { Debug.LogError($"metodo_escolha invocação falhou: {ex.GetType().Name}: {ex.Message}"); }
        }
        else
        { Debug.LogWarning("metodo_escolha não atribuído em novoPredio.Start()"); }

        //        gerenteAmbiente.Geral_novosPrediosConstruidos.Add(this);
//        NP_Vizinhanca(true);
        //        Debug.Log("novo predio start " + np_nome);

        //////////USO DAS CELULAS PARA CHECAR VIZINHOS
        bool criar = true;
        celulasVonNeumann = new Dictionary<Vector2Int, Celula>();
        NP_Celulas_PegarVizinhas(minhaCelula, celulasVonNeumann, Celula.offsetsVonNeumann, criar);//, enderecos_da_vizinhanca);
        criar = false;
//        celulasVonNeumann_Comp = new Dictionary<Vector2Int, Celula>();
        celulasMoore = new Dictionary<Vector2Int, Celula>();
//        NP_Celulas_PegarVizinhas(minhaCelula, celulasVonNeumann_Comp, Celula.offsetsVonNeumannComplemento, criar);//, enderecos_da_vizinhanca_VNcomp);
        NP_Celulas_PegarVizinhas(minhaCelula, celulasMoore, Celula.offsetsMoore, criar);//, enderecos_da_vizinhanca_Moore);

        NP_GuardarVizinhosOriginais();
   
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
//        NP_ChecaVizinhoTrancado();
        NP_ChecarSeEhRua();

        //colocado para, dps de todos os checks, tentar preservar a maior isovista.
//        if (InputsMorfo.boolModoPreservaIso) NP_PreservarMaiorIsovista();
    }

    void aleatorio()
    {

        bool collisionChecker = true;
        int contador = 0;

        ///verificar se o endereco eh espacoConstruido:
        ///1. nao sobrepoe
        if (gerenteAmbiente.Geral_Lugares.Count <= 0) { return; }

        int _np_index = 0;

        //        lugar lugartemp;
        while (collisionChecker == true)
        {
            _np_index = UnityEngine.Random.Range(0, gerenteAmbiente.Geral_Lugares.Count);
            lugartemp = gerenteAmbiente.Geral_Lugares[_np_index];
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
            if (contador > gerenteAmbiente.Geral_Lugares.Count)
            {
                break;
            }
        }

        if (lugartemp != null)
        {
            // desativa colliders imediatamente para evitar detecção física enquanto substituímos
            var cols = lugartemp.GetComponentsInChildren<Collider>();
            foreach (var c in cols) if (c != null) c.enabled = false;

            if(lugartemp.minhaCelula != null)
            {
                // centraliza destruição no próprio lugar
                addCelula(lugartemp.minhaCelula);// minhaCelula = lugartemp.minhaCelula;
                minhaCelula.addnovoPredio(this);
            }


            if (debug_novoPredio) Debug.Log("minhaCelula endereco: "+ minhaCelula.ToString());
//            lugartemp.seDestruir();
        }

        this.transform.position = np_endereco;

    }

    void isoplace()
    {
        NP_CalcularIsovista();

        float ref_medida_geral_ponderada = gerenteAmbiente.Geral_Lugares.Max(casa => casa.medida_geral_ponderada);
        List<lugar> lista_medida_geral_ponderada = gerenteAmbiente.Geral_Lugares.Where(casa => casa.medida_geral_ponderada == ref_medida_geral_ponderada).ToList();
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
                addCelula(lugartemp.minhaCelula);// minhaCelula = lugartemp.minhaCelula;
                minhaCelula.addnovoPredio(this);
            }
        }
        this.transform.position = np_endereco;
    }

    public bool NP_PreservarMaiorIsovista()
    {
        //PRESERVA QUANDO FOR A SELECAO FOR EQUIVALENTE A MAIOR RUA. DAI ELA SE MANTEM COMO RUA
        //EH UM EXTRA, SER RUA DE MODO A NAO TRANCAR OS PREDIOS, E MANTER QD FOR A MAIOR.
        //        Debug.Log("maior distancia desse lugar: " + lugartemp.distanciaMaxima);
        //        Debug.Log("maior distancia da rodada de medida: " + gerenteAmbiente.Geral_Lugares.Max(casa => casa.distanciaMaxima));

        // defesa: garantir que temos dados para comparar
        if (gerenteAmbiente == null)
        {
            if (debug_novoPredio) Debug.LogWarning("NP_PreservarMaiorIsovista: gerenteAmbiente == null para " + np_nome);
            return false;
        }

        if (gerenteAmbiente.Geral_Lugares == null || gerenteAmbiente.Geral_Lugares.Count == 0)
        {
            if (debug_novoPredio) Debug.LogWarning("NP_PreservarMaiorIsovista: Geral_Lugares vazio para " + np_nome);
            return false;
        }

        if (lugartemp == null)
        {
            if (debug_novoPredio) Debug.LogWarning("NP_PreservarMaiorIsovista: lugartemp == null para " + np_nome);
            return false;
        }

        // proteger contra elementos nulos dentro da coleção
        float maxDistancia = gerenteAmbiente.Geral_Lugares
            .Where(c => c != null)
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

    public void addCelula(Celula c)
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
                GameObject go = Instantiate(gerenteAmbiente.vizinhoPossivel, c.posicaoMundo, Quaternion.identity);
                lugar porta_lugar = go.GetComponent<lugar>();
                porta_lugar.addCelula(c);
                c.addLugar(porta_lugar);// cel_vizinha.lugar = porta_lugar;
            }
    }


    public void NP_Celulas_PegarVizinhas(Celula celula_T, Dictionary<Vector2Int, Celula> dicionario_T,  Vector2Int[] matrizVizinhanca_T, bool criar_T)//, List<Vector3> lista_endereco_real_trabalho = null)
    {
        //Dictionary<Vector2Int, Celula> dicionario_T,          -> aonde ficam anotadas as celulas vizinhas
        //Vector2Int[] matrizVizinhanca_T,                      -> referencia para encontrar a vizinhanca
        //bool criar_T,                                         -> define se vai criar_T celulas vizinhas (ou so anotar quais sao)
        //List<Vector3> lista_endereco_real_trabalho = null     -> aonde ficam os enderecos de mundo real das celulas vizinhas || DESNECESSARIOA como entrada, DELETADA
        //minhaCelula (trocada por celula_T)                    -> celula de referencia para pegar vizinhanca

        if (celula_T == null || dicionario_T == null || matrizVizinhanca_T == null)
        {
            Debug.LogWarning("falta parametro de entrada, abortando NP_Celulas_PegarVizinhas\n" +
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

            if (!gerenteAmbiente.livroCelulas.TryGetValue(enderecoCelular, out Celula celula) && criar_T)
            {
                float x = celula_T.posicaoMundo.x + matrizVizinhanca_T[i].x * InputsMorfo.input_distanciaAdjacencia;
                float z = celula_T.posicaoMundo.z + matrizVizinhanca_T[i].y * InputsMorfo.input_distanciaAdjacencia;
                Vector3 enderecoReal = new Vector3(x, celula_T.posicaoMundo.y, z);// celulasVizinhas[Celula.offsetsVonNeumann[i]] = new Celula(Celula.offsetsVonNeumann[i], enderecoReal);

                celula = gerenteAmbiente.CriarCelula(enderecoCelular, enderecoReal);
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
            string texto = "dicionario_T tamanho: " + dicionario_T.Count + ", estado criar_T: " + criar_T +"\n";

            foreach (var kvp in dicionario_T)
            {
                texto += $"{kvp.Key} -> {kvp.Value} : {kvp.Value.novoPredio} : {kvp.Value.lugar}\n";
            }

            Debug.Log(texto);

        }
        debug_novoPredio = false;
    }

    public void NP_CalcularIsovista()
    {
        if (gerenteAmbiente.Geral_Lugares == null || gerenteAmbiente.Geral_Lugares.Count == 0)
        {
            Debug.LogWarning("Nao ha lugares candidatos para avaliar.");
            return;
        }
        //seta a layer onde vai fazer as medidas de isovista, pegar so predios e ruas, e nao pegar lugares
        int _templayer = (1 << LayerMask.NameToLayer("layer_predios"));

        // ===== PRIMEIRO LUGAR ===== para setar o valor de referencia minimo e maxim sem problemas
        ///NAO PRECISOU DO 1o LUGAR PQ BUSCAREFERENCIANORMALIZACAO FAZ L_CALCULEISOVISTAS PARA TODOS
        
        // ===== RESTANTE DA PRIMEIRA VARREDURA =====
        referencia_normalizacao = Normalizador.BuscarReferenciaNormalizacao(gerenteAmbiente.Geral_Lugares, _templayer);

        // ===== SEGUNDA VARREDURA: NORMALIZAR =====
        Normalizador.NomalizarLista(gerenteAmbiente.Geral_Lugares, referencia_normalizacao);

        _isovista_calculada = true;

    }
    public void NP_ChecarSeEhRua()
    {
        bool deveSerRua_T = false;
        if (debug_novoPredio) Debug.Log("checar se eh rua valor de trancado: " + deveSerRua_T);

        // assegura que temos o controle  --- como garantir q foi recurado no awake e tirar esses testes
//        if (gerenteAmbiente == null) gerenteAmbiente = GameObject.Find("ambiente").GetComponent<ControleAglomeracao>();

        // remove qualquer referência antiga para evitar duplicatas/contadores incorretos
        gerenteAmbiente.Geral_novosPrediosConstruidos?.Remove(this);
        gerenteAmbiente.Geral_novosPrediosRuas?.Remove(this);
        // NÃO removemos ainda de Geral_novosPrediosTotal aqui (mantemos registro geral), ou remova se preferir.

        //        tipoEspaco = ScriptableObject.CreateInstance<SO_EspacoConstruido>();

        //VIZINHO TRANCADO TEM Q CHECAR Q ELE PROPRIO NAO VAI FICAR TRANCADO
        checagem_vizinhos_trancados = NP_ChecaVizinhoTrancado(minhaCelula);//, celulasVonNeumann);
        checagem_vizinhos_quina = NP_ChecaVizinhoQuina(minhaCelula);//, celulasVonNeumann_Comp);

        if (_isovista_calculada == false) NP_CalcularIsovista();
        bool rua_por_maior_isovista = false;
        if (InputsMorfo.boolModoPreservaIso)
        {
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
//            NP_CalcularIsovista(); //criar_T um check de q ja foi calculado pra evitar recalcular
            profundidade = lugartemp.iso.medidasNormalizadas.ProfundidadeRua == 1;
            Debug.Log("profundidade rua: ");// + lugartemp.total_profundidade_Rua + "valor referencia: " + referencia_normalizacao.ProfundidadeRua_Min);
            Debug.Log(referencia_normalizacao.Publicar());
            Debug.Log(lugartemp.iso.medidasBrutas.Publicar());
            Debug.Log(lugartemp.iso.medidasNormalizadas.Publicar());
        }

        deveSerRua_T = checagem_vizinhos_trancados || checagem_vizinhos_quina || rua_por_maior_isovista|| profundidade;
        
        if (deveSerRua_T)
        Debug.Log($"deve_ser_rua: {deveSerRua_T} = trancado: {checagem_vizinhos_trancados} + quina: {checagem_vizinhos_quina} + vista: {rua_por_maior_isovista} + profundidade: {profundidade}");

        string rua = deveSerRua_T ? "sim" : "nao";

        if (rua == "nao")
        {
            tipoEspaco = gerenteAmbiente.PegarSO(TipoEspacoConstruido.Predio);// ._so_construir[0];
            np_nome = tipoEspaco.nome + gerenteAmbiente.Geral_novosPrediosConstruidos.Count.ToString();
         
            if (!gerenteAmbiente.Geral_novosPrediosConstruidos.Contains(this))
                gerenteAmbiente.Geral_novosPrediosConstruidos.Add(this);
        }
        else if (rua == "sim")
        {
            tipoEspaco = gerenteAmbiente.PegarSO(TipoEspacoConstruido.Rua);// ._so_construir[0];
            np_nome = tipoEspaco.nome + gerenteAmbiente.Geral_novosPrediosRuas.Count.ToString();

            if (!gerenteAmbiente.Geral_novosPrediosRuas.Contains(this))
                gerenteAmbiente.Geral_novosPrediosRuas.Add(this);
        }

        this.name = np_nome;
        np_tipo = tipoEspaco.tipo;

        if (np_tipo == TipoEspacoConstruido.Rua)
            debug_novoPredio = true;   
        if (debug_novoPredio) Debug.Log("pos checar se eh rua valor de trancado: " + rua + " nome obj: " +np_nome);
        debug_novoPredio = false;

        gameObject.transform.localScale = tipoEspaco.escala;
        var rend = gameObject.GetComponent<Renderer>();
        if (rend != null)
        {
            rend.material.color = tipoEspaco.cor;
        }
        gameObject.layer = LayerMask.NameToLayer(tipoEspaco.layer);

        if (!gerenteAmbiente.Geral_novosPrediosTotal.Contains(this))
            gerenteAmbiente.Geral_novosPrediosTotal.Add(this);
    }

    public bool NP_ChecaVizinhoTrancado(Celula celula_T)//, Dictionary<Vector2Int, Celula> _vizinhanca)
    {
        debug_novoPredio = false;
        bool trancado = false;
//        bool[] vizinhos_trancados = { false, false, false, false };
        bool[] vizinhos_trancados = new bool [Celula.offsetsVonNeumann.Length];

        int i = 0;
        int vizinhospredios = 0;
        foreach (Vector2Int offset in Celula.offsetsVonNeumann)
//            foreach (Celula cel_vizinha in _vizinhanca.Values)
        {
            if (!gerenteAmbiente.livroCelulas.TryGetValue(celula_T.endereco + offset, out Celula cel_vizinha))
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
                if (!gerenteAmbiente.livroCelulas.TryGetValue(cel_vizinha.endereco + offset_vizinho, out Celula vizinho_vizinho))
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
                    /*if (debug_novoPredio)*/ Debug.Log($"Vizinho trancado detectado em {cel_vizinha.endereco} com {countPredios} prédios checagem_vizinhos_trancados.");
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

        sb.AppendLine("===== NP_ChecaVizinhoQuina =====");
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

    public bool NP_ChecaVizinhoQuina(Celula celula_T)//, Dictionary<Vector2Int,Celula> _vizinhanca)
    {
        bool vizinhoEHquina = false;
        List<DebugVizinhoQuina> relatorio = new List<DebugVizinhoQuina>();
        //        if (minhaCelula == null || celulasVonNeumann == null || celulasVonNeumann_Comp == null)
        //        {
        //            NP_Debug_LogRelatorio(relatorio, false);
        //            return false;
        //        }

        foreach (Vector2Int offsetDiagonal in Celula.offsetsVonNeumannComplemento)
   //         foreach (Celula celulaDiagonal in _vizinhanca.Values)
//            foreach (Celula cel_vizinha in celulasVonNeumann_Comp.Values)
            {

            Vector2Int enderecoDiagonal = celula_T.endereco + offsetDiagonal;
            if (!gerenteAmbiente.livroCelulas.TryGetValue(enderecoDiagonal, out Celula celulaDiagonal))
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
//            d.orto1 = new Vector2Int(para_orto.x, minhaCelula.endereco.y);
//            d.orto2 = new Vector2Int(minhaCelula.endereco.x, para_orto.y);
            d.orto1 = new Vector2Int(para_orto.x, celula_T.endereco.y);
            d.orto2 = new Vector2Int(celula_T.endereco.x, para_orto.y);


            // --- ORTO 1 ---
            if (gerenteAmbiente.livroCelulas.TryGetValue(d.orto1, out Celula c1))
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
//                    bool criar = false;

                    ///////REFAZENDO NESSE PONTO
 //                   NP_Celulas_PegarVizinhas(c1, vizinhos_VN, Celula.offsetsVonNeumann, criar);
                    d.nomeOrto1 = c1.lugar.name;
                    d.tipoOrto1 = null;
                    d.orto1EhRua = NP_ChecaVizinhoTrancado(c1);//, vizinhos_VN);
                }
            }

            // --- ORTO 2 ---
            if (gerenteAmbiente.livroCelulas.TryGetValue(d.orto2, out Celula c2))
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
                  //  bool criar = false;

                    ///////REFAZENDO NESSE PONTO
//                    NP_Celulas_PegarVizinhas(c2, vizinhos_VN, Celula.offsetsVonNeumann, criar);
                    d.nomeOrto2 = c2.lugar.name;
                    d.tipoOrto2 = null;
                    d.orto2EhRua = NP_ChecaVizinhoTrancado(c2);//, vizinhos_VN); ;
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
    private void NP_GuardarVizinhosOriginais()
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

    public void Select()
    {
        //Debug.Log("novoPredio clicado: " + this.name +", estado: "+ EstaSelecionado);

        EstaSelecionado = true;
        np_meuRenderer.material.color = tipoEspaco.cor_selecao; //AtualizaCor();
        
        InputsMorfo.IM_propriedades_E_C.SetActive(true); // _propriedades_E_C.SetActive(true);
        InputsMorfo.IM_propriedades_L.SetActive(false); ;// _propriedades_L.SetActive(false);

        ////enderecoCelular qd selecionado
        //click = true;
        //NP_MeusVizinhos();//  esteEC.EC_MeusVizinhos();
        //click = false;

        if (celulasMoore == null) Debug.Log("dictionary Moore null");

        AtualizaCor(celulasMoore, true);

        InputsMorfo.IM_obj_nome.text = np_nome; // IM_nome_obj.text = np_nome; 

        ///enderecoCelular qd criado
        ///
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
        int count_todos = celulasVonNeumann.Values.Count(t => t != null && t.novoPredio != null);
        int count_predios = celulasVonNeumann.Values.Count(p =>
            p != null
                && p.novoPredio != null
                    && p.novoPredio.np_tipo == TipoEspacoConstruido.Predio);

        int count_ruas = celulasVonNeumann.Values.Count(p =>
            p?.novoPredio?.np_tipo == TipoEspacoConstruido.Rua);

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
        np_meuRenderer.material.color = tipoEspaco.cor;// AtualizaCor();
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

        AtualizaCor(celulasMoore, false);

    }

    public void AtualizaCor(Dictionary<Vector2Int,Celula> celulasSelecionadas, bool selecionado)
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
    public void seDestruir()
    {
        if (gerenteAmbiente == null) gerenteAmbiente = GameObject.Find("ambiente").GetComponent<ControleAglomeracao>();

        // Remover de todas as listas onde possa estar registrado
        gerenteAmbiente.Geral_novosPrediosConstruidos?.Remove(this);
        gerenteAmbiente.Geral_novosPrediosRuas?.Remove(this);
        gerenteAmbiente.Geral_novosPrediosTotal?.Remove(this);

        // Finalmente destruir o GameObject
        Destroy(this.gameObject);
    }

    /*
    public void NP_Vizinhanca(bool _gerar)
    {
        enderecos_da_vizinhanca = new List<Vector3>();

        float _passo_angulo = (360 / (int)InputsMorfo.input_totalVizinhos) * Mathf.Deg2Rad;

        for (int i = 0; i < InputsMorfo.input_totalVizinhos; i++)
        {
            float angulo = _passo_angulo * i;
            float _x = Mathf.Cos(angulo) * InputsMorfo.input_distanciaAdjacencia;
            float _z = Mathf.Sin(angulo) * InputsMorfo.input_distanciaAdjacencia;

            Vector3 _endereco_vizinho = this.transform.position + new Vector3(_x, 0, _z);

            if (_gerar)
            {
                //Instantiate(gerenteAmbiente.vizinhoPossivel, _endereco_vizinho, Quaternion.identity);
                enderecos_da_vizinhanca.Add(_endereco_vizinho);
            }
            if (!_gerar) { enderecos_da_vizinhanca.Add(this.transform.position + new Vector3(_x, 0, _z)); }
        }

        if (debug_novoPredio)
        {
            string texto = "metodo tradicional endereco: \n";
            foreach (var v in enderecos_da_vizinhanca)
            {
                texto += v.ToString() + " \n";
            }
            Debug.Log(texto);
        }

    }
    */

    /*
    void VELHO_isoplace()
    {
        ///carregar todos os valores
        ///mutiplicar -> V1*p1 + V2*p2 + V3*p3..../sum(p...)
        ///crirar os pesos para multiplicacao dos pesos.
        ///
        ///for each faz uma atualizacao para todos os lugares disponiveis
        ///
        int _templayer = (1 << LayerMask.NameToLayer("layer_predios"))
                         ;

        foreach (lugar l in gerenteAmbiente.Geral_Lugares)
        {
            l.L_CalculeIsovistas(_templayer);
        }

        float ref_medida_geral_ponderada = gerenteAmbiente.Geral_Lugares.Max(casa => casa.medida_geral_ponderada);
        List<lugar> lista_medida_geral_ponderada = gerenteAmbiente.Geral_Lugares.Where(casa => casa.medida_geral_ponderada == ref_medida_geral_ponderada).ToList();
        if (lista_medida_geral_ponderada == null || lista_medida_geral_ponderada.Count == 0) return;

        int _index_medida_geral_ponderada = UnityEngine.Random.Range(0, lista_medida_geral_ponderada.Count);

        lugartemp = lista_medida_geral_ponderada[_index_medida_geral_ponderada];
        Debug.Log("lugares pra escolha: " + lista_medida_geral_ponderada.Count + ", escolhido: " + lugartemp._nome + ", indice " + _index_medida_geral_ponderada);

        np_endereco = lugartemp._endereco;

        if (lugartemp != null)
        {
            var cols = lugartemp.GetComponentsInChildren<Collider>();
            foreach (var cel_vizinha in cols) if (cel_vizinha != null) cel_vizinha.enabled = false;
            if (lugartemp.minhaCelula != null)
            {
                // centraliza destruição no próprio lugar
                addCelula(lugartemp.minhaCelula);// minhaCelula = lugartemp.minhaCelula;
                minhaCelula.addnovoPredio(this);
            }
        }



        this.transform.position = np_endereco;

    }
    */

    /*
    public bool NP_ChecarQuinas()
    {

        List<Vector3> _enderecos_da_vizinhanca_quina = new List<Vector3>();

        if (np_click_vizinhanca_quina != null) { np_click_vizinhanca_quina.Clear(); }
        np_click_vizinhanca_quina = new List<novoPredio>();
        if (np_click_vizinhanca_quina_orto != null) { np_click_vizinhanca_quina_orto.Clear(); }
        np_click_vizinhanca_quina_orto = new List<novoPredio>();

        float _passo_angulo = (360 / (int)InputsMorfo.input_totalVizinhos) * Mathf.Deg2Rad;
        int i;

        int _vizinhos_quina = 0;
        Vector3 _paracima = new Vector3(0, 10, 0);
        Vector3 _parabaixo = new Vector3(0, -10, 0);

        List<bool> _quinas = new List<bool>();
        bool eh_quina = false;

        Vector3 centro_ref = this.transform.position;

        for (i = 0; i < InputsMorfo.input_totalVizinhos; i++)
        {
            float angulo = _passo_angulo * i + (45f * Mathf.Deg2Rad);
            float _x = Mathf.Sqrt(2) * Mathf.Cos(angulo) * InputsMorfo.input_distanciaAdjacencia;
            float _z = Mathf.Sqrt(2) * Mathf.Sin(angulo) * InputsMorfo.input_distanciaAdjacencia;
            Vector3 _endereco_vizinho_quina = centro_ref + new Vector3(_x, 0, _z);

            if (Physics.Linecast(_endereco_vizinho_quina + _paracima, _endereco_vizinho_quina + _parabaixo, out RaycastHit quina_em_quem))
            {
                string nome_vizinho = quina_em_quem.transform.gameObject.name;

                if (nome_vizinho.Contains("predio"))
                {
                    float v_antes_angulo = _passo_angulo * i;
                    float v_antes_x = Mathf.Cos(v_antes_angulo) * InputsMorfo.input_distanciaAdjacencia;
                    float v_antes_z = Mathf.Sin(v_antes_angulo) * InputsMorfo.input_distanciaAdjacencia;
                    Vector3 v_antes_endereco_vizinho = centro_ref + new Vector3(v_antes_x, 0, v_antes_z);
                    bool _v_antes_ocupado = false;
                    if (Physics.Linecast(v_antes_endereco_vizinho + _paracima, v_antes_endereco_vizinho + _parabaixo, out RaycastHit vizinho_antes))
                    {
                        _v_antes_ocupado = vizinho_antes.transform.gameObject.name.Contains("predio");

                        if (click)
                        { np_click_vizinhanca_quina_orto.Add(vizinho_antes.transform.GetComponent<novoPredio>()); }

                    }

                    float v_dps_angulo = _passo_angulo * (i + 1);
                    float v_dps_x = Mathf.Cos(v_dps_angulo) * InputsMorfo.input_distanciaAdjacencia;
                    float v_dps_z = Mathf.Sin(v_dps_angulo) * InputsMorfo.input_distanciaAdjacencia;
                    Vector3 v_dps_endereco_vizinho = centro_ref + new Vector3(v_dps_x, 0, v_dps_z);
                    bool _v_dps_ocupado = false;
                    if (Physics.Linecast(v_dps_endereco_vizinho + _paracima, v_dps_endereco_vizinho + _parabaixo, out RaycastHit vizinho_dps))
                    {
                        _v_dps_ocupado = vizinho_dps.transform.gameObject.name.Contains("predio");
                        if (click)
                        { np_click_vizinhanca_quina_orto.Add(vizinho_dps.transform.GetComponent<novoPredio>()); }
                    }

                    _vizinhos_quina += 1;
                    bool tb = !(_v_antes_ocupado | _v_dps_ocupado);
                    _quinas.Add(tb);
                    int tind = _quinas.Count;
                    if (debug_novoPredio) Debug.Log("i: " + i + ", total quinas: " + tind);
                    if (debug_novoPredio) Debug.Log("v antes: " + _v_antes_ocupado + "; v dps: " + _v_dps_ocupado + "; Quina: " + _quinas[tind - 1]);

                }
            }

        }

        eh_quina = _quinas.Any(valor => valor);

        string result = string.Join(", ", _quinas);
        if (debug_novoPredio) Debug.Log("Valores de quina: " + result + "; Tem vizinho de quina? " + eh_quina);

        return (eh_quina);
    }
    */
   
}