using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;


public class lugar : MonoBehaviour, ISelecionavel
{
    public Celula minhaCelula;
    [SerializeField] public string enderecoCelula;
    //propriedades relativas interface ISelecionavel
    public bool EstaSelecionado { get; set; }
    public Color corOriginal;
    GameObject _propriedades_E_C;// = GameObject.Find("propriedades_espaco_construido");
    GameObject _propriedades_L;// = GameObject.Find("propriedades_lugar_alocado");
    IsovistaP _iso_display;


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
    public float normalizado_total_profundidade_rua;


    public float medida_geral_ponderada;

    public List<novoPredio> total_obj_visto;
    public List<novoPredio> total_predio_visto;
    public List<novoPredio> total_rua_visto;


    public lugar(Vector3 _meu_end)
    {
        _endereco = _meu_end;
        Start();
    }

    void Awake()
    {
        Controles = GameObject.Find("ambiente").GetComponent<ControleAglomeracao>();    //Controles = Terrain.activeTerrain.GetComponent<ControleAglomeracao>();
        _nome = "lugar " + Controles.Geral_Lugares.Count.ToString();
        this.gameObject.name = _nome;
    }
    void Start()
    {
        //configurando sobre interface ISelecionavel
        EstaSelecionado = false;
        _propriedades_E_C = InputsMorfo.IM_propriedades_E_C; // GameObject.Find("propriedades_espaco_construido");
        _propriedades_L = InputsMorfo.IM_propriedades_L;// GameObject.Find("propriedades_lugar_alocado");

        if (InputsMorfo.tracking)
        {
            Debug.Log("tracking lugar");
        }

// LEVADO PARA AWAKE
//        Controles = GameObject.Find("ambiente").GetComponent<ControleAglomeracao>();  // Controles = Terrain.activeTerrain.GetComponent<ControleAglomeracao>(); 

        int nova_layer = LayerMask.NameToLayer("layer_lugares");
        gameObject.layer = nova_layer;

        half = Controles.TiposEspacoConstruido[0].transform.localScale / 2.1f;

        _endereco = this.transform.position;

// LEVADO PARA AWAKE
//      _nome = "lugar " + Controles.Geral_Lugares.Count.ToString();
        contagem = Controles.contadorlugar;
        Controles.contadorlugar++;

// LEVADO PARA AWAKE
//        this.gameObject.name =  _nome;
        Controles.Geral_Lugares.Add(this);
        _indice = Controles.Geral_Lugares.IndexOf(this);
//        Debug.Log(this.name + ", start indice: " + _indice + "; lugares count: " + Controles.Geral_Lugares.Count);

        ///fazer checagem se esta sobrepondo alguem
//        L_ChecaSobrepor();

        LayerMask _templayer = LayerMask.GetMask("layer_predios", "layer_ruas"/*, "layer_lugares"*/);
//        int _templayer = (1 << LayerMask.NameToLayer("layer_predios"))
//                        | (1 << LayerMask.NameToLayer("layer_ruas"))
//                 //     | (1 << LayerMask.NameToLayer("layer_lugares"))
//                        ;
        L_CalculeIsovistas(_templayer);

    }

    // Update is called once per frame
    void Update()
    {
    }

    public void addCelula(Celula c)
    {
        minhaCelula = c;
        enderecoCelula = c.endereco.ToString();
//        Debug.Log("LUGAR: lugar " + this._nome + " recebeu celula " + enderecoCelula);
    }

    /*
    public void L_CalculeIsovistaMaxima(int _templayer)
    {
        IsovistaP iso = new IsovistaP(_endereco, 360, InputsMorfo.input_distanciaCampoVisao, _templayer);
        iso.campoVisao();
        distanciaMaxima = iso.distanciasPontosContorno.Max();

        normalizado_distanciaMaxima = iso.normalizado_distanciaMaxima;

    }*/

