using UnityEngine;
using UnityEngine.UI;

public class JanelaMundoRender : MonoBehaviour
{
    public Camera cameraMundo;
    public RectTransform janelaVisualizacao;
    public RawImage rawImageMundo;

    private RenderTexture renderTexture;
    private Camera cameraTela;
    private int larguraAtual;
    private int alturaAtual;

    private void Awake()
    {
        EncontrarReferencias();
    }

    private void Start()
    {
        AtualizarRenderTexture();
    }

    private void Update()
    {
        AtualizarRenderTexture();
    }

    private void OnDisable()
    {
        LiberarRenderTexture();
    }

    private void EncontrarReferencias()
    {
        if (cameraMundo == null)
            cameraMundo = Camera.main;

        if (janelaVisualizacao == null)
        {
            GameObject janela = GameObject.Find("JanelaVisualizacao");
            if (janela != null)
                janelaVisualizacao = janela.GetComponent<RectTransform>();
        }

        if (rawImageMundo == null)
        {
            GameObject rawImage = GameObject.Find("RawImage_Mundo");
            if (rawImage != null)
                rawImageMundo = rawImage.GetComponent<RawImage>();
        }

        if (rawImageMundo == null && janelaVisualizacao != null)
            rawImageMundo = janelaVisualizacao.GetComponentInChildren<RawImage>();
    }

    private void AtualizarRenderTexture()
    {
        EncontrarReferencias();

        if (cameraMundo == null || rawImageMundo == null)
            return;

        Rect rectTela = GetScreenRect();
        int novaLargura = Mathf.Max(1, Mathf.RoundToInt(rectTela.width));
        int novaAltura = Mathf.Max(1, Mathf.RoundToInt(rectTela.height));

        if (renderTexture != null && novaLargura == larguraAtual && novaAltura == alturaAtual)
            return;

        LiberarRenderTexture();

        larguraAtual = novaLargura;
        alturaAtual = novaAltura;
        renderTexture = new RenderTexture(larguraAtual, alturaAtual, 24, RenderTextureFormat.ARGB32);
        renderTexture.name = "RT_Mundo";
        renderTexture.Create();

        cameraMundo.targetTexture = renderTexture;
        rawImageMundo.texture = renderTexture;
        GarantirCameraTela();
    }

    private void LiberarRenderTexture()
    {
        if (cameraMundo != null && cameraMundo.targetTexture == renderTexture)
            cameraMundo.targetTexture = null;

        if (rawImageMundo != null && rawImageMundo.texture == renderTexture)
            rawImageMundo.texture = null;

        if (renderTexture != null)
        {
            renderTexture.Release();
            Destroy(renderTexture);
            renderTexture = null;
        }

        if (cameraTela != null)
        {
            Destroy(cameraTela.gameObject);
            cameraTela = null;
        }
    }

    private void GarantirCameraTela()
    {
        if (cameraTela != null)
            return;

        GameObject go = new GameObject("Camera_Tela_UI");
        cameraTela = go.AddComponent<Camera>();
        cameraTela.clearFlags = CameraClearFlags.SolidColor;
        cameraTela.backgroundColor = Color.black;
        cameraTela.cullingMask = 0;
        cameraTela.depth = cameraMundo != null ? cameraMundo.depth - 1f : -100f;
        cameraTela.orthographic = true;
    }

    public Rect GetScreenRect()
    {
        RectTransform alvo = rawImageMundo != null ? rawImageMundo.rectTransform : janelaVisualizacao;
        if (alvo == null)
            return new Rect(0, 0, Screen.width, Screen.height);

        Canvas canvas = alvo.GetComponentInParent<Canvas>();
        Camera canvasCamera = null;
        if (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
            canvasCamera = canvas.worldCamera;

        Vector3[] corners = new Vector3[4];
        alvo.GetWorldCorners(corners);

        Vector2 min = new Vector2(float.MaxValue, float.MaxValue);
        Vector2 max = new Vector2(float.MinValue, float.MinValue);

        for (int i = 0; i < 4; i++)
        {
            Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(canvasCamera, corners[i]);
            min = Vector2.Min(min, screenPoint);
            max = Vector2.Max(max, screenPoint);
        }

        return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
    }

    public bool MouseDentroDaJanela()
    {
        return ContemPonto(Input.mousePosition);
    }

    public bool ContemPonto(Vector2 screenPoint)
    {
        RectTransform alvo = rawImageMundo != null ? rawImageMundo.rectTransform : janelaVisualizacao;
        if (alvo == null)
            return true;

        Canvas canvas = alvo.GetComponentInParent<Canvas>();
        Camera canvasCamera = null;
        if (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
            canvasCamera = canvas.worldCamera;

        return RectTransformUtility.RectangleContainsScreenPoint(alvo, screenPoint, canvasCamera);
    }

    public bool TryGetViewportPoint(Vector2 screenPoint, out Vector2 viewportPoint)
    {
        viewportPoint = Vector2.zero;

        RectTransform alvo = rawImageMundo != null ? rawImageMundo.rectTransform : janelaVisualizacao;
        if (alvo == null || !ContemPonto(screenPoint))
            return false;

        Canvas canvas = alvo.GetComponentInParent<Canvas>();
        Camera canvasCamera = null;
        if (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
            canvasCamera = canvas.worldCamera;

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(alvo, screenPoint, canvasCamera, out Vector2 localPoint))
            return false;

        Rect rect = alvo.rect;
        float x = Mathf.InverseLerp(rect.xMin, rect.xMax, localPoint.x);
        float y = Mathf.InverseLerp(rect.yMin, rect.yMax, localPoint.y);

        viewportPoint = new Vector2(x, y);
        return true;
    }

    public bool TryScreenPointToRay(Vector2 screenPoint, out Ray ray)
    {
        ray = new Ray();

        if (cameraMundo == null || !TryGetViewportPoint(screenPoint, out Vector2 viewportPoint))
            return false;

        ray = cameraMundo.ViewportPointToRay(new Vector3(viewportPoint.x, viewportPoint.y, 0f));
        return true;
    }
}
