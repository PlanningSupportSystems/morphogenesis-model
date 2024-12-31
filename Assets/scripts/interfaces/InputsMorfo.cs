using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;



public class InputsMorfo : MonoBehaviour
{

    public static bool tracking = false;
    //    public static string ptz = "aidentu 4";
    //    public string teste = "assim caraio mermo 4";
    //    [SerializeField]
    //    public float distAdj;
    //    [SerializeField]
    public GameObject obj_p_distanciaMaxima; //public GameObject iAnguloVista;
    public GameObject obj_p_distanciaMinima;
    public GameObject obj_p_distanciaMedia;
    public GameObject obj_p_distanciaTotal;
    public GameObject obj_p_total_obj_visto;
    public GameObject obj_p_total_predio_visto;
    public GameObject obj_p_total_rua_visto;

    public float p_distanciaMaxima = 0;
    public float p_distanciaMinima = 0;
    public float p_distanciaMedia = 0;
    public float p_distanciaTotal = 0;
    public float p_total_obj_visto = 1;
    public float p_total_predio_visto = 0;
    public float p_total_rua_visto = 0;

    public static float peso_distanciaMaxima = 1;
    public static float peso_distanciaMinima = 0;
    public static float peso_distanciaMedia = 0;
    public static float peso_distanciaTotal = 0;
    public static float peso_total_obj_visto = 0;
    public static float peso_total_predio_visto = 0;
    public static float peso_total_rua_visto = 0;


    public static int input_totalCasas;
    public static float input_totalVizinhos;//public static float anguloVista;
    public static float input_distanciaAdjacencia;
    public static float input_distanciaRegional;
    public static float input_distanciaCampoVisao;


    public GameObject iTotalVizinhos; //public GameObject iAnguloVista;
    public GameObject iDistanciaAdjacencia;
    public GameObject iDistanciaRegional;
    public GameObject iDistanciaCampoVisao;
    public Terrain terreno;
    public float tamanho;
//    public GameObject _ambiente;
    public ControleAglomeracao _ambiente;

    //    Text vTotalCasas;
    [SerializeField] InputField itotalCasas;
    [SerializeField] Button gerarAglomeracao;

    [SerializeField] Text vTotalCasas;
    [SerializeField] Text vTotalVizinhos;//[SerializeField] Text vAngVista;
    [SerializeField] Text vDistanciaAdjacencia;
    [SerializeField] Text vDistanciaRegional;
    [SerializeField] Text vDistanciaCampoVisao;

    [SerializeField] Toggle toggleRuaMaisUm;
    [SerializeField] Toggle toggleModoIsovista;
    [SerializeField] Toggle toggleModoRandom;

    public static bool boolRuaMaisUm;
    public static bool boolModoIsovista;
    public static bool boolModoRandom;


    public GameObject paramedidas;
    Vector3 medidas;

    /// <summary>
    /// textos para escritas das propriedades dos 'novoPredio' e 'lugar'
    /// </summary>
    public static Text IM_obj_nome;

    public static GameObject IM_propriedades_E_C;// = GameObject.Find("propriedades_espaco_construido");

    public static GameObject IM_propriedades_L;// = GameObject.Find("propriedades_espaco_construido");

