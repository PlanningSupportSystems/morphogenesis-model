using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using System.Linq;


public class EspacoConstruido : MonoBehaviour
{
    public bool tracking = false;

    public GameObject pfab_Predio;
    public GameObject pfab_Rua;
    public GameObject pfab_meuVizinhoPossivel;

    //public GameObject pontosPreFab;
    ////    public Material avulso;
    ////    public Material quarteirao;
    public string meuNome;
    public Vector3 meuEnderecoXYZ;
    //public int minhaCapacidadeGente;
    //public int meuTotalVizinhos;
    //public float angVizinhoInicial;
    //public float angVizinhoFinal;
    //Mesh _meshIsovista;

    //public bool quadra = false;

    ///// /////////////////////////para deteccao de vizinhanca
    //public int quantidadeDeRaios;  // Altere conforme necessário
    //public float raio;  // Altere conforme necessário
    //public List<float> lista_angulosLivres;
    //public List<float> lista_angulosUsados;
    //public List<Vector3> lista_vetoresLivres;

    ////    public HashSet<Predios> minhaListaPrediosVistos;
    //public List<float> minhaListaDistanciasVistas;
    //public float minhaDistanciaMaximaVista;
    //public float minhaDistanciaMediaVista;
    //public float minhaDistanciaMinimaVista;
    //public float minhaAreaVista;

    //List<Predios> lista_prediosDisponiveis;
    //List<Predios> lista_todosConstruidos;

    ///// <summary>
    ///// sobre a vizinhanca
    ///// </summary>
    public int saldoVizinhos;
    //    List<Vector3> lista_end_vinhancaPossivelTotal;
    HashSet<Vector3> lista_end_vinhancaPossivelTotal;
    //List<Vector3> lista_end_vinhancaOcupada;
    //List<Vector3> lista_end_vinhancaDisponivel;
    public List<EspacoConstruido> meusVizinhos;
    public List<EspacoConstruido> meusPrediosVizinhos;
    public List<EspacoConstruido> minhasRuasVizinhas;

    public List<EspacoConstruido> meusVizinhosClick;
    public List<EspacoConstruido> meusPrediosVizinhosClick;
    public List<EspacoConstruido> minhasRuasVizinhasClick;
    //float distanciaAdjacencia;


    public static Vector3 half = default;

    ControleAglomeracao Controles;



    //    [SerializeField] InputsMorfo valoresEntrada;
    //    public InputsMorfo entradas;
    // Start is called before the first frame update

    private void Awake()
    {
        Debug.Log("espaco construido");
//        Controles = Terrain.activeTerrain.GetComponent<ControleAglomeracao>();
        Controles = GameObject.Find("ambiente").GetComponent<ControleAglomeracao>();
        half = Controles.TiposEspacoConstruido[0].transform.localScale / 2.1f;
        pfab_meuVizinhoPossivel = Controles.possivel;

    }
    public void Start()
    {
        Debug.Log("espaco construido");
        //        EC_EscolherLocalizacao();
        EC_EscolherLocalizacaoIsovista();

        lista_end_vinhancaPossivelTotal = EC_enderecoVizinhanca(meuEnderecoXYZ);
        Controles.Geral_EnderecosVizinhosPossiveis.UnionWith(lista_end_vinhancaPossivelTotal); ///vizinhanca possivel eh adicionada. tem q checar onde ela eh
                                                                                               ///utilizada e atualizada. acho q eh so adicionada

        //        Debug.Log("ola? " + valoresEntrada.teste);
        //        Debug.Log("ola??? " + InputsMorfo.ptz);

        EC_MeusVizinhos();
        //        Debug.Log("ESPACO CONSTRUIDO| geral total espacos construidos: " + Controles.Geral_Predios.Count);
        EC_ChecaVizinhoTrancado();
        //        gameObject.AddComponent<BoxCollider>().size = new Vector3(2, 2, 2);
        //EC_ChecarSeEhRua();


    }

