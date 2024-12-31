using UnityEngine;

public class PrefabCollisionDetection : MonoBehaviour
{
//    public bool sobreposta = false;
    // This method is called whenever a collision occurs
    public bool OnCollisionEnter(Collision collision)
    {
        bool sobreposta = false;

        // Check if the collision involves another object
        if (collision.gameObject != null)
        {
            sobreposta = true;

            Debug.Log("Prefab collided with: " + collision.gameObject.name);
        }
        return sobreposta;
    }
}
