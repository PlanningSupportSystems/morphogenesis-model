using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class botoes_gerente : MonoBehaviour
{
    ControleAglomeracao gerenteAmbiente;
    InputsMorfo inputsMorfo;

    public Button b_apagar;
    public Button b_gerarAglomeracao;
    public Button b_atribuirParametros;
    public Button b_antiTravar;
    public Button downloadImagens;

    public GameObject sliders_isovista;
    public Toggle t_isovista;
    public Toggle t_random;
    //    [SerializeField] Toggle toggleModoRandom;


    // Start is called before the first frame update
    void Start()
    {
        gerenteAmbiente = ControleAglomeracao.Instance;
        inputsMorfo = GameObject.Find("PainelMorfogenese").GetComponent<InputsMorfo>();

        b_atribuirParametros.interactable = true;
        b_gerarAglomeracao.interactable = false;
        b_apagar.interactable = false;
        //        sliders.interactable = false ;

        Toggle_Isovista();
        Toggle_Imagem();
    }   

    public void Toggle_Isovista()
    {
        t_random.isOn = !t_isovista.isOn;
        //        bool liberado = t_isovista.isOn;
        Slider[] sliders = sliders_isovista.GetComponentsInChildren<Slider>();
        // Loop para alterar a propriedade 'interactable' de cada Slider
        foreach (Slider slider in sliders)
        {
            slider.interactable = t_isovista.isOn;
        }
    }

    public void Toggle_Imagem()
    {
        inputsMorfo.toggleApenasImagemFinal.interactable = inputsMorfo.toggleGravarImagens.isOn;
    }

    public void Botao_AtribuirParametros()
    {
        b_atribuirParametros.interactable = true;
        b_gerarAglomeracao.interactable = true;
        b_apagar.interactable = false;
        inputsMorfo.IM_AtribuirCasas();
        inputsMorfo.IM_BotaoConfigurar();

    }

    public void Botao_GerarAglomeracao()
    {
        b_atribuirParametros.interactable = false;
        b_gerarAglomeracao.interactable = false;
        //        b_apagar.interactable = true;
        gerenteAmbiente.CriarAglomeracao();

    }

    public void Botao_Apagar()
    {
        b_atribuirParametros.interactable = true;
        b_gerarAglomeracao.interactable = false;
        b_apagar.interactable = false;
        downloadImagens.interactable = false;

        gerenteAmbiente.CA_IniciarControle();
    }

    public void DownloadImagens()
    {
        downloadImagens.GetComponentInChildren<Text>().text = "preparing...";
        downloadImagens.interactable = false;
        gerenteAmbiente.CA_BaixarImagens();
    }

    public void FimDownload()
    {
        downloadImagens.GetComponentInChildren<Text>().text = "download imagens";
        downloadImagens.interactable = true;
//        StartCoroutine(FimDownloadRotina());
    }

    IEnumerator FimDownloadRotina()
    {
        yield return new WaitForSeconds(1f);

        downloadImagens.GetComponentInChildren<Text>().text = "download imagens";
        downloadImagens.interactable = true;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
