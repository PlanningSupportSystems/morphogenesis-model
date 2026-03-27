using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Celula
{
    public Vector2Int endereco;
    public Vector3 posicaoMundo;

    public lugar lugar;
    public novoPredio novoPredio;

    public bool TemLugar => lugar != null;
    public bool TemNovoPredio => novoPredio != null;
    public bool EstaLivreParaOcupar => lugar != null && novoPredio == null;

    public static readonly Vector2Int[] offsetsVonNeumann = new Vector2Int[]
    {
        new Vector2Int( 1,  0),  // leste
        new Vector2Int( 0,  1),  // norte
        new Vector2Int(-1,  0),   // oeste
        new Vector2Int( 0, -1),  // sul
    };

    public static readonly Vector2Int[] offsetsVonNeumannComplemento = new Vector2Int[]
    {
        new Vector2Int( 1,  1),  // leste
        new Vector2Int( -1,  1),  // norte
        new Vector2Int(-1,  -1),   // oeste
        new Vector2Int( 1, -1),  // sul

//        new Vector2Int(1,  1),  // nordeste
//        new Vector2Int(-1,  1),   // noroeste
//        new Vector2Int(-1, -1),  // sudoeste
//        new Vector2Int(1, -1),  // sudeste
    };

    public static readonly Vector2Int[] offsetsMoore = new Vector2Int[]
    {
        new Vector2Int( 1,  0),  // leste
        new Vector2Int( 1,  1),  // nordeste
        new Vector2Int( 0,  1),  // norte
        new Vector2Int(-1,  1),   // noroeste
        new Vector2Int(-1,  0),  // oeste
        new Vector2Int(-1, -1),  // sudoeste
        new Vector2Int( 0, -1),  // sul
        new Vector2Int( 1, -1),  // sudeste
    };

    public Celula(Vector2Int endereco, Vector3 posicaoMundo)
    {
        this.endereco = endereco;
        this.posicaoMundo = posicaoMundo;
    }

    public void addLugar(lugar lugar)
    {
        if (this.novoPredio != null)
        {
//            Debug.Log("CELULA: celula ja possui novoPredio, nao pode receber lugar");
            return;
        }
        this.lugar = lugar;

        string nomeCampo = this.lugar != null ? this.lugar._nome : null;
        string nomeMostrar = !string.IsNullOrEmpty(nomeCampo) ? nomeCampo : "<null-ou-vazio>";
        string goName = this.lugar != null ? this.lugar.gameObject.name : "<null-go>";
//        Debug.Log($"CELULA: adicionado lugar: {nomeMostrar} (gameObject.name: {goName})");
    }

    public void addnovoPredio(novoPredio novoPredio)
    {
        this.novoPredio = novoPredio;
        if (this.lugar != null)
        {
//            Debug.Log("CELULA: Destruindo lugar: " + this.lugar._nome + " para adicionar novoPredio: "+ this.novoPredio.np_nome);

            this.lugar.seDestruir();
            this.lugar = null;

//            if (this.lugar != null) Debug.Log("CELULA: nao destruiu o lugar: " + this.lugar._nome);
//            if (this.lugar == null) Debug.Log("CELULA: lugar destruido com sucesso"); 
        }
    }

    public void SeDestruir()
    {
        // Destrói e desconecta referências gerenciadas por Unity
        if (novoPredio != null)
        {
            try
            {
                // novoPredio.seDestruir() já chama Destroy(this.gameObject)
                novoPredio.seDestruir();
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Celula.SeDestruir: erro ao destruir novoPredio: {e.Message}");
            }
            novoPredio = null;
        }

        if (lugar != null)
        {
            try
            {
                // lugar.seDestruir() deve remover o GameObject e atualizar listas de ControleAglomeracao
                lugar.seDestruir();
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Celula.SeDestruir: erro ao destruir lugar: {e.Message}");
            }
            lugar = null;
        }

        // Se houver outros recursos (eventos, subscriptions), limpe-os aqui.
    }
}