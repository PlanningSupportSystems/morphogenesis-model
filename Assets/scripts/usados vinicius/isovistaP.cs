using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using System.Linq;


public class IsovistaP
{
    //geradoras
    public Vector3 centro;
    public int totalRaios;
    public float raioVisao;
    public GameObject portaMesh;
    public int layerMask = LayerMask.NameToLayer("layer_predios");

    //gerados
    public List<Vector3> pontosContorno;
    public List<float> distanciasPontosContorno;

    //medidas
    public float distanciaMaxima;
    public List<int> indexMax;
    public float distanciaMedia;
    public float distanciaMinima;
    public List<int> indexMin;
    public float distanciaTotal;
    public Mesh isovistamesh;
    public List <RaycastHit> objVistos;
    public HashSet<EspacoConstruido> espacosVistos;
    public HashSet<novoPredio> npVistos;
    public HashSet<novoPredio> prediosVistos;
    public HashSet<novoPredio> ruasVistos;

    public float normalizado_distanciaMaxima;
    public float normalizado_distanciaMedia;
    public float normalizado_distanciaMinima;
    public float normalizado_distanciaTotal;

    public float maximo_objetos_vistos;
    //medidas, nova definicao

    public IsovistaP(Vector3 _centro, int _totalRaios, float _raioVisao)
    {
        centro = _centro;
        totalRaios = _totalRaios;
        raioVisao = _raioVisao;
        layerMask = (1 << LayerMask.NameToLayer("layer_predios"))
                | (1 << LayerMask.NameToLayer("layer_ruas"))
                //| (1 << LayerMask.NameToLayer("layer_lugares"))
                ;

    }

    public IsovistaP(Vector3 _centro, int _totalRaios, float _raioVisao, int _layerMask)
    {
        centro = _centro;
        totalRaios = _totalRaios;
        raioVisao = _raioVisao;
        layerMask = _layerMask;
//        Debug.Log("campo visao qual a layer: " + layerMask);

    }


