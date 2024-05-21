using System.Collections;
using System.Collections.Generic;
using Meta.XR.MRUtilityKit;
using UnityEngine;

public class MRUK_MeshRenderer_Controller : MonoBehaviour
{
    private MRUK _mruk;

    void Start()
    {
        _mruk = GameObject.FindObjectOfType<MRUK>();
        _mruk.SceneLoadedEvent.AddListener(MRUKSceneLoaded);    //Room is created then scene is loaded
    }

    private void MRUKSceneLoaded()
    {
        DisableMRUKRoomRenderers();
    }

    public void EnableMRUKRoomRenderers()
    {
        Debug.Log("Enabling MRUK Room Renderers");
        ChangeMRUKRoomRenderers(true);
    }
    
    public void DisableMRUKRoomRenderers()
    {
        Debug.Log("Disabling MRUK Room Renderers");
        ChangeMRUKRoomRenderers(false);
    }

    private static void ChangeMRUKRoomRenderers(bool enable)
    {
        MRUKRoom[] mrukRooms = GameObject.FindObjectsOfType<MRUKRoom>();
        foreach (MRUKRoom mrukRoom in mrukRooms)
        {
            // Debug.Log("RoomName: "+mrukRoom.gameObject.name);
            for (int i = 0; i < mrukRoom.transform.childCount; i++)
            {
                Transform child = mrukRoom.transform.GetChild(i);
                Renderer renderer = child.GetComponentInChildren<Renderer>();
                // Renderer renderer = child.GetChild(0).GetComponent<Renderer>();
                if(renderer) renderer.enabled = enable;
                // Debug.Log("ChildName: "+child.gameObject.name);
            }
        }
    }
}
