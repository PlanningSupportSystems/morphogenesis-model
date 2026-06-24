using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PercorrerConfiguracoes : MonoBehaviour
{
    public ControleAglomeracao gerente;
    public int quantidadeCasas = 200;
    public float totalVizinhos = 4f;
    public float distanciaAdjacencia = 2f;
    public float distanciaRegional = 5.5f;
    public bool rodarAoIniciar = false;

    struct TesteSimulacao
    {
        public string nome;
        public ControleAglomeracao.Configuracoes config;
    }

    delegate void ConfigurarPeso(ref ControleAglomeracao.Configuracoes config);

    void Start()
    {
        if (gerente == null)
            gerente = ControleAglomeracao.Instance;

        if (rodarAoIniciar)
            StartCoroutine(RodarTestes());
    }

    public void IniciarTestes()
    {
        StartCoroutine(RodarTestes());
    }

    IEnumerator RodarTestes()
    {
        if (gerente == null)
            gerente = ControleAglomeracao.Instance;

        if (gerente == null)
        {
            Debug.LogError("PercorrerConfiguracoes: gerente nao encontrado.");
            yield break;
        }

        List<TesteSimulacao> testes = CriarTestes();
        string sessao = System.DateTime.Now.ToString("yyyy MM dd_HH mm ss");
        float inicioTotal = Time.realtimeSinceStartup;

        foreach (TesteSimulacao teste in testes)
        {
            float inicioTeste = Time.realtimeSinceStartup;
            string nomeTesteComHora = teste.nome + "_" + sessao;

            yield return StartCoroutine(
                gerente.CA_RodarTesteConfiguracao(teste.config, nomeTesteComHora)
            );

            float duracaoTeste = Time.realtimeSinceStartup - inicioTeste;

            Debug.Log(
                "TESTE_CONCLUIDO " +
                nomeTesteComHora +
                " duracao_s=" + duracaoTeste.ToString("F1") +
                " rodadas=" + gerente.ContadorRodadas +
                " predios=" + gerente.Geral_novosPrediosConstruidos.Count +
                " ruas=" + gerente.Geral_novosPrediosRuas.Count +
                " total=" + gerente.Geral_novosPrediosTotal.Count
            );

            yield return new WaitForEndOfFrame();
        }

        float duracaoTotal = Time.realtimeSinceStartup - inicioTotal;

        Debug.Log(
            "BATERIA_CONCLUIDA testes=" + testes.Count +
            " duracao_total_s=" + duracaoTotal.ToString("F1") +
            " media_s=" + (duracaoTotal / testes.Count).ToString("F1")
        );
    }

    List<TesteSimulacao> CriarTestes()
    {
        List<TesteSimulacao> testes = new List<TesteSimulacao>();

        int indiceTeste = 1;

        AdicionarTesteIso(testes, ref indiceTeste, "dmax-obj-pred-rua-prof_pres10", (ref ControleAglomeracao.Configuracoes config) =>
        {
            AplicarPesos(ref config, 1f, 0f, 1f, 1f, 1f, 1f);
            config.preservaIso = true;
            config.preservaProfundidade = false;
        });

        AdicionarTesteIso(testes, ref indiceTeste, "dmax-rua-prof_pres00", (ref ControleAglomeracao.Configuracoes config) =>
        {
            AplicarPesos(ref config, 1f, 0f, 0f, 0f, 1f, 1f);
            config.preservaIso = false;
            config.preservaProfundidade = false;
        });

        AdicionarTesteIso(testes, ref indiceTeste, "obj-pred_pres01", (ref ControleAglomeracao.Configuracoes config) =>
        {
            AplicarPesos(ref config, 0f, 0f, 1f, 1f, 0f, 0f);
            config.preservaIso = false;
            config.preservaProfundidade = true;
        });

        return testes;
    }

    void AplicarPesos(
        ref ControleAglomeracao.Configuracoes config,
        float distMax,
        float area,
        float objetos,
        float predios,
        float ruas,
        float profundidade
    )
    {
        config.pesoDistanciaMaxima = distMax;
        config.pesoDistanciaTotal = area;
        config.pesoTotalObjVisto = objetos;
        config.pesoTotalPredioVisto = predios;
        config.pesoTotalRuaVisto = ruas;
        config.pesoProfundidadeRua = profundidade;
    }

    void AdicionarTesteIso(
        List<TesteSimulacao> testes,
        ref int indiceTeste,
        string nomePeso,
        ConfigurarPeso configurarPeso
    )
    {
        ControleAglomeracao.Configuracoes config = CriarConfiguracaoBase();
        configurarPeso(ref config);

        testes.Add(new TesteSimulacao
        {
            nome = NomeTeste(indiceTeste++, "iso_" + nomePeso),
            config = config
        });
    }

    ControleAglomeracao.Configuracoes CriarConfiguracaoBase()
    {
        return new ControleAglomeracao.Configuracoes
        {
            totalCasas = quantidadeCasas,
            totalVizinhos = totalVizinhos,
            distanciaAdjacencia = distanciaAdjacencia,
            distanciaRegional = distanciaRegional,
            distanciaCampoVisao = CalcularDiametroCampoVisao(quantidadeCasas, distanciaAdjacencia),

            ruaMaisUm = true,
            modoRandom = false,
            modoIsovista = true,
            modoIsoObj = true,
            preservaIso = false,
            preservaProfundidade = false,

            gravarImagens = true,
            apenasImagemFinal = true,

            pesoDistanciaMaxima = 0f,
            pesoDistanciaMinima = 0f,
            pesoDistanciaMedia = 0f,
            pesoDistanciaTotal = 0f,
            pesoTotalObjVisto = 0f,
            pesoTotalPredioVisto = 0f,
            pesoTotalRuaVisto = 0f,
            pesoProfundidadeRua = 0f
        };
    }

    float CalcularDiametroCampoVisao(int totalCasas, float distanciaEntreCasas)
    {
        float areaAproximada = totalCasas * distanciaEntreCasas * distanciaEntreCasas;
        return 2f * Mathf.Sqrt(areaAproximada / Mathf.PI);
    }

    string NomeTeste(int indice, string descricao)
    {
        return "T" + indice.ToString("D3") + "_" + descricao;
    }

    string NomePesos(float distMax, float area, float objetos, float predios, float ruas, float profundidade)
    {
        List<string> partes = new List<string>();

        AdicionarPeso(partes, "dmax", distMax);
        AdicionarPeso(partes, "area", area);
        AdicionarPeso(partes, "obj", objetos);
        AdicionarPeso(partes, "pred", predios);
        AdicionarPeso(partes, "rua", ruas);
        AdicionarPeso(partes, "prof", profundidade);

        return string.Join("-", partes);
    }

    void AdicionarPeso(List<string> partes, string nome, float valor)
    {
        if (valor == 0f)
            return;

        if (valor == 0.5f)
            partes.Add(nome + "05");
        else if (valor == 1f)
            partes.Add(nome);
    }
}
