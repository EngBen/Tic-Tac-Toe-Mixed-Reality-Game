using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using Quaternion = UnityEngine.Quaternion;
using Random = UnityEngine.Random;
using Vector3 = UnityEngine.Vector3;

/*
	Tasks: 
		- Check if all clickTriggers have been filled
		- AI responses and playerTurns
		- Check for a win-lose or tie condition
		- Program a computer player (an AI) that is smarter than the provided example. 
		  It should read the board and adapt to the player’s movements. The AI should 
		  actively block the real player from winning instantly or winning more than 50% of the games. 
		  The implementation of the min-max algorithm is not required to pass the test.
*/

//TODO: MODULARIZE CODE, Should have a 'GameManager' script
/// At the moment, this script manages the game states

public enum BoardState{none, cross, circle}		///Player uses circle while AI uses cross
public enum TicTacToeTurn{player, AI}
public enum Winner{Player, AI, Tie}
public enum AILevel{easy, hard}

public enum MatchingX_Or_Os
{
	DiagonalLeft, DiagonalRight, VerticalLeft, VerticalMiddle, VerticalRight, HorizontalTop, HorizontalMiddle, 
	HorizontalBottom, None
}

public class UnityEventInt : UnityEvent<int> { }
public class UnityEventWinner : UnityEvent<Winner> { }


public class TicTacToeAI : MonoBehaviour
{

	AILevel _aiLevel = AILevel.easy;	
	private TicTacToeTurn _ticTacToeTurn = TicTacToeTurn.player;	

	private BoardState[,] boardState;

	[SerializeField] private GamePanelsController _gamePanelsController;
	[SerializeField] private int _gridSize = 3;

	[SerializeField] private GameObject _xPrefab;
	[SerializeField] private GameObject _oPrefab;
	[SerializeField] private GameObject _ticTacToeBoard;

	[SerializeField] private TMP_Text gameInfoTMPText;
	[SerializeField] private TMP_Text aiLevelTMPText;
	[SerializeField] private TMP_Text ticTacToeTurnTMPText;
	[SerializeField] private Transform _defaultGameBoardSpawnPoint;
	[SerializeField] private Transform _playerCamera;
	
	private string _playerTurnMessage = "Turn : You";
	private string _aiTurnMessage = "Turn : Computer";
	private ClickTrigger[,] _clickTriggers;

	public UnityEvent onGameStarted = new UnityEvent();
	public UnityEvent CanListenToInputs = new UnityEvent();
	public UnityEventWinner onGameEnded = new UnityEventWinner();

	List<int> _availableCells = new List<int>(); ///0 = [0,0], 2 = [0,1], 3 = [0,2], 4 = [1,0]
	HashSet<int> _usedCells = new HashSet<int>(); ///Used because of its O(1) access in its elements
	private const int TOTAL_NO_OF_MOVES = 9;	//_gridSize * _gridSize
	private bool _noMovesLeft = false;
	private	bool _gameHasAWinner = false;
	
	private const float AI_TIME_DELAY = 1f;
	private	bool _gameStarted = false;
	public bool gameStarted => _gameStarted;
	[HideInInspector]
	public GameObject spawnedTicTacToeBoard;
	// [SerializeField] private bool _autoStartGame = false;

	private Vector3 _targetBoardPosition;
	private Quaternion _targetBoardRotation;
	private Material _xPrefabMaterial;
	private Material _oPrefabMaterial;

	[HideInInspector]
	//Todo: It might be better to just parent the X and Os but bcoz of time limit, this is solution, for now, for clearing up the spawned X and Os
	public List<GameObject> SpawnedXAndOsGameObjects = new List<GameObject>();

	private void Start()
	{
		if (!_gamePanelsController) _gamePanelsController = GameObject.FindObjectOfType<GamePanelsController>();
		_targetBoardPosition = _defaultGameBoardSpawnPoint.position;
		_targetBoardRotation = _defaultGameBoardSpawnPoint.rotation;
		// 'sharedMaterial' is used instead of 'material' because its a prefab object (not located insude scene)
		_xPrefabMaterial = _xPrefab.transform.GetChild(0).GetComponent<Renderer>().sharedMaterial;
		_oPrefabMaterial = _oPrefab.transform.GetChild(0).GetComponent<Renderer>().sharedMaterial;
		// if(_autoStartGame) StartAI(0);
	}

