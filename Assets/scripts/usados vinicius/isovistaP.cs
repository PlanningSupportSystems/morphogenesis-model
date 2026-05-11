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

    public IsovistaP(Vector3 _centro)
    {
        centro = _centro;
//        totalRaios = _totalRaios;
//        raioVisao = _raioVisao;
//        layerMask = LayerMask.GetMask("layer_predios", "layer_ruas");//, "layer_lugares");
                                                                     //        layerMask = (1 << LayerMask.NameToLayer("layer_predios"))
                                                                     //                | (1 << LayerMask.NameToLayer("layer_ruas"))
                                                                     //                //| (1 << LayerMask.NameToLayer("layer_lugares"))
//        ;
    }

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

    private class PontoAngular
    {
        public Vector3 ponto;
        public float angulo;
    }

    public class ResultadoRaioVisao
    {
        public Ray raio;
        public List<RaycastHit> transparentes = new List<RaycastHit>();
        public RaycastHit? bloqueador;

        public bool FoiBloqueado => bloqueador.HasValue;
    }

    public List<ResultadoRaioVisao> VarrerCampoVisao(
    Vector3 centro,
    float raio,
    int qtdRaios,
    LayerMask layerBloqueadores,
    LayerMask layerTransparentes
)
    {
        List<ResultadoRaioVisao> resultados = new List<ResultadoRaioVisao>();

        if (qtdRaios <= 0 || raio <= 0f)
        {
            Debug.LogWarning("VarrerCampoVisao: raio ou qtdRaios inválidos.");
            return resultados;
        }

        int maskTotal = layerBloqueadores.value | layerTransparentes.value;

        if (maskTotal == 0)
        {
            Debug.LogWarning("VarrerCampoVisao: nenhuma layer informada.");
            return resultados;
        }

        float passoAngulo = 360f / qtdRaios;

        for (int i = 0; i < qtdRaios; i++)
        {
            float angRad = passoAngulo * i * Mathf.Deg2Rad;

            Vector3 direcao = new Vector3(
                Mathf.Cos(angRad),
                0f,
                Mathf.Sin(angRad)
            );

            Ray raioAtual = new Ray(centro, direcao);

            ResultadoRaioVisao resultado = ProcessarRaioVisao(
                raioAtual,
                raio,
                maskTotal
            );

            resultados.Add(resultado);
        }

        return resultados;
    }

    private ResultadoRaioVisao ProcessarRaioVisao(
    Ray raioAtual,
    float distanciaMaxima,
    int maskTotal
)
    {
        ResultadoRaioVisao resultado = new ResultadoRaioVisao();
        resultado.raio = raioAtual;

        RaycastHit[] hits = Physics.RaycastAll(
            raioAtual,
            distanciaMaxima,
            maskTotal
        );

        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        foreach (RaycastHit hit in hits)
        {
            if (EhBloqueadorCampoVisao(hit))
            {
                resultado.bloqueador = hit;
                break;
            }

            if (EhTransparenteCampoVisao(hit))
            {
                resultado.transparentes.Add(hit);
                continue;
            }
        }

        return resultado;
    }

    private bool EhBloqueadorCampoVisao(RaycastHit hit)
    {
        novoPredio np = hit.collider.GetComponentInParent<novoPredio>();

        return np != null && np.np_tipo == TipoEspacoConstruido.Predio;
    }

    private bool EhTransparenteCampoVisao(RaycastHit hit)
    {
        novoPredio np = hit.collider.GetComponentInParent<novoPredio>();

        if (np != null && np.np_tipo == TipoEspacoConstruido.Rua)
            return true;

        lugar l = hit.collider.GetComponentInParent<lugar>();

        if (l != null)
            return true;

        return false;
    }

    public List<lugar> LerLugaresVisiveis(List<ResultadoRaioVisao> resultados)
    {
        HashSet<lugar> vistos = new HashSet<lugar>();

        foreach (ResultadoRaioVisao r in resultados)
        {
            foreach (RaycastHit hit in r.transparentes)
            {
                lugar l = hit.collider.GetComponentInParent<lugar>();

                if (l != null)
                    vistos.Add(l);
            }
        }

        return vistos.ToList();
    }

    public int I_CalcularQtdRaios(Renderer objReferencia_T)
    {
        float raioVisao = InputsMorfo.input_distanciaCampoVisao;
        Vector3 tamanho_EspacoConstruido_Vector;

        Renderer rend = objReferencia_T;// GerenteAmbiente.espacoConstruido.GetComponent<Renderer>();
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

        bool debug_iso = true;
        if (debug_iso)
        {
            Debug.Log($"CalcularQtdRaios: raioVisao={raioVisao:F2}, " +
                      $"tamanhoVec={tamanho_EspacoConstruido_Vector}, " +
                      $"tamanhoCelula={tamanhoCelula:F2}, " +
                      $"espacamentoDesejado={espacamentoDesejado:F2}, " +
                      $"qtdCalculada={qtd}, qtdClampada={Mathf.Clamp(qtd, 36, 720)}");
        }
        debug_iso = false;

        return Mathf.Clamp(qtd, 36, 720);
    }


    public void CampoVisao(int _qtdRaios = 0, float _raio = 0, Vector3 _centroIsovista = default(Vector3))  //  ( 0,10000,0))// (0,10000,0))
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

        // proteção: qtdRaios inválida impede cálculos confiáveis
        if (_qtdRaios <= 0)
        {
            Debug.LogWarning($"campoVisao: número de raios inválido ({_qtdRaios}). Abortando cálculo.");
            // zera saídas mínimas para manter consistência
            pontosContorno = new List<Vector3>();
            distanciasPontosContorno = new List<float>();
            profundidadesPorRaio = new List<float>();
            profundidadeMaximaRua = 0f;
            areaIsovista = 0f;
            medidasBrutas = new MedidasBrutas(0, 0, 0, 0, 0, 0, 0, 0);
            return;
        }
        // Arrays para armazenar raios que atingiram e que não atingiram objetos
        //        RaycastHit[] raiosAtingiram = new RaycastHit[_qtdRaios];          //substituido por list pra virar public
        objVistos = new List<RaycastHit>();     // espacosVistos = new HashSet<EspacoConstruido>();
        npVistos = new HashSet<novoPredio>();
        RaycastHit[] raiosNaoAtingiram = new RaycastHit[_qtdRaios];
        List<float> angulosLivres = new List<float>();
        List<float> angulosUsados = new List<float>();
        List<Vector3> vetoresLivres = new List<Vector3>();

        //vertices para mesh isovista
        List<Vector3> _verticesIsovista = new List<Vector3>();
        // inicializar tracking de profundidade de ruas
        profundidadesPorRaio = new List<float>(_qtdRaios);
        profundidadeMaximaRua = 0;
        indicesProfundidadeMaxima = new List<int>();
        
        float _passoAngulo = 360f / _qtdRaios;
        bool fecharMalha = false;
        int quantidadeSetores = 0;

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

        pontosContorno = ReordenarContornoPorSetor(
    _verticesIsovista,
    _centroIsovista,
    _passoAngulo,
    out fecharMalha,
    out quantidadeSetores
);

        // só fecha explicitamente o contorno se for malha fechada
        if (fecharMalha && pontosContorno.Count > 0)
        {
            Vector3 first = pontosContorno[0];
            Vector3 last = pontosContorno[pontosContorno.Count - 1];

            if (Vector2.Distance(new Vector2(first.x, first.z), new Vector2(last.x, last.z)) > 1e-3f)
            {
                pontosContorno.Add(first);
            }
        }

        if (debug_iso)
        {
            Debug.Log($"isovista setores detectados: {quantidadeSetores} | fecharMalha={fecharMalha}");
        }

        // dentro de CampoVisao(), depois de pontosContorno = _verticesIsovista;
