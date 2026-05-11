using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

/// <summary>
/// LEGADO: Este script é um exemplo de como implementar a seleção por clique 
/// em objetos do tipo novoPredio e lugar, com mudanças visuais e atualização de UI. 
/// Ele FOI refatorado para melhor modularidade e reutilização, 
/// mas serve como base para a funcionalidade de seleção.
/// 
/// SUBSITUIDO POR SelectionManager.cs + ISelecionavel.cs, 
/// que implementam uma abordagem mais genérica e escalável para seleção de objetos na cena,
/// as operacoes feitas com select/desecelect passaram a ser controladas por 
/// cada objeto implementando ISelecionavel, lugar.cs e novoPredio.cs
/// </summary>

public class ClickSelect : MonoBehaviour
{
    private Renderer objectRenderer;
    private Color originalColor;
    public static bool click = false;
    //    [SerializeField] Text EC_Nome;
    //    [SerializeField] Text IM_predios_VizinhosTotal_Inicial;
    GameObject _propriedades_E_C;// = GameObject.Find("propriedades_espaco_construido");
    GameObject _propriedades_L;// = GameObject.Find("propriedades_lugar_alocado");
    Text obj_nome;

    IsovistaP _iso_display;

    ControleAglomeracao cs_ambiente;
    novoPredio esteNovoPredio;
    lugar esteLugar;

//    bool selecionado = false;

    void Start()
    {
        /*
        cs_ambiente = GameObject.Find("ambiente").GetComponent<ControleAglomeracao>();
        obj_nome = InputsMorfo.IM_obj_nome;
        _propriedades_E_C = InputsMorfo.IM_propriedades_E_C; // GameObject.Find("propriedades_espaco_construido");
        _propriedades_L = InputsMorfo.IM_propriedades_L;// GameObject.Find("propriedades_lugar_alocado");

        objectRenderer = GetComponent<Renderer>();
        originalColor = objectRenderer.material.color;
        */
    }

    public void QuandoClicado()
    {
        /*
        if (!selecionado)
        {
            Select();
            selecionado = true;
        }
        else
        {
            Deselect();
            selecionado = false;
        }
        */
    }

