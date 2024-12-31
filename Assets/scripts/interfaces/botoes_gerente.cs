using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class botoes_gerente : MonoBehaviour
{
    public Button b_apagar;
    public Button b_gerarAglomeracao;
    public Button b_atribuirParametros;

    public GameObject sliders_isovista;
    public Toggle t_isovista;
    public Toggle t_random;
    //    [SerializeField] Toggle toggleModoRandom;


    // Start is called before the first frame update
    void Start()
    {
        b_atribuirParametros.interactable = true;
        b_gerarAglomeracao.interactable = false;
        b_apagar.interactable = false;
        //        sliders.interactable = false ;

        Toggle_Isovista();
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

    public void Botao_AtribuirParametros()
    {
        b_atribuirParametros.interactable = true;
        b_gerarAglomeracao.interactable = true;
        b_apagar.interactable = false;

    }

    public void Botao_GerarAglomeracao()
    {
        b_atribuirParametros.interactable = false;
        b_gerarAglomeracao.interactable = false;
        b_apagar.interactable = true;

    }

    public void Botao_Apagar()
    {
        b_atribuirParametros.interactable = true;
        b_gerarAglomeracao.interactable = false;
        b_apagar.interactable = false;

    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