//        areaIsovista = CalcularAreaIsovistaXZ(pontosContorno, 1);
        areaIsovista = CalcularAreaIsovistaXZ(pontosContorno, 0);
        if (debug_iso) Debug.Log("area isovista foi de: " + areaIsovista);// 1 pula o centro

        distanciasPontosContorno = new List<float>();
        if (pontosContorno != null && pontosContorno.Count > 0)
        {
            foreach (Vector3 pC in pontosContorno)
            {
                distanciasPontosContorno.Add(Vector3.Distance(_centroIsovista, pC));
            }
            //      Debug.Log("distanciass ISOVISTA " + distanciasPontosContorno.Count + $"[{string.Join(",", distanciasPontosContorno)}]");
        }

        if (distanciasPontosContorno == null || distanciasPontosContorno.Count == 0)
        {
            // sem pontos válidos: zera medidas e sai
            distanciaMaxima = distanciaMinima = distanciaMedia = distanciaTotal = 0f;
            profundidadeMaximaRua = (profundidadesPorRaio != null && profundidadesPorRaio.Count > 0) ? profundidadesPorRaio.Max() : 0f;
            medidasBrutas = new MedidasBrutas(
                distanciaMaxima,
                distanciaMedia,
                distanciaMinima,
                0, 0, 0,
                profundidadeMaximaRua,
                areaIsovista
            );
            return;
        }

        distanciaMaxima = distanciasPontosContorno.Max();
        distanciaMinima= distanciasPontosContorno.Min();
        distanciaMedia = distanciasPontosContorno.Average();
        distanciaTotal = distanciasPontosContorno.Sum();

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
        profundidadeMaximaRua = (profundidadesPorRaio != null && profundidadesPorRaio.Count > 0) ? profundidadesPorRaio.Max() : 0f;
