using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PercorrerConfiguracoes : MonoBehaviour
{
    public ControleAglomeracao gerente;
    public int quantidadeCasas = 10;
    public bool rodarAoIniciar = false;

    struct ConfigTeste
    {
        public string nome;

        public bool random;
        public bool isovista;
        public bool isoOnlyObj;
        public bool preservaIso;
        public bool preservaProfundidade;

        public float distMax;
        public float area;
        public float objetos;
        public float predios;
        public float ruas;
        public float profundidade;
    }

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
        List<ConfigTeste> testes = CriarTestes();
        string sessao = System.DateTime.Now.ToString("yyyy MM dd_HH mm ss");
        float inicioTotal = Time.realtimeSinceStartup;

        foreach (ConfigTeste teste in testes)
        {
            float inicioTeste = Time.realtimeSinceStartup;
            AplicarConfiguracao(teste);

            string nomeTesteComHora = teste.nome + "_" + sessao;

            yield return StartCoroutine(
                gerente.CA_RodarTesteConfiguracao(quantidadeCasas, nomeTesteComHora)
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

    List<ConfigTeste> CriarTestes()
    {
        List<ConfigTeste> testes = new List<ConfigTeste>();

        int indiceTeste = 1;

        testes.Add(new ConfigTeste
        {
            nome = NomeTeste(indiceTeste++, "random_all"),
            random = true,
            isovista = false,
            isoOnlyObj = false,
            preservaIso = false,
            preservaProfundidade = false
        });

        testes.Add(new ConfigTeste
        {
            nome = NomeTeste(indiceTeste++, "random_obj"),
            random = true,
            isovista = false,
            isoOnlyObj = true,
            preservaIso = false,
            preservaProfundidade = false
        });

        for (int mascara = 1; mascara < 63; mascara++)
        {
            if (mascara == 63)
                continue;

            bool distMax = (mascara & 1) != 0;
            bool area = (mascara & 2) != 0;
            bool objetos = (mascara & 4) != 0;
            bool predios = (mascara & 8) != 0;
            bool ruas = (mascara & 16) != 0;
            bool profundidade = (mascara & 32) != 0;

            string nomePesos = NomePesos(distMax, area, objetos, predios, ruas, profundidade);

            for (int preservacao = 0; preservacao < 4; preservacao++)
            {
                bool preservaIso = (preservacao & 1) != 0;
                bool preservaProfundidade = (preservacao & 2) != 0;
                string nomePreservacao = NomePreservacao(preservaIso, preservaProfundidade);

                testes.Add(new ConfigTeste
                {
                    nome = NomeTeste(indiceTeste++, "iso_" + nomePesos + nomePreservacao),
                    random = false,
                    isovista = true,
                    isoOnlyObj = true,
                    preservaIso = preservaIso,
                    preservaProfundidade = preservaProfundidade,
                    distMax = distMax ? 1f : 0f,
                    area = area ? 1f : 0f,
                    objetos = objetos ? 1f : 0f,
                    predios = predios ? 1f : 0f,
                    ruas = ruas ? 1f : 0f,
                    profundidade = profundidade ? 1f : 0f
                });
            }
        }

        return testes;
    }

    string NomeTeste(int indice, string descricao)
    {
        return "T" + indice.ToString("D3") + "_" + descricao;
    }

    string NomePesos(bool distMax, bool area, bool objetos, bool predios, bool ruas, bool profundidade)
    {
        List<string> partes = new List<string>();

        if (distMax) partes.Add("dmax");
        if (area) partes.Add("area");
        if (objetos) partes.Add("obj");
        if (predios) partes.Add("pred");
        if (ruas) partes.Add("rua");
        if (profundidade) partes.Add("prof");

        return string.Join("-", partes);
    }

    string NomePreservacao(bool preservaIso, bool preservaProfundidade)
    {
        if (!preservaIso && !preservaProfundidade)
            return "";

        string nome = "_pres";

        if (preservaIso)
            nome += "-iso";

        if (preservaProfundidade)
            nome += "-prof";

        return nome;
    }

    void AplicarConfiguracao(ConfigTeste c)
    {
        InputsMorfo.input_totalCasas = quantidadeCasas;

        InputsMorfo.boolRuaMaisUm = true;

        InputsMorfo.boolModoRandom = c.random;
        InputsMorfo.boolModoIsovista = c.isovista;
        InputsMorfo.boolModoIsoObj = c.isoOnlyObj;
        InputsMorfo.boolModoPreservaIso = c.preservaIso;
        InputsMorfo.boolModoPreservaProfundidade = c.preservaProfundidade;

        InputsMorfo.boolGravarImagens = true;
        InputsMorfo.boolApenasImagemFinal = true;

        InputsMorfo.peso_distanciaMaxima = c.distMax;
        InputsMorfo.peso_distanciaMinima = 0f;
        InputsMorfo.peso_distanciaMedia = 0f;
        InputsMorfo.peso_distanciaTotal = c.area;
        InputsMorfo.peso_total_obj_visto = c.objetos;
        InputsMorfo.peso_total_predio_visto = c.predios;
        InputsMorfo.peso_total_rua_visto = c.ruas;
        InputsMorfo.peso_total_profundidade_rua = c.profundidade;
    }
}
