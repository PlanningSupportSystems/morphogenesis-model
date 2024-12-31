using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;
using System.Linq;

public class ControleAglomeracao : MonoBehaviour
{
    public bool tracking = true;

    public Button b_apagar;
    public Button b_gerarAglomeracao;
    public Button b_atribuirParametros;

    public GameObject construcao;
    public GameObject possivel;
    public GameObject vp;
    public GameObject quinas;

    /// transformar num enumerate? pra ter os nomes dos objetos, inves de numeracao
    public SO_EspacoConstruido[] _so_construir;  
    

    public float DistanciaObjetos;  //esses dois devem ser subsituidos pela leitura do valor centralizado em InputsMorfo;
    public int QtdVizinhanca;       //

    public List<EspacoConstruido> Geral_TotalEspacosConstruidos;
    public List<EspacoConstruido> Geral_Ruas;
    public List<EspacoConstruido> Geral_Predios;
    public List<Predios> Geral_PrediosConstruidos;
    public List<novoPredio> Geral_novosPrediosConstruidos;
    public List<novoPredio> Geral_novosPrediosRuas;
    public List<novoPredio> Geral_novosPrediosTotal;

    //    public List<Predios> Geral_PrediosConstruidos;
    //    public List<Predios> GeralRuasConstruidos;
    public List<lugar> Geral_Lugares;
    public int contadorlugar;
    public int lugar_obj_vistos_maximo;
    public int lugar_predios_vistos_maximo;
    public int lugar_ruas_vistos_maximo;

    public string Tipo_Localizacao;


    public List<Predios> Geral_VizinhosPossiveis;
    public HashSet<Vector3> Geral_EnderecosVizinhosPossiveis;
    public Material avulso;
    public Material quarteirao;
    public List<IsovistaP> todasIsovistas;
    public Terrain _terreno;


    public  GameObject [] TiposEspacoConstruido;
    public InputField TdistObj;
    public InputField TresViz;
    public Text TespacoConstruido;
    public Text Tpredios;
    public Text Truas;
    public ScreenshotSaver salvarImagens;
    public GameObject gerenteImagens;
    //    public ImageToGifConverter criaGif;

    //    public event System.Action<float, string> mudouTerreno;

    public float terrenoAtual; 

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
        salvarImagens = gerenteImagens.AddComponent<ScreenshotSaver>();

        //        salvarImagens = new ScreenshotSaver();
        //        TdistObj.text = DistanciaObjetos.ToString();
        //        TresViz.text = QtdVizinhanca.ToString();

        //        GameObject.Find("Terrain").GetComponent<ajusteTerreno>().redefinirTerreno(10f);
        //        terreno.GetComponent<ajusteTerreno>().redefinirTerreno(input_totalCasas);

//        InputsMorfo.   .IM_AtribuirCasas();
        CA_IniciarControle();
        DistanciaObjetos = InputsMorfo.input_distanciaAdjacencia;
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

        DistanciaObjetos = InputsMorfo.input_distanciaAdjacencia;     // int.Parse(TdistObj.text);
        QtdVizinhanca = (int)InputsMorfo.input_totalVizinhos;//   int.Parse(TresViz.text);
//        Debug.Log("CONTROLE AGLOMERACAO| total de vizinhos pedidos: " + QtdVizinhanca);

    }

    public void CA_IniciarControle()
    {
        if (tracking)
        {
            Debug.Log("tracking CA_IniciarControle");
            GameObject[] prediosAC = GameObject.FindGameObjectsWithTag("PredioAC");
            Debug.Log("todos prediosAC achados: " + prediosAC.Length + "; pelo array de contagem:" + Geral_TotalEspacosConstruidos.Count);
        }

        Debug.Log("novo aonde estou: " + this.name);

        ///reset/atualizacao dos valores de Input, redicionamento do mapa (caso necessario)
        CAAtualizaValoresInput();
//        this.GetComponent<levelgenerator>().iniciaMapa();

        ///reset das listas que contem variaveis de valores
        if (Geral_EnderecosVizinhosPossiveis == null) {Geral_EnderecosVizinhosPossiveis = new HashSet<Vector3>();}
        else {Geral_EnderecosVizinhosPossiveis.Clear();}

        if (Geral_VizinhosPossiveis == null) {Geral_VizinhosPossiveis = new List<Predios>();} 
        else {Geral_VizinhosPossiveis.Clear();}
        float posInicialX = Terrain.activeTerrain.terrainData.size.x / 2;
        float posInicialZ = Terrain.activeTerrain.terrainData.size.z / 2;
        Geral_EnderecosVizinhosPossiveis.Add(new Vector3 (posInicialX, 1, posInicialZ));

        lugar_obj_vistos_maximo = 1;
        lugar_predios_vistos_maximo = 1;
        lugar_ruas_vistos_maximo = 1;


        if (InputsMorfo.IM_propriedades_E_C != null) { InputsMorfo.IM_propriedades_E_C.SetActive(false); }
        if (InputsMorfo.IM_propriedades_L != null) { InputsMorfo.IM_propriedades_L.SetActive(false); }
        if (InputsMorfo.IM_obj_nome != null) { InputsMorfo.IM_obj_nome.text = "no selection"; }
        

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
            foreach (novoPredio p in Geral_novosPrediosConstruidos)
            {
                Destroy(p.gameObject);
//                DestroyImmediate(p.gameObject);
            }
            Geral_novosPrediosConstruidos.Clear();
        }
