using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using System.Linq;

//[System.Serializable]
public class Predios
{
    public bool tracking = false;

    public GameObject predioPreFab;
    public GameObject pontosPreFab;
    //    public Material avulso;
    //    public Material quarteirao;
    public string meuNome;
    public Vector3 meuEnderecoXYZ;
    public int minhaCapacidadeGente;
    public int meuTotalVizinhos;
    public float angVizinhoInicial;
    public float angVizinhoFinal;
    Mesh _meshIsovista;

    public bool quadra = false;

    /// /////////////////////////para deteccao de vizinhanca
    public int quantidadeDeRaios;  // Altere conforme necessário
    public float raio;  // Altere conforme necessário
    public List<float> lista_angulosLivres;
    public List<float> lista_angulosUsados;
    public List<Vector3> lista_vetoresLivres;

//    public HashSet<Predios> minhaListaPrediosVistos;
    public List<float> minhaListaDistanciasVistas;
    public float minhaDistanciaMaximaVista;
    public float minhaDistanciaMediaVista;
    public float minhaDistanciaMinimaVista;
    public float minhaAreaVista;

    List<Predios> lista_prediosDisponiveis;
    List<Predios> lista_todosConstruidos;

    /// <summary>
    /// sobre a vizinhanca
    /// </summary>
    int totalVizinhosPossiveis;
    List<Vector3> lista_end_vinhancaPossivelTotal;
    List<Vector3> lista_end_vinhancaOcupada;
    List<Vector3> lista_end_vinhancaDisponivel;
    List<Predios> lista_os_vizinhos;
    float distanciaAdjacencia;


    public static Vector3 half = default;


//    readonly ControleAglomeracao Controles = Terrain.activeTerrain.GetComponent<ControleAglomeracao>();
    readonly ControleAglomeracao Controles = GameObject.Find("ambiente").GetComponent<ControleAglomeracao>();

    public Predios(string nome, GameObject tipo, Vector3 endereco, int totalGente, int tVizinhos, float avI, float avF )
    {
        this.predioPreFab = tipo;
        this.meuNome = nome;
        this.meuEnderecoXYZ = endereco;
        minhaCapacidadeGente = totalGente;
        if (tracking)
        {
            Debug.Log("inicializou nome full");
        }

        Start();
    }

    public Predios(string nome, GameObject tipo, Vector3 endereco)
    {
        this.predioPreFab = tipo;
        this.meuNome = nome;
        this.meuEnderecoXYZ = endereco;
        //        minhaCapacidadeGente = totalGente;
        if (tracking)
        {
            Debug.Log("inicializou nome tipo endereco");
        }

        Start();
    }
    public Predios(string nome, GameObject tipo)
    {
        this.predioPreFab = tipo;
        this.meuNome = nome;
        //        this.meuEnderecoXYZ = endereco;
        //        minhaCapacidadeGente = totalGente;
        if (tracking)
        {
            Debug.Log("inicializou nome e tipo");
        }

        Start();
    }
    public Predios(string nome)
    {
 //       this.predioPreFab = tipo;
        this.meuNome = nome;
        //        this.meuEnderecoXYZ = endereco;
        //        minhaCapacidadeGente = totalGente;
        if (tracking)
        {
            Debug.Log("inicializou nome");
        }
        
        Start();
    }

    public void Start()
    {
        if (tracking)
        {
            Debug.Log("tracking P start");
        }
        
        lista_end_vinhancaPossivelTotal = new List<Vector3>();
        lista_end_vinhancaOcupada = new List<Vector3>();
        lista_end_vinhancaDisponivel = new List<Vector3>();
        lista_os_vizinhos = new List<Predios>();

        totalVizinhosPossiveis = (int)InputsMorfo.input_totalVizinhos;
        distanciaAdjacencia = InputsMorfo.input_distanciaAdjacencia;

        //        Debug.Log("0 half-extents: " + half);
//        half = Terrain.activeTerrain.GetComponent<ControleAglomeracao>().TiposEspacoConstruido[0].transform.localScale / 2.1f;
        half = Controles.TiposEspacoConstruido[0].transform.localScale / 2.1f;
        //      Debug.Log("1 half-extents: " + half);

        //        predioPreFab = new GameObject();

        //       Debug.Log("total de vizinhos: "+ totalVizinhosPossiveis + ", distancia de adjacencia: " + distanciaAdjacencia);
        //        preencherVizinhanca();
    }