	public void SetLevelOfDifficulty(AILevel level)
	{
		_aiLevel = level;
	}

	public void UpdateBoardPosition(Vector3 position, Quaternion rotation)
	{
		_targetBoardPosition = position;
		_targetBoardRotation = rotation;
	}


	/// Called by <see cref="PinBoardToSurface"/> script
	public void StartGame(){
		if(_gameStarted) return;
		_gameStarted = true;
		log("Starting Game");

		spawnedTicTacToeBoard = Instantiate(_ticTacToeBoard, _targetBoardPosition, _targetBoardRotation);

		_clickTriggers = new ClickTrigger[_gridSize,_gridSize];
		boardState = new BoardState[_gridSize,_gridSize];
		SpawnedXAndOsGameObjects.Clear();
		_ticTacToeTurn = TicTacToeTurn.player;
		ticTacToeTurnTMPText.text = _playerTurnMessage;
		_availableCells.Clear();
		for(int i=0; i<TOTAL_NO_OF_MOVES; i++){
			_availableCells.Add(i);
		}
		_usedCells.Clear();
		_noMovesLeft = false;
		_gameHasAWinner = false;
		gameInfoTMPText.text = "";
		aiLevelTMPText.text = "AI Level : ";
		aiLevelTMPText.text += _aiLevel==AILevel.easy ? "Easy" : "Hard";
		
		onGameStarted.Invoke();
		StartCoroutine(Broadcast_GameHasStartedEvent());
	}
	
	//Wait for animation to finish so as to start listening to player inputs 
	private IEnumerator Broadcast_GameHasStartedEvent()
	{
		Transform resizableBoard = spawnedTicTacToeBoard.transform.GetChild(0);
		float animationTime = resizableBoard.GetComponent<AnimateLocalScale>().animTime;
		yield return new WaitForSeconds(animationTime + 0.5f);	//+ an allowance
		CanListenToInputs.Invoke();
	}
	
	
	/// Called by each <see cref="ClickTrigger"/> class
	public void RegisterTransform(int _cellX, int _cellY, ClickTrigger clickTrigger)
	{
		_clickTriggers[_cellX, _cellY] = clickTrigger;
		log("ClickTrigger [" + _cellX + "," + _cellY + "] Registered");
	}


	public void PlayerSelects(int _cellX, int _cellY)
	{
		if(_ticTacToeTurn != TicTacToeTurn.player) {
			// gameInfoTMPText.text = "It is not your turn";
			return;
		}
		ClickTriggerSelected(_cellX, _cellY);
	}

	public void AiSelects(int _cellX, int _cellY)
	{
		ClickTriggerSelected(_cellX, _cellY);
	}

	private void ClickTriggerSelected(int _cellX, int _cellY)
	{
		if(_gameHasAWinner || _noMovesLeft) return;
		int _cellIndex = cellIndex(_cellX, _cellY);
		if(_usedCells.Contains(_cellIndex)) {
			gameInfoTMPText.text = "That piece has already been selected";
			return;
		}
		///Reset info text after a player/AI has played
		gameInfoTMPText.text = "";	

		_availableCells.Remove(_cellIndex);
		_usedCells.Add(_cellIndex);
		if(_availableCells.Count <= 0) _noMovesLeft = true; 

		boardState[_cellX, _cellY] = _ticTacToeTurn == TicTacToeTurn.player ? 
					BoardState.circle : BoardState.cross;

		///Instantiate a cross or circle prefab in that cell trigger
		GameObject spawnedObject = Instantiate(
			_ticTacToeTurn == TicTacToeTurn.player ? _oPrefab : _xPrefab,
			_clickTriggers[_cellX, _cellY].transform.position,
			_clickTriggers[_cellX, _cellY].transform.rotation 
		);
		// spawnedObject.transform.parent = spawnedTicTacToeBoard.transform;
		SpawnedXAndOsGameObjects.Add(spawnedObject);
		AudioManager.Instance.PlayXOrO_PrefabSpawnAudioClip();

		var outcome = GameHasBeenWon();
		if(outcome.gameHasBeenWon) {
			_gameHasAWinner = true;
		}
		
		if(_noMovesLeft || _gameHasAWinner) StartCoroutine(EndGame(outcome.matchingXOrOs));
		else TogglePlayerTurn();
	}

