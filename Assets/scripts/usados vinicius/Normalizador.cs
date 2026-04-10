// Normalizador.cs

using UnityEngine;

#region STRUCTS

[System.Serializable]
public struct MedidasBrutas
{
    public float distanciaMaxima;
    public float distanciaMedia;
    public float distanciaMinima;
//    public float distanciaTotal;
    public float totalObjVisto;
    public float totalPrediosVistos;
    public float totalRuasVistas;
    public float ProfundidadeRua;
    public float areaIsovista;

    public string Publicar()
    {
        return
            $"--- MEDIDAS BRUTAS ---\n" +

            $"distanciaMaxima   : {distanciaMaxima}\n" +
            $"distanciaMedia    : {distanciaMedia}\n" +
            $"distanciaMinima   : {distanciaMinima}\n" +

            $"totalObjVisto     : {totalObjVisto}\n" +
            $"totalPredios      : {totalPrediosVistos}\n" +
            $"totalRuas         : {totalRuasVistas}\n" +

            $"profundidadeRua   : {ProfundidadeRua}\n" +
            $"areaIsovista      : {areaIsovista}\n";
    }
    public MedidasBrutas(
                        float distanciaMaxima,
                        float distanciaMedia,
                        float distanciaMinima,
//                        float distanciaTotal,
                        float totalObjVisto,
                        float totalPrediosVistos,
                        float totalRuasVistas,
                        float totalProfundidadeRua,
                        float areaIsovista

                        )
    {
        this.distanciaMaxima = distanciaMaxima;
        this.distanciaMedia = distanciaMedia;
        this.distanciaMinima = distanciaMinima;
 //       this.distanciaTotal = distanciaTotal;

        this.totalObjVisto = totalObjVisto;
        this.totalPrediosVistos = totalPrediosVistos;
        this.totalRuasVistas = totalRuasVistas;
        this.ProfundidadeRua = totalProfundidadeRua;
        this.areaIsovista = areaIsovista;
    }

}

[System.Serializable]
public struct ValoresReferenciaNormalizacao
{
    public float distanciaMaxima_Min;
    public float distanciaMaxima_Max;

    public float distanciaMedia_Min;
    public float distanciaMedia_Max;

    public float distanciaMinima_Min;
    public float distanciaMinima_Max;

//    public float distanciaTotal_Min;
//    public float distanciaTotal_Max;

    public float totalObjVisto_Min;
    public float totalObjVisto_Max;

    public float totalPrediosVistos_Min;
    public float totalPrediosVistos_Max;

    public float totalRuasVistas_Min;
    public float totalRuasVistas_Max;

    public float ProfundidadeRua_Min;
    public float ProfundidadeRua_Max;

    public float areaIsovista_Min;
    public float areaIsovista_Max;