    public void preencherVizinhanca(Vector3 _endereco_ref)
    {
        if (tracking)
        {
            Debug.Log("tracking P preencherVizinhanca");
        }

        //        Debug.Log("PREDIOS| geral total VizinhosPossiveis: " + Controles.Geral_EnderecosVizinhosPossiveis.Count);
        float _passo_angulo = (360 / (int)InputsMorfo.input_totalVizinhos) * Mathf.Deg2Rad;

        for (int i = 0; i < InputsMorfo.input_totalVizinhos; i++) //  totalVizinhosPossiveis; i++)
        {
            float angulo = _passo_angulo * i;
            float _x = Mathf.Cos(angulo) * InputsMorfo.input_distanciaAdjacencia;// distanciaAdjacencia;
            float _z = Mathf.Sin(angulo) * InputsMorfo.input_distanciaAdjacencia;//distanciaAdjacencia;

            Controles.Geral_EnderecosVizinhosPossiveis.Add(_endereco_ref + new Vector3(_x, 0, _z));
//            Debug.Log("dps" +Controles.Geral_EnderecosVizinhosPossiveis.Count + "; end: " +Controles.Geral_EnderecosVizinhosPossiveis[i]);
            //            Debug.Log("dist "+distanciaAdjacencia+"; angulo"+ angulo * Mathf.Rad2Deg+"; i" + i + "; x " + _x +"; z"+ _z + "; vizinho adicionado: " + lista_end_vinhancaPossivelTotal[i]);
        }
  //      Debug.Log("PREDIOS| atualizado geral total VizinhosPossiveis: " + Controles.Geral_EnderecosVizinhosPossiveis.Count);
    }

