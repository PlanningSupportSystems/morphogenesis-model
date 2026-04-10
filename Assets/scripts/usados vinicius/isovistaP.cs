using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;


public class IsovistaP
{
    //geradoras
    public Vector3 centro;
    public int totalRaios;
    public float raioVisao;
    public GameObject portaMesh;
    public LayerMask layerMask = LayerMask.GetMask("layer_predios");

    //gerados
    public List<Vector3> pontosContorno;
    public List<float> distanciasPontosContorno;

    //medidas
    public MedidasBrutas medidasBrutas;
    public MedidasNormalizadas medidasNormalizadas;
    public float distanciaMaxima;
    public List<int> indexMax;
    public float distanciaMedia;
    public float distanciaMinima;
    public List<int> indexMin;
    public float distanciaTotal;
    public Mesh isovistamesh;
    public List <RaycastHit> objVistos;
//    public HashSet<EspacoConstruido> espacosVistos;
    public HashSet<novoPredio> npVistos;
    public HashSet<novoPredio> prediosVistos;
    public HashSet<novoPredio> ruasVistos;
    // profundidade de ruas (por raio) e índices dos raios com profundidade máxima
    public List<float> profundidadesPorRaio;
    public float profundidadeMaximaRua;
    public List<int> indicesProfundidadeMaxima;
    public float areaIsovista;

    public float normalizado_distanciaMaxima;
    public float normalizado_distanciaMedia;
    public float normalizado_distanciaMinima;
    public float normalizado_distanciaTotal;

    public float maximo_objetos_vistos;

    public bool debug_iso = false;
    //medidas, nova definicao

    public IsovistaP(Vector3 _centro, int _totalRaios, float _raioVisao)
    {
        centro = _centro;
        totalRaios = _totalRaios;
        raioVisao = _raioVisao;
        layerMask = LayerMask.GetMask("layer_predios", "layer_ruas");//, "layer_lugares");
//        layerMask = (1 << LayerMask.NameToLayer("layer_predios"))
//                | (1 << LayerMask.NameToLayer("layer_ruas"))
//                //| (1 << LayerMask.NameToLayer("layer_lugares"))
                ;

    }

    public IsovistaP(Vector3 _centro, int _totalRaios, float _raioVisao, LayerMask _layerMask)
    {
        centro = _centro;
        totalRaios = _totalRaios;
        raioVisao = _raioVisao;
        layerMask = _layerMask;
//        Debug.Log("campo visao qual a layer: " + layerMask);

    }


