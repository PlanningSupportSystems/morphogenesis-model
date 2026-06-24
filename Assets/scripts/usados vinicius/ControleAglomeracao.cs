using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;
using System.Linq;

public class ControleAglomeracao : MonoBehaviour
{
    public static ControleAglomeracao Instance { get; private set; }

    public bool tracking = true;

    public botoes_gerente botoes_Gerente;
//    public Button b_apagar;
//    public Button b_gerarAglomeracao;
//    public Button b_atribuirParametros;

    private bool pararDepoisDaRodada = false;

    public GameObject espacoConstruido;
    public GameObject vizinhoPossivel;

    /// transformar num enumerate? pra ter os nomes dos objetos, inves de numeracao
    public SO_EspacoConstruido[] _so_construir;  
    

    public float DistanciaObjetos;  //esses dois devem ser subsituidos pela leitura do valor centralizado em InputsMorfo;
    public int QtdVizinhanca;       //

    //    public List<EspacoConstruido> Geral_TotalEspacosConstruidos;
    //    public List<EspacoConstruido> Geral_Ruas;
    //    public List<EspacoConstruido> Geral_Predios;
    //    public List<Predios> Geral_PrediosConstruidos;
    public SortedDictionary<int, novoPredio> TudoConstruido;
    public List<novoPredio> Geral_novosPrediosConstruidos;
    public List<novoPredio> Geral_novosPrediosRuas;
    public List<novoPredio> Geral_novosPrediosTotal;

    //    public List<Predios> Geral_PrediosConstruidos;
    //    public List<Predios> GeralRuasConstruidos;
    public List<lugar> lugaresAtivos;
    public List<lugar> lugaresDesativados;
    public List<lugar> lugaresBloqueados;
    public SortedDictionary<int, lugar> TodosLugares; 
    
    public int contadorlugar;
    public int ContadorRodadas;
    public bool navegacaoTempoHabilitada = false;
    public string nomeTesteAtual = "";

    public string Tipo_Localizacao;

    public Dictionary<Vector2Int, Celula> livroCelulas = new Dictionary<Vector2Int, Celula>();

    //    public List<Predios> Geral_VizinhosPossiveis;
    public HashSet<Vector3> Geral_EnderecosVizinhosPossiveis;
    public Material avulso;
    public Material quarteirao;
    public List<IsovistaP> todasIsovistas;


//    public  GameObject [] TiposEspacoConstruido;
    public InputField TdistObj;
    public InputField TresViz;
    public Text TespacoConstruido;
    public Text Tpredios;
    public Text Truas;
    public ScreenshotSaver salvarImagens;
    public GameObject gerenteImagens;
    //    public ImageToGifConverter criaGif;

    public ajusteCamera ajustecamera;
    public ajusteTerreno ajusteterreno;

    public float terrenoAtual;

    public Configuracoes configuracaoAtual;

    public struct Configuracoes
    {
        public int totalCasas;
        public float totalVizinhos;
        public float distanciaAdjacencia;
        public float distanciaRegional;
        public float distanciaCampoVisao;

        public bool ruaMaisUm;
        public bool modoRandom;
        public bool modoIsovista;
        public bool modoIsoObj;
        public bool preservaIso;
        public bool preservaProfundidade;

        public bool gravarImagens;
        public bool apenasImagemFinal;

        public float pesoDistanciaMaxima;
        public float pesoDistanciaMinima;
        public float pesoDistanciaMedia;
        public float pesoDistanciaTotal;
        public float pesoTotalObjVisto;
        public float pesoTotalPredioVisto;
        public float pesoTotalRuaVisto;
        public float pesoProfundidadeRua;
    }