    public IsovistaP L_CalculeIsovistas(LayerMask _templayer)
    {

        ///substituir valores para raio_de_visao
        IsovistaP iso = new IsovistaP(_endereco, 360, InputsMorfo.input_distanciaCampoVisao, _templayer);
        iso.campoVisao();
        distanciaMaxima = iso.distanciasPontosContorno.Max();
        distanciaMinima = iso.distanciasPontosContorno.Min();
        distanciaMedia = iso.distanciasPontosContorno.Average();
        distanciaTotal = iso.distanciasPontosContorno.Sum();

        total_obj_visto = new List<novoPredio>(iso.prediosVistos);
      // substitua as 3 linhas por isto:
        var pred = iso.prediosVistos ?? new HashSet<novoPredio>();
        var ruas = iso.ruasVistos ?? new HashSet<novoPredio>();

        total_obj_visto = pred.Union(ruas).Where(p => p != null).ToList();
        total_predio_visto = pred.Where(p => p != null).ToList(); //total_predio_visto = new List<novoPredio>(iso.prediosVistos);
        total_rua_visto = ruas.Where(p => p != null).ToList();    //total_rua_visto = new List<novoPredio>(iso.ruasVistos);

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

//FAZER PARA PROFUNDIDADE DE RUA
//        if (Controles.lugar_ruas_vistos_maximo <= total_rua_visto.Count) { Controles.lugar_ruas_vistos_maximo = total_rua_visto.Count; }
//        normalizado_total_rua_visto = (float)total_rua_visto.Count / Controles.lugar_ruas_vistos_maximo;
        //        Debug.Log("normalizado predios visto: " + normalizado_total_rua_visto + ", lugar predios vistos: " + Controles.lugar_ruas_vistos_maximo + ", total ruas visto: " + total_rua_visto.Count);

        medida_geral_ponderada = InputsMorfo.peso_distanciaMaxima * normalizado_distanciaMaxima + 
                                 InputsMorfo.peso_distanciaMinima * normalizado_distanciaMinima + 
                                 InputsMorfo.peso_distanciaMedia * normalizado_distanciaMedia +
                                 InputsMorfo.peso_distanciaTotal * normalizado_distanciaTotal +
                                 InputsMorfo.peso_total_obj_visto * normalizado_totalObjVisto +         
                                 InputsMorfo.peso_total_predio_visto * normalizado_total_predio_visto +
                                 InputsMorfo.peso_total_rua_visto * normalizado_total_rua_visto + //ajustar o divisor para valor final normalizado tambem
                                 InputsMorfo.peso_total_profundidade_rua * normalizado_total_profundidade_rua; //ajustar o divisor para valor final normalizado tambem


        //        Debug.Log("medida geral ponderada: " + medida_geral_ponderada);
        //Debug.Log("medidas gerais: " + InputsMorfo.peso_distanciaMaxima * normalizado_distanciaMaxima +", "
        //                             + InputsMorfo.peso_distanciaMinima * normalizado_distanciaMinima + ", "
        //                             + InputsMorfo.peso_distanciaMedia * normalizado_distanciaMedia + ", "
        //                             + InputsMorfo.peso_distanciaTotal * normalizado_distanciaTotal + ", "
        //                             + InputsMorfo.peso_total_obj_visto * total_obj_visto.Count + ", "
        //                             + InputsMorfo.peso_total_predio_visto * total_predio_visto.Count + ", "
        //                             + InputsMorfo.peso_total_rua_visto * total_rua_visto.Count);


        return iso;
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

                seDestruir();

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
        // Usa o Collider do objeto para criar uma caixa de verificação na posição atual.
        Collider myCol = GetComponent<Collider>();
        if (myCol == null)
        {
            // sem collider, nada a checar
            return;
        }

        // Center e halfExtents da bounding box do collider
        Vector3 center = myCol.bounds.center;
        Vector3 halfExtents = myCol.bounds.extents;

        // Incluir triggers na checagem
        Collider[] overlaps = Physics.OverlapBox(center, halfExtents, transform.rotation, ~0, QueryTriggerInteraction.Collide);

        // Se encontrar qualquer outro collider que não seja ele mesmo, determinar comportamento
        foreach (var c in overlaps)
        {
            if (c == null) continue;
            if (c.gameObject == this.gameObject) continue;

            // Se for outro 'lugar', preserve a lógica existente de comparação por 'contagem'
            var outroLugar = c.GetComponent<lugar>();
            if (outroLugar != null)
            {
                // mesma regra usada no OnTriggerEnter: se este contagem for maior, marca eraoutrolugar = true (não se destrói)
                if (this.contagem > outroLugar.contagem)
                {
                    eraoutrolugar = true;
                }
                else
                {
                    eraoutrolugar = false;
                    break;
                }
            }
            else
            {
                // se for outro tipo de objeto (ocupando o mesmo espaço), considera como "ocupado"
                eraoutrolugar = true;
            }
        }

        // se não for considerado 'outro lugar' (eraoutrolugar == false), destrói este lugar
        if (!eraoutrolugar)
        {
            seDestruir();
        }
    }

