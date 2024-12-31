using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;
using System.Linq;
using System.Reflection;
using System;

public class novoPredio : MonoBehaviour
{
    //    ControleAglomeracao Controles = Terrain.activeTerrain.GetComponent<ControleAglomeracao>();
    ControleAglomeracao Controles;// = GameObject.Find("ambiente").GetComponent<ControleAglomeracao>();


    public static Vector3 np_half = default;
    public string np_nome;
    public Vector3 np_endereco = default;
    private lugar lugartemp;

    public List<Vector3> enderecos_da_vizinhanca;
    public int np_saldoVizinhos;
    public List<novoPredio> np_meus_vizinhos_predio;
    public List<lugar> np_meus_vizinhos_lugar;

    public List<novoPredio> np_meus_vizinhos_click_predio;
    public List<lugar> np_meus_vizinhos_click_lugar;

    public List<novoPredio> np_click_quinas;
    public List<novoPredio> np_click_vizinhanca_quina;
    public List<novoPredio> np_click_vizinhanca_quina_orto;

    /// <summary>
    /// verificar se vale mais a pena concentrar aki os scriptObj, ou faze-los de prefab
    /// </summary>


    [SerializeField] InputsMorfo valoresEntrada;
    public InputsMorfo entradas;
    // Start is called before the first frame update

    public delegate void MetodoEscolha();
    MetodoEscolha metodo_escolha;

    public SO_EspacoConstruido _tipo_espaco;


    public novoPredio( string _nome)
    {
        np_half = Controles.TiposEspacoConstruido[0].transform.localScale / 2.1f;
        np_nome = _nome;

        atribuirDelegate(Controles.Tipo_Localizacao);
        metodo_escolha();
        //        Debug.Log("novo predio distancia de controles"+Controles.TdistObj.text);
    }

    public novoPredio (string _nome, string nome_metodo)
    {
        np_nome = _nome;
        np_half = Controles.TiposEspacoConstruido[0].transform.localScale / 2.1f;

        atribuirDelegate(nome_metodo);
        metodo_escolha();
//        Debug.Log("novo predio distancia de controles"+Controles.TdistObj.text);
    }
    public novoPredio(Vector3 _endereco)
    {
        GameObject geo = GameObject.CreatePrimitive(PrimitiveType.Cube);
        geo.transform.localScale = Controles._so_construir[2].escala;
        geo.GetComponent<Renderer>().material.color = Controles._so_construir[2].cor;

        Mesh sphereMesh = Resources.GetBuiltinResource<Mesh>("Sphere.fbx");
        geo.GetComponent<MeshFilter>().mesh = sphereMesh;

            }