    public void campoVisao(int _qtdRaios = 0, float _raio = 0, Vector3 _centroIsovista = default(Vector3))  //  ( 0,10000,0))// (0,10000,0))
    {

        if (InputsMorfo.tracking)
        {
            Debug.Log("tracking isovistaP");
        }

        //permitir chamada sem construtores
        if (_qtdRaios == 0) { _qtdRaios = totalRaios; }
        if (_raio == 0) { _raio = raioVisao; }
        if (_centroIsovista == default(Vector3)) { _centroIsovista = centro; }

        //        Debug.Log("centro isovista: " + _centroIsovista);

        // Arrays para armazenar raios que atingiram e que não atingiram objetos
        //        RaycastHit[] raiosAtingiram = new RaycastHit[_qtdRaios];          //substituido por list pra virar public
        objVistos = new List<RaycastHit>();
//        espacosVistos = new HashSet<EspacoConstruido>();
        npVistos = new HashSet<novoPredio>();
        RaycastHit[] raiosNaoAtingiram = new RaycastHit[_qtdRaios];
        List<float> angulosLivres = new List<float>();
        List<float> angulosUsados = new List<float>();
        List<Vector3> vetoresLivres = new List<Vector3>();

        //vertices para mesh isovista
        List<Vector3> _verticesIsovista = new List<Vector3>();
        _verticesIsovista.Add(_centroIsovista);
        // inicializar tracking de profundidade de ruas
        profundidadesPorRaio = new List<float>(_qtdRaios);
        profundidadeMaximaRua = 0;
        indicesProfundidadeMaxima = new List<int>();
        
        float _passoAngulo = 360f / _qtdRaios;

//        Debug.Log("campo visao qual a layer: " + layerMask);
        for (int i = 0; i < _qtdRaios; i++)
        {
            float _radianos = _passoAngulo * i * Mathf.Deg2Rad;                 //angulo em funcao do indice * passo entre raios a checar
            float _raio_graus = _passoAngulo * i;                               //angulo em graus, para guardar quais angulos foram usados ou livres

            float x = _centroIsovista.x + _raio * Mathf.Cos(_radianos);              //referencia no .endereco do Predio
            float z = _centroIsovista.z + _raio * Mathf.Sin(_radianos);

            Vector3 pontoNaCircunferencia = new Vector3(x, _centroIsovista.y, z);

            // Use Ray para representar um raio
            Ray raioAtual = new Ray(_centroIsovista, pontoNaCircunferencia - _centroIsovista);

            // Realize o teste de colisão
            ProcessarRaioIsovista
                (raioAtual, _raio, _raio_graus, layerMask, _centroIsovista, pontoNaCircunferencia,


                                  npVistos, 
                                  angulosUsados,
                                  _verticesIsovista,
                                  angulosLivres,
                                  profundidadesPorRaio
                );
           
        }

        pontosContorno = _verticesIsovista;

        // dentro de campoVisao(), depois de pontosContorno = _verticesIsovista;
        areaIsovista = CalcularAreaIsovistaXZ(pontosContorno, 1);
        if (debug_iso) Debug.Log("area isovista foi de: " + areaIsovista);// 1 pula o centro
        // opcional: normalizar pela área de um círculo com mesmo raio
        //float areaMaxima = Mathf.PI * _raio * _raio;
        //float normalizado_area = areaMaxima > 0f ? areaIsovista / areaMaxima : 0f;

        distanciasPontosContorno = new List<float>();
        foreach(Vector3 pC in pontosContorno)
        {
            distanciasPontosContorno.Add(Vector3.Distance(_centroIsovista, pC));
        }
  //      Debug.Log("distanciass ISOVISTA " + distanciasPontosContorno.Count + $"[{string.Join(",", distanciasPontosContorno)}]");

        distanciaMaxima = distanciasPontosContorno.Max();
        distanciaMinima= distanciasPontosContorno.Min();
        distanciaMedia = distanciasPontosContorno.Average();
        distanciaTotal = distanciasPontosContorno.Sum();

        //nromalizar os valores encontrados todos
        //carregar valores maximo (_raio, ou _raio* _qtdRaios para distancia total) e minimo das medidas feitas
        normalizado_distanciaMaxima = distanciaMaxima / _raio;
        normalizado_distanciaMinima = distanciaMinima / _raio;
        normalizado_distanciaMedia = distanciaMedia / _raio;
        normalizado_distanciaTotal = distanciaTotal / (_raio * _qtdRaios);
//        Debug.Log("isovista normalizar distancia total: " + normalizado_distanciaTotal + ", distancia total: " + distanciaTotal + ", raio: "+ _raio +", qtd raios: "+ _qtdRaios);

        //        indexMax = distanciasPontosContorno.FindAll(dmax => dmax == distanciaMaxima);
        indexMax = new List<int>();
        List<float> asDistMax = new List<float>();
        for (int i = 0; i < distanciasPontosContorno.Count; i++)
        {
            if (distanciasPontosContorno[i] == distanciaMaxima)
            {
                indexMax.Add(i);
                asDistMax.Add(distanciasPontosContorno[i]);
            }
        }

        //indexMin = distanciasPontosContorno.FindAll(dmin => dmin == distanciaMaxima);
        indexMin = new List<int>();
        //indexMin.Clear();
        for (int i = 0; i < distanciasPontosContorno.Count; i++)
        {
            if (distanciasPontosContorno[i] == distanciaMinima)
            {
                indexMin.Add(i);
            }
        }

        prediosVistos = new HashSet<novoPredio>();
        ruasVistos = new HashSet<novoPredio>();
        foreach (novoPredio obj in npVistos)
        {
            if (obj != null)
            {
                //                Debug.Log("nome do novoPredio visto: " + obj.np_nome);
                switch (obj.np_tipo)
                {
                    case TipoEspacoConstruido.Predio:
                        prediosVistos.Add(obj);
                        break;

                    case TipoEspacoConstruido.Rua:
                        ruasVistos.Add(obj);
                        break;
                }
            }
        }
        profundidadeMaximaRua = profundidadesPorRaio.Max();
        if (debug_iso) Debug.Log("profundidade máxima de ruas: " + profundidadeMaximaRua);

        medidasBrutas = new MedidasBrutas(
                distanciaMaxima,
                distanciaMedia,
                distanciaMinima,
                //distanciaTotal,
                prediosVistos.Count + ruasVistos.Count, //total de objetos vistos
                prediosVistos.Count,// totalPrediosVistos,
                ruasVistos.Count,// totalRuasVistas,
                profundidadeMaximaRua,
                areaIsovista
        );


    }

