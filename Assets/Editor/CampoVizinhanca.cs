using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(vizinhancaFOV))]
public class CampoVizinhanca : Editor
{

    protected virtual void OnSceneGUI()
    {
        vizinhancaFOV areaConstruida = (vizinhancaFOV)target;

//        vizinhancaFOV vizinhancaLivre = target as vizinhancaFOV;
        if (areaConstruida == null) return;
        Handles.color = new Color(1, 1, 0, 0.3f);

        //        Handles.DrawWireArc(fow.transform.position, fow.transform.up, fow.transform.forward, 360f, fow.viewRadius);
        //        Handles.DrawWireArc(fow.transform.position, Vector3.up, fow.transform.forward, 360, fow.viewRadius);
        //Handles.CircleHandleCap(0, fow.transform.position, Quaternion.LookRotation(Vector3.up), fow.viewRadius, EventType.Repaint);

        /*        Handles.CircleHandleCap(
                       0,
                       areaConstruida.transform.position,
                       Quaternion.LookRotation(Vector3.up),
                       areaConstruida.raioVizinhanca,
                       EventType.Repaint
                   );
        */

        Handles.DrawSolidDisc(
            areaConstruida.transform.position,
            areaConstruida.transform.up,
            areaConstruida.raioVizinhanca);

        areaConstruida.raioVizinhanca = Handles.ScaleValueHandle(
            areaConstruida.raioVizinhanca,
            areaConstruida.transform.position + areaConstruida.transform.forward * areaConstruida.raioVizinhanca,
            areaConstruida.transform.rotation,
            30,
            Handles.SphereHandleCap,
            1
            );

    }

}