    public void campoVisao(int _qtdRaios = 0, float _raio = 0, Vector3 _centroIsovista = default(Vector3))  //  ( 0,10000,0))// (0,10000,0))
    {
        //permitir chamada sem construtores
        if (_qtdRaios == 0) { _qtdRaios = totalRaios; }
        if (_raio == 0) { _raio = raioVisao; }
      //  Vector3 temp = (0, 10000, 0);
        if (_centroIsovista == default(Vector3)) { _centroIsovista = centro; }

        //        Debug.Log("centro isovista: " + _centroIsovista);

        //        quantidadeDeRaios = totalAngulos;
        //        _raio = distObj;

        // Arrays para armazenar raios que atingiram e que não atingiram objetos
        //        RaycastHit[] raiosAtingiram = new RaycastHit[_qtdRaios];          //substituido por list pra virar public
        objVistos = new List<RaycastHit>();
        espacosVistos = new HashSet<EspacoConstruido>();
        npVistos = new HashSet<novoPredio>();
        RaycastHit[] raiosNaoAtingiram = new RaycastHit[_qtdRaios];
        List<float> angulosLivres = new List<float>();
        List<float> angulosUsados = new List<float>();
        List<Vector3> vetoresLivres = new List<Vector3>();
        //        angulosLivres.Clear();
        //        angulosUsados.Clear();
        //        vetoresLivres.Clear();

        //vertices para mesh isovista
        List<Vector3> _verticesIsovista = new List<Vector3>();
        _verticesIsovista.Add(_centroIsovista);

        float _passoAngulo = 360f / _qtdRaios;

//        Debug.Log("campo visao qual a layer: " + layerMask);
        for (int i = 0; i < _qtdRaios; i++)
        {
            float _radianos = _passoAngulo * i * Mathf.Deg2Rad;                 //angulo em funcao do indice * passo entre raios a checar

            float x = _centroIsovista.x + _raio * Mathf.Cos(_radianos);              //referencia no .endereco do Predio
            float z = _centroIsovista.z + _raio * Mathf.Sin(_radianos);

            Vector3 pontoNaCircunferencia = new Vector3(x, _centroIsovista.y, z);

            // Use Ray para representar um raio
            Ray raioAtual = new Ray(_centroIsovista, pontoNaCircunferencia - _centroIsovista);

            // Use RaycastHit para armazenar informações sobre a colisão
            RaycastHit hitInfo;
            // Realize o teste de colisão
            if (Physics.Raycast(raioAtual, out hitInfo, _raio, layerMask))// && hitInfo.collider != this.predioPreFab.GetComponent<Collider>())
            {
                // Se atingir um objeto, adicione ao array de raios que atingiram
//                Debug.Log("batti no fmigerado: "+ hitInfo.collider.GetComponent<EspacoConstruido>().meuNome);
                objVistos.Add(hitInfo);//raiosAtingiram[i] = hitInfo;
//                espacosVistos.Add(hitInfo.collider.GetComponent<EspacoConstruido>());
                npVistos.Add(hitInfo.collider.GetComponent<novoPredio>());
                angulosUsados.Add(_passoAngulo * i);

                _verticesIsovista.Add(hitInfo.point);

                Debug.DrawRay(_centroIsovista, hitInfo.point - _centroIsovista, Color.red, 10f);

                // Faça o que for necessário com o objeto atingido (por exemplo, acessar hitInfo.collider)
            }
            else
            {
                // Se não atingir nenhum objeto, adicione ao array de raios que não atingiram
                //                raiosNaoAtingiram[i] = raioAtual;
                raiosNaoAtingiram[i] = hitInfo;
                angulosLivres.Add(_passoAngulo * i);
                Vector3 vorigem = raioAtual.origin;
                vetoresLivres.Add(vorigem);

                _verticesIsovista.Add(pontoNaCircunferencia);


                Debug.DrawRay(_centroIsovista, pontoNaCircunferencia - _centroIsovista, Color.blue, 10f);
            }

        }

        pontosContorno = _verticesIsovista;
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
//        indexMax.Clear();
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
                if (obj.np_nome.Contains("predio"))
                {
                    prediosVistos.Add(obj);
                }
                if (obj.np_nome.Contains("rua"))
                {
                    ruasVistos.Add(obj);
                }

            }
        }

    }

    public void isoMesh(List <Vector3> _pontosIso, string _nome)
    {
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

    void CriarIsoMesh(List<Vector3> _pontosIsoMesh, int[] _triMesh, string _nome)
    {

        // Criar uma nova mesh  ---> meshIsovista
//        Mesh mesh = new Mesh();
        isovistamesh = new Mesh();

        // Atribuir os vértices à mesh
        isovistamesh.vertices = _pontosIsoMesh.ToArray();

        // Atribuir triângulos à mesh
        isovistamesh.triangles = _triMesh;

        // Calcular as normais automaticamente
        isovistamesh.RecalculateNormals();

        // Calcular os bounds automaticamente
        isovistamesh.RecalculateBounds();

  ////////////////////////////////      // Crie um objeto para exibir a mesh
        portaMesh = new GameObject("m");// ("ObjetoComMesh"+_nome);
        portaMesh.AddComponent<MeshFilter>();
        // Atribuir a mesh ao MeshFilter
        portaMesh.GetComponent<MeshFilter>().mesh = isovistamesh;// mesh;
        portaMesh.AddComponent<MeshRenderer>();

        Color transparencia = new Color(0.3f, 0.3f, 0.7f, 0.2f);
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
        portaMesh.GetComponent<MeshFilter>().mesh.Clear();
        //Destroy(portaMesh.GetComponent<MeshFilter>().mesh);
        //Destroy(portaMesh);
    }

    int[] IosTriangulo(List<Vector3> vertices)
    {

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