    public ValoresReferenciaNormalizacao BuscarReferenciaNormalizacao(List<lugar>todoLugar)
    {
        // ===== PRIMEIRO LUGAR ===== para setar o valor de referencia minimo e maxim sem problemas
        lugar primeiroLugar = todoLugar[0];
        if (medidasBrutas.Equals(default(MedidasBrutas)))
            campoVisao();   // calculou primeiroLugar.iso.medidasBrutas
        ValoresReferenciaNormalizacao referencias_normalizacao = new ValoresReferenciaNormalizacao(primeiroLugar.iso.medidasBrutas);
        // ===== RESTANTE DA PRIMEIRA VARREDURA ===== ATUALIZACAO DOS VALORES
        for (int i = 1; i < todoLugar.Count; i++)
        {
            lugar lugar = todoLugar[i];
            lugar.L_CalculeIsovistas(layerMask);
            Normalizador.ChecarSeReferencia(ref referencias_normalizacao, lugar.iso.medidasBrutas);// .minhasMedidasBrutas);
        }
        return referencias_normalizacao;
    }

    public void NormalizarMedidas(ValoresReferenciaNormalizacao referencias_normalizacao, List<lugar> todoLugar)
    {
        if (referencias_normalizacao.Equals(default(ValoresReferenciaNormalizacao)))
            BuscarReferenciaNormalizacao(todoLugar);

        // ===== SEGUNDA VARREDURA: NORMALIZAR =====
            medidasNormalizadas = Normalizador.Normalizar(medidasBrutas, referencias_normalizacao);
            //PonderarMedia();
    }
    public void NormalizarMedidas(ValoresReferenciaNormalizacao referencias_normalizacao)
    {
        if (referencias_normalizacao.Equals(default(ValoresReferenciaNormalizacao)))
//            BuscarReferenciaNormalizacao(todoLugar);

        // ===== SEGUNDA VARREDURA: NORMALIZAR =====
        medidasNormalizadas = Normalizador.Normalizar(medidasBrutas, referencias_normalizacao);
        //PonderarMedia();
    }