    public void Select()
    {
        EstaSelecionado = true;
//        Debug.Log("lugar clicado: " + this.name +"estado: "+ EstaSelecionado);

        corOriginal = GetComponent<Renderer>().material.color;
        GetComponent<Renderer>().material.color = Color.yellow; // Exemplo de mudança visual para indicar seleção

        _propriedades_L.SetActive(true);
        _propriedades_E_C.SetActive(false);

        LayerMask _templayer = LayerMask.GetMask("layer_predios", "layer_ruas");//, "layer_lugares");

//        L_CalculeIsovistas(_templayer);
        foreach (lugar l in Controles.Geral_Lugares)
        {
            IsovistaP i = l.L_CalculeIsovistas(_templayer);
            if (l == this) _iso_display = i; //armazenar isovista do lugar selecionado para mostrar mesh depois 
        }

        //            OBJ_nome.GetComponent<Text>().text = esteLugar._nome;
        InputsMorfo.IM_obj_nome.text = _nome;
        InputsMorfo.IM_lugares_texto_Iso_Total_Obj.text = total_obj_visto.Count.ToString();
        InputsMorfo.IM_lugares_texto_Iso_Total_Predios.text = total_predio_visto.Count.ToString();
        InputsMorfo.IM_lugares_texto_Iso_Total_Ruas.text = total_rua_visto.Count.ToString();

        InputsMorfo.IM_lugares_texto_Iso_Distancia_Total.text = normalizado_distanciaTotal.ToString();
        InputsMorfo.IM_lugares_texto_Iso_Distancia_Maxima.text = normalizado_distanciaMaxima.ToString();
        InputsMorfo.IM_lugares_texto_Iso_Distancia_Media.text = normalizado_distanciaMedia.ToString();
        InputsMorfo.IM_lugares_texto_Iso_Distancia_Minima.text = normalizado_distanciaMinima.ToString();

        InputsMorfo.IM_lugares_texto_Iso_Distancia_Ponderada.text = medida_geral_ponderada.ToString();

//        _iso_display = new IsovistaP(_endereco, 360, InputsMorfo.input_distanciaCampoVisao, _templayer);
        _iso_display.campoVisao(360, InputsMorfo.input_distanciaCampoVisao);
        _iso_display.isoMesh(_iso_display.pontosContorno, _nome + "mesh");

        foreach (novoPredio p in total_obj_visto)
        {
            if (p != null) p.np_meuRenderer.material.color = p._tipo_espaco.cor_visto;
        }

    }

    public void Deselect()
    {
        EstaSelecionado = false;
//        Debug.Log("lugar clicado: " + this.name + "estado: " + EstaSelecionado);

        InputsMorfo.IM_obj_nome.text = "no selection";
        _propriedades_E_C.SetActive(false);
        _propriedades_L.SetActive(false);

        GetComponent<Renderer>().material.color = corOriginal;

        // destruir iso mesh se existir
        if (_iso_display != null)
        {
            _iso_display.destruirMesh();
            _iso_display = null;
        }

        foreach (novoPredio p in total_obj_visto)
        {
            if (p != null) p.np_meuRenderer.material.color = p._tipo_espaco.cor;
        }

    }
    public void seDestruir()
    {
        // Remover com segurança da lista de controle
        if (Controles != null && Controles.Geral_Lugares != null && Controles.Geral_Lugares.Contains(this))
        {
            Controles.Geral_Lugares.Remove(this);
        }
        minhaCelula.lugar = null; // Desassocia da célula
        Debug.Log("LUGAR: Destruindo lugar: " + _nome);
        Destroy(this.gameObject);
    }
}
