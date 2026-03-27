using System;
using System.Collections.Generic;
//using System.Diagnostics;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

public class novoPredio : MonoBehaviour, ISelecionavel
{
    //    ControleAglomeracao Controles = Terrain.activeTerrain.GetComponent<ControleAglomeracao>();
    ControleAglomeracao Controles;// = GameObject.Find("ambiente").GetComponent<ControleAglomeracao>();

    /// <summary>
    /// sistema de celulas para vizinhanca, substituiu os raycast e posicao xyz
    /// 
    /// checar quais precisam ficar. tvz so os dictionary e os list<tipoEspacoConstruido>. 
    /// </summary>
    public Dictionary<Vector2Int, Celula> celulasVizinhas = new Dictionary<Vector2Int, Celula>();
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
    [SerializeField] public Vector2Int end_Celula = new Vector2Int (0,0);

    GameObject _propriedades_E_C;// = GameObject.Find("propriedades_espaco_construido");
    GameObject _propriedades_L;// = GameObject.Find("propriedades_lugar_alocado");

    public TipoEspacoConstruido np_tipo;
    Text IM_nome_obj;
    ///enderecoCelular qd criado
    Text IM_predios_VizinhosTotal_Inicial;// = GameObject.Find("TextVizinhosTotal_Inicial");
    Text IM_predios_texto_VizinhosInicial_Predios;// = GameObject.Find("TextVizinhosInicial_Predios");
    Text IM_predios_texto_VizinhosInicial_Ruas;// = GameObject.Find("TextVizinhosInicial_Ruas");

    ///enderecoCelular qd clickado
    Text IM_predios_texto_VizinhosClick_Total;// = GameObject.Find("TextVizinhosTotal_Click");
    Text IM_predios_texto_VizinhosClick_Predios;// = GameObject.Find("TextVizinhosClick_Predios");
    Text IM_predios_texto_VizinhosClick_Ruas;// = GameObject.Find("TextVizinhosClick_Ruas");


    //propriedades relativas interface ISelecionavel
    public bool EstaSelecionado { get; set; }
    public bool souVizinhoSelecionado{ get; set; }
    public Color corOriginal;
    IsovistaP _iso_display;
    public static bool click = false; //FAZER ALGO com os enderecoCelular, analisar

    public Renderer np_meuRenderer;

    bool debug = false;


    public static Vector3 np_half = default;
    public string np_nome;
    public Vector3 np_endereco = default;
    private lugar lugartemp;


    /// <summary>
    /// ////////daki pra baixo td pode sair. substituido pelo sistema de celulas.
    /// revisar, por la, quais precisam ficar.
    /// </summary>
    public List<Vector3> enderecos_da_vizinhanca;
    public List<Vector3> enderecos_da_vizinhanca_VN;
    public List<Vector3> enderecos_da_vizinhanca_VNcomp;
    public List<Vector3> enderecos_da_vizinhanca_Moore;
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

    /// <summary>
    /// verificar se vale mais a pena concentrar aki os scriptObj, ou faze-los de prefab
    /// </summary>

    // Start is called before the first frame update

    public delegate void MetodoEscolha();
    MetodoEscolha metodo_escolha;

    public SO_EspacoConstruido _tipo_espaco;


    public novoPredio(string _nome)
    {
        np_half = Controles.TiposEspacoConstruido[0].transform.localScale / 2.1f;
        np_nome = _nome;

        atribuirDelegate(Controles.Tipo_Localizacao);
        metodo_escolha();
        //        Debug.Log("novo predio distancia de controles"+Controles.TdistObj.text);
    }

    public novoPredio(string _nome, string nome_metodo)
    {
        np_nome = _nome;
        np_half = Controles.TiposEspacoConstruido[0].transform.localScale / 2.1f;

        atribuirDelegate(nome_metodo);
        metodo_escolha();
        //        Debug.Log("novo predio distancia de controles"+Controles.TdistObj.text);
    }
    public novoPredio(Vector3 _endereco)
    {
        GameObject geo = GameObject.CreatePrimitive(PrimitiveType.Cube);
        geo.transform.localScale = _tipo_espaco.escala;// Controles._so_construir[2].escala;
        geo.GetComponent<Renderer>().material.color = _tipo_espaco.cor;//  Controles._so_construir[2].cor;

        Mesh sphereMesh = Resources.GetBuiltinResource<Mesh>("Sphere.fbx");
        geo.GetComponent<MeshFilter>().mesh = sphereMesh;

    }