    public enum FinalRaioIsovista
    {
        Vazio,
        AbertoAposRua,
        BloqueadoPorPredio
    }
    public FinalRaioIsovista ProcessarRaioIsovista
        (Ray raioAtual, float _raio, float _raio_graus, 
        LayerMask _layerMasks, Vector3 _centroIsovista, Vector3 pontoNaCircunferencia,

                      HashSet<novoPredio> npVistos,
                      List<float> angulosUsados,
                      List<Vector3> _verticesIsovista,
                      List<float> angulosLivres,
                      List<float> profundidadeDosRaios

                      )
    {
        FinalRaioIsovista estado = FinalRaioIsovista.Vazio;

        Vector3 pontoFinal = pontoNaCircunferencia;
        float maiorDistanciaRua = -1f;
        Vector3 ultimoPontoRua = pontoNaCircunferencia;
        // conta quantas ruas consecutivas foram vistas antes do primeiro prédio
        int depth = 0;


        RaycastHit[] hits = Physics.RaycastAll(raioAtual, _raio, layerMask);
        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        foreach (RaycastHit hit in hits)
        {
            novoPredio np = hit.collider.GetComponent<novoPredio>();

            if (np == null)
            {
                // se for um 'lugar', considere bloqueador: pare a varredura (não continue contando ruas depois)
                var lg = hit.collider.GetComponentInParent<lugar>();
                if (lg != null)
                {
                    // encontrou um lugar: não conta ruas além dele — finalize o laço
                    estado = FinalRaioIsovista.Vazio; // trata como vazio/aberto (não bloqueado por prédio)
                                                      // não incrementa depth; interrompe a leitura de hits mais distantes
                    break;
                }

                // se não é nem novoPredio nem lugar, ignore este collider e continue (por exemplo, triggers, decorações)
                continue;
            }

            // Se atingir um objeto, adicione ao array de raios que atingiram
            //                Debug.Log("batti no fmigerado: "+ hitInfo.collider.GetComponent<EspacoConstruido>().meuNome);
            npVistos.Add(np); //npVistos.Add(hitInfo.collider.GetComponent<novoPredio>());

            if (np.np_tipo == TipoEspacoConstruido.Rua)
            {
                depth++;
                estado = FinalRaioIsovista.AbertoAposRua;

                if (hit.distance >= maiorDistanciaRua)
                {
                    maiorDistanciaRua = hit.distance;
                    ultimoPontoRua = hit.point;
                }

                continue;
            }
            if (np.np_tipo == TipoEspacoConstruido.Predio)
            {
                estado = FinalRaioIsovista.BloqueadoPorPredio;
                pontoFinal = hit.point;
                break;
            }
        }
        profundidadeDosRaios.Add(depth);
        switch (estado)
        {
            case FinalRaioIsovista.Vazio:
                angulosLivres.Add(_raio_graus);//    angulosUsados.Add(_raio_graus);
                if (!InputsMorfo.boolModoIsoObj)
                {
                    _verticesIsovista.Add(pontoNaCircunferencia); //_verticesIsovista.Add(hitInfo.point);
                }
                Debug.DrawRay(_centroIsovista, pontoNaCircunferencia - _centroIsovista, Color.blue, 10f); //Debug.DrawRay(_centroIsovista, hitInfo.point - _centroIsovista, Color.red, 10f);
                return FinalRaioIsovista.Vazio;

            case FinalRaioIsovista.BloqueadoPorPredio:
                angulosUsados.Add(_raio_graus);//    angulosUsados.Add(_raio_graus);
                _verticesIsovista.Add(pontoFinal);//_verticesIsovista.Add(hitInfo.point);

                Debug.DrawRay(_centroIsovista, pontoFinal - _centroIsovista, Color.red, 10f);//Debug.DrawRay(_centroIsovista, hitInfo.point - _centroIsovista, Color.red, 10f);
                return FinalRaioIsovista.BloqueadoPorPredio;
            case FinalRaioIsovista.AbertoAposRua:
                angulosUsados.Add(_raio_graus);//    angulosUsados.Add(_raio_graus);
                pontoFinal = ultimoPontoRua;
                _verticesIsovista.Add(pontoFinal);//_verticesIsovista.Add(hitInfo.point);

                Debug.DrawRay(_centroIsovista, pontoFinal - _centroIsovista, Color.yellow, 10f);//Debug.DrawRay(_centroIsovista, hitInfo.point - _centroIsovista, Color.red, 10f);

                return FinalRaioIsovista.AbertoAposRua;
        }
        return FinalRaioIsovista.Vazio;
    }


    // Calcula área do polígono projetado no plano XZ usando shoelace,
    // assume vertices em ordem circular. Usa sublista a partir de startIndex (ex.: 1 para pular o centro).
    private float CalcularAreaIsovistaXZ(List<Vector3> verts, int startIndex = 1)
    {
        int n = verts.Count - startIndex;
        if (n < 3) return 0f;

        double sum = 0.0;
        for (int i = 0; i < n; i++)
        {
            Vector3 a = verts[startIndex + i];
            Vector3 b = verts[startIndex + ((i + 1) % n)];
            sum += (double)a.x * b.z - (double)b.x * a.z;
        }
        return Mathf.Abs((float)(sum * 0.5));
    }


    public void isoMesh(List <Vector3> _pontosIso, string _nome)
    {
        if (InputsMorfo.tracking)
        {
            Debug.Log("tracking isovistaP");
        }

        //        if (_pontosIso == default(Vector3)) { _pontosIso = centro; }
        Vector3 altura = new Vector3(0, 0.5f, 0);

        for (int i = 0; i < _pontosIso.Count; i++)
        {
            Vector3 posicaoAtual = _pontosIso[i] + altura;
            //            posicaoAtual.y += 2; // Somando 2 à altura
            _pontosIso[i] = posicaoAtual; // Atualizando a lista
        }

        int[] _tri = IosTriangulo(_pontosIso);
        CriarIsoMesh(_pontosIso, _tri, _nome);

    }

