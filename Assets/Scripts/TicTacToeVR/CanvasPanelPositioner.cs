using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class CanvasPanelPositioner : MonoBehaviour
{
    private enum CanvasPositon
    {
        InfrontOfCamera, OnGameBoard 
    }
    
    
    [SerializeField] private Transform _canvas;
    [SerializeField] private Transform _canvasPositionTransformInfrontOfCamera;
    private TicTacToeAI _ai;
    private CanvasPositon _canvasPosition = CanvasPositon.InfrontOfCamera;
    [SerializeField] private PokeButton _exitButton;


    private void Awake()
    {
        _ai = FindObjectOfType<TicTacToeAI>();
    }
    
    void Start()
    {
        ToggleCanvasPosition(CanvasPositon.InfrontOfCamera);
        _ai.onGameStarted.AddListener(() => ToggleCanvasPosition(CanvasPositon.OnGameBoard));
        _exitButton.OnPressed.AddListener(() => ToggleCanvasPosition(CanvasPositon.InfrontOfCamera));
    }

    private void ToggleCanvasPosition(CanvasPositon canvasPosition)
    {
        _canvasPosition = canvasPosition;
        if (_canvasPosition == CanvasPositon.InfrontOfCamera)
        {
            _canvas.parent = _canvasPositionTransformInfrontOfCamera;
            _canvas.transform.position = _canvasPositionTransformInfrontOfCamera.position;
            _canvas.transform.rotation = _canvasPositionTransformInfrontOfCamera.rotation;
        }
        else
        {
            //See child in prefab of TicTacToeBoard
            Transform secondChildGameObject = _ai.spawnedTicTacToeBoard.transform.GetChild(1);
            _canvas.parent = secondChildGameObject;
            _canvas.transform.position = secondChildGameObject.position;
            _canvas.transform.rotation = secondChildGameObject.rotation ;
        }
    }

    void Update()
    {

    }
}
