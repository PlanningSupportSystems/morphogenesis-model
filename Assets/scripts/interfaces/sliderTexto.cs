using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class sliderTexto : MonoBehaviour
{
    public GameObject objPai;

//    public InputField entradaParent;
    private Slider _slider;
    private Text _texto;
    private InputField _inputField;
    private Button _botaoMais;
    private Button _botaoMenos;


    public event System.Action<float, string> OnvariavelChanged;
    public float variavel;
    public float passo = 1f;

    // Start is called before the first frame update
    void Start()
    {
//        Debug.Log("slider chamando");
        _slider = this.GetComponentInChildren<Slider>();
        if (_slider != null)
        {
            _texto = _slider.GetComponentInChildren<Text>();
            _inputField = this.GetComponentInChildren<InputField>();

            atualizeViaSlider(_slider.value);
            _slider.onValueChanged.AddListener(atualizeViaSlider);

        }


        if (_inputField != null) 
        { 
            atualizeViaTexto(_inputField.text);
            _inputField.onValueChanged.AddListener(atualizeViaTexto);
        }

        ConfigurarBotoesPasso();

        //        OnvariavelChanged?.Invoke(variavel, this.name); //event -> mudanca da variavel; broadcast 

    }

    // Update is called once per frame
    void Update()
    {
    }

    public void atualizeViaSlider(float v)
    {
        variavel = (Mathf.Round(v * 10) / 10);
        _texto.text = variavel.ToString("F1");
        if (_inputField != null) { _inputField.text = _texto.text; }
        
        OnvariavelChanged?.Invoke(variavel, this.name); //event -> mudanca da variavel; broadcast 

    }

    void atualizeViaTexto(string texto)
    {
        float tt = (Mathf.Round(float.Parse(texto) * 10) / 10);
        if (tt > _slider.maxValue) { tt = _slider.maxValue; }
        _texto.text =  tt.ToString("F1");
        _slider.value = float.Parse(texto);//  toint( texto
        OnvariavelChanged?.Invoke(variavel, this.name); //event -> mudanca da variavel; broadcast 

    }

    public void Incrementar()
    {
        AlterarValor(passo);
    }

    public void Decrementar()
    {
        AlterarValor(-passo);
    }

    public void AlterarValor(float valor)
    {
        if (_slider == null)
        {
            _slider = GetComponentInChildren<Slider>();
        }

        if (_slider == null)
        {
            return;
        }

        _slider.value = Mathf.Clamp(
            _slider.value + valor,
            _slider.minValue,
            _slider.maxValue
        );
    }

    void ConfigurarBotoesPasso()
    {
        Button[] botoes = GetComponentsInChildren<Button>(true);

        foreach (Button botao in botoes)
        {
            string nome = botao.gameObject.name.ToLower();

            if (nome.Contains("mais") || nome.Contains("plus"))
            {
                _botaoMais = botao;
            }
            else if (nome.Contains("menos") || nome.Contains("minus"))
            {
                _botaoMenos = botao;
            }
        }

        if (_botaoMais != null)
        {
            _botaoMais.onClick.RemoveListener(Incrementar);
            _botaoMais.onClick.AddListener(Incrementar);
        }

        if (_botaoMenos != null)
        {
            _botaoMenos.onClick.RemoveListener(Decrementar);
            _botaoMenos.onClick.AddListener(Decrementar);
        }
    }

}