    void diagnosticoNormais()
    {
        Vector3[] norms = isovistamesh.normals;
        if (norms != null && norms.Length > 0)
        {
            int neg = 0, zer = 0;
            float minY = float.MaxValue, maxY = float.MinValue;
            for (int i = 0; i < norms.Length; i++)
            {
                float y = norms[i].y;
                if (y < 0f) neg++;
                if (Mathf.Approximately(y, 0f)) zer++;
                if (y < minY) minY = y;
                if (y > maxY) maxY = y;
            }
            if (InputsMorfo.tracking) Debug.Log($"isovista normals: count={norms.Length}, neg={neg}, zero={zer}, minY={minY:F4}, maxY={maxY:F4}");
            // desenhar primeiras 20 normals para inspeção visual (verde)
            int N = Mathf.Min(20, isovistamesh.vertexCount);
            Vector3[] verts = isovistamesh.vertices;
            for (int i = 0; i < N; i++)
            {
                Debug.DrawRay(portaMesh != null ? portaMesh.transform.TransformPoint(verts[i]) : verts[i], (norms[i]) * 0.5f, Color.green, 5f);
            }
        }
    }

    void diganosticoNormaisParaBaixo()
    {
        // Garantir orientação consistente: se a normal média apontar para baixo, inverter winding
        Vector3[] normals = isovistamesh.normals;
        if (normals != null && normals.Length > 0)
        {
            Vector3 avg = Vector3.zero;
            for (int i = 0; i < normals.Length; i++) avg += normals[i];
            avg /= normals.Length;

            if (avg.y < 0f)
            {
                if (InputsMorfo.tracking) Debug.Log($"isovistaP: normais médias apontam p/ baixo (avg.y={avg.y:F3}) — invertendo winding");

                // inverter winding: swap segundo e terceiro índice de cada tri
                int[] tris = isovistamesh.triangles;
                for (int i = 0; i < tris.Length; i += 3)
                {
                    int tmp = tris[i + 1];
                    tris[i + 1] = tris[i + 2];
                    tris[i + 2] = tmp;
                }
                isovistamesh.triangles = tris;
                // recalcula normais com winding corrigido
                isovistamesh.RecalculateNormals();
            }
        }
    }

    void diagnosticoChecarTriangulos()
    {
        Vector3[] verts = isovistamesh.vertices;
        int[] tris = isovistamesh.triangles;
        int deg = 0;
        int faceDown = 0;
        int examplesLogged = 0;
        for (int i = 0; i < tris.Length; i += 3)
        {
            Vector3 a = verts[tris[i]];
            Vector3 b = verts[tris[i + 1]];
            Vector3 c = verts[tris[i + 2]];
            // face normal (world-space not needed for relative check)
            Vector3 fn = Vector3.Cross(b - a, c - a).normalized;
            float area = Vector3.Cross(b - a, c - a).magnitude * 0.5f;
            if (area < 1e-5f) deg++;
            if (fn.y < 0f) { faceDown++; if (examplesLogged < 5) 
                {
//                    Debug.Log($"isovista faceDown example i={i / 3} fn.y={fn.y:F4} area={area:E3}"); examplesLogged++; 
                } 
            }
        }
//        if (InputsMorfo.tracking) Debug.Log($"isovista faces: total={tris.Length / 3}, degenerate={deg}, faceDown={faceDown}");

    }

    void correcaoTriangulosDegenerados(int [] _triMesh)
    {
        int[] trisIn = _triMesh;
        Vector3[] verts = isovistamesh.vertices;
        var good = new List<int>(trisIn.Length);
        int removed = 0;
        const float areaEps = 1e-6f;

        for (int i = 0; i < trisIn.Length; i += 3)
        {
            int i0 = trisIn[i];
            int i1 = trisIn[i + 1];
            int i2 = trisIn[i + 2];

            // segurança: índices válidos
            if (i0 < 0 || i0 >= verts.Length || i1 < 0 || i1 >= verts.Length || i2 < 0 || i2 >= verts.Length)
            {
                removed++;
                continue;
            }

            Vector3 a = verts[i0];
            Vector3 b = verts[i1];
            Vector3 c = verts[i2];

            float area = Vector3.Cross(b - a, c - a).magnitude * 0.5f;
            if (area > areaEps)
            {
                good.Add(i0); good.Add(i1); good.Add(i2);
            }
            else
            {
                removed++;
            }
        }

        if (removed > 0) Debug.Log($"isovista: removidos {removed} triângulos degenerados (vertices={verts.Length})");

        isovistamesh.triangles = good.ToArray();
    }