    public void atribuirDelegate(string _nome_metodo)
    {
        MethodInfo methodInfo = this.GetType().GetMethod(_nome_metodo, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        if (methodInfo != null)
        {
            // Cria o delegate a partir do MethodInfo
           metodo_escolha = (MetodoEscolha)Delegate.CreateDelegate(typeof(MetodoEscolha), this, methodInfo);
        }
        else
        {
            Debug.LogError("Método não encontrado: " + _nome_metodo);
        }
    }

    private void Awake()
    {
//        Debug.Log("novo predio awake");
//        Controles = Terrain.activeTerrain.GetComponent<ControleAglomeracao>();
//        Controles = GameObject.Find("ambiente").GetComponent<ControleAglomeracao>();

//        np_half = Controles.TiposEspacoConstruido[0].transform.localScale / 2.1f;
        //        pfab_meuVizinhoPossivel = Controles.possivel;

    }

    void Start()
    {
        Controles = GameObject.Find("ambiente").GetComponent<ControleAglomeracao>();
        np_half = Controles.TiposEspacoConstruido[0].transform.localScale / 2.1f;
        np_nome = "predio" + Controles.Geral_novosPrediosConstruidos.Count;
        this.gameObject.name = np_nome;

        atribuirDelegate(Controles.Tipo_Localizacao);
        metodo_escolha();
//        Controles.Geral_novosPrediosConstruidos.Add(this);
        NP_Vizinhanca(true);
        //        Debug.Log("novo predio start " + np_nome);
        NP_MeusVizinhos();
        NP_ChecaVizinhoTrancado();

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void aleatorio()
    {

        bool collisionChecker = true;
        int contador = 0;

        ///verificar se o endereco eh possivel:
        ///1. nao sobrepoe
        if (Controles.Geral_Lugares.Count <= 0) { return; }

        int _np_index = 0;

//        lugar lugartemp;
        while (collisionChecker == true)
        {
            _np_index = UnityEngine.Random.Range(0, Controles.Geral_Lugares.Count - 1);
            //      Debug.Log("geral lugares: " + Controles.Geral_Lugares.Count + "index: " + _np_index + "nome prefab: "+ Controles.Geral_Lugares[_np_index].gameObject.name);// + "lugar: " + Controles.Geral_Lugares[_np_index]._endereco);
//            np_endereco = Controles.Geral_Lugares[_np_index]._endereco;
            lugartemp = Controles.Geral_Lugares[_np_index];
            //            this.transform.position = np_endereco;
            np_endereco = lugartemp._endereco;

            ///checar colisao. lembrar q se faces forem coladas, ele considera colisao, dai divisor se 2.1
            collisionChecker = sobrepor.SP_SeEhPosicaoVazia(np_endereco, np_half);
            //Debug.Log("testando colisao de " + this.meuNome + "; #" + contador + " total de enderecos possiveis: " + Controles.Geral_EnderecosVizinhosPossiveis.Count);

            contador++;
            //Debug.Log("contador: "+ contador);
            if (contador > Controles.Geral_Lugares.Count)
            {
                //Debug.Log("estourou opcoes, valor collider: " +collisionChecker + "; contador: "+contador);
                break;
            }
        }
        Controles.Geral_Lugares.Remove(lugartemp);
        Destroy(lugartemp.gameObject);
        //Destroy(Controles.Geral_Lugares[_np_index].gameObject);
        //Controles.Geral_Lugares.RemoveAt(_np_index);
        this.transform.position = np_endereco;

    }

    void isoplace()
    {
        ///carregar todos os valores
        ///mutiplicar -> V1*p1 + V2*p2 + V3*p3..../sum(p...)
        ///crirar os pesos para multiplicacao dos pesos.
        ///
        ///for each faz uma atualizacao para todos os lugares disponiveis
        ///
        int _templayer = (1 << LayerMask.NameToLayer("layer_predios"))
//                       |
//                       (1 << LayerMask.NameToLayer("layer_ruas"))
    //                   | 
    //                   (1 << LayerMask.NameToLayer("layer_lugares"))
                         ;

        foreach (lugar l in Controles.Geral_Lugares)
        {

            l.L_CalculeIsovistas(_templayer);
        }

        ///como resolver a media dos valores agora, ja q todos normalizados
        ///
        ///medida geral ponderada ja eh o somatorio de todos os valores de acordo com o pedo atribuido pelo usuario; 
        ///index medida geral = valor aleatorio entre os q recebem o valor maximo avaliado
        float ref_medida_geral_ponderada = Controles.Geral_Lugares.Max(casa => casa.medida_geral_ponderada);
        List<lugar> lista_medida_geral_ponderada = Controles.Geral_Lugares.Where(casa => casa.medida_geral_ponderada == ref_medida_geral_ponderada).ToList();
        int _index_medida_geral_ponderada = UnityEngine.Random.Range(0, lista_medida_geral_ponderada.Count - 1);

        lugartemp = lista_medida_geral_ponderada[_index_medida_geral_ponderada];
        Debug.Log("lugares pra escolha: " + lista_medida_geral_ponderada.Count + ", escolhido: " + lugartemp._nome+ ", indice "+ _index_medida_geral_ponderada);
//        foreach (lugar l in lista_medida_geral_ponderada)//lugarComTamanhoMaximo)
//        {
////            Debug.Log(l._nome + ", medida: " + l.distanciaTotal);
//        }


        //        lugartemp = Controles.Geral_Lugares.OrderByDescending(lugartemp => lugartemp.distanciaMedia).FirstOrDefault();

        np_endereco = lugartemp._endereco;
        Controles.Geral_Lugares.Remove(lugartemp);
        Destroy(lugartemp.gameObject);
        //Destroy(Controles.Geral_Lugares[_np_index].gameObject);
        //Controles.Geral_Lugares.RemoveAt(_np_index);
        this.transform.position = np_endereco;

//        Debug.Log("verde");

    }

    public void NP_Vizinhanca(bool _gerar)
    {
        //if (tracking)
        //{
        //    Debug.Log("tracking P preencherVizinhanca");
        //}
        enderecos_da_vizinhanca = new List<Vector3>();

        float _passo_angulo = (360 / (int)InputsMorfo.input_totalVizinhos) * Mathf.Deg2Rad;

        for (int i = 0; i < InputsMorfo.input_totalVizinhos; i++) //  totalVizinhosPossiveis; i++)
        {
            float angulo = _passo_angulo * i ;
            float _x = Mathf.Cos(angulo) * InputsMorfo.input_distanciaAdjacencia;// distanciaAdjacencia;
            float _z = Mathf.Sin(angulo) * InputsMorfo.input_distanciaAdjacencia;//distanciaAdjacencia;

            Vector3 _endereco_vizinho = this.transform.position + new Vector3(_x, 0, _z);

            //            Debug.Log("criacao dos viznhos, end: " + _endereco_vizinho);
            if (_gerar) 
            { 
                Instantiate(Controles.vp, _endereco_vizinho, Quaternion.identity);
                enderecos_da_vizinhanca.Add(_endereco_vizinho);
            }
            if (!_gerar) { enderecos_da_vizinhanca.Add(this.transform.position + new Vector3(_x, 0, _z)); }


            //            enderecos.Add(_endereco_ref + new Vector3(_x, 0, _z));
            //            Debug.Log("dps" +Controles.Geral_EnderecosVizinhosPossiveis.Count + "; end: " +Controles.Geral_EnderecosVizinhosPossiveis[i]);
            //            Debug.Log("dist "+distanciaAdjacencia+"; angulo"+ angulo * Mathf.Rad2Deg+"; i" + i + "; x " + _x +"; z"+ _z + "; vizinho adicionado: " + lista_end_vinhancaPossivelTotal[i]);
        }

        //      Debug.Log("PREDIOS| atualizado geral total VizinhosPossiveis: " + Controles.Geral_EnderecosVizinhosPossiveis.Count);
    }

    public void NP_MeusVizinhos() 
        ///calcula os vizinhos existentes, quais sao rua e quais sao predio
    {
        string _tipo_rua = "1saida";// "2ruas"
        //buscar os enderecos da vizinhanca
        HashSet<Vector3> _os_endereco = new HashSet<Vector3>(enderecos_da_vizinhanca);
        //        meusVizinhos = new List<EspacoConstruido>();

        List<novoPredio> _vizinhos_local = new List<novoPredio>();

        if (!ClickSelect.click) 
        {
            np_meus_vizinhos_predio = new List<novoPredio>();
            np_meus_vizinhos_lugar = new List<lugar>();
        } 
        if (ClickSelect.click) 
        {
            np_meus_vizinhos_click_predio = new List<novoPredio>();
            np_meus_vizinhos_click_lugar = new List<lugar>();
        } 


        int vizinho_lugar = 0;
        int vizinho_predio = 0;
        //checar qual tem vizinho e qual ta livre
        foreach (Vector3 v in _os_endereco)
        {
            /////PENSAR SE LINECAST FAZ COM SELECAO DE CAMADAS
            //            Debug.Log("EC_MeusVizinhos| end: " + v + ", eu sou o " + meuNome);
            if (Physics.Linecast(np_endereco, v, out RaycastHit _em_quem))
            {
                string nome_vizinho = _em_quem.transform.gameObject.name;
                //                Debug.Log("qual espaco construido: " + nome_vizinho);

//                if (!InputsMorfo.boolRuaMaisUm && nome_vizinho.Contains("predio"))
                if (nome_vizinho.Contains("predio"))
                {
//                        Debug.Log("o viiznho eh predio");
                    vizinho_predio += 1;
//                    Debug.Log("1 click: " + ClickSelect.click + " vizinho: " + nome_vizinho);
                    if (!ClickSelect.click) { np_meus_vizinhos_predio.Add(_em_quem.transform.GetComponent<novoPredio>()); } //checar qual a funcao de meus vizinhos predio para fazer rua +1
                    if (ClickSelect.click) { np_meus_vizinhos_click_predio.Add(_em_quem.transform.GetComponent<novoPredio>()); } //checar qual a funcao de meus vizinhos predio para fazer rua +1
//                    Debug.Log("vizinhos: " + np_meus_vizinhos_predio.Count + " vizinhos click: " + np_meus_vizinhos_click_predio.Count);

                }
                else if (InputsMorfo.boolRuaMaisUm && nome_vizinho.Contains("rua"))
                {
                    ///ACERTAR PARA NAO FAZER RUAS INFINITAS
  //                  Debug.Log("o viiznho eh rua");
                    vizinho_predio += 1;
//                    Debug.Log("2 click: " + ClickSelect.click);
                    if (!ClickSelect.click) { np_meus_vizinhos_predio.Add(_em_quem.transform.GetComponent<novoPredio>()); } //checar qual a funcao de meus vizinhos predio para fazer rua +1
                    if (ClickSelect.click) { np_meus_vizinhos_click_predio.Add(_em_quem.transform.GetComponent<novoPredio>()); } //checar qual a funcao de meus vizinhos predio para fazer rua +1
                }

                if (_em_quem.transform.GetComponent<lugar>())
                {
    //                Debug.Log("o viiznho eh lugar");
                    vizinho_lugar += 1;
//                    Debug.Log("3 click: " + ClickSelect.click);
                    if (!ClickSelect.click) { np_meus_vizinhos_lugar.Add(_em_quem.transform.GetComponent<lugar>()); }
                    if (ClickSelect.click) { np_meus_vizinhos_click_lugar.Add(_em_quem.transform.GetComponent<lugar>()); }
                }

                //                Debug.Log(InputsMorfo.boolVizinhanca);

                ///REFAZER ISSO EM BREVE
                //para usar o check box e mudar o tipo de vizinhanca da rua
                
            }
            else
            {
                //                Debug.Log(" nao tem ninguem aki");

            }

        }
        NP_ChecarQuinas();

//        np_meus_vizinhos_predio = _vizinhos_local;

        //anotar a diferenca Vizinhanca - Vizinhos
//        np_saldoVizinhos = (int)InputsMorfo.input_totalVizinhos - np_meusVizinhos.Count;

        np_saldoVizinhos = (int)InputsMorfo.input_totalVizinhos - vizinho_predio;

//        Debug.Log(np_nome + "-> saldo de vizinhos: " + InputsMorfo.input_totalVizinhos + " - " + vizinho_predio + " = " + np_saldoVizinhos);

        //        EspacoConstruido t = new EspacoConstruido();


    }


    public void NP_ChecarSeEhRua(bool _viz_trancado)
    {
        //        sobrepor.sobre_por();

        ///2. nao destroi a condicao de ninguem (acesso a rua)
        ///
        //        string rua = P_ChecarDiferenciacaoAcessoRua();
        //        string rua = predioPreFab.GetComponent<sobreposicoes>().SS_ChecaVizinhancadoAdjacente(meuEnderecoXYZ, predioPreFab.transform.localScale / 2.1f);
        //        Debug.Log("elemento: "+this.meuNome + "rua: " + rua);

        string rua = "nao";

        //        rua = P_ChecarDiferenciacaoAcessoRua();

        //        Debug.Log("PREDIOS| ANTES total de espaco construido: " + Controles.Geral_PrediosConstruidos.Count);

//        SO_EspacoConstruido _tipo_espaco = ScriptableObject.CreateInstance<SO_EspacoConstruido>();
        _tipo_espaco = ScriptableObject.CreateInstance<SO_EspacoConstruido>();

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
            np_nome = _tipo_espaco.nome + Controles.Geral_novosPrediosConstruidos.Count.ToString();//  "rua";
            Controles.Geral_novosPrediosConstruidos.Add(this);
            //            Debug.Log("nome SO durante: " + _tipo_espaco.nome);
        }
        else if (rua == "sim")
        {
            //            qualprefab = 1;
            //            Debug.Log("meu nome durante false: " + meuNome);
            _tipo_espaco = Controles._so_construir[1];
            np_nome = _tipo_espaco.nome + Controles.Geral_novosPrediosRuas.Count.ToString();//  "rua";
            Controles.Geral_novosPrediosRuas.Add(this);
            //            Debug.Log("nome SO durante false: " + _tipo_espaco.nome);
//            novalayerName = "Ignore Raycast";

        }

        this.name = np_nome;
//        Debug.Log("meu nome durante dps: " + np_nome);

        //        Instantiate(_tipo_espaco, meuEnderecoXYZ, Quaternion.identity);

        gameObject.transform.localScale = _tipo_espaco.escala;// -= new Vector3(0, 1.6f, 0);
        gameObject.GetComponent<Renderer>().material.color = _tipo_espaco.cor;// Color.white;
        gameObject.layer = LayerMask.NameToLayer(_tipo_espaco.layer); 

        //        Debug.Log("PREDIOS| DPS IF total de espaco construido: " + Controles.Geral_PrediosConstruidos.Count);
        //        Debug.Log("qualprefab: " + qualprefab);

        //        this.transform.position = meuEnderecoXYZ;

        Controles.Geral_novosPrediosTotal.Add(this);


    }

    public void NP_ChecaVizinhoTrancado()
    {
        bool trancado = false;

//        Debug.Log("checando vizinho, total de vizinhos: " + np_meus_vizinhos_predio.Count );        ///////////esse count ta dando errado, dai nao checa a rua.

        this.GetComponent<Collider>().enabled = false; //para nao ser contado qd medir a vizinhanca

        foreach (novoPredio v in np_meus_vizinhos_predio)
        {
            ///checa as propriedades dos vizinhos, qts sao, se sao ruas ou casas
            if (v != null)
            {
                v.NP_MeusVizinhos();
//                Debug.Log("vizinho " + v.np_nome + "tem " + v.np_saldoVizinhos + " saidas");
                if (v.np_saldoVizinhos == 1) { trancado = true; }
            }
            else { Debug.Log("v foi destruido"); }

        }
        //      Debug.Log( np_nome +" com pelo menos 1 vizinho trancado:" + trancado);

        //////////////////////////////////////////////////////////////////////////////////////////////////////////
        bool quina = NP_ChecarQuinas();
        Debug.Log("quina: " + quina + "; trancado: " + trancado);
        trancado = trancado | quina;
        Debug.Log(" novo trancado: " + trancado);


        this.GetComponent<Collider>().enabled = true; //para ficar ativo para as proximas vizinhancas

        //definir se a rua tem 1 vizinho ou tem q 2 vizinhos
        NP_ChecarSeEhRua(trancado);

    }

    public void NP_Vizinhanca_Quina(Vector3 end_quina)
    {
        int i;
        for (i = 0; i < InputsMorfo.input_totalVizinhos; i++)
        {

        }
    }
       
    public bool NP_ChecarQuinas()
    {

        List<Vector3> _enderecos_da_vizinhanca_quina = new List<Vector3>();

        if (np_click_vizinhanca_quina != null) { np_click_vizinhanca_quina.Clear(); }
        np_click_vizinhanca_quina = new List<novoPredio>();
        if (np_click_vizinhanca_quina_orto != null) { np_click_vizinhanca_quina_orto.Clear(); }
        np_click_vizinhanca_quina_orto = new List<novoPredio>();

        float _passo_angulo = (360 / (int)InputsMorfo.input_totalVizinhos) * Mathf.Deg2Rad;
        int i;

        int _vizinhos_quina = 0;
        Vector3 _paracima = new Vector3(0, 10, 0);
        Vector3 _parabaixo = new Vector3(0, -10, 0);

        List<bool> _quinas = new List<bool>();
        bool eh_quina = false;
//        for (i = 0; i < 1; i++)

        Vector3 centro_ref = this.transform.position;  
            
        for ( i = 0; i < InputsMorfo.input_totalVizinhos; i++)
        {
            ///desenvolvendo os enderecos das quinas para cheque de quina
            float angulo = _passo_angulo * i + (45f * Mathf.Deg2Rad);
            float _x = Mathf.Sqrt(2) * Mathf.Cos(angulo) * InputsMorfo.input_distanciaAdjacencia;// distanciaAdjacencia;
            float _z = Mathf.Sqrt(2) * Mathf.Sin(angulo) * InputsMorfo.input_distanciaAdjacencia;//distanciaAdjacencia;
            Vector3 _endereco_vizinho_quina = centro_ref + new Vector3(_x, 0, _z);

//            _enderecos_da_vizinhanca_quina.Add(this.transform.position + new Vector3(_x, 0, _z));

            if (Physics.Linecast(_endereco_vizinho_quina + _paracima, _endereco_vizinho_quina + _parabaixo, out RaycastHit quina_em_quem))
            {
                string nome_vizinho = quina_em_quem.transform.gameObject.name;
                //                Debug.Log("qual espaco construido: " + nome_vizinho);

                if (nome_vizinho.Contains("predio"))
                {
//                    if (ClickSelect.click)     //para selecionar as quinas
//                    { np_click_vizinhanca_quina.Add(_em_quem.transform.GetComponent<novoPredio>()); } //checar qual a funcao de meus vizinhos predio para fazer rua +1

                    //                    NP_Vizinhanca_Quina(_endereco_vizinho);
                    float v_antes_angulo = _passo_angulo * i;
                    float v_antes_x = Mathf.Cos(v_antes_angulo) * InputsMorfo.input_distanciaAdjacencia;// distanciaadjacencia;
                    float v_antes_z = Mathf.Sin(v_antes_angulo) * InputsMorfo.input_distanciaAdjacencia;//distanciaadjacencia;
                    Vector3 v_antes_endereco_vizinho = centro_ref + new Vector3(v_antes_x, 0, v_antes_z);
                    bool _v_antes_ocupado = false;
                    if (Physics.Linecast(v_antes_endereco_vizinho + _paracima, v_antes_endereco_vizinho + _parabaixo, out RaycastHit vizinho_antes))
                    {
                        _v_antes_ocupado = vizinho_antes.transform.gameObject.name.Contains("predio");

                        if (ClickSelect.click & _v_antes_ocupado)
                        { np_click_vizinhanca_quina_orto.Add(vizinho_antes.transform.GetComponent<novoPredio>()); } //checar qual a funcao de meus vizinhos predio para fazer rua +1

                    }
                    //                  Instantiate(Controles.quinas, v_antes_endereco_vizinho, Quaternion.identity);


                    float v_dps_angulo = _passo_angulo * (i + 1);
                    float v_dps_x = Mathf.Cos(v_dps_angulo) * InputsMorfo.input_distanciaAdjacencia;// distanciaadjacencia;
                    float v_dps_z = Mathf.Sin(v_dps_angulo) * InputsMorfo.input_distanciaAdjacencia;//distanciaadjacencia;
                    Vector3 v_dps_endereco_vizinho = centro_ref + new Vector3(v_dps_x, 0, v_dps_z);
                    //                    Vector3 v_dps_endereco_vizinho = _endereco_vizinho + new Vector3(v_dps_x, 0, v_dps_z);
                    bool _v_dps_ocupado = false;
                    if (Physics.Linecast(v_dps_endereco_vizinho + _paracima, v_dps_endereco_vizinho + _parabaixo, out RaycastHit vizinho_dps))
                    {
                        _v_dps_ocupado = vizinho_dps.transform.gameObject.name.Contains("predio");
                        if (ClickSelect.click & _v_dps_ocupado)
                        { np_click_vizinhanca_quina_orto.Add(vizinho_dps.transform.GetComponent<novoPredio>()); } //checar qual a funcao de meus vizinhos predio para fazer rua +1
                    }


                    //                    Instantiate(Controles.quinas, v_dps_endereco_vizinho, Quaternion.identity);

                    //                        Debug.Log("o viiznho eh predio");

                    _vizinhos_quina += 1;
                    bool tb = !(_v_antes_ocupado | _v_dps_ocupado);
                    _quinas.Add(tb);
                    int tind = _quinas.Count;
                    Debug.Log("i: " + i + ", total quinas: " + tind);
                    Debug.Log("v antes: " + _v_antes_ocupado + "; v dps: " + _v_dps_ocupado + "; Quina: " + _quinas[tind - 1]);

                }
            }
        
        }

        //int _vizinhos_quina = 0;
        //Vector3 _paracima = new Vector3(0, 10, 0);
        //Vector3 _parabaixo = new Vector3(0, -10, 0);

        //foreach (Vector3 v in _enderecos_da_vizinhanca_quina)
        //{
        //    //            Debug.Log("EC_MeusVizinhos| end: " + v + ", eu sou o " + meuNome);
        //    if (Physics.Linecast(v+_paracima, v+_parabaixo, out RaycastHit _em_quem))
        //        {
        //            string nome_vizinho = _em_quem.transform.gameObject.name;
        //        //                Debug.Log("qual espaco construido: " + nome_vizinho);

        //        //                if (!InputsMorfo.boolRuaMaisUm && nome_vizinho.Contains("predio"))
        //        if (nome_vizinho.Contains("predio"))
        //        {
        //            //                        Debug.Log("o viiznho eh predio");
        //            _vizinhos_quina += 1;
        //            //                    Debug.Log("1 click: " + ClickSelect.click + " vizinho: " + nome_vizinho);
        //            //if (!ClickSelect.click) { np_meus_vizinhos_predio.Add(_em_quem.transform.GetComponent<novoPredio>()); } //checar qual a funcao de meus vizinhos predio para fazer rua +1
        //            //if (ClickSelect.click) { np_meus_vizinhos_click_predio.Add(_em_quem.transform.GetComponent<novoPredio>()); } //checar qual a funcao de meus vizinhos predio para fazer rua +1

        //        }
        //        //              else if (InputsMorfo.boolRuaMaisUm && nome_vizinho.Contains("rua"))
        //        //              {
        //        //                  ///ACERTAR PARA NAO FAZER RUAS INFINITAS
        //        ////                  Debug.Log("o viiznho eh rua");
        //        //                  vizinho_predio += 1;
        //        //                  //                    Debug.Log("2 click: " + ClickSelect.click);
        //        //                  if (!ClickSelect.click) { np_meus_vizinhos_predio.Add(_em_quem.transform.GetComponent<novoPredio>()); } //checar qual a funcao de meus vizinhos predio para fazer rua +1
        //        //                  if (ClickSelect.click) { np_meus_vizinhos_click_predio.Add(_em_quem.transform.GetComponent<novoPredio>()); } //checar qual a funcao de meus vizinhos predio para fazer rua +1
        //        //              }
        //    }

        //}
        ///a quina eh definida por um conjunto, diagonal ocupada + os 2 adjacentes livres.
        ///tem q monitorar isso a partir da forma
        eh_quina = _quinas.Any(valor => valor);

        // Concatenar os valores em uma linha
        string result = string.Join(", ", _quinas);
        // Imprimir no console
        Debug.Log("Valores de quina: " + result + "; Tem vizinho de quina? " + eh_quina);

///        Debug.Log("tem vizinho de quina? " + eh_quina);
//        return (_vizinhos_quina > 1);
        return (eh_quina);
    }


}
