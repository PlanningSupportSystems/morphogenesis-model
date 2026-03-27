using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ISelecionavel
{
    bool EstaSelecionado { get; set; }
    void Select();
    void Deselect();
}
