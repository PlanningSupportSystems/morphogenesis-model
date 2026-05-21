using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TipoEspacoConstruido
{
    Predio,
    Rua,
    Lugar,
    Bloqueado
}
[CreateAssetMenu]
public class SO_EspacoConstruido : ScriptableObject
{
    public TipoEspacoConstruido tipo;
    public string nome;
    public Vector3 escala;
    public GameObject bloco;
    public string layer;    
    public Color cor;
    public Color cor_selecao;
    public Color cor_visto;

}

