using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class LifeBuddon : MonoBehaviour, IPointerClickHandler {

	public UnityEvent leftEvent;
	public UnityEvent rightEvent;
	public UnityEvent middleEvent;

	public void OnPointerClick(PointerEventData data) {
		switch(data.button)
		{
		case PointerEventData.InputButton.Left:
			leftEvent.Invoke ();
			break;
		case PointerEventData.InputButton.Right:
			rightEvent.Invoke ();
			break;
		case PointerEventData.InputButton.Middle:
			middleEvent.Invoke ();
			break;
		}

	}
}
