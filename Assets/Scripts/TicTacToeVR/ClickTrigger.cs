using System;
using System.Collections.Generic;
using UnityEngine;

/*
	If game starts, player is allowed to click. 
	If player wins/game ends, player is not allowed to click on game anymore
*/
public class ClickTrigger : MonoBehaviour
{
	[SerializeField ] TicTacToeAI _ai;

	[SerializeField]
	private int _cellX = 0;
	[SerializeField]
	private int _cellY = 0;

	[SerializeField]
	private bool canClick;

	private void Start(){

		if(!_ai) _ai = FindObjectOfType<TicTacToeAI>();
		_ai.CanListenToInputs.AddListener(CanListenToInputs);
		_ai.onGameEnded.AddListener((win) => DisableInput());
	}

	private void CanListenToInputs()
	{
		RegisterTransform();
	}
	
	private void RegisterTransform()
	{
		_ai.RegisterTransform(_cellX, _cellY, this);
		canClick = true;
	}

	private void DisableInput(){
		canClick = false;
	}

	
	private void OnTriggerEnter(Collider col)
	{
		// log(name+ " Collided with "+col.name + "of layer "+col.gameObject.layer);
		if (col.CompareTag("PhysicsHands"))
		{
			CellSelected();
		}
	}

	private void CellSelected()
	{
		if(canClick){
			// log("Cell [" + _cellX + "," + _cellY + "] Selected");
			//The function being called will also check if it is user's/player's turn
			_ai.PlayerSelects(_cellX, _cellY);	
			
		}
	}

	private void OnDestroy()
	{
		_ai.CanListenToInputs.RemoveListener(CanListenToInputs);
		_ai.onGameEnded.RemoveListener((win) => {});
	}

	private void log(string logText){
		string className = this.GetType().Name;
		Debug.Log("["+className+"]  " +logText);
	}
	
}