    private void Awake()
    {
        // Verifica se já existe uma instância
        if (Instance != null && Instance != this)
        {
            // Já existe outra instância - destrói este objeto
            Debug.LogWarning($"ControleAglomeracao: Instância duplicada detectada em '{gameObject.name}'. Destruindo...");
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    // ADICIONE este método para limpar a referência quando o objeto for destruído
    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        if (tracking)
        {
            Debug.Log("tracking CA START");
        }

        //// Cria um novo GameObject
        gerenteImagens = new GameObject("gerenteImagens");
        // Anexa o componente ScreenshotSaver ao novo GameObject
        //        ScreenshotSaver screenshotSaver = screenshotSaverObject.AddComponent<ScreenshotSaver>();
        salvarImagens = this.gameObject.AddComponent<ScreenshotSaver>();
        botoes_Gerente = GameObject.Find("PaineisBotoes").GetComponent<botoes_gerente>();

        //        salvarImagens = new ScreenshotSaver();
        //        TdistObj.text = DistanciaObjetos.ToString();
        //        TresViz.text = QtdVizinhanca.ToString();

        //        GameObject.Find("Terrain").GetComponent<ajusteTerreno>().redefinirTerreno(10f);
        //        terreno.GetComponent<ajusteTerreno>().redefinirTerreno(input_totalCasas);

//        InputsMorfo.   .IM_AtribuirCasas();
        CA_IniciarControle();
        DistanciaObjetos = configuracaoAtual.distanciaAdjacencia; // InputsMorfo.input_distanciaAdjacencia;
//        GameObject.Find("Main Camera").GetComponent<ajusteCamera>().reposicionar();
//        GameObject.Find("Main Camera").GetComponent<ajusteCamera>().AdjustCameraToFitAllObjects();


        //        DistanciaObjetos = int.Parse(GameObject.Find("InputDistObj").GetComponent<InputField>.    .GetComponent<InputField>().text);

    }

    public void CAAtualizaValoresInput()
    {
        if (tracking)
        {
            Debug.Log("tracking CAAtualizaValoresInput");
        }

        DistanciaObjetos = configuracaoAtual.distanciaAdjacencia;// InputsMorfo.input_distanciaAdjacencia;     // int.Parse(TdistObj.text);
        QtdVizinhanca = (int)configuracaoAtual.totalVizinhos;// InputsMorfo.input_totalVizinhos;//   int.Parse(TresViz.text);
//        Debug.Log("CONTROLE AGLOMERACAO| total de vizinhos pedidos: " + QtdVizinhanca);

    }

    public void CA_AplicarConfiguracao(Configuracoes config)
    {
        configuracaoAtual = config;
//        InputsMorfo.input_totalVizinhos = config.totalVizinhos;
//        InputsMorfo.input_distanciaAdjacencia = config.distanciaAdjacencia;
//        InputsMorfo.input_distanciaRegional = config.distanciaRegional;
//        InputsMorfo.input_distanciaCampoVisao = config.distanciaCampoVisao;

        InputsMorfo.boolRuaMaisUm = config.ruaMaisUm;
        InputsMorfo.boolModoRandom = config.modoRandom;
        InputsMorfo.boolModoIsovista = config.modoIsovista;
        InputsMorfo.boolModoIsoObj = config.modoIsoObj;
        InputsMorfo.boolModoPreservaIso = config.preservaIso;
        InputsMorfo.boolModoPreservaProfundidade = config.preservaProfundidade;

        InputsMorfo.boolGravarImagens = config.gravarImagens;
        InputsMorfo.boolApenasImagemFinal = config.apenasImagemFinal;

//        InputsMorfo.peso_distanciaMaxima = config.pesoDistanciaMaxima;
//        InputsMorfo.peso_distanciaMinima = config.pesoDistanciaMinima;
//        InputsMorfo.peso_distanciaMedia = config.pesoDistanciaMedia;
//        InputsMorfo.peso_distanciaTotal = config.pesoDistanciaTotal;
//        InputsMorfo.peso_total_obj_visto = config.pesoTotalObjVisto;
//        InputsMorfo.peso_total_predio_visto = config.pesoTotalPredioVisto;
//        InputsMorfo.peso_total_rua_visto = config.pesoTotalRuaVisto;
//        InputsMorfo.peso_total_profundidade_rua = config.pesoProfundidadeRua;

        DistanciaObjetos = config.distanciaAdjacencia;
        QtdVizinhanca = (int)config.totalVizinhos;
    }
    public void CA_IniciarControle()
    {
        if (tracking)
        {
            Debug.Log("tracking CA_IniciarControle");
//            GameObject[] prediosAC = GameObject.FindGameObjectsWithTag("PredioAC");
//            Debug.Log("todos prediosAC achados: " + prediosAC.Length + "; pelo array de contagem:" + Geral_TotalEspacosConstruidos.Count);
        }

        Debug.Log("novo aonde estou: " + this.name);

        ///reset/atualizacao dos valores de Input, redicionamento do mapa (caso necessario)
        CAAtualizaValoresInput();
        //        this.GetComponent<levelgenerator>().iniciaMapa();
        contadorlugar = 0;
        ContadorRodadas = 0;
        navegacaoTempoHabilitada = false;

        InputsMorfo inputs = FindObjectOfType<InputsMorfo>();
        if (inputs != null)
        {
            inputs.IM_ResetarSliderTempo();
        }

        ///reset das listas que contem variaveis de valores
        if (Geral_EnderecosVizinhosPossiveis == null) {Geral_EnderecosVizinhosPossiveis = new HashSet<Vector3>();}
        else {Geral_EnderecosVizinhosPossiveis.Clear();}

//        if (Geral_VizinhosPossiveis == null) {Geral_VizinhosPossiveis = new List<Predios>();} 
//        else {Geral_VizinhosPossiveis.Clear();}
        float posInicialX = Terrain.activeTerrain.terrainData.size.x / 2;
        float posInicialZ = Terrain.activeTerrain.terrainData.size.z / 2;
        Geral_EnderecosVizinhosPossiveis.Add(new Vector3 (posInicialX, 1, posInicialZ));

        if (InputsMorfo.IM_propriedades_E_C != null) { InputsMorfo.IM_propriedades_E_C.SetActive(false); }
        if (InputsMorfo.IM_propriedades_L != null) { InputsMorfo.IM_propriedades_L.SetActive(false); }
        if (InputsMorfo.IM_obj_nome != null) { InputsMorfo.IM_obj_nome.text = "no selection"; }

        LimparCelulas();

        ///deletar as meshes de isovista
        /// 
        var objectsToDelete = GameObject.FindObjectsOfType<GameObject>()
                                        .Where(obj => obj.name == "m")
                                        .ToArray();

        // Loop para destruir os objetos filtrados
        foreach (GameObject obj in objectsToDelete)
        {
            Destroy(obj);
        }

        ///reset das listas que contem gameObject
        ///a lista com todos os espacos construidos faz a destruicao dos objetos. 
        ///as listas q contabilizam os tipos construidos (Predios e Ruas e ...) sao zeradas

        if (Geral_novosPrediosConstruidos == null) { Geral_novosPrediosConstruidos = new List<novoPredio>(); }
        else
        {
            foreach (novoPredio p in Geral_novosPrediosConstruidos.ToArray())
            {
                if (p != null)
                {
                    p.seDestruir();
                }
//                Destroy(p.gameObject);    // DestroyImmediate(p.gameObject);
            }
            Geral_novosPrediosConstruidos.Clear();
        }
//        Debug.Log("CA| total novos predios: " + Geral_novosPrediosConstruidos.Count);

        if (Geral_novosPrediosRuas == null) { Geral_novosPrediosRuas = new List<novoPredio>(); }
        else
        {
            foreach (novoPredio p in Geral_novosPrediosRuas.ToArray())
            {
                if (p != null)
                {
                    p.seDestruir();
                }
//                Destroy(p.gameObject);    // DestroyImmediate(p.gameObject);
            }
            Geral_novosPrediosRuas.Clear();
        }
        //        Debug.Log("CA| total novos predios: " + Geral_novosPrediosRuas.Count);

        if (Geral_novosPrediosTotal == null) { Geral_novosPrediosTotal = new List<novoPredio>(); }
        else
        {
            foreach (novoPredio p in Geral_novosPrediosTotal.ToArray())
            {
                if (p != null)
                {
                    p.seDestruir();
                }

//                Destroy(p.gameObject);    // DestroyImmediate(p.gameObject);
            }
            Geral_novosPrediosTotal.Clear();
        }
        //        Debug.Log("CA| total novos predios: " + Geral_novosPrediosConstruidos.Count);

        if (TodosLugares == null)
        {
            TodosLugares = new SortedDictionary<int, lugar>();
        }
        else
        {
            foreach (var l in TodosLugares.Values.ToArray())
            {
                if (l != null)
                    l.seDestruir();
            }

            TodosLugares.Clear();
            lugaresAtivos?.Clear();
            lugaresDesativados?.Clear();

        }


        if (lugaresAtivos == null) { lugaresAtivos = new List<lugar>(); }
        else
        {
            foreach (lugar p in lugaresAtivos.ToArray())
            {
                if (p != null)
                {
                    p.seDestruir();
                }
            }
            lugaresAtivos.Clear();
        }

        if (lugaresDesativados == null) { lugaresDesativados = new List<lugar>(); }
        contadorlugar = 0;
        Debug.Log("CA| total lugares: " + lugaresAtivos.Count);

        ajusteterreno.redefinirTerreno(configuracaoAtual.totalCasas, configuracaoAtual.distanciaAdjacencia);

//        ajustecamera.AdjustCameraToFitAllObjects();
//        GameObject.Find("Main Camera").GetComponent<ajusteCamera>().AdjustCameraToFitAllObjects();


    }

    public void CriarAglomeracao()
    {
        if (tracking)
        {
            Debug.Log("tracking CA CriarAglomeracao");
            GameObject[] inicio_antes_allObjects = UnityEngine.Object.FindObjectsOfType<GameObject>();
//            Debug.Log("CA_CriarAglomeracao| botao gerar nova apertado: todos gameobj achados: " + inicio_antes_allObjects.Length + "; geral TotalESPACOconstruido: " + Geral_TotalEspacosConstruidos.Count);
        }

        botoes_Gerente.b_apagar.interactable = false;
        botoes_Gerente.b_gerarAglomeracao.interactable = false;
        botoes_Gerente.b_atribuirParametros.interactable = false;

        ///recuperando de inputo valor escrito na entrada de total de espacos a construir
        int quantidade = configuracaoAtual.totalCasas; //InputsMorfo.input_totalCasas;// 0;

        ///redefinicao do tamanho do terreno para acomodar toda a "cidade"////////////////////
//        this.GetComponent<ajusteTerreno>().redefinirTerreno(quantidade);

        CA_IniciarControle();
        //        StartCoroutine(CA_ColocarConstrucoes(quantidade)); //CA_ColocarConstrucoes(quantidade); //
        StartCoroutine(CA_criaLugares(quantidade));
        //StartCoroutine(novoIsovistaTempo(quantidade));

        //       List<Predios> ruas = Geral_PrediosConstruidos.Where(ec => ec.predioPreFab.name == "RuaEC").ToList();
        //       Debug.Log("espacos construidos: " + Geral_PrediosConstruidos.Count + "; construcoes: " + construcoes.Count + "; ruas: " + ruas.Count);
    }

    IEnumerator CA_criaLugares(int tCasas)//public void CA_ColocarConstrucoes(int tCasas)//
    {
        if (tracking)
        {
            Debug.Log("tracking CA_ColocarConstrucoes");
        }

        botoes_Gerente.b_antiTravar.interactable = true;
        ContadorRodadas = 0;
//        Debug.Log("CA TESTANDO| ANTES contagem Geral_TotalEspacosConstruidos: " + Geral_TotalEspacosConstruidos.Count);

        float posInicialX = Terrain.activeTerrain.terrainData.size.x / 2;
        float posInicialZ = Terrain.activeTerrain.terrainData.size.z / 2;
        Vector3 pos_central = new Vector3(posInicialX, 1, posInicialZ);

        Tipo_Localizacao = "aleatorio";
//        if (!InputsMorfo.boolModoRandom && !InputsMorfo.boolModoIsovista) { Tipo_Localizacao = "aleatorio"; }
//        if (InputsMorfo.boolModoRandom) { Tipo_Localizacao = "aleatorio"; }
//        if (InputsMorfo.boolModoRandom && InputsMorfo.boolModoIsovista) { Tipo_Localizacao = "isoplace"; }
        if (configuracaoAtual.modoIsovista /*InputsMorfo.boolModoIsovista*/) { Tipo_Localizacao = "isoplace"; }

        ////////////fazer contorno para casos de os 2 selecionados ou nenhum selecionado
        GameObject go1 = Instantiate(vizinhoPossivel, pos_central, Quaternion.identity);
        lugar porta_lugar = go1.GetComponent<lugar>();
        porta_lugar.Inicializar(this);
        Celula celulaZero = CriarCelula(new Vector2Int(0, 0), pos_central);
        celulaZero.addLugar(porta_lugar);// celulaZero.lugar = porta_lugar;
        porta_lugar.addCelula(celulaZero);

        Debug.Log("CONTROLE AGLOMERACAO: Total de celulas: " + livroCelulas.Count);
        //Debug.Log("celulas: " + livroCelulas.Values.ToString() + "endereco: " + livroCelulas.Keys.ToString());
        Debug.Log(string.Join(", ", livroCelulas));
        ///////////////////////////////////////////////////////////////////////////////

        if (configuracaoAtual.gravarImagens /*InputsMorfo.boolGravarImagens*/) 
            salvarImagens.inicializarImagens();

        

        while (this.Geral_novosPrediosConstruidos.Count < tCasas)
        {

            ContadorRodadas++;

            string nome = "ec_" + ContadorRodadas;

            //            Debug.Log("vezes so far: " + ContadorRodadas +"qts ja foram"+ Geral_novosPrediosConstruidos.Count);
            GameObject go2 = Instantiate(espacoConstruido, new Vector3 (0,20,0), Quaternion.identity);
            novoPredio np = go2.GetComponent<novoPredio>();
            np.Inicializar (this);
            yield return StartCoroutine (np.ExecutarGeracao());

            //            Debug.Log("CA TESTANDO| contagem Geral_Predios: " + Geral_Predios.Count + " p:" + Geral_Predios[Geral_Predios.Count - 1] + "; nomero de ContadorRodadas: " + ContadorRodadas);
            //            IM_predios_texto_VizinhosClick_Predios.GetComponent<Text>().text = esteEC.meusPrediosVizinhosClick.Count(e => e.meuNome.Contains("predio")).ToString();

            //cenas para animacao ===  ScreenCapture.CaptureScreenshot(screenshotName + i + ".png");

            yield return new WaitForEndOfFrame();
            //            yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space));

            TespacoConstruido.text = Geral_novosPrediosTotal.Count.ToString();
            Tpredios.text = Geral_novosPrediosConstruidos.Count.ToString();
            Truas.text = Geral_novosPrediosRuas.Count.ToString();

            if (configuracaoAtual.gravarImagens /*InputsMorfo.boolGravarImagens*/)
//          if (InputsMorfo.boolGravarImagens)
            {
                yield return StartCoroutine(salvarImagens.FotoTela("zena completa" + ContadorRodadas, ScreenshotSaver.MomentoImagem.predioAdicionado));
            }

            //TROCAR PARA STOP UNTIL "QUER CONTINUAR"
            if (ContadorRodadas > 10 * tCasas || Geral_novosPrediosRuas.Count - Geral_novosPrediosConstruidos.Count > 100)
            {
                Debug.Log("ESTOUROU TOTAL de ContadorRodadas");
                break;
            }

            if (pararDepoisDaRodada)
            {
                pararDepoisDaRodada = false;
                break;
            }
        }

        Debug.Log("valor do apenas imagem final: " + InputsMorfo.boolApenasImagemFinal);
        if (configuracaoAtual.apenasImagemFinal)// InputsMorfo.boolApenasImagemFinal)
        {
            string nomeFinal = string.IsNullOrEmpty(nomeTesteAtual)
                ? "final_" + ContadorRodadas
                : nomeTesteAtual + "_final_" + ContadorRodadas;

            yield return StartCoroutine(
                salvarImagens.FotoTela(
                    nomeFinal,
                    ScreenshotSaver.MomentoImagem.simulacaoConcluida
                )
            );
        }


        FindObjectOfType<InputsMorfo>().IM_ConfigurarSliderTempo(ContadorRodadas);

        navegacaoTempoHabilitada = true;

        yield return new WaitForEndOfFrame();

        //        if (InputsMorfo.boolGravarImagens)
        //        {
        //            salvarImagens.ZiparImagens();
        //            salvarImagens.SaveGIF();
        //        }



        if (configuracaoAtual.gravarImagens && salvarImagens.TemImagens())
//        if (InputsMorfo.boolGravarImagens && salvarImagens.TemImagens())
        {
            botoes_Gerente.downloadImagens.interactable = true;
        }

        botoes_Gerente.b_apagar.interactable = true;
        botoes_Gerente.b_antiTravar.interactable = false;


    }

    public SO_EspacoConstruido PegarSO(TipoEspacoConstruido tipo)
    {
        foreach (SO_EspacoConstruido so in _so_construir)
        {
            if (so != null && so.tipo == tipo)
            {
                return so;
            }
        }

        Debug.LogWarning("Nao encontrei SO_EspacoConstruido para o tipo: " + tipo);
        return null;
    }

    public Celula CriarCelula(Vector2Int endereco, Vector3 posicao)
    {
        if (livroCelulas.ContainsKey(endereco))
        {
            Debug.LogWarning("Ja existe celula nesse endereco: " + endereco);
            return livroCelulas[endereco];
        }
        Celula celula = new Celula(endereco, posicao);
        celula.indiceCriacao = ContadorRodadas;
        livroCelulas.Add(endereco, celula);
        return celula;
    }

    public void NotificarErroOcupacao(novoPredio np, lugar lugarTentado)
    {
        Debug.LogWarning($"Erro de ocupação em {lugarTentado?._nome}");

        // aqui você pode:
        // - contabilizar erro
        // - tentar nova geração
        // - logar estatística
    }

    public void LimparCelulas()
    {
        if (livroCelulas == null || livroCelulas.Count == 0) return;

        // snapshot para evitar problemas de iteração enquanto destruímos objetos
        var lista = livroCelulas.Values.ToList();
        foreach (var cel in lista)
        {
            if (cel == null) continue;
            try
            {
                cel.SeDestruir(); // limpeza encapsulada na Celula
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"LimparCelulas: erro ao destruir célula {cel.endereco}: {e.Message}");
            }
        }
        livroCelulas.Clear();

        // Opcional: limpar listas/índices paralelos que mantêm referência a lugares/predios
        if (lugaresAtivos != null) lugaresAtivos.Clear(); // se fizer sentido no fluxo de reinício
    }

