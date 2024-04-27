using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RenderersInChildrenGameObjectsController : MonoBehaviour
{
    private List<Renderer> _renderers = new List<Renderer>();
    [SerializeField] private bool _renderGeometry = true;
    

    void Start()
    {
        foreach (Renderer renderer in transform.GetComponentsInChildren<Renderer>())
        {
            _renderers.Add(renderer);
        }
        ChangeRenderersVisibility(_renderGeometry);
    }

    private void ChangeRenderersVisibility(bool enabled)
    {
        foreach (Renderer rend in _renderers)
        {
            rend.enabled = enabled;
        }
    }

    public void EnableGeometryRender()
    {
        ChangeRenderersVisibility(true);
    }
    
    public void DisableGeometryRender()
    {
        ChangeRenderersVisibility(false);
    }

}