    private void tamanhoprefab()
    {

        if (tracking)
        {
            Debug.Log("tracking IM tamanhoprefab");
        }

        //        paramedidas = GetComponent<ControleAglomeracao>().TiposEspacoConstruido[0];

        // Verifique se o prefab foi atribuído
        if (paramedidas != null)
        {
            // Obtenha o componente MeshFilter do prefab
            MeshFilter meshFilter = paramedidas.GetComponent<MeshFilter>();

            // Verifique se o MeshFilter existe no prefab
            if (meshFilter != null)
            {
                // Obtenha a mesh associada ao MeshFilter
                Mesh mesh = meshFilter.sharedMesh;

                // Verifique se a mesh existe
                if (mesh != null)
                {
                    // Obtenha o tamanho da mesh (bounding box)
                    Vector3 tamanho = mesh.bounds.size;

                    // Exiba o tamanho da mesh no console
                    Debug.Log("Tamanho do prefab: " + tamanho);
                }
                else
                {
                    Debug.LogWarning("O prefab não tem uma mesh associada.");
                }
            }
            else
            {
                Debug.LogWarning("O prefab não tem um componente MeshFilter.");
            }
        }
        else
        {
            Debug.LogWarning("O prefab não foi atribuído.");
        }
    }


void Start()
    {
        if (tracking)
        {
            Debug.Log("tracking IM START");
        }

        _ambiente = GameObject.Find("ambiente").GetComponent<ControleAglomeracao>();

        ///textos e valores dos objetos, novoPredio e lugar
        ///
        IM_obj_nome = GameObject.Find("t_nome_obj").GetComponent<Text>();
        IM_propriedades_E_C = GameObject.Find("propriedades_espaco_construido");
        IM_propriedades_L = GameObject.Find("propriedades_lugar_alocado");

        IM_propriedades_E_C.SetActive(false);
        IM_propriedades_L.SetActive(false);

        //      sliderTexto adj = new sliderTexto();
        iDistanciaAdjacencia.AddComponent<sliderTexto>();
        iDistanciaAdjacencia.GetComponent<sliderTexto>().OnvariavelChanged += IM_AtualizaVariaveis;
        iDistanciaAdjacencia.GetComponent<sliderTexto>().OnvariavelChanged += IM_AtualizaVizinhosMax;//IM_AtualizaVizinhosMax;

        iDistanciaRegional.AddComponent<sliderTexto>();
        iDistanciaRegional.GetComponent<sliderTexto>().OnvariavelChanged += IM_AtualizaVariaveis;

        iDistanciaCampoVisao.AddComponent<sliderTexto>();
        iDistanciaCampoVisao.GetComponent<sliderTexto>().OnvariavelChanged += IM_AtualizaVariaveis;

        ///para ajuste ainda
        tamanho = terreno.terrainData.size.x;
        terreno.GetComponent<ajusteTerreno>().mudouTerreno += IM_AtualizaVariaveis;
        //        Debug.Log("tamanho do terreno " + tamanho);

        //        sliderTexto visada = new sliderTexto();
        iTotalVizinhos.AddComponent<sliderTexto>();
        iTotalVizinhos.GetComponent<sliderTexto>().OnvariavelChanged += IM_AtualizaVariaveis;

        obj_p_distanciaMaxima.AddComponent<sliderTexto>();
        obj_p_distanciaMaxima.GetComponent<sliderTexto>().OnvariavelChanged += IM_AtualizaVariaveis;

        obj_p_distanciaMinima.AddComponent<sliderTexto>();
        obj_p_distanciaMinima.GetComponent<sliderTexto>().OnvariavelChanged += IM_AtualizaVariaveis;

        obj_p_distanciaMedia.AddComponent<sliderTexto>();
        obj_p_distanciaMedia.GetComponent<sliderTexto>().OnvariavelChanged += IM_AtualizaVariaveis;

        obj_p_distanciaTotal.AddComponent<sliderTexto>();
        obj_p_distanciaTotal.GetComponent<sliderTexto>().OnvariavelChanged += IM_AtualizaVariaveis;

        obj_p_total_obj_visto.AddComponent<sliderTexto>();
        obj_p_total_obj_visto.GetComponent<sliderTexto>().OnvariavelChanged += IM_AtualizaVariaveis;

        obj_p_total_predio_visto.AddComponent<sliderTexto>();
        obj_p_total_predio_visto.GetComponent<sliderTexto>().OnvariavelChanged += IM_AtualizaVariaveis;

        obj_p_total_rua_visto.AddComponent<sliderTexto>();
        obj_p_total_rua_visto.GetComponent<sliderTexto>().OnvariavelChanged += IM_AtualizaVariaveis;
    

    //        tamanhoprefab();
    //        medidas = GetComponent<ControleAglomeracao>().TiposEspacoConstruido[0].GetComponent<MeshFilter>().sharedMesh.bounds.size;

    IM_SetarValores();

        iTotalVizinhos.GetComponentInChildren<Slider>().value = 4;
//        iDistanciaAdjacencia.GetComponentInChildren<Slider>().value = 5;
        iDistanciaRegional.GetComponentInChildren<Slider>().value = (tamanho / 3) / 2;
        iDistanciaCampoVisao.GetComponentInChildren<Slider>().value = tamanho;// 50;// tamanho;

            
        peso_distanciaMaxima = p_distanciaMaxima;
        peso_distanciaMinima = p_distanciaMinima;
        peso_distanciaMedia = p_distanciaMedia;
        peso_distanciaTotal = p_distanciaTotal;
        peso_total_obj_visto = p_total_obj_visto;
        peso_total_predio_visto = p_total_predio_visto;
        peso_total_rua_visto = p_total_rua_visto;

//        IM_AtribuirCasas();

    }


void IM_SetarValores()
    {
        if (tracking)
        {
            Debug.Log("tracking IM_SetarValores");
        }
        
        medidas = paramedidas.GetComponent<MeshFilter>().sharedMesh.bounds.size;
        float adjmin = medidas.x *2 ;
        //        Debug.Log("tamanha casa x: "+medidas.x);
        iDistanciaAdjacencia.GetComponentInChildren<Slider>().minValue = adjmin;// + 0.1f;
        iDistanciaAdjacencia.GetComponentInChildren<Slider>().maxValue = adjmin*10;
//        iDistanciaAdjacencia.GetComponentInChildren<Slider>().value = adjmin;

        iDistanciaRegional.GetComponentInChildren<Slider>().maxValue = tamanho / 3;
        iDistanciaRegional.GetComponentInChildren<Slider>().minValue = 1;
 //       iDistanciaRegional.GetComponentInChildren<Slider>().value = (tamanho / 3) / 2;


        iDistanciaCampoVisao.GetComponentInChildren<Slider>().maxValue = tamanho;// 200;//tamanho;
        iDistanciaCampoVisao.GetComponentInChildren<Slider>().minValue = 1;
        //       iDistanciaCampoVisao.GetComponentInChildren<Slider>().value = tamanho/2 ;// 50;// tamanho;

        //iTotalVizinhos.GetComponentInChildren<Slider>().maxValue = 360;
        iTotalVizinhos.GetComponentInChildren<Slider>().minValue = 1;
//        iTotalVizinhos.GetComponentInChildren<Slider>().value = 4;     ///esses valores nao podem ser modificados toda vez q eh reparametrizado, dai definidos so no start
        IM_AtualizaVizinhosMax(0, "0");

    }