	private(bool gameHasBeenWon, BoardState symbolThatWon, MatchingX_Or_Os matchingXOrOs) GameHasBeenWon()
    {
		if(boardState[0,0] != BoardState.none){
			//Horizontal Top-most cells in 3*3 matrix
			if(boardState[0,0] == boardState[0,1] && boardState[0,1] == boardState[0,2])
				return (true, boardState[0,0], MatchingX_Or_Os.HorizontalTop);
			//Vertical Left-most cells in 3*3 matrix
			else if(boardState[0,0] == boardState[1,0] && boardState[1,0] == boardState[2,0])
				return (true, boardState[0,0], MatchingX_Or_Os.VerticalLeft);
			//Diagonal Left in 3*3 matrix
			else if(boardState[0,0] == boardState[1,1] && boardState[1,1] == boardState[2,2])
				return (true, boardState[0,0], MatchingX_Or_Os.DiagonalLeft);
		}

		if(boardState[0,2] != BoardState.none){
			//Vertical Right-most cells in 3*3 matrix
			if(boardState[0,2] == boardState[1,2] && boardState[1,2] == boardState[2,2])
				return (true, boardState[0,2], MatchingX_Or_Os.VerticalRight);
			//Diagonal Right in 3*3 matrix
			else if(boardState[0,2] == boardState[1,1] && boardState[1,1] == boardState[2,0]){
				return (true, boardState[0,2], MatchingX_Or_Os.DiagonalRight);
			}
		}

		if(boardState[2,0] != BoardState.none){
			//Horizontal Bottom-most cells in 3*3 matrix
			if(boardState[2,0] == boardState[2,1] && boardState[2,1] == boardState[2,2])
				return (true, boardState[2,0], MatchingX_Or_Os.HorizontalBottom);
		}

		if(boardState[0,1] != BoardState.none){
			//Vertical Middle cells in 3*3 matrix
			if(boardState[0,1] == boardState[1,1] && boardState[1,1] == boardState[2,1])
				return (true, boardState[0,1], MatchingX_Or_Os.VerticalMiddle);
		}

		if(boardState[1,0] != BoardState.none){
			//Horizontal Middle cells in 3*3 matrix
			if(boardState[1,0] == boardState[1,1] && boardState[1,1] == boardState[1,2])
				return (true, boardState[1,0], MatchingX_Or_Os.HorizontalMiddle);
		}

		return (false, BoardState.none, MatchingX_Or_Os.None);
    }

	private void TogglePlayerTurn(){
		if(_ticTacToeTurn == TicTacToeTurn.player){
			_ticTacToeTurn = TicTacToeTurn.AI;
			ticTacToeTurnTMPText.text = _aiTurnMessage;
			if(_aiLevel == AILevel.easy)
				StartCoroutine(AiNextMove_Easy());
			else 
			{
				///First check if only one move is required to finish game, if so, 
				///there is no need of carrying out the Minimax algorithm
				if(_usedCells.Count >=8) {
					StartCoroutine(AiNextMove_Easy());
				}
				else StartCoroutine(AiNextMove_Hard());
			}
		}
		else if(_ticTacToeTurn == TicTacToeTurn.AI){
			_ticTacToeTurn = TicTacToeTurn.player;
			ticTacToeTurnTMPText.text = _playerTurnMessage;
		}
	}

	/*
		If level == easy, the AI picks a cell at random
	*/
    private IEnumerator AiNextMove_Easy(){
		yield return new WaitForSeconds(AI_TIME_DELAY);
		int randomInt = Random.Range(0, _availableCells.Count);
		int randomCellIndex = _availableCells.ElementAt(randomInt);
		var xy = getMatrix_X_Y_FromCellIndex(randomCellIndex);
		int cellX = xy.x;
		int cellY = xy.y;
		AiSelects(cellX, cellY);
	}