    public void EscolherLocalizacao()
    {
        Start();
        if (tracking)
        {
            Debug.Log("tracking P EscolherLocalizacao");
        }

        //        Debug.Log("inicializou escolher localizacao");

        //        Debug.Log("PREDIOS| espacos ja construidos: " + Controles.Geral_PrediosConstruidos.Count + "; " +
        //          "vizinhos possiveis: " + Controles.Geral_EnderecosVizinhosPossiveis.Count);

        int qualprefab = 0;
//        Controles = GameObject.Find("ambiente").GetComponent<ControleAglomeracao>();
//        GameObject[] tiposPreFab = Terrain.activeTerrain.GetComponent<ControleAglomeracao>().TiposEspacoConstruido;// ControleAglomeracao.TiposEspacoConstruido[0];//   new GameObject();// ControleAglomeracao.TiposEspacoConstruido[0];
        GameObject[] tiposPreFab = GameObject.Find("ambiente").GetComponent<ControleAglomeracao>().TiposEspacoConstruido;// ControleAglomeracao.TiposEspacoConstruido[0];//   new GameObject();// ControleAglomeracao.TiposEspacoConstruido[0];

        //predioPreFab = GameObject.Instantiate (tiposPreFab[0]);
        //predioPreFab.name = this.meuNome;
        //        Debug.Log("instanciou para inicializar. nome:" +predioPreFab.name+ " localizacao:" +predioPreFab.transform.position);

        bool collisionChecker = true;


        ///atribuicao do endereco da primeira casa
        if (Controles.Geral_PrediosConstruidos.Count == 0)
        {
            float posInicialX = Terrain.activeTerrain.terrainData.size.x / 2;
            float posInicialZ = Terrain.activeTerrain.terrainData.size.z / 2;
            meuEnderecoXYZ = new Vector3(posInicialX, 1, posInicialZ);

//            Debug.Log("1a localizacao escolhida: " + meuEnderecoXYZ );
        }
        ///desenvolvimento das demais casas
        else
        {
            ///escolher lugar baseado no tipo de escolha
            ///verificar se o endereco eh possivel:
            ///1. nao sobrepoe

            int contador = 0;
            while (collisionChecker == true){
//                Debug.Log("qtos enderecos possiveis: "+ Controles.Geral_EnderecosVizinhosPossiveis.Count);
//                meuEnderecoXYZ = seleciona_enderecoAleatorio(Controles.Geral_EnderecosVizinhosPossiveis);  ///temporario, por teste de hashset

                ///checar colisao. lembrar q se faces forem coladas, ele considera colisao, dai divisor se 2.1
                collisionChecker = sobrepor.SP_SeEhPosicaoVazia(meuEnderecoXYZ, half);
                //          Debug.Log("testando colisao de " + this.meuNome + "; #" + contador + " total de enderecos possiveis: " + Controles.Geral_EnderecosVizinhosPossiveis.Count);

                contador++;
//                Debug.Log("contador: "+ contador);
                if (contador > Controles.Geral_EnderecosVizinhosPossiveis.Count)
                {
    //                Debug.Log("estourou opcoes, valor collider: " +collisionChecker + "; contador: "+contador);
                    break;
                }
            }

        }

        //        sobrepor.sobre_por();

        ///2. nao destroi a condicao de ninguem (acesso a rua)
        ///

        //        string rua = predioPreFab.GetComponent<sobreposicoes>().SS_ChecaVizinhancadoAdjacente(meuEnderecoXYZ, predioPreFab.transform.localScale / 2.1f);
        //        Debug.Log("elemento: "+this.meuNome + "rua: " + rua);


        //        rua = P_ChecarDiferenciacaoAcessoRua();

        //        Debug.Log("PREDIOS| ANTES total de espaco construido: " + Controles.Geral_PrediosConstruidos.Count);

        SO_EspacoConstruido _tipo_espaco = ScriptableObject.CreateInstance<SO_EspacoConstruido>();

        predioPreFab = GameObject.Instantiate(Controles.TiposEspacoConstruido[0]);
        string rua = "nao";
        rua = P_ChecarDiferenciacaoAcessoRua();

        //SO_EspacoConstruido _tipo_espaco = SO_EspacoConstruido.CreateInstance(SO_EspacoConstruido);//  . _tipo_espaco = SO_EspacoConstruido.CreateInstance (so_  ();// Controles._so_construir[0]; 
        //        SO_EspacoConstruido _tipo_espaco = new SO_EspacoConstruido();// Controles._so_construir[0]; 
        Debug.Log("em predios pos checagem. rua: "+ rua);
//        rua = "nao";
        if (rua == "nao")
        {
            _tipo_espaco =  Controles._so_construir[0];
            //          qualprefab = 0;
            Controles.Geral_PrediosConstruidos.Add(this);
        }
        else if (rua == "sim") 
        {
            _tipo_espaco = Controles._so_construir[1];
            //          qualprefab = 1;
            //predioPreFab.name = "rua";
            //predioPreFab.transform.localScale -= new Vector3(0, 1.6f, 0);
            //predioPreFab.GetComponent<Renderer>().material.color = Color.white;
//            Controles.Geral_Ruas.Add(this);
                }

//        GameObject.Instantiate(predioPreFab);
//        predioPreFab.name = this.meuNome;

        predioPreFab.name = _tipo_espaco.nome;//  "rua";
        predioPreFab.transform.localScale = _tipo_espaco.escala;// -= new Vector3(0, 1.6f, 0);
        predioPreFab.GetComponent<Renderer>().material.color = _tipo_espaco.cor;// Color.white;

        //        Debug.Log("PREDIOS| DPS IF total de espaco construido: " + Controles.Geral_PrediosConstruidos.Count);
        //        Debug.Log("qualprefab: " + qualprefab);

        //        predioPreFab = tiposPreFab[qualprefab];//  Terrain.activeTerrain.GetComponent<ControleAglomeracao>().TiposEspacoConstruido[0];// ControleAglomeracao.TiposEspacoConstruido[0];//   new GameObject();// ControleAglomeracao.TiposEspacoConstruido[0];
        predioPreFab.transform.position = meuEnderecoXYZ;
        //        predioPreFab = GameObject.Instantiate(tiposPreFab[qualprefab], meuEnderecoXYZ, Quaternion.identity);

//        Controles.Geral_PrediosConstruidos.Add(this);


        preencherVizinhanca(meuEnderecoXYZ);
  //      Debug.Log("PREDIOS| elemento: " + this.meuNome + "; indo checar vizinhanca com o endereco" + this.meuEnderecoXYZ);
        //        Debug.Log("prefab: "+predioPreFab.name+"prefab tipo: "+qualprefab+"; eh rua: " + qualprefab);

        //        lista_todosConstruidos.Add(this);
        //        Debug.Log("localizacao escolhida, todos construidos: " + Terrain.activeTerrain.GetComponent<ControleAglomeracao>().Geral_PrediosConstruidos.Count + "vizinhos possiveis: "+ Terrain.activeTerrain.GetComponent<ControleAglomeracao>().Geral_VizinhosPossiveis.Count);
    }