//        profundidadeMaximaRua = profundidadesPorRaio.Max();
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
        if (todoLugar == null || todoLugar.Count == 0)
        {
            Debug.LogWarning("BuscarReferenciaNormalizacao: lista todoLugar nula ou vazia. Retornando valor default.");
            return default(ValoresReferenciaNormalizacao);
        }
        // ===== PRIMEIRO LUGAR ===== para setar o valor de referencia minimo e maxim sem problemas
        lugar primeiroLugar = todoLugar[0];
        if (medidasBrutas.Equals(default(MedidasBrutas)))
            CampoVisao();   // calculou primeiroLugar.iso.medidasBrutas
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
        RaycastHit ultimoHitRua = default;
        bool temUltimoHitRua = false;
        // conta quantas ruas consecutivas foram vistas antes do primeiro prédio
        int depth = 0;


        RaycastHit[] hits = Physics.RaycastAll(raioAtual, _raio, _layerMasks);
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

            /*
            string nome = hit.collider != null ? hit.collider.name : "null";
            string tipo = "sem novoPredio";
            novoPredio npDebug = hit.collider.GetComponent<novoPredio>();
            if (npDebug != null) tipo = npDebug.np_tipo.ToString();

            Debug.Log(
                $"ISO hit ang={_raio_graus:F1} | dist={hit.distance:F3} | ponto={hit.point} | collider={nome} | tipo={tipo}"
            );
            */


            if (np.np_tipo == TipoEspacoConstruido.Rua)
            {
                depth++;
                estado = FinalRaioIsovista.AbertoAposRua;

                if (hit.distance >= maiorDistanciaRua)
                {
                    maiorDistanciaRua = hit.distance;
                    ultimoPontoRua = hit.point;
                    ultimoHitRua = hit;
                    temUltimoHitRua = true;
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

        if (estado == FinalRaioIsovista.AbertoAposRua && temUltimoHitRua)
        {
            Vector3 dir = raioAtual.direction.normalized;

            // começa fora, no fim teórico do raio, e volta
            Vector3 origemReversa = _centroIsovista + dir * (_raio + 0.05f);
            Ray raioReverso = new Ray(origemReversa, -dir);

            RaycastHit hitSaida;
            Vector3 pontoSaidaRua = ultimoHitRua.point; // fallback
            bool achouSaida = false;

            if (ultimoHitRua.collider != null &&
                ultimoHitRua.collider.Raycast(raioReverso, out hitSaida, _raio + 0.1f))
            {
                pontoSaidaRua = hitSaida.point;
                achouSaida = true;
            }
            // fallback:
            // se não achou saída do bloco final, assume que o raio terminou dentro dele
            // e usa o fim do próprio raycast como contorno
            if (!achouSaida)
            {
                pontoSaidaRua = pontoNaCircunferencia;
            }

            ultimoPontoRua = pontoSaidaRua;

//            Debug.DrawLine(ultimoHitRua.point, pontoSaidaRua, Color.cyan, 10f);
//            Debug.Log(
//                $"ISO rua FINAL ang={_raio_graus:F1} | entrada={ultimoHitRua.point} | saida={pontoSaidaRua} | collider={ultimoHitRua.collider.name}"
//            );
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

    private List<Vector3> ReordenarContornoPorSetor(
    List<Vector3> verticesOriginais,
    Vector3 centroRef,
    float passoAngular,
    out bool fecharMalha,
    out int quantidadeSetores,
    float toleranciaMult = 1.5f)
    {
        fecharMalha = false;
        quantidadeSetores = 0;

        if (verticesOriginais == null || verticesOriginais.Count == 0)
            return new List<Vector3>();

        float epsCentro = 1e-3f;
        float epsDuplicado = 1e-3f;
        float tolerancia = passoAngular * toleranciaMult;

        List<PontoAngular> pts = new List<PontoAngular>();

        // limpa ponto colado no centro e duplicados
        foreach (Vector3 p in verticesOriginais)
        {
            if (Vector3.Distance(p, centroRef) <= epsCentro)
                continue;

            bool duplicado = false;
            foreach (var q in pts)
            {
                if (Vector3.Distance(p, q.ponto) <= epsDuplicado)
                {
                    duplicado = true;
                    break;
                }
            }

            if (duplicado) continue;

            Vector3 d = p - centroRef;
            float ang = Mathf.Atan2(d.z, d.x) * Mathf.Rad2Deg;
            if (ang < 0f) ang += 360f;

            pts.Add(new PontoAngular { ponto = p, angulo = ang });
        }

        if (pts.Count < 2)
            return pts.Select(t => t.ponto).ToList();

        // ordena por ângulo crescente
        pts.Sort((a, b) => a.angulo.CompareTo(b.angulo));

        // acha o maior salto angular (incluindo wrap)
        int indiceQuebra = -1;
        float maiorSalto = -1f;
        int setores = 1;

        for (int i = 0; i < pts.Count; i++)
        {
            int prox = (i + 1) % pts.Count;
            float a1 = pts[i].angulo;
            float a2 = pts[prox].angulo;

            float delta = (prox == 0) ? (a2 + 360f - a1) : (a2 - a1);

            if (delta > tolerancia)
                setores++;

            if (delta > maiorSalto)
            {
                maiorSalto = delta;
                indiceQuebra = i;
            }
        }

        quantidadeSetores = setores;

        // se o maior salto não excede o passo+tolerância, considera volta completa
        fecharMalha = maiorSalto <= tolerancia;

        // rota a lista para começar logo após a maior quebra
        List<Vector3> reordenado = new List<Vector3>(pts.Count);
        for (int k = 1; k <= pts.Count; k++)
        {
            int idx = (indiceQuebra + k) % pts.Count;
            reordenado.Add(pts[idx].ponto);
        }

        return reordenado;
    }
   
    public void isoMesh(List <Vector3> _pontosIso, string _nome)
    {
        if (InputsMorfo.tracking)
        {
            Debug.Log("tracking isovistaP");
        }

        if (_pontosIso == null || _pontosIso.Count < 2)
        {
            Debug.LogWarning("isoMesh: pontos insuficientes para criar mesh (precisa de pelo menos 2 pontos de contorno). Abortando.");
            return;
        }

        //        if (_pontosIso == default(Vector3)) { _pontosIso = centro; }
        Vector3 altura = new Vector3(0, 2.5f, 0);

        // Garantir que o primeiro elemento seja o centro (index 0)
        var verts = new List<Vector3>(1 + _pontosIso.Count);
        verts.Add(centro);          // centro em index 0
        verts.AddRange(_pontosIso); // contorno em 1..N

        // remover duplicata próxima ao centro por segurança
        for (int i = verts.Count - 1; i >= 1; i--)
        {
            if (Vector3.Distance(verts[i], centro) <= 1e-3f)
                verts.RemoveAt(i);
        }

        for (int i = 0; i < verts.Count; i++)
        {
            verts[i] += altura; // eleva todos os vértices para evitar z-fighting com o chão
            //Vector3 posicaoAtual = _pontosIso[i] + altura;
            //            posicaoAtual.y += 2; // Somando 2 à altura
            //_pontosIso[i] = posicaoAtual; // Atualizando a lista
        }

        /*
        for (int i = 1; i < verts.Count; i++)
        {
            Vector3 dir = verts[i] - verts[0];
            float ang = Mathf.Atan2(dir.z, dir.x) * Mathf.Rad2Deg;
            if (ang < 0) ang += 360f;

            float dist = dir.magnitude;

            Debug.Log($"ISO ponto {i}: ang={ang:F2} | dist={dist:F2} | pos={verts[i]}");
        }
        */

        bool fecharMalha = false;
        if (_pontosIso != null && _pontosIso.Count >= 3)
        {
            Vector3 primeiro = _pontosIso[0];
            Vector3 ultimo = _pontosIso[_pontosIso.Count - 1];
            fecharMalha = Vector2.Distance(
                new Vector2(primeiro.x, primeiro.z),
                new Vector2(ultimo.x, ultimo.z)
            ) <= 1e-3f;
        }
//        int[] _tri = IosTriangulo(verts);
        int[] _tri = IosTriangulo(verts, fecharMalha);
        CriarIsoMesh(verts, _tri, _nome);
//        CriarIsoMesh(_pontosIso, _tri, _nome);

    }


    void CriarIsoMesh(List<Vector3> _pontosIsoMesh, int[] _triMesh, string _nome)
    {
        if (InputsMorfo.tracking)
        {
            Debug.Log("tracking isovistaP");
        }

        if (_pontosIsoMesh == null || _pontosIsoMesh.Count == 0)
        {
            Debug.LogWarning("CriarIsoMesh: lista de vértices vazia. Abortando criação de mesh.");
            return;
        }

        // Criar uma nova mesh  ---> meshIsovista
        //        Mesh mesh = new Mesh();
        isovistamesh = new Mesh();

        // Atribuir os vértices à mesh
        isovistamesh.vertices = _pontosIsoMesh.ToArray();

        // validar triângulos: garantir que cada índice está no intervalo válido
        int vertsCount = _pontosIsoMesh.Count;
        var goodTris = new List<int>();
        if (_triMesh != null && _triMesh.Length > 0)
        {
            for (int i = 0; i < _triMesh.Length; i += 3)
            {
                if (i + 2 >= _triMesh.Length) break;
                int i0 = _triMesh[i];
                int i1 = _triMesh[i + 1];
                int i2 = _triMesh[i + 2];
                if (i0 >= 0 && i0 < vertsCount && i1 >= 0 && i1 < vertsCount && i2 >= 0 && i2 < vertsCount)
                {
                    goodTris.Add(i0); goodTris.Add(i1); goodTris.Add(i2);
                }
                else
                {
                    // descarta triângulo inválido
                }
            }
        }
        else
        {
            Debug.LogWarning("CriarIsoMesh: triângulos vazios/ nulos. Mesh será criada sem faces.");
        }

        // Atribuir triângulos à mesh
        isovistamesh.triangles = _triMesh;
//        correcaoTriangulosDegenerados(_triMesh);

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
    void correcaoTriangulosDegenerados(int[] _triMesh)
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
            if (fn.y < 0f)
            {
                faceDown++; if (examplesLogged < 5)
                {
                    Debug.Log($"isovista faceDown example i={i / 3} fn.y={fn.y:F4} area={area:E3}"); examplesLogged++;
                }
            }
        }
//        Debug.Log($"isovista faces: total={tris.Length / 3}, degenerate={deg}, faceDown={faceDown}");

    }


    int[] IosTriangulo(List<Vector3> vertices, bool fecharMalha, float epsilonArea = 0.0001f)
    {
        if (InputsMorfo.tracking)
        {
            Debug.Log("tracking isovistaP");
        }

        if (vertices == null)
        {
            Debug.LogWarning("IosTrianguloRobusto: lista de vertices == null");
            return System.Array.Empty<int>();
        }

        // precisa de: 1 centro + pelo menos 2 pontos de contorno para 1 triangulo,
        // mas para uma malha fechada decente o ideal eh 1 centro + 3 pontos de contorno
        if (vertices.Count < 3)
        {
            Debug.LogWarning($"IosTrianguloRobusto: vertices insuficientes ({vertices.Count})");
            return System.Array.Empty<int>();
        }

        Vector3 centro = vertices[0];
        List<int> triangulos = new List<int>();
        int totalSetores = vertices.Count - 1;

        int degenerados = 0;
        int invertidosCorrigidos = 0;

        //        for (int i = 1; i <= totalSetores; i++)
        //        {
        //            int i1 = i;
        //            int i2 = (i < totalSetores) ? i + 1 : 1;
        int ultimoLoop = fecharMalha ? totalSetores : totalSetores - 1;

        for (int i = 1; i <= ultimoLoop; i++)
        {
            int i1 = i;
            int i2 = (i < totalSetores) ? i + 1 : 1;

            if (!fecharMalha && i == totalSetores)
                break;

            Vector3 a = vertices[i1] - centro;
            Vector3 b = vertices[i2] - centro;

            // cross no plano XZ
            float crossY = a.x * b.z - a.z * b.x;

            // area assinada do triangulo no XZ
            float area = Mathf.Abs(crossY) * 0.5f;

            // descarta triangulo degenerado ou quase degenerado
            if (area <= epsilonArea)
            {
                degenerados++;

                if (InputsMorfo.tracking)
                {
                    Debug.LogWarning(
                        $"IosTrianguloRobusto: triangulo degenerado descartado | setor={i} | idx=(0,{i1},{i2}) | area={area}"
                    );
                }

                continue;
            }

            // Se crossY > 0, winding coerente para normal para cima no XZ.
            // Se < 0, inverte localmente.
            triangulos.Add(0);

            if (crossY > 0f)
            {
                triangulos.Add(i1);
                triangulos.Add(i2);
            }
            else
            {
                triangulos.Add(i2);
                triangulos.Add(i1);
                invertidosCorrigidos++;

                if (InputsMorfo.tracking)
                {
                    Debug.LogWarning(
                        $"IosTrianguloRobusto: winding invertido corrigido | setor={i} | idx original=(0,{i1},{i2}) | crossY={crossY}"
                    );
                }
            }
        }

        if (InputsMorfo.tracking)
        {
            Debug.Log(
                $"IosTrianguloRobusto: vertices={vertices.Count}, setores={totalSetores}, " +
                $"triangulos finais={triangulos.Count / 3}, degenerados removidos={degenerados}, " +
                $"invertidos corrigidos={invertidosCorrigidos}"
            );
        }

        return triangulos.ToArray();
    }

}