    void IM_AtualizaVizinhosMax(float novaVar, string varAtualizado)
    {
        if (tracking)
        {
            Debug.Log("tracking IM_AtualizaVizinhosMax");
        }
        
        //        float pqs = valor;
        float _catOposto = medidas.x; //0.5f;// 
        float _catAdjacente = iDistanciaAdjacencia.GetComponentInChildren<Slider>().value; //0.866f; para angulo = 30graus
        float _anguloMinimo = Mathf.Rad2Deg * Mathf.Atan2(_catOposto, _catAdjacente);
        //        iDistanciaAdjacencia.GetComponentInChildren<Slider>().value = adjmin;
        int maxVizinhos = (int)(360 / _anguloMinimo);

//        Debug.Log("cat oposto:" + _catOposto + ",cat adj:" + _catAdjacente  + " angulo" + _anguloMinimo + "max vizinhos:" + maxVizinhos);
        iTotalVizinhos.GetComponentInChildren<Slider>().maxValue = maxVizinhos;
    }

    void IM_AtualizaVariaveis(float novaVar, string varAtualizado) 
    {
        if (tracking)
        {
            Debug.Log("tracking IM_AtualizaVariaveis");
        }

        switch (varAtualizado)
            {
            case "terreno":
                tamanho = novaVar;
                IM_SetarValores();
                break;
            case "totalVizinhos":
                input_totalVizinhos = novaVar;
//                Debug.Log("input_totalVizinhos " + input_totalVizinhos);
                break;
            case "distanciaAdjacencia":
                    input_distanciaAdjacencia = novaVar;
                    break;
            case "distanciaRegional":
                    input_distanciaRegional = novaVar;
                    break;
            case "distanciaCampoVisao":
                    input_distanciaCampoVisao = novaVar;
                    break;
            case "input_peso_distancia_maxima":
                peso_distanciaMaxima = p_distanciaMaxima = novaVar;
                break;
            case "input_peso_distancia_minima":
                peso_distanciaMinima = p_distanciaMinima = novaVar;
                break;
            case "input_peso_distancia_media":
                peso_distanciaMedia = p_distanciaMedia = novaVar;
                break;
            case "input_peso_distancia_total":
                peso_distanciaTotal = p_distanciaTotal = novaVar;
                break;
            case "input_peso_objetos_vistos":
                peso_total_obj_visto = p_total_obj_visto = novaVar;
                break;
            case "input_peso_predios_vistos":
                peso_total_predio_visto = p_total_predio_visto = novaVar;
                break;
            case "input_peso_ruas_vistas":
                peso_total_rua_visto = p_total_rua_visto = novaVar;
                break;
        }


    }

    /// <summary>
    /// carrega o valor total de casas e atualiza o tamanho do mapa para acomodar
    /// </summary>
    public void IM_AtribuirCasas()
    {
        if (tracking)
        {
            Debug.Log("tracking IM_AtribuirCasas");
        }

        boolRuaMaisUm = toggleRuaMaisUm.isOn;

//        Debug.Log(boolVizinhanca);

        boolModoIsovista = toggleModoIsovista.isOn;
        boolModoRandom = toggleModoRandom.isOn;
//        Debug.Log("modo rua +1: " + boolRuaMaisUm + ", modo isovista: " + boolModoIsovista + ", modo random: " + boolModoRandom);

        vTotalCasas.text = itotalCasas.text;
        input_totalCasas = int.Parse(vTotalCasas.text);

        //dimensiona mapa (totalCasas);
        terreno.GetComponent<ajusteTerreno>().redefinirTerreno(input_totalCasas);
        //        terreno.GetComponent<levelgenerator>().iniciaMapa();

        //        terreno.GetComponent<ControleAglomeracao>().CA_IniciarControle();
        _ambiente.CA_IniciarControle();//  .GetComponent<ControleAglomeracao>().CA_IniciarControle();
        gerarAglomeracao.interactable = true;
    }

    public void IM_BotaoConfigurar()
    {
        //        Debug.Log("botao configurar, total de vizinhos:" + input_totalVizinhos);
        if (tracking)
        {
            Debug.Log("tracking IM_BotaoConfigurar");
        }

        vTotalCasas.text = itotalCasas.text;
        vTotalVizinhos.text = input_totalVizinhos.ToString();
        vDistanciaAdjacencia.text = input_distanciaAdjacencia.ToString();
        vDistanciaRegional.text = input_distanciaRegional.ToString();
        vDistanciaCampoVisao.text = input_distanciaCampoVisao.ToString();
//        terreno.GetComponent<ControleAglomeracao>().CAAtualizaValoresInput();
    }

}