    public void CA_NavegarTempo(int ciclo)
    {
        if (!navegacaoTempoHabilitada) return;

        foreach (Celula celula in livroCelulas.Values)
        {
            celula.AplicarTempo(ciclo);
        }
    }
    
    public void CA_PararAoTravar()
    {
//        StopAllCoroutines();

        pararDepoisDaRodada = true;

        FindObjectOfType<InputsMorfo>().IM_ConfigurarSliderTempo(ContadorRodadas);
        navegacaoTempoHabilitada = true;

        botoes_Gerente.b_gerarAglomeracao.interactable = true;
        botoes_Gerente.b_atribuirParametros.interactable = true;
    }

    public void CA_BaixarImagens()
    {
        if (salvarImagens == null)
        {
            Debug.LogWarning("CA_BaixarImagens: salvarImagens nao inicializado.");
            botoes_Gerente.FimDownload();
            return;
        }

//        salvarImagens.BaixarZipWebGL();
//        salvarImagens.BaixarGifWebGL();
        salvarImagens.BaixarZipComGifWebGL();

#if !UNITY_WEBGL || UNITY_EDITOR
    botoes_Gerente.FimDownload();
#endif

    }

    public IEnumerator CA_RodarTesteConfiguracao(Configuracoes config, string nomeTeste)
    {
        nomeTesteAtual = nomeTeste;

        CA_AplicarConfiguracao(config);
        CA_IniciarControle();

        yield return new WaitForEndOfFrame();

        yield return StartCoroutine(CA_criaLugares(config.totalCasas));

        nomeTesteAtual = "";
    }

