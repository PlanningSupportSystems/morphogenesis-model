using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using System.Linq;


public class lugar : MonoBehaviour
{
//    ControleAglomeracao Controles;// = GameObject.Find("ambiente").GetComponent<ControleAglomeracao>();
    private ControleAglomeracao Controles;// = Terrain.activeTerrain.GetComponent<ControleAglomeracao>();
    private Vector3 half = default;
    public Vector3 _endereco = default;
    public string _nome;
    public bool eraoutrolugar = false;
    public int contagem = 0;
    private int _indice;

    public float distanciaMaxima;
    public float distanciaMinima;
    public float distanciaMedia;
    public float distanciaTotal;

    public float normalizado_distanciaTotal;
    public float normalizado_distanciaMaxima;
    public float normalizado_distanciaMedia;
    public float normalizado_distanciaMinima;

    public float normalizado_totalObjVisto;         //criar leito em Controle Aglomeracao para total de predios
    public float normalizado_total_predio_visto;
    public float normalizado_total_rua_visto;


    public float medida_geral_ponderada;

    public List<novoPredio> total_obj_visto;
    public List<novoPredio> total_predio_visto;
    public List<novoPredio> total_rua_visto;


    public lugar(Vector3 _meu_end)
    {
        _endereco = _meu_end;
        Start();
    }
    void Start()
    {
        //        Controles = Terrain.activeTerrain.GetComponent<ControleAglomeracao>();
        Controles = GameObject.Find("ambiente").GetComponent<ControleAglomeracao>();

        int nova_layer = LayerMask.NameToLayer("layer_lugares");
        gameObject.layer = nova_layer;

        half = Controles.TiposEspacoConstruido[0].transform.localScale / 2.1f;

        _endereco = this.transform.position;
        _nome = "lugar " + Controles.Geral_Lugares.Count.ToString();
        contagem = Controles.contadorlugar;
        Controles.contadorlugar++;

        this.gameObject.name =  _nome;
        Controles.Geral_Lugares.Add(this);
        _indice = Controles.Geral_Lugares.IndexOf(this);
//        Debug.Log(this.name + ", start indice: " + _indice + "; lugares count: " + Controles.Geral_Lugares.Count);

        ///fazer checagem se esta sobrepondo alguem
        L_ChecaSobrepor();

        int _templayer = (1 << LayerMask.NameToLayer("layer_predios"))
                 //                       |
                 //                       (1 << LayerMask.NameToLayer("layer_ruas"))
                 //                   | 
                 //                   (1 << LayerMask.NameToLayer("layer_lugares"))
                 ;
        L_CalculeIsovistas(_templayer);

    }

    // Update is called once per frame
    void Update()
    {
    }

