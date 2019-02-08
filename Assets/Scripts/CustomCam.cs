using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomCam : MonoBehaviour {

	float mouseSense = 10.0f;

	// Update is called once per frame
	void Update () {
		if (Input.GetMouseButton (2) && (!Input.GetKey(KeyCode.LeftShift) && !Input.GetMouseButton (1))) {
			transform.parent.transform.Rotate (new Vector3 (-Input.GetAxis ("Mouse Y"), Input.GetAxis ("Mouse X"), 0) * mouseSense);
			transform.parent.transform.eulerAngles = new Vector3 (transform.parent.transform.eulerAngles.x, transform.parent.transform.eulerAngles.y, 0);
		}
		if (Input.GetMouseButton (2) && (Input.GetKey(KeyCode.LeftShift) || Input.GetMouseButton (1))) {
			transform.parent.position += transform.TransformVector(new Vector3 (Input.GetAxis ("Mouse X"), -Input.GetAxis ("Mouse Y"), 0) * mouseSense) / 50f;
			//transform.parent.transform.eulerAngles = new Vector3 (transform.parent.transform.eulerAngles.x, transform.parent.transform.eulerAngles.y, 0);
		}
		transform.localPosition = new Vector3 (0f,0f,Mathf.Clamp(transform.localPosition.z - (Input.mouseScrollDelta.y * (transform.localPosition.z / 10)),-100f,-1f));
	}
}
