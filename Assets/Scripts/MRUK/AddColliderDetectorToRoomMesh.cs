using System.Collections;
using System.Collections.Generic;
using Meta.XR.MRUtilityKit;
using UnityEngine;

public class AddColliderDetectorToRoomMesh : MonoBehaviour
{
    private MRUK _mruk;
    
    void Start()
    {
        _mruk = GameObject.FindObjectOfType<MRUK>();
        _mruk.SceneLoadedEvent.AddListener(MRUKSceneLoaded);    //Room is created then scene is loaded
    }

    private void MRUKSceneLoaded()
    {
        // At the moment, catering for table ands walls only
        MRUKAnchor[] mrukAnchors = GameObject.FindObjectsOfType<MRUKAnchor>();
        foreach (MRUKAnchor mrukAnchor in mrukAnchors)
        {
            if(mrukAnchor.gameObject.name != "TABLE" && mrukAnchor.gameObject.name != "WALL_FACE"
                && mrukAnchor.gameObject.name != "FLOOR") continue;

            Transform colliderTransform = mrukAnchor.transform.GetChild(0);
            if (!colliderTransform.TryGetComponent(out PinBoardToSurface pinBoardToWall))
            {
                // Set mesh collider 'convex' property as a rigidbody will be added to the object
                if (colliderTransform.TryGetComponent(out MeshCollider meshCollider))
                {
                    meshCollider.convex = true;
                }
                colliderTransform.gameObject.AddComponent<PinBoardToSurface>();
                //// The rigidbody has to be added to the gameobject with the collider so that the collider can register
                //// detection when collided with Meta XR physics hands colliders.
                Rigidbody rb = colliderTransform.gameObject.AddComponent<Rigidbody>();
                rb.useGravity = false;
                rb.constraints = RigidbodyConstraints.FreezeAll;
            }
        }
    }
}