    public ValoresReferenciaNormalizacao(MedidasBrutas primeiro)
    {
//        min_distanciaTotal = primeiro.distanciaTotal;
//        max_distanciaTotal = primeiro.distanciaTotal;

        distanciaMaxima_Min = primeiro.distanciaMaxima;
        distanciaMaxima_Max = primeiro.distanciaMaxima;

        distanciaMedia_Min = primeiro.distanciaMedia;
        distanciaMedia_Max = primeiro.distanciaMedia;

        distanciaMinima_Min = primeiro.distanciaMinima;
        distanciaMinima_Max = primeiro.distanciaMinima;

        totalObjVisto_Min = primeiro.totalObjVisto;
        totalObjVisto_Max = primeiro.totalObjVisto;

        totalPrediosVistos_Min = primeiro.totalPrediosVistos;
        totalPrediosVistos_Max = primeiro.totalPrediosVistos;

        totalRuasVistas_Min = primeiro.totalRuasVistas;
        totalRuasVistas_Max = primeiro.totalRuasVistas;

        ProfundidadeRua_Min = primeiro.ProfundidadeRua;
        ProfundidadeRua_Max = primeiro.ProfundidadeRua;

        areaIsovista_Min = primeiro.areaIsovista;
        areaIsovista_Max = primeiro.areaIsovista;
    }
    public string Publicar()
    {
        return
            $"--- REFERENCIA NORMALIZACAO ---\n" +

            $"distanciaMaxima   | Min: {distanciaMaxima_Min}   Max: {distanciaMaxima_Max}\n" +
            $"distanciaMedia    | Min: {distanciaMedia_Min}    Max: {distanciaMedia_Max}\n" +
            $"distanciaMinima   | Min: {distanciaMinima_Min}   Max: {distanciaMinima_Max}\n" +

            $"totalObjVisto     | Min: {totalObjVisto_Min}     Max: {totalObjVisto_Max}\n" +
            $"totalPredios      | Min: {totalPrediosVistos_Min} Max: {totalPrediosVistos_Max}\n" +
            $"totalRuas         | Min: {totalRuasVistas_Min}   Max: {totalRuasVistas_Max}\n" +

            $"profundidadeRua   | Min: {ProfundidadeRua_Min} Max: {ProfundidadeRua_Max}\n" +
            $"areaIsovista      | Min: {areaIsovista_Min}      Max: {areaIsovista_Max}\n";
    }
}

[System.Serializable]
public struct MedidasNormalizadas
{
    public float distanciaMaxima;
    public float distanciaMinima;
    public float distanciaMedia;
    public float distanciaTotal;

    public float totalObjVisto;
    public float totalPrediosVistos;
    public float totalRuasVistas;
    public float ProfundidadeRua;
    public float areaIsovista;

    public string Publicar()
    {
        return
            $"--- MEDIDAS NORMALIZADAS ---\n" +

            $"distanciaMaxima   : {distanciaMaxima}\n" +
            $"distanciaMedia    : {distanciaMedia}\n" +
            $"distanciaMinima   : {distanciaMinima}\n" +
            $"distanciaTotal    : {distanciaTotal}\n" +

            $"totalObjVisto     : {totalObjVisto}\n" +
            $"totalPredios      : {totalPrediosVistos}\n" +
            $"totalRuas         : {totalRuasVistas}\n" +

            $"profundidadeRua   : {ProfundidadeRua}\n" +
            $"areaIsovista      : {areaIsovista}\n";
    }
}

#endregion

#region NORMALIZADOR

public static class Normalizador
{
    public static MedidasNormalizadas Normalizar(
        MedidasBrutas brutas,
        ValoresReferenciaNormalizacao referencia)
    {
        return new MedidasNormalizadas
        {
            distanciaMaxima = NormalizarValor(brutas.distanciaMaxima, referencia.distanciaMaxima_Min, referencia.distanciaMaxima_Max),
            distanciaMinima = NormalizarValor(brutas.distanciaMinima, referencia.distanciaMinima_Min, referencia.distanciaMinima_Max),
            distanciaMedia = NormalizarValor(brutas.distanciaMedia, referencia.distanciaMedia_Min, referencia.distanciaMedia_Max),
//            distanciaTotal = NormalizarValor(brutas.distanciaTotal, referencia.distanciaTotal_Min, referencia.distanciaTotal_Max),
            totalObjVisto = NormalizarValor(brutas.totalObjVisto, referencia.totalObjVisto_Min, referencia.totalObjVisto_Max),
            totalPrediosVistos = NormalizarValor(brutas.totalPrediosVistos, referencia.totalPrediosVistos_Min, referencia.totalPrediosVistos_Max),
            totalRuasVistas = NormalizarValor(brutas.totalRuasVistas, referencia.totalRuasVistas_Min, referencia.totalRuasVistas_Max),
            ProfundidadeRua = NormalizarValor(brutas.ProfundidadeRua, referencia.ProfundidadeRua_Min, referencia.ProfundidadeRua_Max),

            areaIsovista = NormalizarValor(brutas.areaIsovista, referencia.areaIsovista_Min, referencia.areaIsovista_Max)
        };
    }
    private static float NormalizarValor(float valor, float min, float max)
    {
        if (Mathf.Approximately(max, min))
            return 1f; //se os valores sao muitos proximos, ela nao faz diferenca. AVALIAR sobre atribuir 1 ou 0.

        return Mathf.Clamp01((valor - min) / (max - min));
    }

