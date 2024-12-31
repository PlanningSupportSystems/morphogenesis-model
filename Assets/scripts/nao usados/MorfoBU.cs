using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class MorfoBU : MonoBehaviour                ///morfogenese bottom up: formacao de ruas a quarteiroes a partir de locacao de novas casas
{

//    public List<Vector3> espacoConstruido;
//    public float distanciaVizinhanca = 30;


//    // Start is called before the first frame update
//    public void Start()
//    {
////        InputsMorfo t = new InputsMorfo();    propriedade nao static
////        Debug.Log("MorfoBU rolando " + InputsMorfo.ptz + t.teste);   propriedade static ->  ptz

//        espacoConstruido.Clear();
//        float inXmapa = GetComponent<levelgenerator>().terrain.GetComponent<Terrain>().terrainData.size.x/2;
//        float inZmapa = GetComponent<levelgenerator>().terrain.GetComponent<Terrain>().terrainData.size.z/2;

//        Vector3 tvector = new Vector3(inXmapa, 0f, inZmapa);
//        float Ymapa = Terrain.activeTerrain.SampleHeight(tvector);


//        espacoConstruido.Add(new Vector3(inXmapa, Ymapa, inZmapa));

//  //      GetComponent<levelgenerator>().enderecos = espacoConstruido;

////        Instantiate(GetComponent<levelgenerator>().casa, espacoConstruido[0], Quaternion.identity);

////        GetComponent<levelgenerator>().LG_ColocaPredios();


//    }


//    public void MB_MapaBottomUp()
//    {
//        GameObject prediosNovos = GetComponent<levelgenerator>().PredioAC;      //colocando os predios genericos inves de casa e trabalho

//        List<Predios> predios = GetComponent<levelgenerator>().predios;         //puxando do <levelgenerator>

//        foreach (Predios p in predios)
//        {
//            p.predioPreFab = prediosNovos;                                      //aki faz a troca de casa e trabalho pelos predios genericos
//        }        


//        float distanciaPredios = GetComponent<levelgenerator>().distanciaPredios;                              //

//        float passoRad = 2 * Mathf.PI / predios.Count;
//        float raio = (distanciaPredios) / (passoRad / 3);
//        //        Debug.Log("LEVEL-MAPA CIRCULO: passo(rad): " + passoRad);

//        Collider terreno = GetComponent<levelgenerator>().terrain.GetComponent<TerrainCollider>();             //puxando do <levelgenerator>

//        terreno.GetComponent<Terrain>().terrainData.size = new Vector3(raio * 2 + 20, 0, raio * 2 + 20);       //dimensionamento variavel atribuido aki

//        float lateralX = terreno.bounds.max.x - terreno.bounds.min.x;
//        float lateralZ = terreno.bounds.max.z - terreno.bounds.min.z;

////        int index = 1;

//        Vector3 tvector = new Vector3(0f, 0f, 0f);
//        float tempY = Terrain.activeTerrain.SampleHeight(tvector);


//        Vector3 posicaoZero = new Vector3(lateralX / 2, tempY, lateralZ / 2);
//        //            Debug.Log("LEVEL-MAPA CIRCULO: coordendas: " + posicao);
//        //            Debug.Log("LEVEL\MAPA CIRCULO: coordendas\n x: " + (indexX * passoX) + ", z: " + (indexZ * passoZ) + ", y: " + tempY);

//        List<Vector3> endLocal = new List<Vector3>
//        {
//            posicaoZero
//        };// (predios.Count);
//        //endLocal.Add(posicaoZero);
//        //List<Vector3> endLocal = GetComponent<levelgenerator>().enderecos;
//        //endLocal.Add(posicaoZero);
//        //        enderecos.Add(posicaoZero);

//        float distPredios = 5; // distancia base entre predios
//        float rangeDistPredios = 20 / 100; //range de aleatoriedade entre os predios 

////        List<int> idPredios = new List<int>();
////        for (int i = 0; i < GetComponent<levelgenerator>().predios.Count; i++)
////        {
////            idPredios[i] = i;
////        }

//        predios[0].meuEnderecoXYZ = endLocal[0];
//        for (int i = 1; i < GetComponent<levelgenerator>().predios.Count; i++)
//        {
//            //selecao aleatoria dos pontos ja existentes
//            int idTemp = Random.Range(0, i-1);
//            if (idTemp < 0) { idTemp = 0; }
//            predios[idTemp].CalcularAnguloVizinhos(endLocal, distanciaVizinhanca);


//            float x = endLocal[idTemp].x;                           //Mathf.Cos(passoRad * index) * raio + lateralX / 2;                //  X = DISTANCIA_PADRAO * COS(ANGULO ATUAL * I * PASSO_RAD) 
//            float z = endLocal[idTemp].z;                           // Mathf.Sin(passoRad * index) * raio + lateralZ / 2;                // 

//            float Vdist = Random.Range(distPredios / 2, distPredios) * (1 + Random.Range(0, rangeDistPredios));

//            //int a = i - 1;
//            float angInicial = predios[idTemp].angVizinhoInicial;
//            float angFinal =  predios[idTemp].angVizinhoFinal;
            
//            float direcao;

////            predios[idTemp].P_DetectaVizinhos();
////            List<float> ang = new List<float>(predios[idTemp].angulosLivres);

////            List<float> ang = new List<float>( predios[idTemp].predioPreFab.GetComponent<detectarVizinhanca>().angulosLivres);
////            Debug.Log("angulos livres: " + ang.Count);
//            /*
//            while (direcao >= predios[i].angVizinhoInicial && direcao <= predios[i].angVizinhoFinal)
//            {
//                // Se estiver, gerar um novo ângulo aleatório
//                direcao = Random.Range(0f, 360f);
//            }
//            */
//            direcao = Mathf.Deg2Rad * Random.Range(angInicial,angFinal);   //feito em intervalo de angulo, trocado para angulos disponiveis <detectarVizinhanca>
// //           if (ang == null) { break; }
////            direcao = ang[Random.Range(0, ang.Count)];

//            x += Mathf.Cos(direcao) * Vdist;                //  X = DISTANCIA_PADRAO * COS(ANGULO ATUAL * I * PASSO_RAD) 
//            z += Mathf.Sin(direcao) * Vdist;                // 

//            //Debug.Log("LEVEL-MAPA morfogenese: direcao: " + direcao);

//            Vector3 posicao = new Vector3(x, tempY, z);
//            //            Debug.Log("LEVEL-MAPA CIRCULO: coordendas: " + posicao);
//            //            Debug.Log("LEVEL\MAPA CIRCULO: coordendas\n x: " + (indexX * passoX) + ", z: " + (indexZ * passoZ) + ", y: " + tempY);

//            endLocal.Add(posicao);


//            predios[i].meuEnderecoXYZ = posicao;
////            Debug.Log("predio:"+i+", total predios: " + predios.Count + "; total de enderecos: " + endLocal.Count);
////            predios[i].CalcularAnguloVizinhos(endLocal, distanciaVizinhanca);
//            //            Instantiate(predios[i].tipoPredio, posicao, Quaternion.identity);

////            index += 1;

//        }

//        GetComponent<levelgenerator>().enderecos = endLocal;
////        GetComponent<levelgenerator>().terrain.       .terrain = terreno;

//        GetComponent<levelgenerator>().LG_ColocaPredios();




////        Debug.Log("fazer aki do comeco");
//    }





///*
//public void mapaMorfogenese()
//{

//    float passoRad = 2 * Mathf.PI / predios.Count;
//    float raio = (distanciaPredios) / (passoRad / 3);
//    //        Debug.Log("LEVEL-MAPA CIRCULO: passo(rad): " + passoRad);

//    terrain.GetComponent<Terrain>().terrainData.size = new Vector3(raio * 2 + 20, 0, raio * 2 + 20);       //dimensionamento variavel atribuido aki

//    Collider terreno = terrain.GetComponent<TerrainCollider>();
//    float lateralX = terreno.bounds.max.x - terreno.bounds.min.x;
//    float lateralZ = terreno.bounds.max.z - terreno.bounds.min.z;

//    int index = 1;

//    Vector3 tvector = new Vector3(0f, 0f, 0f);
//    float tempY = Terrain.activeTerrain.SampleHeight(tvector);

//    Vector3 posicaoZero = new Vector3(lateralX / 2, tempY, lateralZ / 2);
//    //            Debug.Log("LEVEL-MAPA CIRCULO: coordendas: " + posicao);
//    //            Debug.Log("LEVEL\MAPA CIRCULO: coordendas\n x: " + (indexX * passoX) + ", z: " + (indexZ * passoZ) + ", y: " + tempY);

//    enderecos.Add(posicaoZero);

//    float distPredios = 5; // distancia base entre predios
//    float rangeDistPredios = 20 / 100; //range de aleatoriedade entre os predios 

//    for (int i = 0; i < predios.Count; i++)
//    {
//        //            Debug.Log("LEVEL-MAPA CIRCULO: index x: " + index);
//        float x = enderecos[i].x;                           //Mathf.Cos(passoRad * index) * raio + lateralX / 2;                //  X = DISTANCIA_PADRAO * COS(ANGULO ATUAL * I * PASSO_RAD) 
//        float z = enderecos[i].z;                           // Mathf.Sin(passoRad * index) * raio + lateralZ / 2;                // 

//        float Vdist = Random.Range(distPredios / 2, distPredios) * (1 + Random.Range(0, rangeDistPredios));
//        float direcao = Random.Range(0, 2 * Mathf.PI);

//        x += Mathf.Cos(direcao) * Vdist;                //  X = DISTANCIA_PADRAO * COS(ANGULO ATUAL * I * PASSO_RAD) 
//        z += Mathf.Sin(direcao) * Vdist;                // 

//        Debug.Log("LEVEL-MAPA morfogenese: direcao: " + direcao);

//        Vector3 posicao = new Vector3(x, tempY, z);
//        //            Debug.Log("LEVEL-MAPA CIRCULO: coordendas: " + posicao);
//        //            Debug.Log("LEVEL\MAPA CIRCULO: coordendas\n x: " + (indexX * passoX) + ", z: " + (indexZ * passoZ) + ", y: " + tempY);

//        enderecos.Add(posicao);
//        //            Instantiate(predios[i].tipoPredio, posicao, Quaternion.identity);

//        index += 1;

//    }
//    LG_ColocaPredios();



//}
//*/

//// Update is called once per frame
////void Update()
////    {
        
////    }
}
