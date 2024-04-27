using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//TODO: WOULD BE BETTER TO FIND FUNCTION THAT ADDS THE RIGIDBODY COMPONENT ON THE FINGERS RATHER THAN DOING IT THIS WAY
/// <summary>
/// Gets the 2nd child which will be have colliders and rigidbodes of the hands.They are created when hand is detected 
/// </summary>
public class TagSetter_PhysicsHands : MonoBehaviour
{
    private int _childGameObjectCount = 0; 
    private string _physicsHandsTagName = "PhysicsHands"; 
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        int childCount = transform.childCount;
        if(childCount > 1 && _childGameObjectCount <= 1){
            SetTagsOnHandColliders();
        }
        _childGameObjectCount = childCount;
    }

    /// When tag is set on just the rigidbody children of virtual hands, an object with an 'OnCollision' cannot be
    /// able to register that the hand collider is of that tag. So that's why the tag is set on the colliders instead
    void SetTagsOnHandColliders()
    {
        Transform secondChildTransform = transform.GetChild(1);

        Transform parentGameObject = secondChildTransform;
        for (int i = 0; i < parentGameObject.childCount; i++)
        {
            GameObject rigidBodyGameObject = parentGameObject.GetChild(i).gameObject;
            rigidBodyGameObject.tag = _physicsHandsTagName;
            for (int k = 0; k < rigidBodyGameObject.transform.childCount; k++)
            {
                GameObject colliderGameObject = rigidBodyGameObject.transform.GetChild(k).gameObject;
                colliderGameObject.tag = _physicsHandsTagName;
            }
        }
    }
    
    private void log(string logText){
    	string className = this.GetType().Name;
    	Debug.Log("["+className+"]  " +logText);
    }
}
