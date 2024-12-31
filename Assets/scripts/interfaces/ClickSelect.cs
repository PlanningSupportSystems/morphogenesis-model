using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;


public class ClickSelect : MonoBehaviour
{
    private Renderer objectRenderer;
    private Color originalColor;
    public static bool click = false;
    //    [SerializeField] Text EC_Nome;
    //    [SerializeField] Text EC_NumVizinhos;
    GameObject _propriedades_E_C;// = GameObject.Find("propriedades_espaco_construido");
    GameObject _propriedades_L;// = GameObject.Find("propriedades_lugar_alocado");
    Text obj_nome;

    IsovistaP _iso_display;

    ControleAglomeracao cs_ambiente;
    novoPredio esteNovoPredio;
    lugar esteLugar;

    void Start()
    {
        cs_ambiente = GameObject.Find("ambiente").GetComponent<ControleAglomeracao>();
        obj_nome = InputsMorfo.IM_obj_nome;
        _propriedades_E_C = InputsMorfo.IM_propriedades_E_C; // GameObject.Find("propriedades_espaco_construido");
        _propriedades_L = InputsMorfo.IM_propriedades_L;// GameObject.Find("propriedades_lugar_alocado");

        objectRenderer = GetComponent<Renderer>();
        originalColor = objectRenderer.material.color;
    }

