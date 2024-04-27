using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

public class AnimateLocalScale : MonoBehaviour{

	[FormerlySerializedAs("_animTime")] public float animTime = 2f;
	[SerializeField] AnimationCurve _growthCurve;	
	[SerializeField] AnimationCurve _minimizeCurve;	
	[SerializeField] Vector3 _scale = Vector3.one;

	[HideInInspector] public UnityEvent gameObjectHasBeenMinimized = new UnityEvent();
	[HideInInspector] public bool _animateOnEnable = true;
	private bool _isAnimating = false;
		
	private void OnEnable()
	{
		if(_animateOnEnable) StartCoroutine(SpawnCoroutine());
	}

	IEnumerator SpawnCoroutine(){
		yield return null;
		for(float t = 0 ; t <= animTime; t += Time.deltaTime){
			yield return new WaitForFixedUpdate();
			transform.localScale = _scale * _growthCurve.Evaluate( t/animTime);
		}
		_isAnimating = false;
	}

	public void MinimizeObject()
	{
		if(_isAnimating) return;
		_isAnimating = true;
		StartCoroutine(MinimizeObjectCoroutine());
	}
	
	IEnumerator MinimizeObjectCoroutine(){
		yield return null;
		Vector3 initialLocalScale = transform.localScale;
		for(float t = 0 ; t <= animTime; t += Time.deltaTime){
			yield return new WaitForFixedUpdate();
			transform.localScale = initialLocalScale * _minimizeCurve.Evaluate( t/animTime);
		}
		gameObjectHasBeenMinimized.Invoke();
		_isAnimating = false;
	}
	

}
