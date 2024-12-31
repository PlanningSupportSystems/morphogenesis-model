using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ajusteCamera : MonoBehaviour
{

    public List<Transform> alvos;
    //buscar tudo com tag predio
    public Vector3 offset;
    public GameObject terreno;





    public void reposicionar()
    {
      //  terreno = _terreno;// GameObject.Find("Terrain");
        Vector3 centro = centroCam();
//        offset = new Vector3(-centro.x * 2 / 3 /*- 3*/, alturaCam() / 2f, -centro.z * 2 / 3/*-10*/);
        offset = new Vector3(0, alturaCam() / 2f, 0);
        transform.position = centro + offset;
        transform.LookAt (centro);
        transform.GetComponent<Camera>().orthographic = true;
        if (InputsMorfo.input_totalCasas == null) { transform.GetComponent<Camera>().orthographicSize = 5; }
        else { transform.GetComponent<Camera>().orthographicSize = Mathf.Sqrt( InputsMorfo.input_totalCasas ) * 3; }
        
        //        Debug.Log("centro: " + centro + " altura: " + offset);

    }

    float alturaCam()
    {
     //   GameObject terreno = GameObject.Find("Terrain");
        float corda;
        float x = terreno.GetComponent<Terrain>().terrainData.size.x;
        float z = terreno.GetComponent<Terrain>().terrainData.size.z;
        if (z < x) { corda = x; } else { corda = z; }

//        float altura = ((-corda) * 3);//  / Mathf.Sin(30));
        float altura = ((-corda) / (Mathf.Sin(30)/2));
        return altura ;
    }

    
    public Vector3 centroCam( )
    {

        //      GameObject terreno = GameObject.Find("Terrain");
        //        Debug.Log("posicao do terreno: " + terreno.transform.position);
        //        float corda;
        Vector3 centroTerreno = new Vector3(terreno.GetComponent<Terrain>().terrainData.size.x / 2, 0, terreno.GetComponent<Terrain>().terrainData.size.z / 2); ;

//        Vector3 centroTerreno = new Vector3(terreno.GetComponent<Terrain>().terrainData.size.x/2, 0, terreno.GetComponent<Terrain>().terrainData.size.z/2);
    //    Debug.Log("centro do terreno: " + centroTerreno);

        //Debug.Log("centro bounds: " + bounds.center);
        return centroTerreno;//  bounds.center;
    }


    /// <summary>
    /// /////
    //    using UnityEngine;

    //public class CameraController : MonoBehaviour
    //{
    public float moveSpeed = 10f;
    public float rotationSpeed = 100f;
    public float zoomSpeed = 1000f;
    public float minZoomDistance = 2f;
    public float maxZoomDistance = 20f;
    public float heightSpeed = 5f;
    public float minHeight = 1f;
    public float maxHeight = 5000f;

    public RectTransform areaMenu;

    public void Start()
    {
//        AdjustCameraToFitAllObjects();
    }
    private void Update()
    {
        if (cameraTaNoMenu())
        {
            return; // Se o ponteiro não está na área permitida, saia do método Update
        }
        // Movimento da câmera com as setas do teclado
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        // Controle da altura com as teclas "X" e "Z"
        float heightInput = Input.GetAxis("Jump");

        Vector3 movement = new Vector3(horizontal, heightInput, vertical) * moveSpeed * Time.deltaTime;
        transform.Translate(movement, Space.Self);

        // Rotação da câmera com o mouse
        if (Input.GetMouseButton(0))
        {
            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");

            Vector3 rotation = new Vector3(-mouseY, mouseX, 0f) * rotationSpeed * Time.deltaTime;
            transform.Rotate(rotation, Space.Self);
        }

        // Zoom in e zoom out com o mousepad
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        Vector3 zoom = transform.forward * scroll * zoomSpeed * Time.deltaTime;
        transform.Translate(zoom, Space.World);

        // Limitar o zoom dentro do intervalo definido
        Vector3 position = transform.position;
        position.y = Mathf.Clamp(position.y, minZoomDistance, maxZoomDistance);
        transform.position = position;

    }
    //}

    private bool cameraTaNoMenu()
    {
        Vector2 mousePosition = Input.mousePosition;
        return RectTransformUtility.RectangleContainsScreenPoint(areaMenu, mousePosition, null);
    }

    /// </summary>

    public void AdjustCameraToFitAllObjects()
    {
        Camera camera = Camera.main;
        if (camera == null)
            return;

        // Coletar todos os Renderers na cena
        Renderer[] renderers = FindObjectsOfType<Renderer>();
        if (renderers.Length == 0)
            return;

        // Determinar os limites dos objetos
        Bounds bounds = new Bounds(renderers[0].bounds.center, renderers[0].bounds.size);
        foreach (Renderer renderer in renderers)
        {
            bounds.Encapsulate(renderer.bounds);
        }

        // Ajustar a posição da câmera
        Vector3 center = bounds.center;
        float distance = bounds.extents.magnitude / Mathf.Tan(camera.fieldOfView * 0.5f * Mathf.Deg2Rad);

        // Ajustar a posição da câmera para centralizar
        camera.transform.position = new Vector3(center.x, center.y, center.z - distance);

        // A câmera deve olhar para o centro dos objetos
        camera.transform.LookAt(center);
    }


}
