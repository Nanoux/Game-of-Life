using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SaveButton : MonoBehaviour {

	public void Press(){
		GameObject.FindObjectOfType<LifeManager>().OpenFile (GetComponentInChildren<Text> ().text);
	}
}