    public string P_ChecarDiferenciacaoAcessoRua()
    {
        if (tracking)
        {
            Debug.Log("tracking P_ChecarDiferenciacaoAcessoRua");
        }

        //quais sao os vizinhos
        //quantos vizinhos tem cada vizinho
        string rua = predioPreFab.GetComponent<sobreposicoes>().SS_ChecaVizinhancadoAdjacente(meuEnderecoXYZ, half);
        Debug.Log("rua: " + rua);
        return rua;

    }

    public Vector3 seleciona_enderecoAleatorio(List<Vector3> _lista_possibilidades)
    {
        if (tracking)
        {
            Debug.Log("tracking P seleciona_enderecoAleatorio");
        }

        //Controles.Geral_VizinhosPossiveis
        int index = Random.Range(0, _lista_possibilidades.Count - 1);
        Vector3 endreturn = _lista_possibilidades[index];
//        Debug.Log("total de vizinhos possiveis: " + Controles.Geral_EnderecosVizinhosPossiveis.Count + "; endereco escolhido aleatorio: " + endreturn);
        _lista_possibilidades.RemoveAt(index);// 
        //Controles.Geral_EnderecosVizinhosPossiveis.RemoveAt(index);

        return endreturn;
    }


//    Predios p_adjacenteAleatorio()
//    {
//        if (tracking)
//        {
//            Debug.Log("tracking p_adjacenteAleatorio");
//        }

//        int index;
//        /////////////////////////////separado no teste
//        if (lista_prediosDisponiveis.Count == 0)
//        {
//            if (Controles.Geral_PrediosConstruidos.Count > 0)//   lista_todosConstruidos.Count > 0)
//            {
//                lista_prediosDisponiveis.Add(Controles.Geral_PrediosConstruidos[0]); // lista_todosConstruidos[0]);
//            }
//            //                Debug.Log(Geral_PrediosConstruidos.Count + " espacos3");
//            //                Debug.Log("nao tinha vizinho, novo vizinho atribuido");
//        }

//        index = Random.Range(0, lista_prediosDisponiveis.Count);
//        //            Debug.Log("index: " + index + "vizinhos possiveis: " + Geral_VizinhosPossiveis.Count);
//        Predios vizinhoRef = lista_prediosDisponiveis[index];
////        Debug.Log("index " + index + " casa #" + i);

//        int al = vizinhoRef.lista_angulosLivres.Count; //      Debug.Log("angulos" + al);// vizinhoRef.angulosLivres);
//        int vl = vizinhoRef.lista_vetoresLivres.Count; //    Debug.Log("vetores" + vl);// vizinhoRef.angulosLivres);

//        //se nao tem procura outro
//        if (al == 0)
//        {
//            //                Debug.Log("teste quadra");
//            //                Geral_VizinhosPossiveis.Remove(vizinhoRef);
//            return null;
//        }

//        ///se achou, cria
//        Vector3 _novaPos = Terrain.activeTerrain.GetComponent<ControleAglomeracao>().selecionaAngAleatorio(vizinhoRef,0,0);
//        //        Vector3 _novaPos = vizinhoRef.meuEnderecoXYZ + new Vector3(15, 0, 0);// = new Vector3();
//        ///seleciona aleatoriamente entre vizinhos disponiveis
//        //        _novaPos = Terrain.activeTerrain.GetComponent<ControleAglomeracao>().selecionaAngAleatorio(vizinhoRef,0,0);


//        //seleciona se eh predio ou rua
//        int tipo = 0;
//        /*       if (vizinhoRef.quadra)
//               {
//                   tipo = 1;
//       //            i--;
//               }// * 1;// 
//        */
//        int inome = Controles.Geral_PrediosConstruidos.Count; // lista_todosConstruidos.Count;


//        ///////////////////////////////////////////////////////////////////////////////////////
//        Predios _predTemp = new Predios("ec" + inome, Terrain.activeTerrain.GetComponent<ControleAglomeracao>().TiposEspacoConstruido[tipo], _novaPos) 
//        {
//            quadra = vizinhoRef.quadra
//        };
//        //_predTemp.quadra = vizinhoRef.quadra;
//        Debug.Log("predio return quadra: " + _predTemp.quadra);
//        return _predTemp;
//    }