    public void atribuirDelegate(string _nome_metodo)
    {
        MethodInfo methodInfo = this.GetType().GetMethod(_nome_metodo, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        if (methodInfo != null)
        {
            // Cria o delegate a partir do MethodInfo
            metodo_escolha = (MetodoEscolha)Delegate.CreateDelegate(typeof(MetodoEscolha), this, methodInfo);
        }
        else
        {
            Debug.LogError("Método não encontrado: " + _nome_metodo);
        }
    }


    void Start()
    {
        if (InputsMorfo.tracking)
        {
            Debug.Log("tracking novoPredio");
        }

        Controles = GameObject.Find("ambiente").GetComponent<ControleAglomeracao>();
        np_half = Controles.TiposEspacoConstruido[0].transform.localScale / 2.1f;
        np_nome = "predio" + Controles.Geral_novosPrediosConstruidos.Count;
        this.gameObject.name = np_nome;
        np_meuRenderer = this.gameObject.GetComponent<Renderer>();

        //configurando sobre interface ISelecionavel
        EstaSelecionado = false;
        _propriedades_E_C = InputsMorfo.IM_propriedades_E_C; // GameObject.Find("propriedades_espaco_construido");
        _propriedades_L = InputsMorfo.IM_propriedades_L;// GameObject.Find("propriedades_lugar_alocado");

        //IM_nome_obj = InputsMorfo.IM_obj_nome;// = np_nome;

        atribuirDelegate(Controles.Tipo_Localizacao);
        metodo_escolha();
        //        Controles.Geral_novosPrediosConstruidos.Add(this);
        NP_Vizinhanca(true);
        //        Debug.Log("novo predio start " + np_nome);

        //////////USO DAS CELULAS PARA CHECAR VIZINHOS
        bool criar = true;
        celulasVizinhas = new Dictionary<Vector2Int, Celula>();
        celulasVonNeumann = new Dictionary<Vector2Int, Celula>();
        celulasVonNeumann_Comp = new Dictionary<Vector2Int, Celula>();
        celulasMoore = new Dictionary<Vector2Int, Celula>();
        NP_Celulas_PegarVizinhas(celulasVizinhas, Celula.offsetsVonNeumann, criar, enderecos_da_vizinhanca);
        criar = false;
        NP_Celulas_PegarVizinhas(celulasVonNeumann, Celula.offsetsVonNeumann, criar, enderecos_da_vizinhanca_VN);
        NP_Celulas_PegarVizinhas(celulasVonNeumann_Comp, Celula.offsetsVonNeumannComplemento, criar, enderecos_da_vizinhanca_VNcomp);
        NP_Celulas_PegarVizinhas(celulasMoore, Celula.offsetsMoore, criar, enderecos_da_vizinhanca_Moore);

        NP_GuardarVizinhosOriginais();
   
        debug = false;
        if (debug)
        {
            Debug.Log(celulasVizinhas == null
            ? "dictionary = NULL"
            : $"dictionary OK | count_todos = {celulasVizinhas.Count}");
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
        debug = false;

        //////////USO DE POSICAO VECTOR3 E RAYCAST PARA CHECAR VIZINHOS
        NP_MeusVizinhos();
        NP_ChecaVizinhoTrancado();

        //colocado para, dps de todos os checks, tentar preservar a maior isovista.
        if (InputsMorfo.boolModoPreservaIso) NP_PreservarMaiorIsovista();
    }

    void aleatorio()
    {

        bool collisionChecker = true;
        int contador = 0;

        ///verificar se o endereco eh possivel:
        ///1. nao sobrepoe
        if (Controles.Geral_Lugares.Count <= 0) { return; }

        int _np_index = 0;

        //        lugar lugartemp;
        while (collisionChecker == true)
        {
            _np_index = UnityEngine.Random.Range(0, Controles.Geral_Lugares.Count);
            lugartemp = Controles.Geral_Lugares[_np_index];
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
            if (contador > Controles.Geral_Lugares.Count)
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


            if (debug) Debug.Log("minhaCelula endereco: "+ minhaCelula.ToString());
//            lugartemp.seDestruir();
        }

        this.transform.position = np_endereco;

    }
    void isoplace()
    {
        ///carregar todos os valores
        ///mutiplicar -> V1*p1 + V2*p2 + V3*p3..../sum(p...)
        ///crirar os pesos para multiplicacao dos pesos.
        ///
        ///for each faz uma atualizacao para todos os lugares disponiveis
        ///
        int _templayer = (1 << LayerMask.NameToLayer("layer_predios"))
                         ;

        foreach (lugar l in Controles.Geral_Lugares)
        {
            l.L_CalculeIsovistas(_templayer);
        }

        float ref_medida_geral_ponderada = Controles.Geral_Lugares.Max(casa => casa.medida_geral_ponderada);
        List<lugar> lista_medida_geral_ponderada = Controles.Geral_Lugares.Where(casa => casa.medida_geral_ponderada == ref_medida_geral_ponderada).ToList();
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

    public void NP_PreservarMaiorIsovista()
    {
        //PRESERVA QUANDO FOR A SELECAO FOR EQUIVALENTE A MAIOR RUA. DAI ELA SE MANTEM COMO RUA
        //EH UM EXTRA, SER RUA DE MODO A NAO TRANCAR OS PREDIOS, E MANTER QD FOR A MAIOR.
        //        Debug.Log("maior distancia desse lugar: " + lugartemp.distanciaMaxima);
        //        Debug.Log("maior distancia da rodada de medida: " + Controles.Geral_Lugares.Max(casa => casa.distanciaMaxima));

        // defesa: garantir que temos dados para comparar
        if (Controles == null)
        {
            if (debug) Debug.LogWarning("NP_PreservarMaiorIsovista: Controles == null para " + np_nome);
            return;
        }

        if (Controles.Geral_Lugares == null || Controles.Geral_Lugares.Count == 0)
        {
            if (debug) Debug.LogWarning("NP_PreservarMaiorIsovista: Geral_Lugares vazio para " + np_nome);
            return;
        }

        if (lugartemp == null)
        {
            if (debug) Debug.LogWarning("NP_PreservarMaiorIsovista: lugartemp == null para " + np_nome);
            return;
        }

        // proteger contra elementos nulos dentro da coleção
        float maxDistancia = Controles.Geral_Lugares
            .Where(c => c != null)
            .Select(c => c.distanciaMaxima)
            .DefaultIfEmpty(float.MinValue)
            .Max();

        if (debug) Debug.Log($"NP_PreservarMaiorIsovista: lugartemp.distanciaMaxima={lugartemp.distanciaMaxima}, maxDist={maxDistancia}");

      
        if (Mathf.Approximately(lugartemp.distanciaMaxima, maxDistancia))
        {
            NP_ChecarSeEhRua(true);
            if (debug) Debug.Log("vai ser rua pra preservar a maior isovista");
        }

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
                GameObject go = Instantiate(Controles.vp, c.posicaoMundo, Quaternion.identity);
                lugar porta_lugar = go.GetComponent<lugar>();
                porta_lugar.addCelula(c);
                c.addLugar(porta_lugar);// c.lugar = porta_lugar;
            }
    }


    public void NP_Celulas_PegarVizinhas(Dictionary<Vector2Int, Celula> dicionario_trabalho,  Vector2Int[] matriz_vizinhanca, bool criar, List<Vector3> lista_endereco_real_trabalho = null)
    {

        //        bool criar = true;
        Vector3 celula_Mundo = new Vector3(0, 0, 0);
//        Vector2Int[] enderecoCelular = new Vector2Int[matriz_vizinhanca.Length];
//        Vector3[] enderecoReal = new Vector3[matriz_vizinhanca.Length];
        if (debug) Debug.Log("nome do predio: " + np_nome + "\n" +
                             "endereco do predio: " + minhaCelula.posicaoMundo.ToString());
        if (minhaCelula != null)
        {
            celula_Mundo = minhaCelula.posicaoMundo;

            if (debug) Debug.Log("minhaCelula endereco: " + minhaCelula.endereco.ToString() + "\n" +
                                 "minhaCelula lugar no mundo: " + minhaCelula.posicaoMundo.ToString());
            //            celula_Mundo = minhaCelula.posicaoMundo;
        }
        else
        {
            Debug.LogWarning("sem celula");
        }


//        celulasVizinhas
        if (dicionario_trabalho == null) dicionario_trabalho = new Dictionary<Vector2Int, Celula>();
        ////////////SE FOR CRIAR TEM Q ZERAR a lista de celulas vizinhas
//        Debug.Log("estado do dictionary: " + dicionario_trabalho.Count);
        else if (criar && dicionario_trabalho != null)
        {
            Debug.Log("NP CRIAR CELULAS: total celulasvizinhas: " + dicionario_trabalho.Count);
            dicionario_trabalho.Clear();
            if (lista_endereco_real_trabalho == null) lista_endereco_real_trabalho = new List<Vector3>();
            lista_endereco_real_trabalho.Clear();
        }

        for (int i = 0; i < matriz_vizinhanca.Length; i++)

        {
            if (minhaCelula == null) break; 
            Vector2Int enderecoCelular = minhaCelula.endereco + matriz_vizinhanca[i];
            //enderecoReal[i] = enderecoReal;
//            NP_CriarCelulas(enderecoCelular[i], enderecoReal[i], criar);

            if (!Controles.livroCelulas.TryGetValue(enderecoCelular, out Celula celula) && criar)
            {
                float x = minhaCelula.posicaoMundo.x + matriz_vizinhanca[i].x * InputsMorfo.input_distanciaAdjacencia;
                float z = minhaCelula.posicaoMundo.z + matriz_vizinhanca[i].y * InputsMorfo.input_distanciaAdjacencia;// tamanhoCelula;
                Vector3 enderecoReal = new Vector3(x, minhaCelula.posicaoMundo.y, z);// celulasVizinhas[Celula.offsetsVonNeumann[i]] = new Celula(Celula.offsetsVonNeumann[i], enderecoReal);
                celula = Controles.CriarCelula(enderecoCelular, enderecoReal);
                if (celula != null)
                    NP_ColocarLugarNaCelula(celula);
                lista_endereco_real_trabalho.Add(celula.posicaoMundo);
            }
            if (celula == null)
                continue;
            dicionario_trabalho.Add(enderecoCelular, celula);
//            enderecos_da_vizinhanca.Add(celula.posicaoMundo);

        }

        debug = true;
        if (debug)
        {
            string texto = "dicionario_trabalho tamanho: " + dicionario_trabalho.Count + ", estado criar: " + criar +"\n";

            foreach (var kvp in dicionario_trabalho)
            {
                texto += $"{kvp.Key} -> {kvp.Value} : {kvp.Value.novoPredio} : {kvp.Value.lugar}\n";
            }

            Debug.Log(texto);

        }
        debug = false;
    }


    public void NP_ChecarSeEhRua(bool _viz_trancado)
    {
        if (debug) Debug.Log("checar se eh rua valor de trancado: " + _viz_trancado);

        // assegura que temos o controle
        if (Controles == null) Controles = GameObject.Find("ambiente").GetComponent<ControleAglomeracao>();

        // remove qualquer referência antiga para evitar duplicatas/contadores incorretos
        Controles.Geral_novosPrediosConstruidos?.Remove(this);
        Controles.Geral_novosPrediosRuas?.Remove(this);
        // NÃO removemos ainda de Geral_novosPrediosTotal aqui (mantemos registro geral), ou remova se preferir.

        _tipo_espaco = ScriptableObject.CreateInstance<SO_EspacoConstruido>();
        string rua = _viz_trancado ? "sim" : "nao";

        if (rua == "nao")
        {
            _tipo_espaco = Controles.PegarSO(TipoEspacoConstruido.Predio);// ._so_construir[0];
            np_nome = _tipo_espaco.nome + Controles.Geral_novosPrediosConstruidos.Count.ToString();
         
            if (!Controles.Geral_novosPrediosConstruidos.Contains(this))
                Controles.Geral_novosPrediosConstruidos.Add(this);
        }
        else if (rua == "sim")
        {
            _tipo_espaco = Controles.PegarSO(TipoEspacoConstruido.Rua);// ._so_construir[0];
            np_nome = _tipo_espaco.nome + Controles.Geral_novosPrediosRuas.Count.ToString();

            if (!Controles.Geral_novosPrediosRuas.Contains(this))
                Controles.Geral_novosPrediosRuas.Add(this);
        }

        this.name = np_nome;
        np_tipo = _tipo_espaco.tipo;

        if (debug) Debug.Log("pos checar se eh rua valor de trancado: " + rua + " nome obj: " +np_nome);


        gameObject.transform.localScale = _tipo_espaco.escala;
        var rend = gameObject.GetComponent<Renderer>();
        if (rend != null)
        {
            rend.material.color = _tipo_espaco.cor;
        }
        gameObject.layer = LayerMask.NameToLayer(_tipo_espaco.layer);


        if (!Controles.Geral_novosPrediosTotal.Contains(this))
            Controles.Geral_novosPrediosTotal.Add(this);

    }

    public void NP_ChecaVizinhoTrancado()
    {
        // Objetivo:
        // - Para cada vizinho que seja prédio, contar quantos enderecoCelular desse vizinho são prédios e quantas são ruas.
        // - Se existir algum vizinho que já tem (totalVizinhos - 1) prédios e 0 ruas, então este novo endereço seria o último disponível
        //   e trancaria esse vizinho -> marcar trancado = true (então este endereço deve virar rua).
        if (Controles == null) Controles = GameObject.Find("ambiente").GetComponent<ControleAglomeracao>();

        bool trancado = false;

        // Desabilita o collider deste objeto para não influenciar as checagens
        var myCol = this.GetComponent<Collider>();
        if (myCol != null) myCol.enabled = false;

        int totalVizinhos = Mathf.Max(1, (int)InputsMorfo.input_totalVizinhos);

        // Garantir lista inicializada
        if (np_meus_vizinhos_predio == null) np_meus_vizinhos_predio = new List<novoPredio>();

        foreach (novoPredio v in np_meus_vizinhos_predio)
        {
            if (v == null) continue;

            // Se o vizinho já for rua, não precisa preocupar-se com ele (não será "trancado" por virar casa)
            if (v.np_nome != null && v.np_nome.ToLower().Contains("rua")) continue;

            // Conta enderecoCelular do vizinho: quantos são prédios e quantos são ruas (sem mutar estado)
            CountNeighborTypes(v, out int prediosCount, out int ruasCount);

            // Se o vizinho já tem (totalVizinhos - 1) prédios e 0 ruas,
            // então o último espaço sendo ocupado por uma casa trancaria ele -> forçar rua aqui
            //            if (prediosCount >= totalVizinhos - 1 && ruasCount == 0)
            //            {
            //                trancado = true;
            //                break;
            //            }

            v.NP_MeusVizinhos();
            if (debug) Debug.Log("saldo enderecoCelular do vizinho: " +v.np_saldoVizinhos);
            if (v.np_saldoVizinhos <= 1)
            {
                trancado = true;
                break;
            }
        }

        if (debug) Debug.Log("trancado após checar enderecoCelular? " + trancado);
        // Também considerar a condição de quina existente
        //        bool quina = NP_ChecarQuinas();
        //        Debug.Log("quina: " + quina + "; trancado: " + trancado);
        //        trancado = trancado | quina;
        //        Debug.Log(" novo trancado: " + trancado);

        // Reativa collider deste objeto
        if (myCol != null) myCol.enabled = true;

        // Decide tipo com base no resultado
        NP_ChecarSeEhRua(trancado);
    }

    /// <summary>
    /// Conta, sem alterar estado, quantos enderecoCelular do predio 'p' são prédios e quantos são ruas.
    /// Usa a mesma lógica angular de vizinhança (InputsMorfo.input_totalVizinhos e input_distanciaAdjacencia).
    /// </summary>
    private void CountNeighborTypes(novoPredio p, out int prediosCount, out int ruasCount)
    {
        prediosCount = 0;
        ruasCount = 0;

        if (p == null) return;

        int qtd = Mathf.Max(1, (int)InputsMorfo.input_totalVizinhos);
        float passo = (360f / qtd) * Mathf.Deg2Rad;

        Vector3 centro = p.transform.position;
        Vector3 acima = new Vector3(0, 10f, 0);
        Vector3 abaixo = new Vector3(0, -10f, 0);

        for (int i = 0; i < qtd; i++)
        {
            float ang = passo * i;
            float x = Mathf.Cos(ang) * InputsMorfo.input_distanciaAdjacencia;
            float z = Mathf.Sin(ang) * InputsMorfo.input_distanciaAdjacencia;
            Vector3 alvo = centro + new Vector3(x, 0, z);

            // Linecast de cima para baixo (consistente com NP_ChecarQuinas e NP_MeusVizinhos)
            if (Physics.Linecast(alvo + acima, alvo + abaixo, out RaycastHit hit))
            {
                if (hit.transform == null) continue;
                string nome = hit.transform.gameObject.name.ToLower();
                if (nome.Contains("predio")) prediosCount++;
                else if (nome.Contains("rua")) ruasCount++;
                // outros tipos são ignorados para este critério
            }
        }
    }


    private void NP_GuardarVizinhosOriginais()
    {
        //////      VIZINHOS VON NEUMANN
        vizinhos_originais_tipos_VN = celulasVonNeumann.Values
            .Where(t => t != null && t.novoPredio != null)
            .Select(t => t.novoPredio.np_tipo).ToList();


        //////      VIZINHOS complementar VON NEUMANN
        vizinhos_originais_tipos_VNC = celulasVonNeumann_Comp.Values
            .Where(t => t != null && t.novoPredio != null)
            .Select(t => t.novoPredio.np_tipo).ToList();
    }

    public void Select()
    {
        EstaSelecionado = true;
        np_meuRenderer.material.color = _tipo_espaco.cor_selecao; //AtualizaCor();
        
        Debug.Log("novoPredio clicado: " + this.name +", estado: "+ EstaSelecionado);

        _propriedades_L.SetActive(false);
        _propriedades_E_C.SetActive(true);

        ////enderecoCelular qd selecionado
        //click = true;
        //NP_MeusVizinhos();//  esteEC.EC_MeusVizinhos();
        //click = false;

        if (celulasVonNeumann == null) Debug.Log("dictionary von nuemann null");

        Dictionary<Vector2Int, Celula> celulasSelecionadas = new Dictionary<Vector2Int, Celula>();
        NP_Celulas_PegarVizinhas(celulasSelecionadas, Celula.offsetsVonNeumann, false);
        AtualizaCor(celulasSelecionadas, true);
        
        celulasSelecionadas.Clear();
        NP_Celulas_PegarVizinhas(celulasSelecionadas, Celula.offsetsVonNeumannComplemento, false);
        AtualizaCor(celulasSelecionadas, true);

        InputsMorfo.IM_obj_nome.text = np_nome; // IM_nome_obj.text = np_nome; 

        ///enderecoCelular qd criado
        ///
        int count_original_todos = vizinhos_originais_tipos_VN.Count();
        int count_original_predios = vizinhos_originais_tipos_VN.Count(p =>
            p == TipoEspacoConstruido.Predio);

        int count_original_ruas = vizinhos_originais_tipos_VN.Count(p =>
            p == TipoEspacoConstruido.Rua);

        // .Count(c => c.novoPredio is TipoEspacoConstruido.Predio);
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

        _iso_display = new IsovistaP(np_endereco, 360, InputsMorfo.input_distanciaCampoVisao, _templayer);
        _iso_display.campoVisao(360, InputsMorfo.input_distanciaCampoVisao);
        _iso_display.isoMesh(_iso_display.pontosContorno, np_nome + "mesh");

    }

    public void Deselect()
    {
        EstaSelecionado = false;
        np_meuRenderer.material.color = _tipo_espaco.cor;// AtualizaCor();
        Debug.Log("novoPredio clicado: " + this.name + "estado: " + EstaSelecionado);

        InputsMorfo.IM_obj_nome.text = "no selection";
        _propriedades_E_C.SetActive(false);
        _propriedades_L.SetActive(false);

        // destruir iso mesh se existir
        if (_iso_display != null)
        {
            _iso_display.destruirMesh();
            _iso_display = null;
        }

        Dictionary<Vector2Int, Celula> celulasSelecionadas = new Dictionary<Vector2Int, Celula>();
        NP_Celulas_PegarVizinhas(celulasSelecionadas, Celula.offsetsVonNeumann, false);
        AtualizaCor(celulasSelecionadas, false);
       
        celulasSelecionadas.Clear();// = new Dictionary<Vector2Int, Celula>();
        NP_Celulas_PegarVizinhas(celulasSelecionadas, Celula.offsetsVonNeumannComplemento, false);
        AtualizaCor(celulasSelecionadas, false);

        /*
        if (np_click_vizinhanca_quina != null)
        {
            foreach (novoPredio vz in np_click_vizinhanca_quina)
            {
                if (vz == null) continue;
                vz.souVizinhoSelecionado = false;
                vz.np_meuRenderer.material.color = vz._tipo_espaco.cor; // vz.AtualizaCor();
            }
        }

        if (np_click_vizinhanca_quina_orto != null)
        {
            foreach (novoPredio vz in np_click_vizinhanca_quina_orto)
            {
                if (vz == null) continue;
                vz.souVizinhoSelecionado = false;
                vz.np_meuRenderer.material.color = vz._tipo_espaco.cor; // vz.AtualizaCor();
            }
        }
        */

    }

    public void AtualizaCor(Dictionary<Vector2Int,Celula> celulasSelecionadas, bool selecionado)
    {
        if (celulasSelecionadas == null)
        {
            Debug.Log("dictionary vizinhos null");
            return;
        }
        if (celulasSelecionadas != null)
        {
            Debug.Log("select click vizinhos Von Neumann, contados: " + celulasSelecionadas.Count);
            foreach (Celula vz in celulasSelecionadas.Values)
            {
                if (vz != null && vz.novoPredio != null && vz.novoPredio.np_meuRenderer != null && vz.novoPredio._tipo_espaco != null)
                {
                    Debug.Log("atualizando vizinho. selecionado: " + selecionado +
                        "novopredio: " + vz.novoPredio + "novopredio.tipo: " + vz.novoPredio._tipo_espaco);
                    if (selecionado) vz.novoPredio.np_meuRenderer.material.color = vz.novoPredio._tipo_espaco.cor_selecao;
                    if (!selecionado) vz.novoPredio.np_meuRenderer.material.color = vz.novoPredio._tipo_espaco.cor;

                }
            }
        }
        /*   if (EstaSelecionado)
           {
               np_meuRenderer.material.color = Color.yellow; // Cor de seleção
               return;
           }
           if (souVizinhoSelecionado)
           {
               np_meuRenderer.material.color = Color.cyan; // Cor para vizinho selecionado (se aplicável)
               return;
           }

           if (!EstaSelecionado || !souVizinhoSelecionado) np_meuRenderer.material.color = _tipo_espaco.cor; // Cor padrão ou cor do tipo de espaço
      */
    }
    public void seDestruir()
    {
        if (Controles == null) Controles = GameObject.Find("ambiente").GetComponent<ControleAglomeracao>();

        // Remover de todas as listas onde possa estar registrado
        Controles.Geral_novosPrediosConstruidos?.Remove(this);
        Controles.Geral_novosPrediosRuas?.Remove(this);
        Controles.Geral_novosPrediosTotal?.Remove(this);

        // Finalmente destruir o GameObject
        Destroy(this.gameObject);
    }


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
                    if (debug) Debug.Log("i: " + i + ", total quinas: " + tind);
                    if (debug) Debug.Log("v antes: " + _v_antes_ocupado + "; v dps: " + _v_dps_ocupado + "; Quina: " + _quinas[tind - 1]);

                }
            }

        }

        eh_quina = _quinas.Any(valor => valor);

        string result = string.Join(", ", _quinas);
        if (debug) Debug.Log("Valores de quina: " + result + "; Tem vizinho de quina? " + eh_quina);

        return (eh_quina);
    }


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
                //Instantiate(Controles.vp, _endereco_vizinho, Quaternion.identity);
                enderecos_da_vizinhanca.Add(_endereco_vizinho);
            }
            if (!_gerar) { enderecos_da_vizinhanca.Add(this.transform.position + new Vector3(_x, 0, _z)); }
        }

        if (debug)
        {
            string texto = "metodo tradicional endereco: \n";
            foreach (var v in enderecos_da_vizinhanca)
            {
                texto += v.ToString() + " \n";
            }
            Debug.Log(texto);
        }

    }
    public void NP_MeusVizinhos()
    {
        string _tipo_rua = "1saida";
        HashSet<Vector3> _os_endereco = new HashSet<Vector3>(enderecos_da_vizinhanca);

        if (click)//!ClickSelect.click)
        {
            np_meus_vizinhos_predio = new List<novoPredio>();
            np_meus_vizinhos_rua = new List<novoPredio>();
            np_meus_vizinhos_lugar = new List<lugar>();
        }
        if (click)//ClickSelect.click)
        {
            np_meus_vizinhos_click_predio = new List<novoPredio>();
            np_meus_vizinhos_click_rua = new List<novoPredio>();
            np_meus_vizinhos_click_lugar = new List<lugar>();
        }

        int vizinho_lugar = 0;
        int vizinho_predio = 0;

        foreach (Vector3 v in _os_endereco)
        {
            if (Physics.Linecast(np_endereco, v, out RaycastHit _em_quem))
            {
                string nome_vizinho = _em_quem.transform.gameObject.name;

                if (nome_vizinho.Contains("predio"))
                {
                    vizinho_predio += 1;
                    //                    if (!ClickSelect.click) { np_meus_vizinhos_predio.Add(_em_quem.transform.GetComponent<novoPredio>()); }
                    //                    if (ClickSelect.click) { np_meus_vizinhos_click_predio.Add(_em_quem.transform.GetComponent<novoPredio>()); }
                    if (!click) { np_meus_vizinhos_predio.Add(_em_quem.transform.GetComponent<novoPredio>()); }
                    if (click) { np_meus_vizinhos_click_predio.Add(_em_quem.transform.GetComponent<novoPredio>()); }
                }
                else if (InputsMorfo.boolRuaMaisUm && nome_vizinho.Contains("rua"))
                {
                    vizinho_predio += 1;
                    if (!click) { np_meus_vizinhos_rua.Add(_em_quem.transform.GetComponent<novoPredio>()); }
                    if (click) { np_meus_vizinhos_click_rua.Add(_em_quem.transform.GetComponent<novoPredio>()); }
                }

                if (_em_quem.transform.GetComponent<lugar>())
                {
                    vizinho_lugar += 1;
                    if (!click) { np_meus_vizinhos_lugar.Add(_em_quem.transform.GetComponent<lugar>()); }
                    if (click) { np_meus_vizinhos_click_lugar.Add(_em_quem.transform.GetComponent<lugar>()); }
                }
            }
        }
        NP_ChecarQuinas();

        np_saldoVizinhos = (int)InputsMorfo.input_totalVizinhos - vizinho_predio;
        if (debug) Debug.Log("saldo enderecoCelular: " + np_saldoVizinhos + "; totalVizinhos: " + InputsMorfo.input_totalVizinhos + "; enderecoCelular prédio: " + vizinho_predio + "; enderecoCelular lugar: " + vizinho_lugar);
    }

}