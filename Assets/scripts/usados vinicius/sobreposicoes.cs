using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
public class sobreposicoes : MonoBehaviour
{
    public bool tracking = false;
//    public List<string> osQbateu;
    public Collider[] colliders;
    public bool bateu;
    List<Vector3> vizinhosParaChecar;

    private void Start()
    {
//        Debug.Log("tracking SS START");

        //       osQbateu = new List<string>();
    }

    private void OnTriggerEnter(Collider ooutro)
    {
        if (tracking)
        {
            Debug.Log("tracking SS OnTriggerEnter");
        }


        //        osQbateu.Add(ooutro.name);
        //        Debug.Log("bateu no ooutro" + ooutro.name);

    }


    public List<Vector3> SS_Vizinhanca(Vector3 _endereco_ref)
    {
        if (tracking)
        {
            Debug.Log("tracking SS_Vizinhanca");
        }
        //        vizinhosParaChecar.Clear();
        List<Vector3> vizinhanca = new List<Vector3>();
        //        Debug.Log("PREDIOS| local totalVizinhosPossiveis: " + totalVizinhosPossiveis);
        float _passo_angulo = (360 / (int)InputsMorfo.input_totalVizinhos) * Mathf.Deg2Rad;

        for (int i = 0; i < InputsMorfo.input_totalVizinhos; i++)
        {
            float angulo = _passo_angulo * i;
            float _x = Mathf.Cos(angulo) * InputsMorfo.input_distanciaAdjacencia;
            float _z = Mathf.Sin(angulo) * InputsMorfo.input_distanciaAdjacencia;
            //            _endereco_ref += new Vector3(_x, 0, _z);

            vizinhanca.Add(_endereco_ref + new Vector3(_x, 0, _z));
            //            Debug.Log("dps" +Controles.Geral_EnderecosVizinhosPossiveis.Count + "; end: " +Controles.Geral_EnderecosVizinhosPossiveis[i]);
            //            Debug.Log("dist "+distanciaAdjacencia+"; angulo"+ angulo * Mathf.Rad2Deg+"; i" + i + "; x " + _x +"; z"+ _z + "; vizinho adicionado: " + lista_end_vinhancaPossivelTotal[i]);
        }
        return vizinhanca;
    }

    public List<Vector3> SS_QuaisOsVizinhos(Vector3 end_checar, Vector3 half_extents)
    {
        if (tracking)
        {
            Debug.Log("tracking SS_QuaisOsVizinhos");
        }

        List<Vector3> sao_esses_vizinhos = new List<Vector3>();

        List<Vector3> _vizinhos_potenciais = new List<Vector3>(SS_Vizinhanca(end_checar));
        //        _vizinhos_potenciais = sobrepor.SP_Vizinhanca(end_checar);

//        Debug.Log("vizinhos potenciais: " + _vizinhos_potenciais.Count);

        foreach (Vector3 vp in _vizinhos_potenciais)
        {
            if (Physics.CheckBox(vp, half_extents, Quaternion.identity))
            {
                sao_esses_vizinhos.Add(vp);
//              Debug.Log(vp + "é vizinho");
            } else if (!Physics.CheckBox(vp, half_extents, Quaternion.identity))
            {
    //            Debug.Log(vp + "NAO é vizinho");
            }
        }
        //  Debug.Log("total de vizinhos: " + sao_esses_vizinhos.Count);

        return sao_esses_vizinhos;
    }

    public List<Vector3> SS_QuaisOsVizinhos_DosVizinhos(Vector3 end_checar, Vector3 half_extents)
    {
        if (tracking)
        {
            Debug.Log("tracking SS_QuaisOsVizinhos_DosVizinhos");
        }

        List<Vector3> sao_esses_vizinhos = new List<Vector3>();

        List<Vector3> _vizinhos_potenciais = SS_Vizinhanca(end_checar);
        //        _vizinhos_potenciais = sobrepor.SP_Vizinhanca(end_checar);

        //        Debug.Log("vizinhos: ");

        foreach (Vector3 vp in _vizinhos_potenciais)
        {
            if (Physics.CheckBox(vp, half_extents, Quaternion.identity))
            {
                sao_esses_vizinhos.Add(vp);
                //              Debug.Log(vp + "é vizinho");
            }
            else if (!Physics.CheckBox(vp, half_extents, Quaternion.identity))
            {
                //            Debug.Log(vp + "NAO é vizinho");
            }
        }
        //  Debug.Log("total de vizinhos: " + sao_esses_vizinhos.Count);

        return sao_esses_vizinhos;
    }