    Vector3 p_selecionaAngAleatorio(Predios _vizinhoComV)
    {
        if (tracking)
        {
            Debug.Log("tracking p_selecionaAngAleatorio");
        }

        float angulo = _vizinhoComV.lista_angulosLivres[Random.Range(0, _vizinhoComV.lista_angulosLivres.Count)];

        float radianos = angulo * Mathf.Deg2Rad;

        float x = _vizinhoComV.meuEnderecoXYZ.x + (InputsMorfo.input_distanciaAdjacencia * Mathf.Cos(radianos));
        float z = _vizinhoComV.meuEnderecoXYZ.z + (InputsMorfo.input_distanciaAdjacencia * Mathf.Sin(radianos));

        return new Vector3(x, _vizinhoComV.meuEnderecoXYZ.y, z);
        //        Vector3 novaPos = new Vector3(x, _vizinhoComV.meuEnderecoXYZ.y, z);

    }

    // Função para calcular o ângulo formado pelos vizinhos em uma determinada distância
    public float CalcularAnguloVizinhos(/*GameObject objetoOriginal*/ List<Vector3> listaObjetos, float distanciaMaxima)
    {
        if (tracking)
        {
            Debug.Log("tracking P CalcularAnguloVizinhos");
        }

        // Posição do objeto original
        Vector3 posOriginal = this.meuEnderecoXYZ; //  transform.position;//objetoOriginal.transform.position;

        // Lista para armazenar vizinhos dentro da distância especificada
        List<Vector3> vizinhos = new List<Vector3>();
//        Debug.Log("eu " + this.meuNome + "; enderecos para teste: " + listaObjetos.Count);
//        Debug.Log("eu " + this.meuEnderecoXYZ);
        // Filtrar os vizinhos dentro da distância especificada
        foreach (var objeto in listaObjetos)
        {
//            Debug.Log("vizinho " + objeto);
            if (objeto != posOriginal/*objetoOriginal*/)
            {
                float distancia = Vector3.Distance(posOriginal, objeto/*.transform.position*/);
//                Debug.Log("distancia: " + distancia);
                if (distancia <= distanciaMaxima)
                {
                    vizinhos.Add(objeto);
                }
            }
        }

        // Calcular o vetor médio dos vizinhos
        Vector3 vetorMedio = Vector3.zero;
        foreach (var vizinho in vizinhos)
        {
//            Debug.Log("vetor atual " + vetorMedio);
//            Debug.Log("vizinho atual " + vizinho);
                vetorMedio += vizinho/*.transform.position*/ - posOriginal;
//            Debug.Log("vetor atualizado " + vetorMedio);
        }
//        Debug.Log("vetor atualizado " + vetorMedio);

        // Calcular o ângulo usando a função Atan2
        float anguloRad = Mathf.Atan2(vetorMedio.z, vetorMedio.x);
        //  Debug.Log("angulo radiano" + anguloRad);

        //float azInicial;
        //float azFinal;
        CalcularIntervaloAzimute(vizinhos, out float azInicial, out float azFinal);
        //this.angVizinhoFinal = azInicial;
        //this.angVizinhoFinal = azFinal;

        // Converter o ângulo para graus
        float anguloGraus = anguloRad * Mathf.Rad2Deg;
        Debug.Log("eu sou o " + this.meuNome+ " e tenho esses vizinhos " + vizinhos.Count);
        Debug.Log("angulo inicial " + azInicial + " angulo final: " + azFinal);
        Debug.Log("enderecos: " + listaObjetos.Count);
        //Debug.Log("total pessoas: " + GetComponent<MorfoBU>().predios.Count);
        //        Debug.Log("vizinhos: " + vizinhos);
        

        return anguloGraus;
    }