	/*
		If level == hard, the AI carries out a Minimax algorithm 
		The algorithm isn't that efficient in blocking user ftom winning so 
		function 'IsUserAboutToWin' is added to try blockthe user from winning the game
	*/
	private IEnumerator AiNextMove_Hard()
	{
		yield return new WaitForSeconds(AI_TIME_DELAY);

		int bestScore = int.MinValue;
		int bestRow = -1;
		int bestCol = -1;

		// Iterate through all empty cells on the board
		for (int i = 0; i < _gridSize; i++)
		{
			for (int j = 0; j < _gridSize; j++)
			{
				if (boardState[i, j] == BoardState.none)
				{
					// Try placing the AI's symbol at the empty cell
					boardState[i, j] = BoardState.cross;
					int score = MiniMax(boardState, 0, false);
					boardState[i, j] = BoardState.none; // Undo the move

					// Choose the move with the highest score
					if (score > bestScore)
					{
						bestScore = score;
						bestRow = i;
						bestCol = j;
					}
				}
			}
		}

		// AI uses the best move
		AiSelects(bestRow, bestCol);
	}

	/*Performs a depth-first search, alternating between maximizing and minimizing 
	  player turns to determine the best move. The 'isMaximizing' boolean indicates 
	  whether the current player is maximizing (AI player) or minimizing (human player)
	  the score.
	*/
	private int MiniMax(BoardState[,] boardState, int depth, bool isMaximizing)
	{
		var result = GameHasBeenWon();

		// If game has been won or all cells have been filled
		if (result.gameHasBeenWon || IsBoardFullyFilled())
		{
			return result.symbolThatWon == BoardState.cross ? 1 : 
				result.symbolThatWon == BoardState.circle ? -1 : 0;
		}

		if (isMaximizing)
		{
			int bestScore = int.MinValue;
			for (int i = 0; i < _gridSize; i++)
			{
				for (int j = 0; j < _gridSize; j++)
				{
					if (boardState[i, j] == BoardState.none)
					{
						boardState[i, j] = BoardState.cross;
						int score = MiniMax(boardState, depth + 1, false);
						boardState[i, j] = BoardState.none; // Undo the move
						bestScore = Math.Max(score, bestScore);
					}
				}
			}
			return bestScore;
		}
		else
		{
			int bestScore = int.MaxValue;
			for (int i = 0; i < _gridSize; i++)
			{
				for (int j = 0; j < _gridSize; j++)
				{
					if (boardState[i, j] == BoardState.none)
					{
						boardState[i, j] = BoardState.circle;
						int score = MiniMax(boardState, depth + 1, true);
						boardState[i, j] = BoardState.none; // Undo the move
						bestScore = Math.Min(score, bestScore);
					}
				}
			}
			return bestScore;
		}
	}

    private bool IsBoardFullyFilled()
    {
		for (int i = 0; i < _gridSize; i++)
        {
            for (int j = 0; j < _gridSize; j++)
            {
                if (boardState[i, j] == BoardState.none)
                    return false;
            }
        }
        return true;
    }

    private IEnumerator EndGame(MatchingX_Or_Os matchingXOrOs)
	{
		yield return new WaitForSeconds(AI_TIME_DELAY);
		Winner winner = !_gameHasAWinner ? Winner.Tie  : _ticTacToeTurn == TicTacToeTurn.player ? Winner.Player : Winner.AI;
		onGameEnded.Invoke(winner);
		_gameStarted = false;
		DisplayMatchingX_Or_O(matchingXOrOs, winner);
		// if(_autoStartGame) StartCoroutine(RestartGameAfterDelay());
	}

	// IEnumerator RestartGameAfterDelay()
	// {
	// 	yield return new WaitForSeconds(3f);
	// 	StartAI(0);
	// }