    public string SS_ChecaVizinhancadoAdjacente(Vector3 end_checagem, Vector3 half_extents)
    {
        if (tracking)
        {
            Debug.Log("tracking SS_ChecaVizinhancadoAdjacente");
        }

        ///  providenciar uma rotina para zerar todos os valores qd for comecar a checagem. aparentemente ta migrando leitura de uma checagem para outra
        ///

        string eh_pra_ser_rua = "nao";

        Debug.Log("antes 1a chegagem - checando vizinhanca de" + end_checagem);

        //GameObject[] allObjects = UnityEngine.Object.FindObjectsOfType<GameObject>();
        //// Iterar sobre cada GameObject
        //foreach (GameObject obj in allObjects)
        //{
        //    // Obter todos os colliders do GameObject atual
        //    Collider[] colliders = obj.GetComponentsInChildren<Collider>();
        //    // Destruir cada collider
        //    foreach (Collider collider in colliders)
        //    {
        //        //                string nomedopai = collider.gameObject.GetComponentInParent<Predios>().meuNome;
        //      //  Debug.Log("nome dos colider: " + collider.name); //+ "; parent: " + nomedopai);
        //    }
        //}

        List<Vector3> quais_vizinhos_existem = SS_QuaisOsVizinhos(end_checagem, half_extents);

        //        Debug.Log("total de vizinhos: " + quais_vizinhos_existem.Count);
        Debug.Log("checando vizinhanca de" + end_checagem+ " total de vizinhos: " + quais_vizinhos_existem.Count);
        // Converter cada Vector3 em uma string no formato desejado usando LINQ
//        string vetorString = string.Join(" ", quais_vizinhos_existem.Select(v => "(" + v.x + ", " + v.y + ", " + v.z + ")"));
        // Imprimir a string resultante em uma única linha de debug
  //      Debug.Log("vizinhos Vetores presentes na lista: " + vetorString);

        int _vizinhos_livres = (int)InputsMorfo.input_totalVizinhos;

        if (quais_vizinhos_existem.Count > 0)
        {
            Debug.Log("tem mais de um vizinho");

            int c = 0;
            foreach (Vector3 ve in quais_vizinhos_existem)
            //            for (int i =0; i< quais_vizinhos_existem.Count; i++)
            {
                List<Vector3> v_do_v = SS_QuaisOsVizinhos_DosVizinhos(ve, half_extents);
                //                List<Vector3> v_do_v = SS_QuaisOsVizinhos_DosVizinhos(quais_vizinhos_existem[i], half_extents);
 //               Debug.Log("ve: " + ve + " total d vizinhos: " + v_do_v.Count + "; input total vizinhos pedidos: " + _vizinhos_livres + "; saldo: " + (_vizinhos_livres - v_do_v.Count));

                if (_vizinhos_livres - v_do_v.Count < 2)
                {
                    eh_pra_ser_rua = "sim";
   //                 Debug.Log("eh pra ser rua: " + eh_pra_ser_rua);
                }
            }
        }

        quais_vizinhos_existem.Clear();
        Debug.Log("eh pra ser rua, pre return: " + eh_pra_ser_rua);

        return eh_pra_ser_rua;

    }
    // Método para verificar colisão com prefabs existentes
    public bool SS_ChecaSePosicaoVazia(Vector3 endereco_teste, Vector3 half_extents)
    {
        if (tracking)
        {
            Debug.Log("tracking SS_ChecaSePosicaoVazia");
        }


        // Obtém todos os colliders dos prefabs existentes
        bateu = Physics.CheckBox(endereco_teste, half_extents, Quaternion.identity);
        //        colliders = Physics.OverlapBox(endereco_teste, half_extents, Quaternion.identity);
        //        Debug.Log("bateu em qtos? " + colliders.Length);

        return bateu;

////            Debug.Log("nao bateu");
    }
}