    public void CalcularIntervaloAzimute(List<Vector3> vetores, out float azimuteInicial, out float azimuteFinal)
    {
        if (tracking)
        {
            Debug.Log("tracking P CalcularIntervaloAzimute");
        }

        // Inicializar azimute inicial e final
        azimuteInicial = float.MaxValue;
        azimuteFinal = float.MinValue;

        vetores.Sort((v1, v2) => Mathf.Atan2(v1.y, v1.x).CompareTo(Mathf.Atan2(v2.y, v2.x)));
        Debug.Log("lista dos vetores: " + vetores.Count);
        if (vetores.Count < 1)
        {
            azimuteInicial = 0;
            azimuteFinal = 360;
            return;
        }

        // Calcular o azimute para cada vetor
        foreach (Vector3 vetor in vetores)
        {
            // Calcular o azimute em radianos
            float azimute = Mathf.Atan2(vetor.z, vetor.x);
           
            // Atualizar os valores mínimo e máximo
            if (azimute < azimuteInicial)
            {
                azimuteInicial = azimute;
            }

            if (azimute > azimuteFinal)
            {
                azimuteFinal = azimute;
            }
        }

        // Converter os azimutes para graus
        azimuteInicial *= Mathf.Rad2Deg;
        azimuteFinal *= Mathf.Rad2Deg;

        // Ajustar para o intervalo [0, 360)
        azimuteInicial = (azimuteInicial + 360) % 360;
        azimuteFinal = (azimuteFinal + 360) % 360;
    }

    
    public void P_DetectaVizinhos(int totalAngulos, float distObj, Vector3 enderecoAtual)
    {
        if (tracking)
        {
            Debug.Log("tracking P_DetectaVizinhos");
        }

        olhaIsovista(enderecoAtual);// distObj, enderecoAtual);
        return;

        quantidadeDeRaios = totalAngulos;
        raio = distObj;
        // Arrays para armazenar raios que atingiram e que não atingiram objetos
        RaycastHit[] raiosAtingiram = new RaycastHit[quantidadeDeRaios];
        RaycastHit[] raiosNaoAtingiram = new RaycastHit[quantidadeDeRaios];
        lista_angulosLivres = new List<float>();
        lista_angulosUsados = new List<float>();
        lista_vetoresLivres = new List<Vector3>();
        //        angulosLivres.Clear();
        //        angulosUsados.Clear();
        //        vetoresLivres.Clear();

        //vertices para mesh isovista
        List<Vector3> _verticesIsovista = new List<Vector3> { meuEnderecoXYZ }; 
        //List<Vector3> _verticesIsovista = new List<Vector3>();
        ////        meuEnderecoXYZ = enderecoAtual;
        //_verticesIsovista.Add(meuEnderecoXYZ);

        float _passoAngulo = 360f / quantidadeDeRaios;

        for (int i = 0; i < quantidadeDeRaios; i++)
        {
            float _radianos = _passoAngulo * i * Mathf.Deg2Rad;                 //angulo em funcao do indice * passo entre raios a checar

            float x = meuEnderecoXYZ.x + raio * Mathf.Cos(_radianos);              //referencia no .endereco do Predio
            float z = meuEnderecoXYZ.z + raio * Mathf.Sin(_radianos);

            Vector3 pontoNaCircunferencia = new Vector3(x, meuEnderecoXYZ.y, z);

            // Use Ray para representar um raio
            Ray raioAtual = new Ray(meuEnderecoXYZ, pontoNaCircunferencia - meuEnderecoXYZ);
            
            // Use RaycastHit para armazenar informações sobre a colisão
//            RaycastHit hitInfo;
            // Realize o teste de colisão
            if (Physics.Raycast(raioAtual, out RaycastHit hitInfo, raio) && hitInfo.collider != this.predioPreFab.GetComponent<Collider>())
            {
                // Se atingir um objeto, adicione ao array de raios que atingiram
                raiosAtingiram[i] = hitInfo;
                lista_angulosUsados.Add(_passoAngulo*i);

                _verticesIsovista.Add(hitInfo.point);

                Debug.DrawRay(meuEnderecoXYZ, pontoNaCircunferencia - meuEnderecoXYZ, Color.red, 10f);

                // Faça o que for necessário com o objeto atingido (por exemplo, acessar hitInfo.collider)
            }
            else
            {
                // Se não atingir nenhum objeto, adicione ao array de raios que não atingiram
                //                raiosNaoAtingiram[i] = raioAtual;
                raiosNaoAtingiram[i] = hitInfo;
                lista_angulosLivres.Add(_passoAngulo * i);
                Vector3 vorigem = raioAtual.origin; //raioAtual.GetPoint(raio);
                lista_vetoresLivres.Add(vorigem);

                _verticesIsovista.Add(pontoNaCircunferencia);


                Debug.DrawRay(meuEnderecoXYZ, pontoNaCircunferencia - meuEnderecoXYZ, Color.blue, 10f);
            }

        }

        if(lista_angulosLivres.Count == 1)
        {
            quadra = true;
//            Debug.Log("quadra" + quadra);
        }

  /*      foreach (Vector3 v in _verticesIsovista)
        {
            Instantiate(pontosPreFab, v, Quaternion.identity);
            //            Debug.Log("LEVEL-COLOCA PREDIOS: nome predio: "+ p.meuNome + "endereco " + p.meuEnderecoXYZ);
        }
*/

    }