    public static void ChecarSeReferencia(ref ValoresReferenciaNormalizacao referencia, MedidasBrutas medidas)
    {
       /* // distanciaTotal
        if (medidas.distanciaTotal < referencia.min_distanciaTotal)
            referencia.min_distanciaTotal = medidas.distanciaTotal;
        if (medidas.distanciaTotal > referencia.max_distanciaTotal)
            referencia.max_distanciaTotal = medidas.distanciaTotal;
        */

        // distanciaMaxima
        if (medidas.distanciaMaxima < referencia.distanciaMaxima_Min)
            referencia.distanciaMaxima_Min = medidas.distanciaMaxima;
        if (medidas.distanciaMaxima > referencia.distanciaMaxima_Max)
            referencia.distanciaMaxima_Max = medidas.distanciaMaxima;

        // distanciaMedia
        if (medidas.distanciaMedia < referencia.distanciaMedia_Min)
            referencia.distanciaMedia_Min = medidas.distanciaMedia;
        if (medidas.distanciaMedia > referencia.distanciaMedia_Max)
            referencia.distanciaMedia_Max = medidas.distanciaMedia;

        // distanciaMinima
        if (medidas.distanciaMinima < referencia.distanciaMinima_Min)
            referencia.distanciaMinima_Min = medidas.distanciaMinima;
        if (medidas.distanciaMinima > referencia.distanciaMinima_Max)
            referencia.distanciaMinima_Max = medidas.distanciaMinima;

        // distanciaTotal;
//        if (medidas.distanciaTotal < referencia.distanciaTotal_Min)
//            referencia.distanciaTotal_Min = medidas.distanciaTotal;
//        if (medidas.distanciaTotal > referencia.distanciaTotal_Max)
//            referencia.distanciaTotal_Max = medidas.distanciaTotal;

        // totalObjVisto
        if (medidas.totalObjVisto < referencia.totalObjVisto_Min)
            referencia.totalObjVisto_Min = medidas.totalObjVisto;
        if (medidas.totalObjVisto > referencia.totalObjVisto_Max)
            referencia.totalObjVisto_Max = medidas.totalObjVisto;

        // totalPredioVisto
        if (medidas.totalPrediosVistos < referencia.totalPrediosVistos_Min)
            referencia.totalPrediosVistos_Min = medidas.totalPrediosVistos;
        if (medidas.totalPrediosVistos > referencia.totalPrediosVistos_Max)
            referencia.totalPrediosVistos_Max = medidas.totalPrediosVistos;

        // totalRuaVisto
        if (medidas.totalRuasVistas < referencia.totalRuasVistas_Min)
            referencia.totalRuasVistas_Min = medidas.totalRuasVistas;
        if (medidas.totalRuasVistas > referencia.totalRuasVistas_Max)
            referencia.totalRuasVistas_Max = medidas.totalRuasVistas;

        // ProfundidadeRua
        if (medidas.ProfundidadeRua < referencia.ProfundidadeRua_Min)
            referencia.ProfundidadeRua_Min = medidas.ProfundidadeRua;
        if (medidas.ProfundidadeRua > referencia.ProfundidadeRua_Max)
            referencia.ProfundidadeRua_Max = medidas.ProfundidadeRua;

        // areaIsovista
        if (medidas.areaIsovista < referencia.areaIsovista_Min)
            referencia.areaIsovista_Min = medidas.areaIsovista;
        if (medidas.areaIsovista > referencia.areaIsovista_Max)
            referencia.areaIsovista_Max = medidas.areaIsovista;

    }
}

#endregion