    public void Select()
    {

//        esteNovoPredio = this.GetComponent<novoPredio>();
//        esteLugar = this.GetComponent<lugar>();
//        ControleAglomeracao GerenteAmbiente = Terrain.activeTerrain.GetComponent<ControleAglomeracao>();
//        ControleAglomeracao GerenteAmbiente = cs_ambiente.GetComponent<ControleAglomeracao>();

        //GameObject OBJ_nome = GameObject.Find("t_nome_obj");
//        Text OBJ_nome = GameObject.Find("t_nome_obj").GetComponent<Text>();

        if (esteNovoPredio) 
        {
            /*
            objectRenderer.material.color = Color.green; // Change color to indicate selection

            _propriedades_E_C.SetActive(true);
            _propriedades_L.SetActive(false);

            ///vizinhos qd criado
            GameObject IM_predios_VizinhosTotal_Inicial = GameObject.Find("TextVizinhosTotal_Inicial");
            GameObject IM_predios_texto_VizinhosInicial_Predios = GameObject.Find("TextVizinhosInicial_Predios");
            GameObject IM_predios_texto_VizinhosInicial_Ruas = GameObject.Find("TextVizinhosInicial_Ruas");

            ///vizinhos qd clickado
            GameObject IM_predios_texto_VizinhosClick_Total = GameObject.Find("TextVizinhosTotal_Click");
            GameObject IM_predios_texto_VizinhosClick_Predios = GameObject.Find("TextVizinhosClick_Predios");
            GameObject IM_predios_texto_VizinhosClick_Ruas = GameObject.Find("TextVizinhosClick_Ruas");

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
            IM_predios_VizinhosTotal_Inicial.GetComponent<Text>().text = esteNovoPredio.np_meus_vizinhos_predio.Count.ToString();
            IM_predios_texto_VizinhosInicial_Predios.GetComponent<Text>().text = esteNovoPredio.np_meus_vizinhos_predio.Count(en => en.np_nome.Contains("predio")).ToString();
            IM_predios_texto_VizinhosInicial_Ruas.GetComponent<Text>().text = esteNovoPredio.np_meus_vizinhos_rua.Count(en => en.np_nome.Contains("rua")).ToString();

            IM_predios_texto_VizinhosClick_Total.GetComponent<Text>().text = esteNovoPredio.np_meus_vizinhos_click_predio.Count.ToString();
            IM_predios_texto_VizinhosClick_Predios.GetComponent<Text>().text = esteNovoPredio.np_meus_vizinhos_click_predio.Count(en => en.np_nome.Contains("predio")).ToString();
            IM_predios_texto_VizinhosClick_Ruas.GetComponent<Text>().text = esteNovoPredio.np_meus_vizinhos_click_rua.Count(en => en.np_nome.Contains("rua")).ToString();

            int _templayer = (1 << LayerMask.NameToLayer("layer_predios"))
                           | (1 << LayerMask.NameToLayer("layer_ruas"))
//                         | (1 << LayerMask.NameToLayer("layer_lugares"))
                           ;

            Debug.Log("click campo visao qual a layer: " + _templayer);

            iso = new IsovistaP(esteNovoPredio.np_endereco, 360, InputsMorfo.input_distanciaCampoVisao, _templayer);
            iso.CampoVisao(360, InputsMorfo.input_distanciaCampoVisao);
            iso.isoMesh(iso.pontosContorno, esteNovoPredio.np_nome + "mesh");
            */

        }
        if (esteLugar) 
        {
        /*    
            _propriedades_L.SetActive(true);
            _propriedades_E_C.SetActive(false);

            Text IM_lugares_texto_Iso_Total_Obj = GameObject.Find("Valor_Iso_Total_Obj").GetComponent<Text>();
            Text IM_lugares_texto_Iso_Total_Predios = GameObject.Find("Valor_Iso_Total_Predios").GetComponent<Text>();
            Text IM_lugares_texto_Iso_Total_Ruas = GameObject.Find("Valor_Iso_Total_Ruas").GetComponent<Text>();

            Text IM_lugares_texto_Iso_Distancia_Total = GameObject.Find("Valor_Dist_Total").GetComponent<Text>();
            Text IM_lugares_texto_Iso_Distancia_Maxima = GameObject.Find("Valor_Dist_Max").GetComponent<Text>();
            Text IM_lugares_texto_Iso_Distancia_Media = GameObject.Find("Valor_Dist_Ave").GetComponent<Text>();
            Text IM_lugares_texto_Iso_Distancia_Minima = GameObject.Find("Valor_Dist_Min").GetComponent<Text>();

            Text IM_lugares_texto_Iso_Distancia_Ponderada = GameObject.Find("Valor_Media_Ponderada").GetComponent<Text>();

            int _templayer = (1 << LayerMask.NameToLayer("layer_predios"))
                           | (1 << LayerMask.NameToLayer("layer_ruas"))
                      //   | (1 << LayerMask.NameToLayer("layer_lugares"))
                           ;
            foreach (lugar l in cs_ambiente.lugaresAtivos)
            {
                l.L_CalculeIsovistas(_templayer);
            }

//            OBJ_nome.GetComponent<Text>().text = esteLugar._nome;
            obj_nome.text = esteLugar._nome;
            IM_lugares_texto_Iso_Total_Obj.text = esteLugar.total_obj_visto.Count.ToString();
            IM_lugares_texto_Iso_Total_Predios.text = esteLugar.total_predio_visto.Count.ToString();
            IM_lugares_texto_Iso_Total_Ruas.text = esteLugar.total_rua_visto.Count.ToString();

            IM_lugares_texto_Iso_Distancia_Total.text = esteLugar.normalizado_distanciaTotal.ToString();
            IM_lugares_texto_Iso_Distancia_Maxima.text = esteLugar.normalizado_distanciaMaxima.ToString();
            IM_lugares_texto_Iso_Distancia_Media.text = esteLugar.normalizado_distanciaMedia.ToString();
            IM_lugares_texto_Iso_Distancia_Minima.text = esteLugar.normalizado_distanciaMinima.ToString();

            IM_lugares_texto_Iso_Distancia_Ponderada.text = esteLugar.medida_geral_ponderada.ToString();

            _templayer = (1 << LayerMask.NameToLayer("layer_predios"))
                       | (1 << LayerMask.NameToLayer("layer_ruas"))
     //                | (1 << LayerMask.NameToLayer("layer_lugares"))
                       ;
            iso = new IsovistaP( esteLugar._endereco, 360, InputsMorfo.input_distanciaCampoVisao, _templayer);
            iso.CampoVisao(360, InputsMorfo.input_distanciaCampoVisao);
            iso.isoMesh(iso.pontosContorno, esteLugar._nome + "mesh");
*/
        }

    }

    public void Deselect()
    {
        /*
        if (esteLugar) return;
        // Reverte cor do próprio objeto
        if (objectRenderer != null)
            objectRenderer.material.color = originalColor;

        // destruir iso mesh se existir
        if (iso != null)
        {
            iso.destruirMesh();
            iso = null;
        }

        // Se foi um novoPredio selecionado, restaura cores dos vizinhos (com checagens de null)
        if (esteNovoPredio != null)
        {
            if (esteNovoPredio.np_click_vizinhanca_quina != null)
            {
                foreach (novoPredio vz in esteNovoPredio.np_click_vizinhanca_quina)
                {
                    if (vz == null) continue;
                    Renderer vz_objectRenderer = vz.GetComponent<Renderer>();
                    if (vz_objectRenderer == null) continue;
                    if (vz.tipoEspaco != null)
                        vz_objectRenderer.material.color = vz.tipoEspaco.cor;
                }
            }

            if (esteNovoPredio.np_click_vizinhanca_quina_orto != null)
            {
                foreach (novoPredio vz in esteNovoPredio.np_click_vizinhanca_quina_orto)
                {
                    if (vz == null) continue;
                    Renderer vz_objectRenderer = vz.GetComponent<Renderer>();
                    if (vz_objectRenderer == null) continue;
                    if (vz.tipoEspaco != null)
                        vz_objectRenderer.material.color = vz.tipoEspaco.cor;
                }
            }
        }

        // Se foi um lugar selecionado, não há vizinhos de novoPredio para restaurar aqui,
        // mas garantimos que o painel da UI seja escondido.
//        if (!esteLugar)
//        {
            // Não há vizinhos de novoPredio para restaurar, mas garantimos que o painel da UI seja escondido.
            if (_propriedades_E_C != null) _propriedades_E_C.SetActive(false);
            if (_propriedades_L != null) _propriedades_L.SetActive(false);
            if (obj_nome != null) obj_nome.text = "no selection";
//        }
    
        // limpa referências locais
        esteNovoPredio = null;
        esteLugar = null;
        */
    }
}
