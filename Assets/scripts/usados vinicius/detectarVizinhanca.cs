using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class detectarVizinhanca : MonoBehaviour
{

    public int quantidadeDeRaios = 8;  // Altere conforme necessário
    public float raio = 5f;  // Altere conforme necessário
    public List<float> angulosLivres2;
    public List<float> angulosUsados2;


    void Start()
    {
        GerarRaios();
    }


    void GerarRaios()
    {
        // Arrays para armazenar raios que atingiram e que não atingiram objetos
        RaycastHit[] raiosAtingiram = new RaycastHit[quantidadeDeRaios];
        RaycastHit[] raiosNaoAtingiram = new RaycastHit[quantidadeDeRaios];

        for (int i = 0; i < quantidadeDeRaios; i++)
        {
            float angulo = i * 360f / quantidadeDeRaios;
            float radianos = angulo * Mathf.Deg2Rad;

            float x = transform.position.x + raio * Mathf.Cos(radianos);
            float z = transform.position.z + raio * Mathf.Sin(radianos);

            Vector3 pontoNaCircunferencia = new Vector3 (x, transform.position.y, z);

            // Use Ray para representar um raio
            Ray raioAtual = new Ray(transform.position, pontoNaCircunferencia - transform.position);

            // Use RaycastHit para armazenar informações sobre a colisão
            RaycastHit hitInfo;

            // Realize o teste de colisão
            if (Physics.Raycast(raioAtual, out hitInfo, raio) && hitInfo.collider != GetComponent<Collider>())
            {
                // Se atingir um objeto, adicione ao array de raios que atingiram
                raiosAtingiram[i] = hitInfo;
                angulosUsados2.Add(angulo);
                Debug.DrawRay(transform.position, pontoNaCircunferencia - transform.position, Color.red, 10f);

                // Faça o que for necessário com o objeto atingido (por exemplo, acessar hitInfo.collider)
            }
            else
            {
                // Se não atingir nenhum objeto, adicione ao array de raios que não atingiram
//                raiosNaoAtingiram[i] = raioAtual;
                raiosNaoAtingiram[i] = hitInfo;
                angulosLivres2.Add(angulo);
                Debug.DrawRay(transform.position, pontoNaCircunferencia - transform.position, Color.blue, 10f);
            }


        }
//        return (angulosLivres);
//        Debug.Log(raiosAtingiram.Length + " raios bateram em alguem");
        // Crie raios visualmente (para debug)
        foreach (RaycastHit r in raiosAtingiram)
        {
  //          Debug.Log("bateu em" + r.collider );

        }

//        Debug.Log(raiosNaoAtingiram.Length + " raios nao bateram em alguem");
        foreach (RaycastHit r in raiosNaoAtingiram)
        {
    //        Debug.Log("bateu em" + r.collider);

        }
        //Debug.Log("total angulos " + quantidadeDeRaios);
        //Debug.Log("angulos livres " + angulosLivres2.Count);
        //Debug.Log("angulos usados " + angulosLivres2.Count);



    }



}
