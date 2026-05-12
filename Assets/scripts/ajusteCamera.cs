using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ajusteCamera : MonoBehaviour
{

    public List<Transform> alvos;
    //buscar tudo com tag predio
    public Vector3 offset;
    public GameObject terreno;

    // flag para lembrar se o clique atual começou sobre UI (areaMenu1 ou areaMenu2)
    private bool pointerDownOnUI = false;

    //public class CameraController : MonoBehaviour
    ////
    public float moveSpeed = 200f;            // pan speed (keyboard / mouse)
    public float rotationSpeed = 400f;       // orbit speed (mouse) - aumentado
    public float zoomSpeed = 20000f;          // dolly speed (scroll) - aumentado
    public float minZoomDistance = 2f;       // min distance to target (dolly)
    public float maxZoomDistance = 200f;     // max distance to target (dolly)
    public float heightSpeed = 5f;
    public float minHeight = 1f;
    public float maxHeight = 5000f;

    public RectTransform areaMenu1;
    public RectTransform areaMenu2;
    public JanelaMundoRender janelaMundo;

    // orbit state
    private float yaw = 0f;
    private float pitch = 30f;
    private float distance = 50f; // current distance to target

    // persistent pivot and last visible rect
    private Vector3 pivotWorld;
    private Rect lastVisibleRect;

    void Start()
    {
        ObterJanelaMundo();

        // inicializa distância e ângulos a partir da transform atual e do centro do terreno
        Vector3 target = centroCam();
        distance = Vector3.Distance(transform.position, target);
        Vector3 angles = transform.eulerAngles;
        yaw = angles.y;
        pitch = angles.x;

        // inicializa pivot a partir da área visível atual
        lastVisibleRect = GetVisibleScreenRect();
        pivotWorld = ScreenPointToWorldOnTerrainPlane(lastVisibleRect.center);
    }

    private void Update()
    {
        // Ao pressionar qualquer botão do mouse, registramos se o clique começou sobre UI (panel)
        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2))
        {
            pointerDownOnUI = cameraTaNoMenu();
        }

        // Se o usuário soltou qualquer botão, resetamos a flag (libera controle da câmera)
        if (Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1) || Input.GetMouseButtonUp(2))
        {
            pointerDownOnUI = false;
        }

        // Se o clique atual começou sobre UI, BLOQUEAMOS totalmente qualquer ajuste de posição/rotacao/zoom
        if (pointerDownOnUI)
        {
            return;
        }

        // Se o ponteiro estiver sobre o menu agora, também bloqueamos (cobre scroll/teclas)
        if (cameraTaNoMenu())
        {
            return;
        }

        // checa se a área visível mudou (menus reposicionados/visibilidade alterada)
        Rect currentVisibleRect = GetVisibleScreenRect();
        if (VisibleRectChanged(currentVisibleRect, lastVisibleRect))
        {
            // recenter pivot para o novo centro visível, mantendo yaw/pitch/distance
            pivotWorld = ScreenPointToWorldOnTerrainPlane(currentVisibleRect.center);
            lastVisibleRect = currentVisibleRect;
        }

        // --- ZOOM (dolly) ---
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.0001f)
        {
            Camera cam = GetComponent<Camera>();
            if (cam != null && cam.orthographic)
            {
                cam.orthographicSize = Mathf.Clamp(cam.orthographicSize - scroll * (zoomSpeed * 0.0005f), 0.5f, 200f);
            }
            else
            {
                float scale = Mathf.Max(0.02f, distance * 0.02f);
                distance = Mathf.Clamp(distance - scroll * zoomSpeed * scale * Time.deltaTime, minZoomDistance, maxZoomDistance);
            }
        }

        // --- ROTATION (orbit) ---
        if (Input.GetMouseButton(0))
        {
            float mx = Input.GetAxis("Mouse X");
            float my = Input.GetAxis("Mouse Y");

            yaw += mx * rotationSpeed * Time.deltaTime;
            pitch -= my * rotationSpeed * Time.deltaTime;
            pitch = Mathf.Clamp(pitch, 5f, 85f);
        }

        // --- PAN ---
        Vector3 panDelta = Vector3.zero;
        if (Input.GetMouseButton(2) || Input.GetMouseButton(1))
        {
            float mx = Input.GetAxis("Mouse X");
            float my = Input.GetAxis("Mouse Y");

            Vector3 right = transform.right;
            Vector3 forward = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;

            float panScale = moveSpeed * Mathf.Max(0.02f, distance * 0.02f);
            panDelta = (-right * mx + -forward * my) * panScale * Time.deltaTime;
        }
        else
        {
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");
            if (Mathf.Abs(horizontal) > 0.0001f || Mathf.Abs(vertical) > 0.0001f)
            {
                Vector3 right = transform.right;
                Vector3 forward = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
                panDelta = (right * horizontal + forward * vertical) * moveSpeed * Time.deltaTime * (distance * 0.02f);
            }
        }

        // aplica pan no pivot persistente
        pivotWorld += panDelta;

        // recomputa posição da câmera a partir de yaw/pitch/distance em torno do pivot persistente
        Quaternion rot = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 offsetFromTarget = rot * new Vector3(0f, 0f, -distance);
        transform.position = pivotWorld + offsetFromTarget;
        transform.LookAt(pivotWorld);

        // mantém a câmera dentro de um intervalo de altura
        Vector3 pos = transform.position;
        pos.y = Mathf.Clamp(pos.y, minHeight, maxHeight);
        transform.position = pos;
    }

    // decide se dois rects diferem significativamente
    private bool VisibleRectChanged(Rect a, Rect b, float eps = 1f)
    {
        if (Mathf.Abs(a.xMin - b.xMin) > eps) return true;
        if (Mathf.Abs(a.xMax - b.xMax) > eps) return true;
        if (Mathf.Abs(a.yMin - b.yMin) > eps) return true;
        if (Mathf.Abs(a.yMax - b.yMax) > eps) return true;
        return false;
    }

    // Retorna o rect da tela disponível para visualização depois de "remover" menus.
    // Estratégia: parte da tela inteira e, para cada painel atribuído, se o painel toca uma borda,
    // "corta" essa borda do rect visível (esquerda/direita/top/bottom).
    private Rect GetVisibleScreenRect()
    {
        if (janelaMundo != null)
            return janelaMundo.GetScreenRect();

        Rect visible = new Rect(0, 0, Screen.width, Screen.height);

        Rect[] menus = new Rect[2];
        menus[0] = areaMenu1 != null ? ScreenRectFromRectTransform(areaMenu1) : new Rect();
        menus[1] = areaMenu2 != null ? ScreenRectFromRectTransform(areaMenu2) : new Rect();

        float eps = 1f; // tolerância de borda

        foreach (var m in menus)
        {
            if (m.width <= 0 || m.height <= 0) continue;

            // se o menu toca na borda esquerda da tela -> cortar à direita do menu
            if (m.xMin <= visible.xMin + eps && m.xMax > visible.xMin + eps)
            {
                visible.xMin = Mathf.Max(visible.xMin, m.xMax);
            }
            // se o menu toca na borda direita -> cortar esquerda
            if (m.xMax >= visible.xMax - eps && m.xMin < visible.xMax - eps)
            {
                visible.xMax = Mathf.Min(visible.xMax, m.xMin);
            }
            // se o menu toca na borda inferior -> cortar inferior
            if (m.yMin <= visible.yMin + eps && m.yMax > visible.yMin + eps)
            {
                visible.yMin = Mathf.Max(visible.yMin, m.yMax);
            }
            // se o menu toca na borda superior -> cortar superior
            if (m.yMax >= visible.yMax - eps && m.yMin < visible.yMax - eps)
            {
                visible.yMax = Mathf.Min(visible.yMax, m.yMin);
            }
        }

        // evita rect inválido
        if (visible.width < 10f) visible.width = Screen.width;
        if (visible.height < 10f) visible.height = Screen.height;

        return visible;
    }

    // Converte um RectTransform para Rect em coordenadas de tela (Screen space)
    private Rect ScreenRectFromRectTransform(RectTransform rt)
    {
        if (rt == null) return new Rect();

        Canvas canvas = rt.GetComponentInParent<Canvas>();
        Camera cam = null;
        if (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
            cam = canvas.worldCamera;

        Vector3[] corners = new Vector3[4];
        rt.GetWorldCorners(corners);

        Vector2 min = new Vector2(float.MaxValue, float.MaxValue);
        Vector2 max = new Vector2(float.MinValue, float.MinValue);

        for (int i = 0; i < 4; i++)
        {
            Vector2 sp = RectTransformUtility.WorldToScreenPoint(cam, corners[i]);
            min = Vector2.Min(min, sp);
            max = Vector2.Max(max, sp);
        }

        return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
    }

    // Converte um ponto de tela para um ponto no mundo sobre o plano do terreno (y = terreno.position.y)
    private Vector3 ScreenPointToWorldOnTerrainPlane(Vector2 screenPoint)
    {
        Camera cam = Camera.main ?? GetComponent<Camera>();
        if (cam == null || terreno == null) return centroCam();

        Ray ray;
        if (janelaMundo != null && janelaMundo.TryScreenPointToRay(screenPoint, out Ray rayJanela))
            ray = rayJanela;
        else
            ray = cam.ScreenPointToRay(screenPoint);

        float terrainY = terreno.transform.position.y;
        Plane plane = new Plane(Vector3.up, new Vector3(0f, terrainY, 0f));
        if (plane.Raycast(ray, out float enter))
        {
            return ray.GetPoint(enter);
        }
        return centroCam();
    }

    private bool cameraTaNoMenu()
    {
        if (ObterJanelaMundo() != null)
            return !janelaMundo.MouseDentroDaJanela();

        // 1) Se houver um EventSystem e o ponteiro estiver sobre qualquer elemento UI -> bloquear
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return true;

        Vector2 mousePosition = Input.mousePosition;

        // Helper: verifica um RectTransform considerando o Canvas/Camera correta
        bool CheckRect(RectTransform rt)
        {
            if (rt == null) return false;

            Canvas canvas = rt.GetComponentInParent<Canvas>();
            Camera cam = null;
            if (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
                cam = canvas.worldCamera;

            return RectTransformUtility.RectangleContainsScreenPoint(rt, mousePosition, cam);
        }

        bool over1 = CheckRect(areaMenu1);
        bool over2 = CheckRect(areaMenu2);

        return over1 || over2;
    }

    private JanelaMundoRender ObterJanelaMundo()
    {
        if (janelaMundo != null)
            return janelaMundo;

        janelaMundo = FindObjectOfType<JanelaMundoRender>();
        if (janelaMundo != null)
            return janelaMundo;

        GameObject janela = GameObject.Find("JanelaVisualizacao");
        if (janela != null)
            janelaMundo = janela.AddComponent<JanelaMundoRender>();

        return janelaMundo;
    }

    // ... métodos existentes (centroCam, reposicionar, alturaCam, AdjustCameraToFitAllObjects) mantidos ...

    public Vector3 centroCam()
    {
        Vector3 centroTerreno = new Vector3(terreno.GetComponent<Terrain>().terrainData.size.x / 2, 0, terreno.GetComponent<Terrain>().terrainData.size.z / 2);
        return centroTerreno;
    }

    public void reposicionar()
    {
        Vector3 centro = centroCam();
        offset = new Vector3(0, alturaCam() / 2f, 0);
        transform.position = centro + offset;
        transform.LookAt(centro);
        GetComponent<Camera>().orthographic = true;
        if (InputsMorfo.input_totalCasas == null) { GetComponent<Camera>().orthographicSize = 5; }
        else { GetComponent<Camera>().orthographicSize = Mathf.Sqrt(InputsMorfo.input_totalCasas) * 3; }

        // atualizar pivot para o centro do terreno (porque reposicionar centraliza na cena)
        pivotWorld = centro;
        lastVisibleRect = GetVisibleScreenRect();
    }

    float alturaCam()
    {
        float corda;
        float x = terreno.GetComponent<Terrain>().terrainData.size.x;
        float z = terreno.GetComponent<Terrain>().terrainData.size.z;
        corda = (z < x) ? x : z;
        float altura = ((-corda) / (Mathf.Sin(30) / 2));
        return altura;
    }

    public void AdjustCameraToFitAllObjects()
    {
        Camera camera = Camera.main;
        if (camera == null)
            return;

        Renderer[] renderers = FindObjectsOfType<Renderer>();
        if (renderers.Length == 0)
            return;

        Bounds bounds = new Bounds(renderers[0].bounds.center, renderers[0].bounds.size);
        foreach (Renderer renderer in renderers)
        {
            bounds.Encapsulate(renderer.bounds);
        }

        Vector3 center = bounds.center;
        float distance = bounds.extents.magnitude / Mathf.Tan(camera.fieldOfView * 0.5f * Mathf.Deg2Rad);
        camera.transform.position = new Vector3(center.x, center.y, center.z - distance);
        camera.transform.LookAt(center);

        // atualiza pivot para o novo centro calculado
        pivotWorld = center;
        lastVisibleRect = GetVisibleScreenRect();
    }
}