    public void L_CalculeIsovistas(int _templayer)
    {

        ///substituir valores para raio_de_visao
        IsovistaP iso = new IsovistaP(_endereco, 360, InputsMorfo.input_distanciaCampoVisao, _templayer);
        iso.campoVisao();
        distanciaMaxima = iso.distanciasPontosContorno.Max();
        distanciaMinima = iso.distanciasPontosContorno.Min();
        distanciaMedia = iso.distanciasPontosContorno.Average();
        distanciaTotal = iso.distanciasPontosContorno.Sum();
        total_obj_visto = new List<novoPredio>(iso.prediosVistos);
        total_obj_visto.Union(iso.ruasVistos);//(iso.npVistos);
        total_predio_visto = new List<novoPredio>(iso.prediosVistos);
        total_rua_visto = new List<novoPredio>(iso.ruasVistos);

        normalizado_distanciaMaxima = iso.normalizado_distanciaMaxima;
        normalizado_distanciaMinima = iso.normalizado_distanciaMinima;
        normalizado_distanciaMedia = iso.normalizado_distanciaMedia;
        normalizado_distanciaTotal = iso.normalizado_distanciaTotal;

        //"ouvir" em Controle Aglomeracao para total de objetos VISTOS. isso vale para predios e para ruas tambem.
        if (Controles.lugar_obj_vistos_maximo < total_obj_visto.Count) { Controles.lugar_obj_vistos_maximo = total_obj_visto.Count; }
        normalizado_totalObjVisto = (float)total_obj_visto.Count / Controles.lugar_obj_vistos_maximo;
//        Debug.Log("normalizado obj visto: " + normalizado_totalObjVisto + ", lugar obj vistos: " +Controles.lugar_obj_vistos_maximo + ", total obj visto: "+ total_obj_visto.Count);

        if (Controles.lugar_predios_vistos_maximo <= total_predio_visto.Count) { Controles.lugar_predios_vistos_maximo = total_predio_visto.Count; }
        normalizado_total_predio_visto = (float)total_predio_visto.Count / Controles.lugar_predios_vistos_maximo;
//        Debug.Log("normalizado predios visto: " + normalizado_total_predio_visto + ", lugar predios vistos: " + Controles.lugar_predios_vistos_maximo + ", total predios visto: " + total_predio_visto.Count);

        if (Controles.lugar_ruas_vistos_maximo <= total_rua_visto.Count) { Controles.lugar_ruas_vistos_maximo = total_rua_visto.Count; }
        normalizado_total_rua_visto = (float)total_rua_visto.Count / Controles.lugar_ruas_vistos_maximo;
//        Debug.Log("normalizado predios visto: " + normalizado_total_rua_visto + ", lugar predios vistos: " + Controles.lugar_ruas_vistos_maximo + ", total ruas visto: " + total_rua_visto.Count);


        medida_geral_ponderada = InputsMorfo.peso_distanciaMaxima * normalizado_distanciaMaxima + 
                                 InputsMorfo.peso_distanciaMinima * normalizado_distanciaMinima + 
                                 InputsMorfo.peso_distanciaMedia * normalizado_distanciaMedia +
                                 InputsMorfo.peso_distanciaTotal * normalizado_distanciaTotal +
                                 InputsMorfo.peso_total_obj_visto * normalizado_totalObjVisto +         
                                 InputsMorfo.peso_total_predio_visto * normalizado_total_predio_visto + 
                                 InputsMorfo.peso_total_rua_visto * normalizado_total_rua_visto; //ajustar o divisor para valor final normalizado tambem


//        Debug.Log("medida geral ponderada: " + medida_geral_ponderada);
        //Debug.Log("medidas gerais: " + InputsMorfo.peso_distanciaMaxima * normalizado_distanciaMaxima +", "
        //                             + InputsMorfo.peso_distanciaMinima * normalizado_distanciaMinima + ", "
        //                             + InputsMorfo.peso_distanciaMedia * normalizado_distanciaMedia + ", "
        //                             + InputsMorfo.peso_distanciaTotal * normalizado_distanciaTotal + ", "
        //                             + InputsMorfo.peso_total_obj_visto * total_obj_visto.Count + ", "
        //                             + InputsMorfo.peso_total_predio_visto * total_predio_visto.Count + ", "
        //                             + InputsMorfo.peso_total_rua_visto * total_rua_visto.Count);



    }


    private void OnTriggerEnter(Collider other)
    {
        if (Controles == null)
        {
//            Controles = Terrain.activeTerrain.GetComponent<ControleAglomeracao>();
            Controles = GameObject.Find("ambiente").GetComponent<ControleAglomeracao>();

        }

        //        Debug.Log("triggger. " + this.name + ", contagem # " + contagem + ", lugares count: " + Controles.Geral_Lugares.Count + ", bati num lugar " + other.name );
        //        Debug.Log("ta dentro de alguem trigger, "+ Controles.Geral_Lugares.Count + " lugares, eu " + this._nome + ", dentro de " + other.name);

        if (other.TryGetComponent<lugar>(out lugar l))
        {
//            Debug.Log(this.name + " bati num lugar " + l.name + " #" + l.contagem);
            if (contagem > l.contagem)
            {
                eraoutrolugar = true;
//                Debug.Log(this.name + "#" + " eraoutro " + eraoutrolugar + "; "+l.name+ " "+ l.eraoutrolugar);
            }
        }
 
        if (!eraoutrolugar)
        {
            if (this.gameObject != null)
            {
//                Debug.Log("total lugares count " + Controles.Geral_Lugares.Count + ", indice: " + Controles.Geral_Lugares.IndexOf(this) + ", " + _indice);

                Controles.Geral_Lugares.Remove(this);
                Destroy(this.gameObject);

//                Debug.Log(this.name + " foi removido.");
            }
        }
    }


    private void OnCollisionEnter()
    {
        Debug.Log("ta dentro de alguem collision");

    }

    private void L_ChecaSobrepor()
    {
        RaycastHit[] hits = Physics.RaycastAll(_endereco, Vector3.up, 10);
    }
}
