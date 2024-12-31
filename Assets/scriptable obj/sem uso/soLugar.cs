using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "tipo lugar", menuName = "lugar")]
public class so_Predios : ScriptableObject
{
    public string nome;
    public int capacidade;

    public int[] horarios;
    int[] horaComeca;
    int []horaFim;

    public so_Predios()
    {
        if (horarios != null)
        {
            horaComeca = new int[horarios.Length / 2];
            for (int i = 0; i <= horarios.Length; i++)
            {
                horaComeca[i] = horarios[i * 2];
                Debug.Log("soPREDIO: hora de comecar: " + horaComeca[i]);
            }
            horaFim = new int[horarios.Length / 2];

        }

    }
}
