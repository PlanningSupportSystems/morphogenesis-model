using UnityEngine;


public class SelectionManager : MonoBehaviour
{
    private Camera mainCamera;
    private ClickSelect currentSelection;

    void Start()
    {
        mainCamera = Camera.main;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) // Left mouse button
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            //            Debug.Log("cliques funfando");

            //            int layer_predios = LayerMask.NameToLayer("Default");
            //int layer_ruas = LayerMask.NameToLayer("Ignore Raycast");

            int layerMask = (1 << LayerMask.NameToLayer("layer_predios"))
                            | (1 << LayerMask.NameToLayer("layer_ruas"))
                            | (1 << LayerMask.NameToLayer("layer_lugares"));

            if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask))
            {
                Debug.Log("raycast " + hit);
                ClickSelect selectable = hit.collider.GetComponent<ClickSelect>();
                if (selectable != null)
                {
                    if (currentSelection != null)
                    {
                        currentSelection.Deselect();
                    }

                    currentSelection = selectable;
                    currentSelection.Select();
                }
            }
        }
    }
}
