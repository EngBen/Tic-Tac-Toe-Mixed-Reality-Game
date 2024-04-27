using UnityEngine;

public class CollisionDetection : MonoBehaviour
{
    void Start()
    {
        
    }

    void OnCollisionEnter(Collision collision)
    {
        log("Collided with "+collision.collider.name);
    }
    
    private void log(string logText){
        string className = this.GetType().Name;
        Debug.Log("["+className+"]  " +logText);
    }
}
