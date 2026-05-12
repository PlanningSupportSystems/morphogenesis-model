using UnityEngine;


public class SelectionManager : MonoBehaviour
{
    private Camera mainCamera;
    private JanelaMundoRender janelaMundo;
    private ClickSelect currentSelection;
    private ClickSelect selectedGameObject;
    private ISelecionavel selecaoAtual;
    private ISelecionavel clicado;
    //    private ISelecionavel selecionado;

    void Start()
    {
        mainCamera = Camera.main;
        ObterJanelaMundo();
        selectedGameObject = null;
        selecaoAtual = null;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) // Left mouse button
        {
            Ray ray;
            if (ObterJanelaMundo() != null)
            {
                if (!janelaMundo.TryScreenPointToRay(Input.mousePosition, out ray))
                    return;
            }
            else
            {
                ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            }

            RaycastHit hit;

            int layerMask = (1 << LayerMask.NameToLayer("layer_predios"))
                            | (1 << LayerMask.NameToLayer("layer_ruas"))
                            | (1 << LayerMask.NameToLayer("layer_lugares"));

            if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask))
            {
//                Debug.Log("cliques funfando");

                clicado = hit.collider.GetComponent<ISelecionavel>();
//                Debug.Log("clicado: " + clicado);

                ProcessarClick();

                /*
                ClickSelect selectable = hit.collider.GetComponent<ClickSelect>();
                if (selectable != null)
                {
                    // se clicar no mesmo ja selecionado -> desseleciona
                    if (currentSelection == selectable)
                    {
                        currentSelection.Deselect();
                        currentSelection = null;
                        selectedGameObject = null;
                    }
                    else
                    {
                        // se houver um selecionado diferente, desseleciona o anterior
                        if (currentSelection != null)
                        {
                            currentSelection.Deselect();
                        }

                        // seleciona o novo
                        currentSelection = selectable;
                        selectedGameObject = selectable;
                        currentSelection.Select();
                    }
                }
                */
            }
            else
            {
                LimparSelecao();
                /*
                // clique em vazio: desseleciona atual se houver
                if (currentSelection != null)
                {
                    currentSelection.Deselect();
                    currentSelection = null;
                    selectedGameObject = null;
                }
                */
            }
        }
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

    void ProcessarClick()
    {
//        Debug.Log("processando click ");
        if (clicado == selecaoAtual)
        {
            LimparSelecao();
            return;
        }

        LimparSelecao();

        if (clicado != null)
        {
            selecaoAtual = clicado;
            selecaoAtual.Select();
        }
    }

    void LimparSelecao()
    {
        if (selecaoAtual != null)
        {
            selecaoAtual.Deselect();
            selecaoAtual = null;
        }
    }
}