//        Debug.Log("CA| total novos predios: " + Geral_novosPrediosConstruidos.Count);

        if (Geral_novosPrediosRuas == null) { Geral_novosPrediosRuas = new List<novoPredio>(); }
        else
        {
            foreach (novoPredio p in Geral_novosPrediosRuas)
            {
                Destroy(p.gameObject);
                //                DestroyImmediate(p.gameObject);
            }
            Geral_novosPrediosRuas.Clear();
        }
        //        Debug.Log("CA| total novos predios: " + Geral_novosPrediosRuas.Count);

        if (Geral_novosPrediosTotal == null) { Geral_novosPrediosTotal = new List<novoPredio>(); }
        else
        {
            foreach (novoPredio p in Geral_novosPrediosTotal)
            {
                Destroy(p.gameObject);
                //                DestroyImmediate(p.gameObject);
            }
            Geral_novosPrediosTotal.Clear();
        }
        //        Debug.Log("CA| total novos predios: " + Geral_novosPrediosConstruidos.Count);


        if (Geral_Lugares == null) { Geral_Lugares = new List<lugar>(); }
        else
        {
            foreach (lugar p in Geral_Lugares)
            {
                DestroyImmediate(p.gameObject);
            }
            Geral_Lugares.Clear();
        }
        contadorlugar = 0;
        Debug.Log("CA| total lugares: " + Geral_Lugares.Count);

        GameObject.Find("Main Camera").GetComponent<ajusteCamera>().AdjustCameraToFitAllObjects();


    }

    public void CriarAglomeracao()
    {
        if (tracking)
        {
            Debug.Log("tracking CA CriarAglomeracao");
            GameObject[] inicio_antes_allObjects = UnityEngine.Object.FindObjectsOfType<GameObject>();
//            Debug.Log("CA_CriarAglomeracao| botao gerar nova apertado: todos gameobj achados: " + inicio_antes_allObjects.Length + "; geral TotalESPACOconstruido: " + Geral_TotalEspacosConstruidos.Count);
        }

            
        b_apagar.interactable = true;
        b_gerarAglomeracao.interactable = false;
        b_atribuirParametros.interactable = false;

//    gerarAglomeracao.interactable = true;


        ///recuperando de inputo valor escrito na entrada de total de espacos a construir
        int quantidade = InputsMorfo.input_totalCasas;// 0;

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

        //cenas para animacao ===  string screenshotName = "Screenshot_";// + Time.frameCount; // Nome da captura de tela
        //cenas para animacao ===  ScreenCapture.CaptureScreenshot(screenshotName + ".png");
        //salvarImagens = new ScreenshotSaver();

        int rodadas = 0;
//        Debug.Log("CA TESTANDO| ANTES contagem Geral_TotalEspacosConstruidos: " + Geral_TotalEspacosConstruidos.Count);

        float posInicialX = Terrain.activeTerrain.terrainData.size.x / 2;
        float posInicialZ = Terrain.activeTerrain.terrainData.size.z / 2;
        Vector3 pos_central = new Vector3(posInicialX, 1, posInicialZ);

        Geral_Lugares = new List<lugar>();

        //        Debug.Log("geral predios a fazer: " + tCasas);

        if (!InputsMorfo.boolModoRandom && !InputsMorfo.boolModoIsovista) { Tipo_Localizacao = "aleatorio"; }
        if (InputsMorfo.boolModoRandom && InputsMorfo.boolModoIsovista) { Tipo_Localizacao = "isoplace"; }

        if (InputsMorfo.boolModoRandom) { Tipo_Localizacao = "aleatorio"; }
        if (InputsMorfo.boolModoIsovista) { Tipo_Localizacao = "isoplace"; }
        ////////////fazer contorno para casos de os 2 selecionados ou nenhum selecionado
        Instantiate(vp, pos_central, Quaternion.identity);

        while (this.Geral_novosPrediosConstruidos.Count < tCasas)
        {
            string nome = "ec_" + rodadas;

//            Debug.Log("vezes so far: " + rodadas +"qts ja foram"+ Geral_novosPrediosConstruidos.Count);
            Instantiate(possivel, new Vector3 (0,20,0), Quaternion.identity);

            //            Debug.Log("CA TESTANDO| contagem Geral_Predios: " + Geral_Predios.Count + " p:" + Geral_Predios[Geral_Predios.Count - 1] + "; nomero de rodadas: " + rodadas);
            //            EC_PrediosVizinhosClick.GetComponent<Text>().text = esteEC.meusPrediosVizinhosClick.Count(e => e.meuNome.Contains("predio")).ToString();

            //salvarImagens.FotoTela("cena" + rodadas);
            //cenas para animacao ===  ScreenCapture.CaptureScreenshot(screenshotName + i + ".png");
            rodadas++;

//          yield return StartCoroutine(salvarImagens.FotoTela("zena" + rodadas));
            yield return new WaitForEndOfFrame();
            //            yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space));

            TespacoConstruido.text = Geral_novosPrediosTotal.Count.ToString();
            Tpredios.text = Geral_novosPrediosConstruidos.Count.ToString();
            Truas.text = Geral_novosPrediosRuas.Count.ToString();

            //TROCAR PARA STOP UNTIL "QUER CONTINUAR"
            if (rodadas > 100 * tCasas)
            {
                Debug.Log("ESTOUROU TOTAL de rodadas");
                break;
            }

        }

//        string screenshotName = "Screenshot_";// + Time.frameCount; // Nome da captura de tela
        //ScreenCapture.CaptureScreenshot(screenshotName + ".png");
//        salvarImagens = new ScreenshotSaver();
//        salvarImagens.FotoTela("zena" + rodadas);
        yield return StartCoroutine(salvarImagens.FotoTela("zena" + rodadas));

        //        yield return new WaitForEndOfFrame();

        //        salvarImagens.ZiparImagens();
        //        salvarImagens.SaveGIF();
    }


}