    public IEnumerator CA_RodarTesteConfiguracao(int quantidade, string nomeTeste)
    {
        Configuracoes config = new Configuracoes
        {
            totalCasas = quantidade,
            totalVizinhos = InputsMorfo.input_totalVizinhos,
            distanciaAdjacencia = InputsMorfo.input_distanciaAdjacencia,
            distanciaRegional = InputsMorfo.input_distanciaRegional,
            distanciaCampoVisao = InputsMorfo.input_distanciaCampoVisao,

            ruaMaisUm = InputsMorfo.boolRuaMaisUm,
            modoRandom = InputsMorfo.boolModoRandom,
            modoIsovista = InputsMorfo.boolModoIsovista,
            modoIsoObj = InputsMorfo.boolModoIsoObj,
            preservaIso = InputsMorfo.boolModoPreservaIso,
            preservaProfundidade = InputsMorfo.boolModoPreservaProfundidade,

            gravarImagens = InputsMorfo.boolGravarImagens,
            apenasImagemFinal = InputsMorfo.boolApenasImagemFinal,

            pesoDistanciaMaxima = InputsMorfo.peso_distanciaMaxima,
            pesoDistanciaMinima = InputsMorfo.peso_distanciaMinima,
            pesoDistanciaMedia = InputsMorfo.peso_distanciaMedia,
            pesoDistanciaTotal = InputsMorfo.peso_distanciaTotal,
            pesoTotalObjVisto = InputsMorfo.peso_total_obj_visto,
            pesoTotalPredioVisto = InputsMorfo.peso_total_predio_visto,
            pesoTotalRuaVisto = InputsMorfo.peso_total_rua_visto,
            pesoProfundidadeRua = InputsMorfo.peso_total_profundidade_rua
        };

        yield return StartCoroutine(CA_RodarTesteConfiguracao(config, nomeTeste));
    }


}