    public void Select()
    {

        objectRenderer.material.color = Color.green; // Change color to indicate selection
        esteNovoPredio = this.GetComponent<novoPredio>();
        esteLugar = this.GetComponent<lugar>();
//        ControleAglomeracao Controles = Terrain.activeTerrain.GetComponent<ControleAglomeracao>();
//        ControleAglomeracao Controles = cs_ambiente.GetComponent<ControleAglomeracao>();


        //GameObject OBJ_nome = GameObject.Find("t_nome_obj");
        Text OBJ_nome = GameObject.Find("t_nome_obj").GetComponent<Text>();

        if (esteNovoPredio) 
        {
            _propriedades_E_C.SetActive(true);
            _propriedades_L.SetActive(false);

            ///vizinhos qd criado
            GameObject EC_NumVizinhos = GameObject.Find("TextVizinhosTotal_Inicial");
            GameObject EC_PrediosVizinhos = GameObject.Find("TextVizinhosInicial_Predios");
            GameObject EC_RuasVizinhas = GameObject.Find("TextVizinhosInicial_Ruas");

            ///vizinhos qd clickado
            GameObject EC_NumVizinhosClick = GameObject.Find("TextVizinhosTotal_Click");
            GameObject EC_PrediosVizinhosClick = GameObject.Find("TextVizinhosClick_Predios");
            GameObject EC_RuasVizinhasClick = GameObject.Find("TextVizinhosClick_Ruas");

            ////vizinhos qd selecionado
            click = true;
            esteNovoPredio.NP_MeusVizinhos();//  esteEC.EC_MeusVizinhos();
            click = false;

            if (esteNovoPredio.np_click_vizinhanca_quina != null)
            {
                foreach (novoPredio vz in esteNovoPredio.np_click_vizinhanca_quina)
                {
                    Renderer vz_objectRenderer = vz.GetComponent<Renderer>();

                    vz_objectRenderer.material.color = Color.blue; // Change color to indicate selection
                }
            }

            if (esteNovoPredio.np_click_vizinhanca_quina_orto != null)
            {
                foreach (novoPredio vz in esteNovoPredio.np_click_vizinhanca_quina_orto)
                {
                    Renderer vz_objectRenderer = vz.GetComponent<Renderer>();

                    vz_objectRenderer.material.color = Color.gray; // Change color to indicate selection
                }

            }
            //            _propriedades_E_C.gameObject.transform.Find("t_ec_nome").GetComponent<Text>().text = esteNovoPredio.np_nome;
            //            OBJ_nome.GetComponent<Text>().text = esteNovoPredio.np_nome;

            obj_nome.text = esteNovoPredio.np_nome;
//            OBJ_nome.text = esteNovoPredio.np_nome;
            EC_NumVizinhos.GetComponent<Text>().text = esteNovoPredio.np_meus_vizinhos_predio.Count.ToString();
            EC_PrediosVizinhos.GetComponent<Text>().text = esteNovoPredio.np_meus_vizinhos_predio.Count(en => en.np_nome.Contains("predio")).ToString();
            EC_RuasVizinhas.GetComponent<Text>().text = esteNovoPredio.np_meus_vizinhos_predio.Count(en => en.np_nome.Contains("rua")).ToString();

            EC_NumVizinhosClick.GetComponent<Text>().text = esteNovoPredio.np_meus_vizinhos_click_predio.Count.ToString();
            EC_PrediosVizinhosClick.GetComponent<Text>().text = esteNovoPredio.np_meus_vizinhos_click_predio.Count(en => en.np_nome.Contains("predio")).ToString();
            EC_RuasVizinhasClick.GetComponent<Text>().text = esteNovoPredio.np_meus_vizinhos_click_predio.Count(en => en.np_nome.Contains("rua")).ToString();

            int _templayer = (1 << LayerMask.NameToLayer("layer_predios"))
//                | 
//                (1 << LayerMask.NameToLayer("layer_ruas"))
//                | 
                //(1 << LayerMask.NameToLayer("layer_lugares"))
                ;
            Debug.Log("click campo visao qual a layer: " + _templayer);

            _iso_display = new IsovistaP(esteNovoPredio.np_endereco, 360, InputsMorfo.input_distanciaCampoVisao, _templayer);
            _iso_display.campoVisao(360, InputsMorfo.input_distanciaCampoVisao);
            _iso_display.isoMesh(_iso_display.pontosContorno, esteNovoPredio.np_nome + "mesh");


        }
        if (esteLugar) 
        {
            _propriedades_L.SetActive(true);
            _propriedades_E_C.SetActive(false);

            Text ISO_TotalObj = GameObject.Find("Valor_Iso_Total_Obj").GetComponent<Text>();
            Text ISO_TotalPredios = GameObject.Find("Valor_Iso_Total_Predios").GetComponent<Text>();
            Text ISO_TotalRuas = GameObject.Find("Valor_Iso_Total_Ruas").GetComponent<Text>();

            Text ISO_DistTotal = GameObject.Find("Valor_Dist_Total").GetComponent<Text>();
            Text ISO_DistMax = GameObject.Find("Valor_Dist_Max").GetComponent<Text>();
            Text ISO_DistAve = GameObject.Find("Valor_Dist_Ave").GetComponent<Text>();
            Text ISO_DistMin = GameObject.Find("Valor_Dist_Min").GetComponent<Text>();

            Text ISO_DistPonderada = GameObject.Find("Valor_Media_Ponderada").GetComponent<Text>();

            int _templayer = (1 << LayerMask.NameToLayer("layer_predios"))
                                //                | 
                                //                (1 << LayerMask.NameToLayer("layer_ruas"))
                                //                | 
                                //(1 << LayerMask.NameToLayer("layer_lugares"))
                                ;
            foreach (lugar l in cs_ambiente.Geral_Lugares)
            {
                l.L_CalculeIsovistas(_templayer);
            }

//            OBJ_nome.GetComponent<Text>().text = esteLugar._nome;
            obj_nome.text = esteLugar._nome;
            ISO_TotalObj.text = esteLugar.total_obj_visto.Count.ToString();
            ISO_TotalPredios.text = esteLugar.total_predio_visto.Count.ToString();
            ISO_TotalRuas.text = esteLugar.total_rua_visto.Count.ToString();

            ISO_DistTotal.text = esteLugar.normalizado_distanciaTotal.ToString();
            ISO_DistMax.text = esteLugar.normalizado_distanciaMaxima.ToString();
            ISO_DistAve.text = esteLugar.normalizado_distanciaMedia.ToString();
            ISO_DistMin.text = esteLugar.normalizado_distanciaMinima.ToString();

            ISO_DistPonderada.text = esteLugar.medida_geral_ponderada.ToString();

            _templayer = (1 << LayerMask.NameToLayer("layer_predios"))
                             //                | 
                             //                (1 << LayerMask.NameToLayer("layer_ruas"))
                             //                | 
                             //(1 << LayerMask.NameToLayer("layer_lugares"))
                                ;
            _iso_display = new IsovistaP( esteLugar._endereco, 360, InputsMorfo.input_distanciaCampoVisao, _templayer);
            _iso_display.campoVisao(360, InputsMorfo.input_distanciaCampoVisao);
            _iso_display.isoMesh(_iso_display.pontosContorno, esteLugar._nome + "mesh");

        }

    }

    public void Deselect()
    {
        objectRenderer.material.color = originalColor; // Revert to original color
        _iso_display.destruirMesh();

        foreach (novoPredio vz in esteNovoPredio.np_click_vizinhanca_quina)
        {
            Renderer vz_objectRenderer = vz.GetComponent<Renderer>();

            vz_objectRenderer.material.color = vz._tipo_espaco.cor; // Change color to indicate selection
//            esteNovoPredio.np_click_vizinhanca_quina.Remove(vz);
        }
//        esteNovoPredio.np_click_vizinhanca_quina.Clear();

        foreach (novoPredio vz in esteNovoPredio.np_click_vizinhanca_quina_orto)
        {
            Renderer vz_objectRenderer = vz.GetComponent<Renderer>();

            vz_objectRenderer.material.color = vz._tipo_espaco.cor; // Change color to indicate selection
//            esteNovoPredio.np_click_vizinhanca_quina_orto.Remove(vz);
        }
//        esteNovoPredio.np_click_vizinhanca_quina_orto.Clear();
    }
}