    public void olhaIsovista( Vector3 enderecoAtual)// float distObj, Vector3 enderecoAtual)
    {
        if (tracking)
        {
            Debug.Log("tracking P olhaIsovista");
        }

        quantidadeDeRaios = (int)InputsMorfo.input_totalVizinhos;// Mathf.RoundToInt(360/InputsMorfo.input_totalVizinhos);// totalAngulos;
        raio = InputsMorfo.input_distanciaCampoVisao;//  distObj;
        // Arrays para armazenar raios que atingiram e que não atingiram objetos
        RaycastHit[] raiosAtingiram = new RaycastHit[quantidadeDeRaios];
        RaycastHit[] raiosNaoAtingiram = new RaycastHit[quantidadeDeRaios];
        lista_angulosLivres = new List<float>();
        lista_angulosUsados = new List<float>();
        lista_vetoresLivres = new List<Vector3>();
//        vetoresLivres = new List<Vector3>();

//        minhaListaPrediosVistos = new HashSet<Predios>();
        minhaListaDistanciasVistas = new List<float>();

    //        angulosLivres.Clear();
    //        angulosUsados.Clear();
    //        vetoresLivres.Clear();

    //vertices para mesh isovista
    List<Vector3> _verticesIsovista = new List<Vector3>();
       meuEnderecoXYZ = enderecoAtual;
    _verticesIsovista.Add(meuEnderecoXYZ);
//    _verticesIsovista.Add(enderecoAtual);

        float _passoAngulo = InputsMorfo.input_totalVizinhos; //360f / quantidadeDeRaios;

        for (int i = 0; i < quantidadeDeRaios; i++)
        {
            float _radianos = _passoAngulo * i * Mathf.Deg2Rad;                 //angulo em funcao do indice * passo entre raios a checar

            float x = meuEnderecoXYZ.x + raio * Mathf.Cos(_radianos);              //referencia no .endereco do Predio
            float z = meuEnderecoXYZ.z + raio * Mathf.Sin(_radianos);

            Vector3 pontoNaCircunferencia = new Vector3(x, meuEnderecoXYZ.y, z);

            // Use Ray para representar um raio
            Ray raioAtual = new Ray(meuEnderecoXYZ, pontoNaCircunferencia - meuEnderecoXYZ);

            // Use RaycastHit para armazenar informações sobre a colisão
//            RaycastHit hitInfo;
            
            // Realize o teste de colisão
            if (Physics.Raycast(raioAtual, out RaycastHit hitInfo, raio) && hitInfo.collider != this.predioPreFab.GetComponent<Collider>())
            {
                // Se atingir um objeto, adicione ao array de raios que atingiram
                raiosAtingiram[i] = hitInfo;
                lista_angulosUsados.Add(_passoAngulo * i);

                _verticesIsovista.Add(hitInfo.point);

                Debug.DrawRay(meuEnderecoXYZ, pontoNaCircunferencia - meuEnderecoXYZ, Color.red, 10f);

                string nome = hitInfo.collider.gameObject.name;// GetComponent<Predios>().meuNome    ;//  .collider.gameObject.name;// GetComponentInParent<GameObject>().name;//GetComponent<Predios>().meuNome;//   
                                                                                                  //                prediosVistos.Add(hitInfo.collider.GetComponent<Predios>());
                Debug.Log("nome do collider: " + nome);
                // Faça o que for necessário com o objeto atingido (por exemplo, acessar hitInfo.collider)
            }
            else
            {
                // Se não atingir nenhum objeto, adicione ao array de raios que não atingiram
                //                raiosNaoAtingiram[i] = raioAtual;
                raiosNaoAtingiram[i] = hitInfo;
                lista_angulosLivres.Add(_passoAngulo * i);
                Vector3 vorigem = raioAtual.origin; //raioAtual.GetPoint(raio);
                lista_vetoresLivres.Add(vorigem);

                _verticesIsovista.Add(pontoNaCircunferencia);


                Debug.DrawRay(meuEnderecoXYZ, pontoNaCircunferencia - meuEnderecoXYZ, Color.blue, 10f);
            }
            float _distRayCast = Vector3.Distance(hitInfo.point, meuEnderecoXYZ);
//            Debug.Log("distancias vistas!!!: " + _distRayCast + "obj1 " + hitInfo.point + " obj2 " + meuEnderecoXYZ);

//            Debug.Log("predios vistos: " + prediosVistos.Count);


            if (_distRayCast != null) { minhaListaDistanciasVistas.Add(_distRayCast); }
            //           lista_distanciasVistas.Add(_distRayCast);
            minhaDistanciaMaximaVista = minhaListaDistanciasVistas.Max();
            minhaDistanciaMediaVista = minhaListaDistanciasVistas.Average();
            minhaDistanciaMinimaVista = minhaListaDistanciasVistas.Min();
            minhaAreaVista = minhaListaDistanciasVistas.Sum() * Mathf.Cos(Mathf.Deg2Rad * InputsMorfo.input_totalVizinhos);
//            Debug.Log("total de distancias: " + lista_distanciasVistas.Count +
//                "\n distancia Max: " + distanciaMaximaVista +
//                "\n distancia Media: " + distanciaMediaVista+
//                "\n distancia Min: " + distanciaMinimaVista +
////                "\n coseno: " + Mathf.Cos(Mathf.Deg2Rad * InputsMorfo.anguloVista) +
//                "\n area vista: " + areaVista );

        }

        //     Debug.Log("distancias vistas: " + lista_distanciasVistas.Count);
        if (lista_angulosLivres.Count == 1)
        {
            quadra = true;
            //            Debug.Log("quadra" + quadra);
        }

        /*      foreach (Vector3 v in _verticesIsovista)
              {
                  Instantiate(pontosPreFab, v, Quaternion.identity);
                  //            Debug.Log("LEVEL-COLOCA PREDIOS: nome predio: "+ p.meuNome + "endereco " + p.meuEnderecoXYZ);
              }
      */

    }

}







