using System.Collections;
using System.Collections.Generic;
using Oculus.Interaction;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

public class PokeButton : MonoBehaviour
{
    [SerializeField, Interface(typeof(IInteractableView))]
    private UnityEngine.Object _interactableView;
    
    private IInteractableView InteractableView { get; set; }
    public UnityEvent OnPressed;
    public UnityEvent OnReleased;
    public bool _buttonSelected = false;
    
    // One can choose to hide the button so that its events can be listened to but the button isnt interactable
    private bool _isHidden = false;

    
    protected virtual void Awake()
    {
        InteractableView = _interactableView as IInteractableView;
    }
    
    void Start()
    {
        InteractableView.WhenStateChanged += (args => OnPokeInteractableStateChanged());
    }
    
    void OnDestroy()
    {
        InteractableView.WhenStateChanged -= (args => {});
    }
    
    private void OnPokeInteractableStateChanged()
    {
        if(_isHidden) return;
        switch (InteractableView.State)
        {
            case InteractableState.Select:      //Called when the button moves to its max limit
                // log("Poke Interactable Selected");
                _buttonSelected = true;
                OnPressed.Invoke();
                break;
            case InteractableState.Hover:
                // log("Poke Interactable Hovered");
                break;
            case InteractableState.Normal:      //Called when the button is back to its default state
                if (_buttonSelected) {
                    // log("Poke Interactable Released");
                    _buttonSelected = false;
                    OnReleased.Invoke();
                }
                break;
            case InteractableState.Disabled:
                // log("Poke Interactable Disabled");
                break;
        }
    }

    public void Hide()
    {
        transform.GetComponent<RenderersInChildrenGameObjectsController>().DisableGeometryRender();
        _isHidden = true;
    }
    
    public void Show()
    {
        transform.GetComponent<RenderersInChildrenGameObjectsController>().EnableGeometryRender();
        _isHidden = false;
    }

    private void log(string logText){
        string className = this.GetType().Name;
        Debug.Log("["+className+"]  " +logText);
    }
}
