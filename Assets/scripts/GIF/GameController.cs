using UnityEngine;

public class GameController : MonoBehaviour
{
    public GameObject _screen;
    private ScreenshotSaver screenshotSaver;
    bool a = true;

    private void Start()
    {
        _screen = GameObject.Find("ScreenshotSaverObject");
        // Obtém a referência ao componente ScreenshotSaver
        screenshotSaver = _screen.GetComponent<ScreenshotSaver>();
    }

    private void Update()
    {
        // Apenas para demonstração, pressionar a tecla P captura e salva a tela
        if (Input.GetKeyDown(KeyCode.P))
        {
            screenshotSaver.CaptureAndSaveScreenshot("screenshot.jpg");
            Debug.Log("captura de tela");
        }
    }
}
