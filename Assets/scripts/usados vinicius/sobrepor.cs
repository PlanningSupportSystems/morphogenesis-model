using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class sobrepor
{
    // Método para verificar colisão com prefabs existentes

    public static bool SP_SeEhPosicaoVazia(Vector3 endereco_teste, Vector3 half_extents)
    {
        
        if (InputsMorfo.tracking)
        {
            Debug.Log("tracking SP_SeEhPosicaoVazia");
        }

//        bool bateu = false;
        // Obtém todos os colliders dos prefabs existentes
//        if (Physics.OverlapBox(endereco_teste, half_extents).Length > 0) {
//            bateu = true;
//        }
        bool bateu = Physics.CheckBox(endereco_teste, half_extents, Quaternion.identity);
        //      bool bateu = Physics.BoxCast(endereco_teste, half_extents, Quaternion.identity.ToEulerAngles(), out RaycastHit quembati);//    (endereco_teste, half_extents, Quaternion.identity);
        //        Debug.Log("bateu em qtos? " + colliders.Length);
        //        Debug.Log("bati: "+ bateu);
        return bateu;
    }

    public static List<Vector3> SP_Vizinhanca(Vector3 _endereco_ref)
    {
        if (InputsMorfo.tracking)
        {
            Debug.Log("tracking SP_Vizinhanca");
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

    public static List<Predios> SP_QuaisVizinhos(Vector3 end_cheque)
    {
        if (InputsMorfo.tracking)
        {
            Debug.Log("tracking SP_QuaisVizinhos");
        }
        
        List<Predios> _os_vizinhos = new List<Predios>();
        // Obtém todos os colliders dos prefabs existentes
        Collider[] colliders = Physics.OverlapBox(end_cheque, Predios.half, Quaternion.identity);
//        Collider[] colliders = Physics.OverlapBox(transform.position, transform.localScale / 2f, Quaternion.identity);

        // Verifica se houve colisão com outros prefabs
//        foreach (Collider collider in colliders)
//        {
//            if (collider.    .gameObject   .gameObject != gameObject)
//            {
//                _os_vizinhos.Add(collider.GetComponentInParent<Predios>());
////                return true; // Retorna verdadeiro se houver colisão com outros prefabs
//            }
//        }

        return _os_vizinhos; // Retorna falso se não houver colisão com outros prefabs
  //      return false; // Retorna falso se não houver colisão com outros prefabs
    }


}