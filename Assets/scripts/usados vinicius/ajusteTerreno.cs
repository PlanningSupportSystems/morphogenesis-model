using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ajusteTerreno : MonoBehaviour
{

    public Terrain terreno;
    public Camera cam;

    public event System.Action<float, string> mudouTerreno;

    // Start is called before the first frame update
    void Start()
    {
//        redefinirTerreno(10f);

    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public void redefinirTerreno(float _qtdCasas)
    {
        ///redefinicao do tamanho do terreno para acomodar toda a "cidade"
        float raio = Mathf.Sqrt(_qtdCasas)* 5 * InputsMorfo.input_distanciaAdjacencia ;
        terreno.terrainData.size = new Vector3(raio, 0, raio);
        terreno.transform.position = new Vector3(0, 0, 0);
//        Debug.Log("novo tamanho: " + raio);
        cam.GetComponent<ajusteCamera>().reposicionar();

        //        Debug.Log("LEVEL-MAPA CIRCULO: passo(rad): " + passoRad);
        //        terreno. .size = new Vector3(raio, 0, raio);       //dimensionamento variavel atribuido aki
        //       terrenoAtual = raio;

        mudouTerreno?.Invoke(raio, "terreno"); //event -> mudanca da variavel; broadcast 

    }
}