    void CriarIsoMesh(List<Vector3> _pontosIsoMesh, int[] _triMesh, string _nome)
    {
        if (InputsMorfo.tracking)
        {
            Debug.Log("tracking isovistaP");
        }


        // Criar uma nova mesh  ---> meshIsovista
        //        Mesh mesh = new Mesh();
        isovistamesh = new Mesh();

        // Atribuir os vértices à mesh
        isovistamesh.vertices = _pontosIsoMesh.ToArray();

        // Atribuir triângulos à mesh
//        isovistamesh.triangles = _triMesh;
        correcaoTriangulosDegenerados(_triMesh);

        // Calcular as normais automaticamente
        isovistamesh.RecalculateNormals();

        // Calcular os bounds automaticamente
        isovistamesh.RecalculateBounds();
        diagnosticoNormais();
        diganosticoNormaisParaBaixo();
        diagnosticoChecarTriangulos();


        ////////////////////////////////      // Crie um objeto para exibir a mesh
        portaMesh = new GameObject("m");// ("ObjetoComMesh"+_nome);
        portaMesh.AddComponent<MeshFilter>();
        // Atribuir a mesh ao MeshFilter
        portaMesh.GetComponent<MeshFilter>().mesh = isovistamesh;// mesh;
        portaMesh.AddComponent<MeshRenderer>();

        Color transparencia = new Color(1.0f, 0.6f, 0.0f, 0.5f); //new Color(0.3f, 0.3f, 0.7f, 0.2f);
                                                                 // Cor verde claro com 50% de transparência (R,G,B,Alpha)
                                                                 //        originalColor = new Color(0.6f, 1.0f, 0.6f, 0.5f);
        
                MeshRenderer meshRenderer = portaMesh.GetComponent<MeshRenderer>();

                meshRenderer.material.shader = Shader.Find("Standard");
                meshRenderer.material.SetColor("_Color", transparencia);
                meshRenderer.material.SetFloat("_Mode", 3); // Modo de rendering transparente
                meshRenderer.material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                meshRenderer.material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                meshRenderer.material.SetInt("_ZWrite", 0);
                meshRenderer.material.DisableKeyword("_ALPHATEST_ON");
                meshRenderer.material.EnableKeyword("_ALPHABLEND_ON");
                meshRenderer.material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                meshRenderer.material.renderQueue = 3000;
                



    }

    public void destruirMesh()
    {
        if (InputsMorfo.tracking)
        {
            Debug.Log("tracking isovistaP");
        }

        if (portaMesh == null) return;

        // Remove referência da MeshFilter e destrói a mesh criada dinamicamente
        var mf = portaMesh.GetComponent<MeshFilter>();
        if (mf != null)
        {
            mf.mesh = null;
            if (isovistamesh != null)
            {
                Object.Destroy(isovistamesh);
                isovistamesh = null;
            }
        }

        // Destrói material instanciado (acessar .material cria uma instância)
        var mr = portaMesh.GetComponent<MeshRenderer>();
        if (mr != null)
        {
            var mat = mr.material;
            if (mat != null)
            {
                Object.Destroy(mat);
            }
        }

        // Destrói o GameObject (e todos os filhos)
        Object.Destroy(portaMesh);
        portaMesh = null;
    }

    int[] IosTriangulo(List<Vector3> vertices)
    {
        if (InputsMorfo.tracking)
        {
            Debug.Log("tracking isovistaP");
        }


        int _totalTri = vertices.Count - 1;
        int[] _triangulosIsovista = new int[_totalTri * 3];
      //  Debug.Log("pos ver os raycast, qtd indices triangulos: " + _triangulosIsovista.Length + ",tamanho dos vertices: " + vertices.Count);

        // Definir triângulos para formar faces
        //        int[] tri = new int[] { 0, 1, 2, 0, 2, 3 };

        for (int i = 0; i < _totalTri; i++)
        {

            int a = i * 3;
            _triangulosIsovista[a] = 0;
            _triangulosIsovista[a + 2] = i + 1;
            _triangulosIsovista[a + 1] = i + 2;

            //  Debug.Log("valor de i: " + i + " totalTri: " + _totalTri + " valor de a: " + a);

            if (i == (_totalTri -1))
            {
                //  Debug.Log("i = verticies count " + i);
                _triangulosIsovista[a + 2] = i+1;
                _triangulosIsovista[a + 1] = 1;
            }
        }
  //      Debug.Log("triangulos isovista " + _triangulosIsovista.Length + $"[{string.Join(",", _triangulosIsovista)}]");
        //Debug.Log("verticies " + vertices.Count + $"[{string.Join(",", vertices)}]");

        return _triangulosIsovista;

    }



}
