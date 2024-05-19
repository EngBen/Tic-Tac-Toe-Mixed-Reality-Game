using System;
using System.Collections;
using System.Collections.Generic;
using Meta.XR.MRUtilityKit;
using UnityEngine;
using UnityEngine.Serialization;
using Utilities.XR;

public class PinBoardToSurface : MonoBehaviour
{
    TicTacToeAI _ai;
    private GamePanelsController _gamePanelsController;
    private GameObject _bareTacToeBoard;
    private RenderersInChildrenGameObjectsController _renderersInBareGameBoard;
    private bool _canPinBoardToSurface = false;
    private Transform _player;
    public bool _drawGizmos = false    ;  
    

    void Start()
    {
        _ai = FindObjectOfType<TicTacToeAI>();
        _ai.onGameStarted.AddListener(OnGameStarted);

        _gamePanelsController = FindObjectOfType<GamePanelsController>();
        _gamePanelsController.OnGameDifficultyLevelSelected.AddListener(OnGameDifficultyLevelSelected);
        
        _bareTacToeBoard = GameObject.FindGameObjectWithTag("BareTicTacToeBoard");
        _renderersInBareGameBoard = _bareTacToeBoard.transform.GetComponent<RenderersInChildrenGameObjectsController>();
        _renderersInBareGameBoard.DisableGeometryRender();

        _player = GameObject.Find("CenterEyeAnchor").transform;
        if (!_player) throw new Exception("Cannot find player in Scene");
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
        if (!_bareTacToeBoard)
        {
            Debug.LogError("Activate TicTacToeBoard in scene");
            return;
        }
        
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
            
            if(_drawGizmos) XRGizmos.DrawPointer(_targetPosition, -collisionNormal, Color.green);
        
            _bareTacToeBoard.transform.position = _targetPosition;
            _bareTacToeBoard.transform.rotation = bestPoseRotation;
            
            // The board always was horizontal against any surface and I wanted it to be vertical on vertical surface
            // Therefore, solution is to rotate the board to align with the collision plane / face the collision plane
            // Rotation using its Y axis worked for both horizontal and vertical surfaces
            Quaternion _targetRotation = Quaternion.FromToRotation(_bareTacToeBoard.transform.up, -collisionNormal) * 
                                        bestPoseRotation;
            _bareTacToeBoard.transform.rotation = _targetRotation;
 
            
            //// The next operation change the board's orientation so that the board and the canvas placed next to it can face the player
            Vector3 directionFromPlayerToBoard = _player.position - _bareTacToeBoard.transform.position;
            Vector3 boardPositionRelativeToPlayer = new Vector3();

            // Collision normal on a horizontal surface is parallel to Vector3.up
            bool isHorizontalSurface = Mathf.Abs(Vector3.Dot(collisionNormal, Vector3.up)) >= 0.9f;
            if (isHorizontalSurface)
            {
                boardPositionRelativeToPlayer = _bareTacToeBoard.transform.position + 
                                                        new Vector3(directionFromPlayerToBoard.x, 0f, directionFromPlayerToBoard.z);
                if(_drawGizmos) XRGizmos.DrawPointer(_bareTacToeBoard.transform.position, 
                    boardPositionRelativeToPlayer - _bareTacToeBoard.transform.position, Color.magenta);
                
                _bareTacToeBoard.transform.LookAt(boardPositionRelativeToPlayer, -Vector3.up); // '-Vector3.up' so that the board can face up
                // Apply a rotation offset to make the board and the canvas placed next to it to face the player
                Vector3 rotationOffset = new Vector3(0, 180f, 0f);
                _targetRotation = _bareTacToeBoard.transform.rotation * Quaternion.Euler(rotationOffset);
                //TODO: Consider horizontal surface facing downwards e.g ceiling


                if (sceneAnchor.name == "FLOOR")
                {
                    //Give an offset to because the in some instances (like when testing using the Quest 3), the board
                    // might be spawned below the floor and cant be tappable, so the offset enables the board to be
                    // spawned on top of the room mesh. The multiplication factor is manually tuned
                    Vector3 offset = collisionNormal * 0.06f;
                    _targetPosition -= offset;
                    //TODO: Also consider horizontal surface facing downwards e.g ceiling
                }
            }
            else
            {
                // The X and Z axes of the board keep switching up + the axes aren't completely parallel to the world
                // X and Z axes. So setting x or z value to 0 to get the player position is not straight-forward
                // Also don't really need to get player position to rotate the board along the normal of the vertical
                // surface and face the user because is not as necessary as on a horizontal surface (one can view the board
                // on any side of a horizontal surface but on vertical surface, user will normally just view it on upright posture).
                // Also tried to see if the collisionNormal is parallel to Z or X axis and if they both face in same
                // direction or opposite direction but due to the 'X and Z axes of the board keep switching up' problem,
                // also this didnt work
                Vector3 rotationOffset = new Vector3();
 
                Vector3 ZAxisNormal = Vector3.Cross(-collisionNormal, Vector3.forward);
                if(_drawGizmos) XRGizmos.DrawPointer(_bareTacToeBoard.transform.position, 
                    ZAxisNormal, Color.blue);
                
                Vector3 XAxisNormal = Vector3.Cross(-collisionNormal, Vector3.right);
                if(_drawGizmos) XRGizmos.DrawPointer(_bareTacToeBoard.transform.position, 
                    XAxisNormal, Color.red);

                // Setting the value to 0.9 or 0.5 didn't work even if when visually viewing the normals, they look to be completely
                // parallel to Y-axis
                float parallelAxisConsiderationValue = 0f;
                if (_drawGizmos)
                {
                    if ((Vector3.Dot(ZAxisNormal, Vector3.up) > parallelAxisConsiderationValue)) 
                        log("ZAxisNormal(Blue) is Positive Parallel to Y-axis");
                    if ((Vector3.Dot(ZAxisNormal, Vector3.up) < -parallelAxisConsiderationValue)) 
                        log("ZAxisNormal(Blue) is Negative Parallel to Y-axis");
                    if ((Vector3.Dot(XAxisNormal, Vector3.up) > parallelAxisConsiderationValue)) 
                        log("XAxisNormal(Red) is Positive Parallel to Y-axis");
                    if ((Vector3.Dot(XAxisNormal, Vector3.up) < -parallelAxisConsiderationValue)) 
                        log("XAxisNormal(Red) is Negative Parallel to Y-axis");
                }
                    
                //// Checking if the normal generated by the collision normal and Vector3.right/forward is parallel to Y axis (Vector3.up)
                
                // ZAxisNormal pointing up, XAxisNormal pointing down
                if ((Vector3.Dot(ZAxisNormal, Vector3.up) > parallelAxisConsiderationValue) && 
                    (Vector3.Dot(XAxisNormal, Vector3.up) < -parallelAxisConsiderationValue))
                {
                    log("Vertical Surface: Condition 1 satisfied");
                    rotationOffset = new Vector3(0, 180f, 180f);
                    _targetRotation = _bareTacToeBoard.transform.rotation * Quaternion.Euler(rotationOffset);
                }
                // XAxisNormal pointing up, ZAxisNormal pointing down
                else if ((Vector3.Dot(XAxisNormal, Vector3.up) > parallelAxisConsiderationValue) && 
                         (Vector3.Dot(ZAxisNormal, Vector3.up) < -parallelAxisConsiderationValue))
                {
                    log("Vertical Surface: Condition 2 satisfied");
                    rotationOffset = new Vector3(180, 0, 0);
                    _targetRotation = _bareTacToeBoard.transform.rotation * Quaternion.Euler(rotationOffset);
                }
                // Both XAxisNormal and ZAxisNormal pointing up
                else if ((Vector3.Dot(ZAxisNormal, Vector3.up) > parallelAxisConsiderationValue) && 
                         (Vector3.Dot(XAxisNormal, Vector3.up) > parallelAxisConsiderationValue))
                {
                    log("Vertical Surface: Condition 3 satisfied");
                    rotationOffset = new Vector3(0, 180f, 180f);
                    _targetRotation = _bareTacToeBoard.transform.rotation * Quaternion.Euler(rotationOffset);
                }
                // Both XAxisNormal and ZAxisNormal pointing down
                else if ((Vector3.Dot(ZAxisNormal, Vector3.up) < -parallelAxisConsiderationValue) && 
                         (Vector3.Dot(XAxisNormal, Vector3.up) < -parallelAxisConsiderationValue))
                {
                    log("Vertical Surface: Condition 4 satisfied");
                    rotationOffset = new Vector3(180, 0, 0);
                    _targetRotation = _bareTacToeBoard.transform.rotation * Quaternion.Euler(rotationOffset);
                }
                else
                {
                    Debug.LogWarning("Vertical Surface condition Not Considered. Check it out");
                }
            }
       
            ContinuousSettingOfBoardTransform(_targetPosition, _targetRotation);
     
            _ai.UpdateBoardPosition(_targetPosition, _targetRotation);
        }
    }

    private void ContinuousSettingOfBoardTransform(Vector3 position, Quaternion rotation)
    {
        _bareTacToeBoard.transform.position = position;
        _bareTacToeBoard.transform.rotation = rotation;
    }
    
    private void OnDestroy()
    {
        _ai.onGameStarted.RemoveListener(OnGameStarted);
        _gamePanelsController.OnGameDifficultyLevelSelected.RemoveListener(OnGameDifficultyLevelSelected);
    }
    
    
    private void log(string logText){
        string className = this.GetType().Name;
        Debug.Log("["+className+"]  " +logText);
    }
}
