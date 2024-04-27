using System;
using System.Collections;
using System.Collections.Generic;
using Meta.XR.MRUtilityKit;
using UnityEngine;
using UnityEngine.Serialization;

public class PinBoardToSurface : MonoBehaviour
{
    TicTacToeAI _ai;
    private GamePanelsController _gamePanelsController;
    private GameObject _bareTacToeBoard;
    private RenderersInChildrenGameObjectsController _renderersInBareGameBoard;
    private bool _canPinBoardToSurface = false;
    // public bool _continuousPositioningOfBoard = false;  //If set to true, ensure there is an active TicTacToeBoard in scene

    //Debugging
    private LineRenderer _lineRenderer;

    void Start()
    {
        _ai = FindObjectOfType<TicTacToeAI>();
        _ai.onGameStarted.AddListener(OnGameStarted);

        _gamePanelsController = FindObjectOfType<GamePanelsController>();
        _gamePanelsController.OnGameDifficultyLevelSelected.AddListener(OnGameDifficultyLevelSelected);
        
        _bareTacToeBoard = GameObject.FindGameObjectWithTag("BareTicTacToeBoard");
        _renderersInBareGameBoard = _bareTacToeBoard.transform.GetComponent<RenderersInChildrenGameObjectsController>();
        _renderersInBareGameBoard.DisableGeometryRender();
        
        _lineRenderer = GameObject.FindGameObjectWithTag("LineRenderer").GetComponent<LineRenderer>();
    }

    private void OnGameDifficultyLevelSelected()
    {
        _canPinBoardToSurface = true;
        _renderersInBareGameBoard.EnableGeometryRender();
    }
    

    private void OnGameStarted()
    {
        _canPinBoardToSurface = false;
        _renderersInBareGameBoard.DisableGeometryRender();
    }
    
    void OnCollisionEnter(Collision collision)
    {
        if(!_canPinBoardToSurface) return;
        Collider col = collision.collider;
        if (col.CompareTag("PhysicsHands"))
        {
            PinBoardToMRUKMeshSurface(collision);
        }
    }

    void PinBoardToMRUKMeshSurface(Collision collision)
    {
        Vector3 collisionOrigin = collision.contacts[0].point;
        Vector3 collisionNormal = collision.contacts[0].normal;
            
        Ray ray = new Ray(collisionOrigin, collisionNormal);
            
        MRUKAnchor sceneAnchor = null;
        MRUK.PositioningMethod positioningMethod = MRUK.PositioningMethod.DEFAULT;
        Pose? bestPose = MRUK.Instance?.GetCurrentRoom()?.
            GetBestPoseFromRaycast(ray, Mathf.Infinity, new LabelFilter(), out sceneAnchor, positioningMethod);
        if (bestPose.HasValue && sceneAnchor)
        {
            // log(" MRUKAnchor anchor name : "+sceneAnchor.name);
            Vector3 _targetPosition = bestPose.Value.position;
            Quaternion bestPoseRotation = bestPose.Value.rotation;
                
            // The board always was horizontal against any surface and I wanted it to be vertical on vertical surface
            //// Therefore, solution is to rotate the board to align with the collision plane / face the collision plane
            // Could use '_ticTacToeBoard.transform.up' or  'Vector3.up' but former was more stable
            
            // Quaternion targetRotation = Quaternion.FromToRotation(Vector3.up, collisionNormal) * bestPoseRotation;
            //// OR
            _bareTacToeBoard.transform.position = _targetPosition;
            _bareTacToeBoard.transform.rotation = bestPoseRotation;
            Quaternion _targetRotation = Quaternion.FromToRotation(_bareTacToeBoard.transform.up, collisionNormal) * 
                                        bestPoseRotation;
            
       
                
            //Give an offset to the board so that it is spawned on top of the room mesh. The multiplication factor is manually tuned
            Vector3 offset = collisionNormal * 0.04f;
            _targetPosition -= offset;
                
            ContinuousSettingOfBoardTransform(_targetPosition, _targetRotation);
     
            _ai.UpdateBoardPosition(_targetPosition, _targetRotation);
            // UpdateDebugLineRenderer(collisionOrigin, collisionNormal);
        }
    }

    private void ContinuousSettingOfBoardTransform(Vector3 position, Quaternion rotation)
    {
        if(!_bareTacToeBoard) Debug.LogError("Activate TicTacToeBoard in scene");
        _bareTacToeBoard.transform.position = position;
        _bareTacToeBoard.transform.rotation = rotation;
        
        // // Got the location of the virtualHand using the Hierarchy tab in Unity Editor
        // Transform virtualHandCollider = collision.collider.transform;
        // Transform handVisual = virtualHandCollider.parent.transform.parent.transform.parent;
        // Transform _virtualHand = handVisual.GetChild(0);
        // _ticTacToeBoard.transform.LookAt(_virtualHand.position);
    }
    
    private void UpdateDebugLineRenderer(Vector3 collisionOrigin, Vector3 collisionNormal)
    {
        _lineRenderer.positionCount = 0;
        _lineRenderer.positionCount++;
        int index = _lineRenderer.positionCount - 1;
        _lineRenderer.SetPosition(index, collisionOrigin);
        _lineRenderer.positionCount++;
        _lineRenderer.SetPosition(++index, collisionOrigin + collisionNormal*10);
    }
    
    private void OnDestroy()
    {
        _ai.onGameStarted.RemoveListener(OnGameStarted);

        _gamePanelsController.OnGameDifficultyLevelSelected.RemoveListener(OnGameDifficultyLevelSelected);
        _gamePanelsController.OnGameExited.RemoveListener((() => {}));
    }
    
    
    private void log(string logText){
        string className = this.GetType().Name;
        Debug.Log("["+className+"]  " +logText);
    }
}
