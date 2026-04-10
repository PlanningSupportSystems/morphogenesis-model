using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
//using UnityEditor.Experimental.GraphView;
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

    //    ControleAglomeracao Controles;// = GameObject.Find("ambiente").GetComponent<ControleAglomeracao>();
    private ControleAglomeracao Controles;// = Terrain.activeTerrain.GetComponent<ControleAglomeracao>();

    private Vector3 half = default;
    public Vector3 _endereco = default;
    public string _nome;
    public bool eraoutrolugar = false;
    public int contagem = 0;
    private int _indice;

    public IsovistaP iso;
    public MedidasBrutas minhasMedidasBrutas;
    public MedidasNormalizadas medidasNormalizadas;
    public float medida_geral_ponderada;

    public bool debug_lugar = false;

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
        if (debug_lugar) Debug.Log("LUGAR: lugar " + this._nome + " recebeu celula " + enderecoCelula);
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
        iso = new IsovistaP(_endereco, 360, InputsMorfo.input_distanciaCampoVisao, _templayer);
        iso.campoVisao();


        if (debug_lugar)
        {
            //        Debug.Log("medida geral ponderada: " + medida_geral_ponderada);
            //Debug.Log("medidas gerais: " + InputsMorfo.peso_distanciaMaxima * normalizado_distanciaMaxima +", "
            //                             + InputsMorfo.peso_distanciaMinima * normalizado_distanciaMinima + ", "
            //                             + InputsMorfo.peso_distanciaMedia * normalizado_distanciaMedia + ", "
            //                             + InputsMorfo.peso_distanciaTotal * normalizado_distanciaTotal + ", "
            //                             + InputsMorfo.peso_total_obj_visto * total_obj_visto.Count + ", "
            //                             + InputsMorfo.peso_total_predio_visto * total_predio_visto.Count + ", "
            //                             + InputsMorfo.peso_total_rua_visto * total_rua_visto.Count);
        }

        return iso;
    }

    public void PonderarMedia()
    {
        float soma = 0f;
        float somaPesos = 0f;

        AcumularPonderacao("distMax", medidasNormalizadas.distanciaMaxima, InputsMorfo.peso_distanciaMaxima, ref soma, ref somaPesos);
        AcumularPonderacao("distMin", medidasNormalizadas.distanciaMinima, InputsMorfo.peso_distanciaMinima, ref soma, ref somaPesos);
        AcumularPonderacao("distMedia", medidasNormalizadas.distanciaMedia, InputsMorfo.peso_distanciaMedia, ref soma, ref somaPesos);
        AcumularPonderacao("objVisto", medidasNormalizadas.totalObjVisto, InputsMorfo.peso_total_obj_visto, ref soma, ref somaPesos);
        AcumularPonderacao("predio", medidasNormalizadas.totalPrediosVistos, InputsMorfo.peso_total_predio_visto, ref soma, ref somaPesos);
        AcumularPonderacao("rua", medidasNormalizadas.totalRuasVistas, InputsMorfo.peso_total_rua_visto, ref soma, ref somaPesos);
//        AcumularPonderacao("profundidade rua", medidasNormalizadas.ProfundidadeRua, InputsMorfo.peso_total_profundidade_rua, ref soma, ref somaPesos);
        AcumularPonderacao("area vista", medidasNormalizadas.areaIsovista, InputsMorfo.peso_distanciaTotal, ref soma, ref somaPesos);

        medida_geral_ponderada = (somaPesos > 0f) ? soma / somaPesos : 0f;
    }
    void AcumularPonderacao(string nome, float valor, float peso, ref float soma, ref float somaPesos)
    {
        if (peso <= 0f) {
            if (debug_lugar) Debug.Log($"{nome} IGNORADO (peso = 0)");
            return;
        }
        float contribuicao = valor * peso;
        soma += valor * peso;
        somaPesos += peso;

        debug_lugar = false;
        if (debug_lugar) Debug.Log(
                        $"{nome} | valor: {valor:F3} | peso: {peso:F3} | contrib: {contribuicao:F3} " +
                        $"| soma: {soma:F3} | somaPesos: {somaPesos:F3}"
        );
        debug_lugar = false;
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

        // ===== PRIMEIRO LUGAR ===== para setar o valor de referencia minimo e maxim sem problemas
        lugar primeiroLugar = Controles.Geral_Lugares[0];
        primeiroLugar.L_CalculeIsovistas(_templayer);   // calculou primeiroLugar.minhasMedidasBrutas

        ValoresReferenciaNormalizacao referencia_normalizacao = new ValoresReferenciaNormalizacao(primeiroLugar.iso.medidasBrutas);// minhasMedidasBrutas);

        // ===== RESTANTE DA PRIMEIRA VARREDURA =====
        for (int i = 1; i < Controles.Geral_Lugares.Count; i++)
        {
            lugar lugar = Controles.Geral_Lugares[i];
            lugar.L_CalculeIsovistas(_templayer);
            Normalizador.ChecarSeReferencia(ref referencia_normalizacao, lugar.iso.medidasBrutas);// minhasMedidasBrutas);
        }

        // ===== SEGUNDA VARREDURA: NORMALIZAR =====
        for (int i = 0; i < Controles.Geral_Lugares.Count; i++)
        {
            lugar lugar = Controles.Geral_Lugares[i];
            medidasNormalizadas = Normalizador.Normalizar(lugar.iso.medidasBrutas/*minhasMedidasBrutas*/, referencia_normalizacao);
            PonderarMedia();
        }

        //            OBJ_nome.GetComponent<Text>().text = esteLugar._nome;
        InputsMorfo.IM_obj_nome.text = _nome;
        InputsMorfo.IM_lugares_texto_Iso_Total_Obj.text = /*minhasMedidasBrutas*/ iso.medidasBrutas.totalObjVisto.ToString() + " / " + medidasNormalizadas.totalObjVisto.ToString("F2");// total_obj_visto.Count.ToString();
        InputsMorfo.IM_lugares_texto_Iso_Total_Predios.text = /*minhasMedidasBrutas */ iso.medidasBrutas.totalPrediosVistos.ToString() + " / " + medidasNormalizadas.totalPrediosVistos.ToString("F2");//  total_predio_visto.Count.ToString();
        InputsMorfo.IM_lugares_texto_Iso_Total_Ruas.text = /*minhasMedidasBrutas */ iso.medidasBrutas.totalRuasVistas.ToString() + " / " + medidasNormalizadas.totalRuasVistas.ToString("F2");// total_rua_visto.Count.ToString();

        //area isovista usada no lugar de distancia total. era uma tentativa d integracao
        InputsMorfo.IM_lugares_texto_Iso_Distancia_Total.text = medidasNormalizadas.areaIsovista.ToString("F2") + " / " + /*minhasMedidasBrutas */ iso.medidasBrutas.areaIsovista.ToString("F2");// normalizado_distanciaTotal.ToString("F2");

        InputsMorfo.IM_lugares_texto_Iso_Distancia_Maxima.text = medidasNormalizadas.distanciaMaxima.ToString("F2") + " / " + /*minhasMedidasBrutas */ iso.medidasBrutas.distanciaMaxima.ToString("F2");// normalizado_distanciaMaxima.ToString();
        InputsMorfo.IM_lugares_texto_Iso_Distancia_Media.text = medidasNormalizadas.distanciaMedia.ToString("F2") + " / " + /*minhasMedidasBrutas */ iso.medidasBrutas.distanciaMedia.ToString("F2");// normalizado_distanciaMedia.ToString();
        InputsMorfo.IM_lugares_texto_Iso_Distancia_Minima.text = medidasNormalizadas.distanciaMinima.ToString("F2") + " / " + /*minhasMedidasBrutas */ iso.medidasBrutas.distanciaMinima.ToString("F2");// normalizado_distanciaMinima.ToString();

        InputsMorfo.IM_lugares_texto_Iso_Profundidade_Rua.text = medidasNormalizadas.ProfundidadeRua.ToString("F2") + " / " + /*minhasMedidasBrutas */ iso.medidasBrutas.ProfundidadeRua.ToString("F2");// normalizado_total_profundidade_Rua.ToString();

        InputsMorfo.IM_lugares_texto_Iso_Distancia_Ponderada.text = medida_geral_ponderada.ToString("F2");

        //        iso = new IsovistaP(_endereco, 360, InputsMorfo.input_distanciaCampoVisao, _templayer);
//        iso = L_CalculeIsovistas(_templayer);
//        iso.campoVisao(360, InputsMorfo.input_distanciaCampoVisao);
        iso.isoMesh(iso.pontosContorno, _nome + "mesh");

        debug_lugar = true;
        if (debug_lugar) Debug.Log("produnidade rua: " + iso.medidasBrutas.ProfundidadeRua);// total_profundidade_Rua);
        debug_lugar = false;

        if (iso.prediosVistos != null)
        {
            foreach (novoPredio p in iso.prediosVistos)
            {
                if (p != null) p.np_meuRenderer.material.color = p._tipo_espaco.cor_visto;
            }
        }

        if (iso.ruasVistos != null)
        {
            foreach (novoPredio p in iso.ruasVistos)
            {
                if (p != null) p.np_meuRenderer.material.color = p._tipo_espaco.cor_visto;
            }
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
        if (iso != null)
        {
            iso.destruirMesh();
            iso = null;
        }

        if (iso.prediosVistos != null)
        {
            foreach (novoPredio p in iso.prediosVistos)
            {
                if (p != null) p.np_meuRenderer.material.color = p._tipo_espaco.cor;
            }
        }
        if (iso.ruasVistos != null)
        {
            foreach (novoPredio p in iso.ruasVistos)
            {
                if (p != null) p.np_meuRenderer.material.color = p._tipo_espaco.cor;
            }

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
        if (debug_lugar) Debug.Log("LUGAR: Destruindo lugar: " + _nome);
        Destroy(this.gameObject);
    }
}
