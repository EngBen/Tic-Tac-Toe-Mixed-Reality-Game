using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class GamePanelsController : MonoBehaviour
{
	//UI Panels
	[SerializeField] private GameObject _startingMenuPanel;
	[SerializeField] private GameObject _setGameBoardPositionPanel ;
	[SerializeField] private GameObject _gameOutcomePanel;
	[SerializeField] private GameObject gameInfoPanel;
	//Buttons
	[SerializeField] private PokeButton _easyDifficultyButton;
	[SerializeField] private PokeButton _hardDifficultyButton;
	[SerializeField] private PokeButton _boardPositionConfirmedButton;
	[SerializeField] private PokeButton _exitButton;
	//TMP
	[SerializeField] private TMP_Text _gameOutcomeMessage;
	private TicTacToeAI _ai;
	private MRUK_MeshRenderer_Controller _mrukMeshRendererController;

	public UnityEvent OnGameDifficultyLevelSelected;
	public UnityEvent OnBoardPositionConfirmed;
	public UnityEvent OnGameExited;

	private void Awake()
	{
		_ai = FindObjectOfType<TicTacToeAI>();
		_mrukMeshRendererController = FindObjectOfType<MRUK_MeshRenderer_Controller>();
	}
	
	public void Start()
	{
		_ai.onGameStarted.AddListener(OnGameStarted);
		_ai.onGameEnded.AddListener(OnGameEnded);
		_easyDifficultyButton.OnPressed.AddListener(() => SetLevelOfHardness(AILevel.easy));
		_hardDifficultyButton.OnPressed.AddListener(() => SetLevelOfHardness(AILevel.hard));
		_boardPositionConfirmedButton.OnPressed.AddListener(BoardPositionConfirmed);
		_exitButton.OnPressed.AddListener(ExitButtonPressed);
		ResetCanvases();	
	}

	private void ResetCanvases()
	{
		EnableStartingMenuPanel();
		DisableGameBoardPositionPanel();
		DisableGameInfoPanel();
		DisableGameOutcomePanel();
		HidePokeButton(_exitButton);
		HidePokeButton(_boardPositionConfirmedButton);
	}

	
	private void SetLevelOfHardness(AILevel level)
	{
		_ai.SetLevelOfDifficulty(level);
		DisableStartingMenuPanel();
		EnableGameBoardPositionPanel();
		ShowPokeButton(_boardPositionConfirmedButton);
		OnGameDifficultyLevelSelected.Invoke();
		AudioManager.Instance.PlayButtonPressAudioClip();
		
		#if UNITY_EDITOR
			_mrukMeshRendererController.EnableMRUKRoomRenderers();
		#endif
	}
	
	private void BoardPositionConfirmed()
	{
		_ai.StartGame();
		OnBoardPositionConfirmed.Invoke();
		AudioManager.Instance.PlayButtonPressAudioClip();
		log("Board Position Confirmed! (Button Tapped)");
		
		#if UNITY_EDITOR
			_mrukMeshRendererController.DisableMRUKRoomRenderers();
        #endif
	}
	
	private void OnGameStarted()
	{
		log("On Game Started");
		DisableGameBoardPositionPanel();
		HidePokeButton(_boardPositionConfirmedButton);
		EnableGameInfoPanel();
		ShowPokeButton(_exitButton);;
	}
	
	private void OnGameEnded(Winner winner)
	{
		DisableGameInfoPanel();
		EnableGameOutcomePanel();
		_gameOutcomeMessage.text = winner == Winner.Tie ? "Tie" : winner == Winner.AI ? "Computer Player won!" : "You won!";
	}



	private void DisableStartingMenuPanel() {
		_startingMenuPanel.SetActive(false);
    }

    private void EnableStartingMenuPanel() {
	    _startingMenuPanel.SetActive(true);
    }
    
    private void DisableGameBoardPositionPanel() {
	    _setGameBoardPositionPanel.SetActive(false);
    }

    private void EnableGameBoardPositionPanel() {
	    _setGameBoardPositionPanel.SetActive(true);
    }
    
	private void DisableGameInfoPanel() {
		gameInfoPanel.SetActive(false);
    }

	private void EnableGameInfoPanel() {
		gameInfoPanel.SetActive(true);
    }

	private void DisableGameOutcomePanel() {
		_gameOutcomePanel.SetActive(false);
	}
	
	private void EnableGameOutcomePanel() {
		_gameOutcomePanel.SetActive(true);
	}
	
	private void ShowPokeButton(PokeButton pokeButton)
	{
		pokeButton.Show();
	}
	
	private void HidePokeButton(PokeButton pokeButton)
	{
		pokeButton.Hide();
	}
	
	

	public void ExitButtonPressed()
	{
		_gameOutcomeMessage.text = ""; //If there was a game outcome message, reset
		DisableGameInfoPanel();		//If user exited during game
		DisableGameOutcomePanel();
		HidePokeButton(_exitButton);
		
		AnimateLocalScale animateLocalScale = _ai.spawnedTicTacToeBoard.transform.GetChild(0).GetComponent<AnimateLocalScale>();
		animateLocalScale.MinimizeObject();
		animateLocalScale.gameObjectHasBeenMinimized.AddListener(GoBackToMainMenu);

		foreach (GameObject spawnedXAndOsGameObject in _ai.SpawnedXAndOsGameObjects)
		{
			if (spawnedXAndOsGameObject.transform.TryGetComponent(out AnimateLocalScale animLocalScale))
			{
				animLocalScale.MinimizeObject();
			}
		}

		_ai.GameExitedPrematurely();
		AudioManager.Instance.PlayExitGameButtonPressAudioClip();
	}

	private void GoBackToMainMenu()
	{
		Destroy(_ai.spawnedTicTacToeBoard);
		foreach (GameObject spawnedXAndOsGameObject in _ai.SpawnedXAndOsGameObjects)
		{
			Destroy(spawnedXAndOsGameObject);
		}
		EnableStartingMenuPanel();
		OnGameExited.Invoke();
	}

	private void OnDestroy()
	{
		_ai.onGameStarted.RemoveListener(OnGameStarted);
		_ai.onGameEnded.RemoveListener(OnGameEnded);
		_easyDifficultyButton.OnPressed.RemoveListener(() => {});
		_hardDifficultyButton.OnPressed.RemoveListener(() => {});
		_boardPositionConfirmedButton.OnPressed.RemoveListener(BoardPositionConfirmed);
		_exitButton.OnPressed.RemoveListener(ExitButtonPressed);
	}
	
	private void log(string logText){
		string className = this.GetType().Name;
		Debug.Log("["+className+"]  " +logText);
	}
}
