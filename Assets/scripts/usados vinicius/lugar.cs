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
    //    ControleAglomeracao GerenteAmbiente;// = GameObject.Find("ambiente").GetComponent<ControleAglomeracao>();
    private ControleAglomeracao GerenteAmbiente;// = Terrain.activeTerrain.GetComponent<ControleAglomeracao>();

    public int indiceCriacao;
    public int indiceLugar;
    public int contagem = 0;

    public Celula minhaCelula;
    [SerializeField] public string enderecoCelula;
    //propriedades relativas interface ISelecionavel
    public bool EstaSelecionado { get; set; }
    public Color corOriginal;
    GameObject _propriedades_E_C;// = GameObject.Find("propriedades_espaco_construido");
    GameObject _propriedades_L;// = GameObject.Find("propriedades_lugar_alocado");


    private float meio_EspacoConstruido = default;
    private Vector3 meio_EspacoConstruido_Vector = default;
    public Vector3 _endereco = default;
    public string _nome;
    public bool eraoutrolugar = false;

    public IsovistaP iso;                   //incorporou todas as medidas sobre isovista. 
//    public MedidasBrutas minhasMedidasBrutas;         
//    public MedidasNormalizadas medidasNormalizadas;
    public float medida_geral_ponderada;    //unica coisa q ficou fora foi essa

    bool debug_lugar = false;


    void Awake()
    {
    }

    public void Inicializar(ControleAglomeracao gerente)
    {
        GerenteAmbiente = gerente;

        if (GerenteAmbiente.TodosLugares == null)
            GerenteAmbiente.TodosLugares = new SortedDictionary<int, lugar>();

        if (GerenteAmbiente.lugaresAtivos == null)
            GerenteAmbiente.lugaresAtivos = new List<lugar>();

        indiceCriacao = GerenteAmbiente.ContadorRodadas;
        indiceLugar = GerenteAmbiente.contadorlugar;
        contagem = GerenteAmbiente.contadorlugar;
        GerenteAmbiente.contadorlugar++;

        _nome = "lugar " + indiceCriacao.ToString() + "_" + indiceLugar.ToString();
        gameObject.name = _nome;
        gameObject.layer = LayerMask.NameToLayer("layer_lugares");

        if (!GerenteAmbiente.TodosLugares.ContainsKey(indiceLugar))
            GerenteAmbiente.TodosLugares.Add(indiceLugar, this);

        if (!GerenteAmbiente.lugaresAtivos.Contains(this))
            GerenteAmbiente.lugaresAtivos.Add(this);

    }

    void Start()
    {
        if (GerenteAmbiente == null)
        {
            Debug.LogError("lugar.Start: ControleAglomeracao não inicializado.");
            return;
        }

        //configurando sobre interface ISelecionavel
        EstaSelecionado = false;
        _propriedades_E_C = InputsMorfo.IM_propriedades_E_C; // GameObject.Find("propriedades_espaco_construido");
        _propriedades_L = InputsMorfo.IM_propriedades_L;// GameObject.Find("propriedades_lugar_alocado");

        if (InputsMorfo.tracking)
        {
            Debug.Log("tracking lugar");
        }

        meio_EspacoConstruido_Vector = GerenteAmbiente.espacoConstruido.transform.localScale / 2f;
        _endereco = this.transform.position;

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
//        minhaCelula.indiceCriacaoLugar = indiceLugar;
        if (debug_lugar) Debug.Log("LUGAR: lugar " + this._nome + " recebeu celula " + enderecoCelula);
    }


    int CalcularQtdRaios()
    {
        float raioVisao = InputsMorfo.input_distanciaCampoVisao;
        Vector3 tamanho_EspacoConstruido_Vector;

        Renderer rend = GerenteAmbiente.espacoConstruido.GetComponent<Renderer>();
        if (rend == null)
        {
            Debug.LogWarning("espacoConstruido sem Renderer! Usando padrão.");
            tamanho_EspacoConstruido_Vector = new Vector3(1f, 1f, 1f);
        }
        else
        {
            // CORREÇÃO: Só atribui se rend não for null
            tamanho_EspacoConstruido_Vector = rend.bounds.size;
        }
        float tamanhoCelula = Mathf.Min(tamanho_EspacoConstruido_Vector.x, tamanho_EspacoConstruido_Vector.z);

        float espacamentoDesejado = tamanhoCelula;// * 0.5f;
        int qtd = Mathf.CeilToInt((2f * Mathf.PI * raioVisao) / espacamentoDesejado);

        debug_lugar = false;
        if (debug_lugar)
        {
            Debug.Log($"CalcularQtdRaios: raioVisao={raioVisao:F2}, " +
                      $"tamanhoVec={tamanho_EspacoConstruido_Vector}, " +
                      $"tamanhoCelula={tamanhoCelula:F2}, " +
                      $"espacamentoDesejado={espacamentoDesejado:F2}, " +
                      $"qtdCalculada={qtd}, qtdClampada={Mathf.Clamp(qtd, 36, 720)}");
        }
        debug_lugar = false;

        return Mathf.Clamp(qtd, 36, 720);
    }
    public IsovistaP L_CalculeIsovistas(LayerMask _templayer)
    {

        Renderer rend = GerenteAmbiente.espacoConstruido.GetComponent<Renderer>();

        int totalRaios = CalcularQtdRaios();
        ///substituir valores para raio_de_visao
        iso = new IsovistaP(_endereco, totalRaios, InputsMorfo.input_distanciaCampoVisao, _templayer);
        iso.CampoVisao();

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
        ///resultado iso.medidasBrutas
    }

    public void PonderarMedia()
    {
        float soma = 0f;
        float somaPesos = 0f;

        AcumularPonderacao("distMax", iso.medidasNormalizadas.distanciaMaxima, InputsMorfo.peso_distanciaMaxima, ref soma, ref somaPesos);
        AcumularPonderacao("distMin", iso.medidasNormalizadas.distanciaMinima, InputsMorfo.peso_distanciaMinima, ref soma, ref somaPesos);
        AcumularPonderacao("distMedia", iso.medidasNormalizadas.distanciaMedia, InputsMorfo.peso_distanciaMedia, ref soma, ref somaPesos);
        AcumularPonderacao("objVisto", iso.medidasNormalizadas.totalObjVisto, InputsMorfo.peso_total_obj_visto, ref soma, ref somaPesos);
        AcumularPonderacao("predio", iso.medidasNormalizadas.totalPrediosVistos, InputsMorfo.peso_total_predio_visto, ref soma, ref somaPesos);
        AcumularPonderacao("rua", iso.medidasNormalizadas.totalRuasVistas, InputsMorfo.peso_total_rua_visto, ref soma, ref somaPesos);
//        AcumularPonderacao("profundidade rua", medidasNormalizadas.ProfundidadeRua, InputsMorfo.peso_total_profundidade_rua, ref soma, ref somaPesos);
        AcumularPonderacao("area vista", iso.medidasNormalizadas.areaIsovista, InputsMorfo.peso_distanciaTotal, ref soma, ref somaPesos);

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
        if (GerenteAmbiente == null)
        {
//            GerenteAmbiente = Terrain.activeTerrain.GetComponent<ControleAglomeracao>();
            GerenteAmbiente = GameObject.Find("ambiente").GetComponent<ControleAglomeracao>();

        }

        //        Debug.Log("triggger. " + this.name + ", contagem # " + contagem + ", lugares count: " + GerenteAmbiente.lugaresAtivos.Count + ", bati num lugar " + other.name );
        //        Debug.Log("ta dentro de alguem trigger, "+ GerenteAmbiente.lugaresAtivos.Count + " lugares, eu " + this._nome + ", dentro de " + other.name);

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
                //                Debug.Log("total lugares count " + GerenteAmbiente.lugaresAtivos.Count + ", indice: " + GerenteAmbiente.lugaresAtivos.IndexOf(this) + ", " + _indice);

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
//        Debug.Log($"[SELECT {_nome}] iso={iso != null}, pontos={iso?.pontosContorno?.Count ?? -1}");


        LayerMask _templayer = LayerMask.GetMask("layer_predios", "layer_ruas");//, "layer_lugares");
//        L_CalculeIsovistas(_templayer); // <<<< (normalizador.buscarRef calcula isovista de todos, entao nao precisa)
        ///primeira varredura, atualizando valores de referencia
        ValoresReferenciaNormalizacao referencia_normalizacao = Normalizador.BuscarReferenciaNormalizacao(GerenteAmbiente.lugaresAtivos, _templayer);

//        Debug.Log($"[SELECT {_nome}] brutas: area={iso.medidasBrutas.areaIsovista:F2}, distMax={iso.medidasBrutas.distanciaMaxima:F2}");

        ///segunda varredura, referencia para atualizacoes. no caso, so atualiza o THIS
        Normalizador.NomalizarLista(new List<lugar>{this}, referencia_normalizacao);
//        Debug.Log($"[SELECT {_nome}] norm: area={iso.medidasNormalizadas.areaIsovista:F2}, distMax={iso.medidasNormalizadas.distanciaMaxima:F2}");

//        medidasNormalizadas = Normalizador.Normalizar(iso.medidasBrutas, referencia_normalizacao);
//        PonderarMedia();

        InputsMorfo.IM_obj_nome.text = _nome;
        InputsMorfo.IM_lugares_texto_Iso_Total_Obj.text = iso.medidasNormalizadas.totalObjVisto.ToString("F2") + " n / " + iso.medidasBrutas.totalObjVisto.ToString();
        InputsMorfo.IM_lugares_texto_Iso_Total_Predios.text = iso.medidasNormalizadas.totalPrediosVistos.ToString("F2") + "N / " +  iso.medidasBrutas.totalPrediosVistos.ToString();
        InputsMorfo.IM_lugares_texto_Iso_Total_Ruas.text = iso.medidasNormalizadas.totalRuasVistas.ToString("F2")  + " N / " + iso.medidasBrutas.totalRuasVistas.ToString();

        //area isovista usada no lugar de distancia total. era uma tentativa d integracao
        InputsMorfo.IM_lugares_texto_Iso_Distancia_Total.text =
            iso.medidasNormalizadas.areaIsovista.ToString("F2") + "n / " + iso.medidasBrutas.areaIsovista.ToString("F2");

        InputsMorfo.IM_lugares_texto_Iso_Distancia_Maxima.text = 
            iso.medidasNormalizadas.distanciaMaxima.ToString("F2") + "n / " + iso.medidasBrutas.distanciaMaxima.ToString("F2");
        InputsMorfo.IM_lugares_texto_Iso_Distancia_Media.text = 
            iso.medidasNormalizadas.distanciaMedia.ToString("F2") + "n / " + iso.medidasBrutas.distanciaMedia.ToString("F2");
        InputsMorfo.IM_lugares_texto_Iso_Distancia_Minima.text = 
            iso.medidasNormalizadas.distanciaMinima.ToString("F2") + "n / " + iso.medidasBrutas.distanciaMinima.ToString("F2");

        InputsMorfo.IM_lugares_texto_Iso_Profundidade_Rua.text = 
            iso.medidasNormalizadas.ProfundidadeRua.ToString("F2") + "n / " + iso.medidasBrutas.ProfundidadeRua.ToString("F2");

        InputsMorfo.IM_lugares_texto_Iso_Distancia_Ponderada.text = medida_geral_ponderada.ToString("F2");

        iso.isoMesh(iso.pontosContorno, _nome + "mesh");

        debug_lugar = false;
        if (debug_lugar) Debug.Log("produnidade rua: " + iso.medidasBrutas.ProfundidadeRua);
        debug_lugar = false;

        if (iso.npVistos != null)
        {
            foreach (novoPredio p in iso.npVistos)
            {
                if (p != null) p.np_meuRenderer.material.color = p.tipoEspaco.cor_visto;
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

        if (iso.npVistos != null)
        {
            foreach (novoPredio p in iso.npVistos)
            {
                if (p != null) p.np_meuRenderer.material.color = p.tipoEspaco.cor;
            }
        }

        // destruir iso mesh se existir
        if (iso != null)
        {
            iso.destruirMesh();
            iso = null;
        }


    }
    public void seDestruir()
    {
        if (GerenteAmbiente == null)
            GerenteAmbiente = ControleAglomeracao.Instance ?? GameObject.Find("ambiente")?.GetComponent<ControleAglomeracao>();

        if (GerenteAmbiente != null)
        {
            GerenteAmbiente.lugaresAtivos?.Remove(this);
            GerenteAmbiente.lugaresDesativados?.Remove(this);
            GerenteAmbiente.TodosLugares?.Remove(indiceLugar);
        }

        if (minhaCelula != null && minhaCelula.lugar == this)
            minhaCelula.lugar = null; // Desassocia da célula apenas na limpeza total

        if (debug_lugar) Debug.Log("LUGAR: Destruindo lugar: " + _nome);
        Destroy(this.gameObject);
    }
    public void Desativar()
    {
        if (GerenteAmbiente == null)
            GerenteAmbiente = ControleAglomeracao.Instance ?? GameObject.Find("ambiente")?.GetComponent<ControleAglomeracao>();

        if (GerenteAmbiente != null)
        {
            GerenteAmbiente.lugaresAtivos?.Remove(this);

            if (GerenteAmbiente.lugaresDesativados == null)
                GerenteAmbiente.lugaresDesativados = new List<lugar>();

            if (GerenteAmbiente.TodosLugares != null && !GerenteAmbiente.TodosLugares.ContainsKey(indiceLugar))
                GerenteAmbiente.TodosLugares.Add(indiceLugar, this);

            if (!GerenteAmbiente.lugaresDesativados.Contains(this))
                GerenteAmbiente.lugaresDesativados.Add(this);
        }

        gameObject.SetActive(false);
    }}