	private void DisplayMatchingX_Or_O(MatchingX_Or_Os matchingXOrOs, Winner winner)
	{
		if(winner == Winner.Tie) return;
		log("Displaying matching XOrOs: "+matchingXOrOs.ToString());
		Transform resizableBoard = spawnedTicTacToeBoard.transform.GetChild(0);
		if (resizableBoard.childCount > 2)
		{
			Transform matchingXOrOsDisplay = resizableBoard.GetChild(2);
			if (matchingXOrOsDisplay.transform.childCount > 2)
			{
				for (int i = 0; i < matchingXOrOsDisplay.transform.childCount; i++)
				{
					log(matchingXOrOsDisplay.transform.GetChild(i).name);
				}
				Transform diagonalDisplays = matchingXOrOsDisplay.transform.GetChild(0);
				Transform verticalDisplays = matchingXOrOsDisplay.transform.GetChild(1);
				Transform horizontalDisplays = matchingXOrOsDisplay.transform.GetChild(2);
				Transform display = diagonalDisplays.GetChild(0);
				
				// Is Diagonal
				if (matchingXOrOs == MatchingX_Or_Os.DiagonalRight) 
					display = diagonalDisplays.GetChild(0);
				else if (matchingXOrOs == MatchingX_Or_Os.DiagonalLeft) 
					display = diagonalDisplays.GetChild(1);
				
				// Is Vertical
				else if (matchingXOrOs == MatchingX_Or_Os.VerticalMiddle) 
					display = verticalDisplays.GetChild(0);
				else if (matchingXOrOs == MatchingX_Or_Os.VerticalLeft) 
					display = verticalDisplays.GetChild(1);
				else if (matchingXOrOs == MatchingX_Or_Os.VerticalRight) 
					display = verticalDisplays.GetChild(2);
				
				// Is Horizontal
				else if (matchingXOrOs == MatchingX_Or_Os.HorizontalMiddle) 
					display = horizontalDisplays.GetChild(0);
				else if (matchingXOrOs == MatchingX_Or_Os.HorizontalTop) 
					display = horizontalDisplays.GetChild(1);
				else if (matchingXOrOs == MatchingX_Or_Os.HorizontalBottom) 
					display = horizontalDisplays.GetChild(2);

				display.GetComponent<AnimateLocalScale>().MaximizeObject();
				Renderer displayRenderer = display.GetComponent<Renderer>();
				if(winner == Winner.Player) displayRenderer.material = _oPrefabMaterial;
				else if(winner == Winner.AI) displayRenderer.material = _xPrefabMaterial;
				Debug.Log("winner:"+ winner.ToString());

			}
		}
	}
	
	public void GameExitedPrematurely()
	{
		_gameStarted = false;
	}
	
	private void OnDestroy(){
		StopAllCoroutines();
	}


	///Cell indexes are as such : 0 = [0,0], 2 = [0,1], 3 = [0,2], 4 = [1,0]
	private int cellIndex(int _cellX, int _cellY)
    {
		if(_cellX == 0 && _cellY == 0){
			return 0;
		}
		else if(_cellX == 0 && _cellY == 1){
			return 1;
		}
		else if(_cellX == 0 && _cellY == 2){
			return 2;
		}
		else if(_cellX == 1 && _cellY == 0){
			return 3;
		}
		else if(_cellX == 1 && _cellY == 1){
			return 4;
		}
		else if(_cellX == 1 && _cellY == 2){
			return 5;
		}
		else if(_cellX == 2 && _cellY == 0){
			return 6;
		}
		else if(_cellX == 2 && _cellY == 1){
			return 7;
		}
		else if(_cellX == 2 && _cellY == 2){
			return 8;
		}

		Debug.LogError("Cell provided is Invalid");
		return 0;
    }

	private (int x,int y) getMatrix_X_Y_FromCellIndex(int cellIndex)
    {
		if(cellIndex == 0){
			return (0,0);
		}
		else if(cellIndex == 1){
			return (0,1);
		}
		else if(cellIndex == 2){
			return (0,2);
		}
		else if(cellIndex == 3){
			return (1,0);
		}
		else if(cellIndex == 4){
			return (1,1);
		}
		else if(cellIndex == 5){
			return (1,2);
		}
		else if(cellIndex == 6){
			return (2,0);
		}
		else if(cellIndex == 7){
			return (2,1);
		}
		else if(cellIndex == 8){
			return (2,2);
		}

		Debug.LogError("Cell Index provided is Invalid");
		return (0,0);
    }
	
	private void log(string logText){
		string className = this.GetType().Name;
		Debug.Log("["+className+"]  " +logText);
	}

}