    public void EC_ChecaVizinhoTrancado()
    {
        Debug.Log("espaco construido");

        bool trancado = false;

        this.GetComponent<Collider>().enabled = false; //para nao ser contado qd medir a vizinhanca

        foreach (EspacoConstruido v in meusVizinhos)
        {
            ///checa as propriedades dos vizinhos, qts sao, se sao ruas ou casas
            v.EC_MeusVizinhos();
            Debug.Log("vizinho " + v.meuNome + "tem " + v.saldoVizinhos + " saidas");

            if (v.saldoVizinhos == 1) { trancado = true; }
        }
        //Debug.Log( meuNome +" com pelo menos 1 vizinho trancado:" + trancado);

        this.GetComponent<Collider>().enabled = true; //para ficar ativo para as proximas vizinhancas

        //definir se a rua tem 1 vizinho ou tem q 2 vizinhos
        EC_ChecarSeEhRua(trancado);

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void EC_ChecarSeEhRua(bool _viz_trancado)
    {
        Debug.Log("espaco construido");
        //        sobrepor.sobre_por();

        ///2. nao destroi a condicao de ninguem (acesso a rua)
        ///
        //        string rua = P_ChecarDiferenciacaoAcessoRua();
        //        string rua = predioPreFab.GetComponent<sobreposicoes>().SS_ChecaVizinhancadoAdjacente(meuEnderecoXYZ, predioPreFab.transform.localScale / 2.1f);
        //        Debug.Log("elemento: "+this.meuNome + "rua: " + rua);

        string rua = "nao";

        //        rua = P_ChecarDiferenciacaoAcessoRua();

        //        Debug.Log("PREDIOS| ANTES total de espaco construido: " + Controles.Geral_PrediosConstruidos.Count);

        SO_EspacoConstruido _tipo_espaco = ScriptableObject.CreateInstance<SO_EspacoConstruido>();

        //SO_EspacoConstruido _tipo_espaco = SO_EspacoConstruido.CreateInstance(SO_EspacoConstruido);//  . _tipo_espaco = SO_EspacoConstruido.CreateInstance (so_  ();// Controles._so_construir[0]; 
        //        SO_EspacoConstruido _tipo_espaco = new SO_EspacoConstruido();// Controles._so_construir[0]; 

        if (_viz_trancado) { rua = "sim"; }
        else if (!_viz_trancado) { rua = "nao"; }
        //        rua = "nao";

        //        Debug.Log("meu nome antes: " + meuNome);
        if (rua == "nao")
        {
            //          qualprefab = 0;
            //            Debug.Log("meu nome durante: " + meuNome);
            _tipo_espaco = Controles._so_construir[0];
            meuNome = _tipo_espaco.nome + Controles.Geral_Predios.Count.ToString();//  "rua";
            Controles.Geral_Predios.Add(this);
            //            Debug.Log("nome SO durante: " + _tipo_espaco.nome);
        }
        else if (rua == "sim")
        {
            //            qualprefab = 1;
            //            Debug.Log("meu nome durante false: " + meuNome);
            _tipo_espaco = Controles._so_construir[1];
            meuNome = _tipo_espaco.nome + Controles.Geral_Ruas.Count.ToString();//  "rua";
            Controles.Geral_Ruas.Add(this);
            //            Debug.Log("nome SO durante false: " + _tipo_espaco.nome);
        }

        this.name = meuNome;
        Debug.Log("meu nome durante dps: " + meuNome);

        //        Instantiate(_tipo_espaco, meuEnderecoXYZ, Quaternion.identity);

        gameObject.transform.localScale = _tipo_espaco.escala;// -= new Vector3(0, 1.6f, 0);
        gameObject.GetComponent<Renderer>().material.color = _tipo_espaco.cor;// Color.white;

        //        Debug.Log("PREDIOS| DPS IF total de espaco construido: " + Controles.Geral_PrediosConstruidos.Count);
        //        Debug.Log("qualprefab: " + qualprefab);

        //        this.transform.position = meuEnderecoXYZ;

        Controles.Geral_TotalEspacosConstruidos.Add(this);


    }

    public void EC_EscolherLocalizacao()
    {
        if (tracking)
        {
            Debug.Log("tracking EC_EscolherLocalizacao");
            Debug.Log("EC| espacos ja construidos: " + Controles.Geral_PrediosConstruidos.Count + "; " +
              "vizinhos possiveis: " + Controles.Geral_EnderecosVizinhosPossiveis.Count);
        }

        //        gameObject.name = meuNome = "EC " + Controles.Geral_Predios.Count.ToString();//  "rua";

        bool collisionChecker = true;

        ///escolher lugar baseado no tipo de escolha
        ///verificar se o endereco eh possivel:
        ///1. nao sobrepoe

        int contador = 0;
        while (collisionChecker == true)
        {
            //Debug.Log("qtos enderecos possiveis: "+ Controles.Geral_EnderecosVizinhosPossiveis.Count);
            meuEnderecoXYZ = EC_seleciona_enderecoAleatorio(Controles.Geral_EnderecosVizinhosPossiveis);
            // meuEnderecoXYZ = new Vector3(0, 0, contador);

            ///checar colisao. lembrar q se faces forem coladas, ele considera colisao, dai divisor se 2.1
            collisionChecker = sobrepor.SP_SeEhPosicaoVazia(meuEnderecoXYZ, half);
            //Debug.Log("testando colisao de " + this.meuNome + "; #" + contador + " total de enderecos possiveis: " + Controles.Geral_EnderecosVizinhosPossiveis.Count);

            contador++;
            //Debug.Log("contador: "+ contador);
            if (contador > Controles.Geral_EnderecosVizinhosPossiveis.Count)
            {
                //Debug.Log("estourou opcoes, valor collider: " +collisionChecker + "; contador: "+contador);
                break;
            }
        }
        Controles.Geral_EnderecosVizinhosPossiveis.Remove(meuEnderecoXYZ);

        //      }


        //gameObject.transform.position = meuEnderecoXYZ;
        this.transform.position = meuEnderecoXYZ;

        //        lista_todosConstruidos.Add(this);
        //        Debug.Log("localizacao escolhida, todos construidos: " + Terrain.activeTerrain.GetComponent<ControleAglomeracao>().Geral_PrediosConstruidos.Count + "vizinhos possiveis: "+ Terrain.activeTerrain.GetComponent<ControleAglomeracao>().Geral_VizinhosPossiveis.Count);
    }

    public void EC_EscolherLocalizacaoIsovista()
    {
        Debug.Log("espaco construido");

        if (tracking)
        {
            Debug.Log("tracking EC_EscolherLocalizacao");
        }

        //        Debug.Log("EC| inicializou escolher localizacao");
        //        Debug.Log("EC| espacos ja construidos: " + Controles.Geral_PrediosConstruidos.Count + "; " +
        //          "vizinhos possiveis: " + Controles.Geral_EnderecosVizinhosPossiveis.Count);

        //        gameObject.name = meuNome = "EC " + Controles.Geral_Predios.Count.ToString();//  "rua";

        bool collisionChecker = true;

        ///escolher lugar baseado no tipo de escolha
        ///verificar se o endereco eh possivel:
        ///1. nao sobrepoe

        List<IsovistaP> _vizinhos = new List<IsovistaP>();
        int contador = 0;
        foreach (Vector3 _endereco in Controles.Geral_EnderecosVizinhosPossiveis)
        {
            IsovistaP temp = new IsovistaP(_endereco, 8, 10);
            temp.campoVisao(12, 10);
            _vizinhos.Add(temp);
        }
        IsovistaP t2 = _vizinhos.OrderByDescending(item => item.espacosVistos.Count).FirstOrDefault();
        meuEnderecoXYZ = t2.centro;
        //            HashSet<Vector3> _rankingVizinhos = _vizinhos.
        //        Debug.Log("EC lugar iso| antes, Geral_EnderecosVizinhosPossiveis: " + Controles.Geral_EnderecosVizinhosPossiveis.Count);
        Controles.Geral_EnderecosVizinhosPossiveis.Remove(meuEnderecoXYZ);
        Debug.Log("EC lugar iso| depois, Geral_EnderecosVizinhosPossiveis: " + Controles.Geral_EnderecosVizinhosPossiveis.Count);

        //        }

        //gameObject.transform.position = meuEnderecoXYZ;
        this.transform.position = meuEnderecoXYZ;

        //      Debug.Log("PREDIOS| elemento: " + this.meuNome + "; indo checar vizinhanca com o endereco" + this.meuEnderecoXYZ);
        //        Debug.Log("prefab: "+predioPreFab.name+"prefab tipo: "+qualprefab+"; eh rua: " + qualprefab);

        //        lista_todosConstruidos.Add(this);
        //        Debug.Log("localizacao escolhida, todos construidos: " + Terrain.activeTerrain.GetComponent<ControleAglomeracao>().Geral_PrediosConstruidos.Count + "vizinhos possiveis: "+ Terrain.activeTerrain.GetComponent<ControleAglomeracao>().Geral_VizinhosPossiveis.Count);
    }



    public Vector3 EC_seleciona_enderecoAleatorio(HashSet<Vector3> _lista_possibilidades)
    {
        if (tracking)
        {
            Debug.Log("tracking P seleciona_enderecoAleatorio");
        }

        //Controles.Geral_VizinhosPossiveis
        int index = Random.Range(0, _lista_possibilidades.Count - 1);

        Vector3 endreturn = new Vector3();// = _lista_possibilidades[index];
        int currentIndex = 0;
        foreach (var item in _lista_possibilidades)
        {
            if (currentIndex == index)
            {
                endreturn = item;
                _lista_possibilidades.Remove(item);// 
            }
            currentIndex++;
        }
        //            Vector3 endreturn = _lista_possibilidades[index];

        //        Debug.Log("total de vizinhos possiveis: " + Controles.Geral_EnderecosVizinhosPossiveis.Count + "; endereco escolhido aleatorio: " + endreturn);
        //        _lista_possibilidades.Remove(endreturn);// 
        //        _lista_possibilidades.RemoveAt(index);// 
        //Controles.Geral_EnderecosVizinhosPossiveis.RemoveAt(index);

        return endreturn;
    }


    public HashSet<Vector3> EC_enderecoVizinhanca(Vector3 _endereco_ref)
    {
        if (tracking)
        {
            Debug.Log("tracking P preencherVizinhanca");
        }

        HashSet<Vector3> enderecos = new HashSet<Vector3>();
        //        Debug.Log("PREDIOS| geral total VizinhosPossiveis: " + Controles.Geral_EnderecosVizinhosPossiveis.Count);
        float _passo_angulo = (360 / (int)InputsMorfo.input_totalVizinhos) * Mathf.Deg2Rad;

        for (int i = 0; i < InputsMorfo.input_totalVizinhos; i++) //  totalVizinhosPossiveis; i++)
        {
            float angulo = _passo_angulo * i;
            float _x = Mathf.Cos(angulo) * InputsMorfo.input_distanciaAdjacencia;// distanciaAdjacencia;
            float _z = Mathf.Sin(angulo) * InputsMorfo.input_distanciaAdjacencia;//distanciaAdjacencia;

            enderecos.Add(_endereco_ref + new Vector3(_x, 0, _z));
            //            Instantiate(pfab_meuVizinhoPossivel,_endereco_ref + new Vector3(_x, 0, _z), Quaternion.identity);
            //            pfab_meuVizinhoPossivel.transform.position = _endereco_ref + new Vector3(_x, 0, _z);
            //            pfab_meuVizinhoPossivel.GetComponent<MeshFilter>().mesh = Resources.GetBuiltinResource<Mesh>("Cube.fbx");
            //            Controles.Geral_EnderecosVizinhosPossiveis.Add(_endereco_ref + new Vector3(_x, 0, _z));
            //            Debug.Log("dps" +Controles.Geral_EnderecosVizinhosPossiveis.Count + "; end: " +Controles.Geral_EnderecosVizinhosPossiveis[i]);
            //            Debug.Log("dist "+distanciaAdjacencia+"; angulo"+ angulo * Mathf.Rad2Deg+"; i" + i + "; x " + _x +"; z"+ _z + "; vizinho adicionado: " + lista_end_vinhancaPossivelTotal[i]);
        }

        return enderecos;
        //      Debug.Log("PREDIOS| atualizado geral total VizinhosPossiveis: " + Controles.Geral_EnderecosVizinhosPossiveis.Count);
    }

    public void EC_MeusVizinhos()
    {
        string _tipo_rua = "1saida";// "2ruas"
        //buscar os enderecos da vizinhanca
        HashSet<Vector3> _os_endereco = EC_enderecoVizinhanca(meuEnderecoXYZ);
        //        meusVizinhos = new List<EspacoConstruido>();

        List<EspacoConstruido> _vizinhos_local = new List<EspacoConstruido>();


        //checar qual tem vizinho e qual ta livre
        foreach (Vector3 v in _os_endereco)
        {

            //            Debug.Log("EC_MeusVizinhos| end: " + v + ", eu sou o " + meuNome);
            if (Physics.Linecast(meuEnderecoXYZ, v, out RaycastHit _em_quem))
            {
                string nome = _em_quem.transform.gameObject.name;
                //              Debug.Log("qual espaco construido: " + _em_quem.transform.GetComponent<EspacoConstruido>().meuNome);

                //                Debug.Log(InputsMorfo.boolVizinhanca);
                if (!InputsMorfo.boolRuaMaisUm && nome.Contains("predio"))  //para usar o check box e mudar o tipo de vizinhanca da rua
                {
                    _vizinhos_local.Add(_em_quem.transform.GetComponent<EspacoConstruido>());
                    //                   meusVizinhos.Add(_em_quem.transform.GetComponent<EspacoConstruido>());
                }
                else if (InputsMorfo.boolRuaMaisUm)
                {
                    _vizinhos_local.Add(_em_quem.transform.GetComponent<EspacoConstruido>());
                    //                   meusVizinhos.Add(_em_quem.transform.GetComponent<EspacoConstruido>());
                }

            }
            else
            {
                //                Debug.Log(" nao tem ninguem aki");

            }

        }

        if (ClickSelect.click)
        {
            meusVizinhosClick = _vizinhos_local;
            meusPrediosVizinhosClick = meusVizinhosClick.Where(ec => ec.meuNome.Contains("pre")).ToList();
            minhasRuasVizinhasClick = meusVizinhosClick.Where(ec => ec.meuNome.Contains("rua")).ToList();
            //            Debug.Log("meus vizinhos predios: " + meusPrediosVizinhosClick.Count);

            IsovistaP _totalVistos1 = new IsovistaP(meuEnderecoXYZ, 8, 10);
            _totalVistos1.campoVisao(12, 10);
            _totalVistos1.isoMesh(_totalVistos1.pontosContorno, meuNome + "mesh");

            //            vistosMedia = AddValueAndGetAverage(_totalVistos1.objVistos.Count);
            int _espacoVistos = _totalVistos1.espacosVistos.Count;
            int _objVistos = _totalVistos1.objVistos.Count;
            float _distMax = _totalVistos1.distanciaMaxima;
            float _distMedia = _totalVistos1.distanciaMedia;
            Debug.Log("isovistas, total obj visto: " + _objVistos + "espacos unicos vistos: " + _espacoVistos + ", dist Max: " + _distMax + ", dist Media: " + _distMedia);

        }
        else if (!ClickSelect.click)
        {
            meusVizinhos = _vizinhos_local;
            meusPrediosVizinhos = meusVizinhos.Where(ec => ec.meuNome.Contains("pre")).ToList();
            minhasRuasVizinhas = meusVizinhos.Where(ec => ec.meuNome.Contains("rua")).ToList();
        }

        //anotar a diferenca Vizinhanca - Vizinhos
        saldoVizinhos = (int)InputsMorfo.input_totalVizinhos - meusVizinhos.Count;

        Debug.Log(meuNome + "-> saldo de vizinhos: " + InputsMorfo.input_totalVizinhos + " - " + meusVizinhos.Count + " = " + saldoVizinhos);

        //        EspacoConstruido t = new EspacoConstruido();


    }


}
