using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System;

public class LifeManager : MonoBehaviour
{

	public static Dictionary<Vector3,Cell> lifeLattice;
	public static List<Cell> cellList;
	public static List<myVector3> saveFile;
	static List<Cell> removeList;
	static List<Cell> addList;
	public GameObject cellFab;
	public GameObject buttFab;

	public int cellsProcessesed = 100;
	bool isRunning = false;
	bool isEdit = true;

	public Slider cellUpdateSlider;
	public Toggle autoUpdateToggle;
	public Button editButton;
	public InputField field;
	public GameObject saveList;

	public Button[] lifeBuddons;
	public int[] values = {2,2,1,0,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2};

	Vector3[] directions = {
		Vector3.up, 
		Vector3.up + Vector3.right,
		Vector3.up + Vector3.left,
		Vector3.up + Vector3.forward,
		Vector3.up + Vector3.back,
		Vector3.up + Vector3.forward + Vector3.right,
		Vector3.up + Vector3.forward + Vector3.left,
		Vector3.up + Vector3.back + Vector3.right,
		Vector3.up + Vector3.back + Vector3.left,
		Vector3.zero + Vector3.right,
		Vector3.zero + Vector3.left,
		Vector3.zero + Vector3.forward,
		Vector3.zero + Vector3.back,
		Vector3.zero + Vector3.forward + Vector3.right,
		Vector3.zero + Vector3.forward + Vector3.left,
		Vector3.zero + Vector3.back + Vector3.right,
		Vector3.zero + Vector3.back + Vector3.left,
		Vector3.down, 
		Vector3.down + Vector3.right,
		Vector3.down + Vector3.left,
		Vector3.down + Vector3.forward,
		Vector3.down + Vector3.back,
		Vector3.down + Vector3.forward + Vector3.right,
		Vector3.down + Vector3.forward + Vector3.left,
		Vector3.down + Vector3.back + Vector3.right,
		Vector3.down + Vector3.back + Vector3.left,
	};

	void Start ()
	{
		lifeLattice = new Dictionary<Vector3,Cell> ();
		cellList = new List<Cell> ();
		addList = new List<Cell> ();
		removeList = new List<Cell> ();
		saveFile = new List<myVector3> ();

		field.text = "Untitled";

		SpawnNow(Vector3.zero);
		SpawnNow (Vector3.up);
		SpawnNow(Vector3.down);
		saveFile.Add (new myVector3(Vector3.zero));
		saveFile.Add (new myVector3(Vector3.up));
		saveFile.Add (new myVector3(Vector3.down));

		//StartCoroutine(ForceUpNeighbors ());
	}
	void Update(){
		if (Input.GetKeyDown (KeyCode.Space)) {
			Debug.Log (isRunning);
			if (!isRunning && !isEdit) {
				CellUpdate ();
			}
		}
		if (Input.GetKeyDown (KeyCode.Escape)) {
			if (saveList.activeSelf) {
				for (int i = saveList.transform.GetChild (0).GetChild (0).childCount - 1; i > -1; i--) {
					Destroy(saveList.transform.GetChild (0).GetChild (0).GetChild(i).gameObject);
				}
				saveList.SetActive (false);
			}
		}
		if (isEdit) {
			if (Input.GetMouseButtonDown (0)) {
				Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
				RaycastHit hit;
				if (Physics.Raycast (ray, out hit,Mathf.Infinity)) {
					Vector3 pos = hit.collider.gameObject.transform.position + hit.normal.normalized;
					SpawnNow (pos);
					saveFile.Add (new myVector3(pos));
				}
			}
			if (Input.GetMouseButtonDown (1) && !Input.GetMouseButton(2)){
				Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
				RaycastHit hit;
				if (Physics.Raycast (ray, out hit,Mathf.Infinity)) {
					KillNow (hit.collider.transform.position);
					myVector3 target = null;
					for (int i = 0; i < saveFile.Count; i++) {
						if (saveFile [i].getValue () == hit.collider.transform.position) {
							target = saveFile [i];
						}
					}
					saveFile.Remove (target);
				}
			}
		}
		cellsProcessesed = (int)cellUpdateSlider.value;
		cellUpdateSlider.GetComponentInChildren<Text> ().text = cellsProcessesed + "";
	}

	void CellUpdate ()
	{
		for (int i = lifeLattice.Values.Count - 1; i > -1; i--) {
			if (cellList[i].myObject != null) {
				if (values[cellList[i].neighbors] == 2) {
					KillNow (cellList[i].position);
					//Debug.Log (cellList [i].neighbors);
				}
			} else {
				if (values[cellList[i].neighbors] == 0) {
					SpawnNow (cellList[i].position);
				}
			}
		}
		StartCoroutine(ForceUpNeighbors ());
	}
	public void SpawnNow(Vector3 pos){
		if (!lifeLattice.ContainsKey(pos)) {
			lifeLattice [pos] = new Cell (pos);
			cellList.Add (lifeLattice [pos]);
			//ForceUpNeighbor(lifeLattice [pos]);
		}
		if (lifeLattice [pos].myObject == null) {
			lifeLattice [pos].myObject = Instantiate (cellFab, pos, Quaternion.identity, this.transform) as GameObject;
			lifeLattice [pos].toUpdate = true;
		}
	}
	public void KillNow(Vector3 pos){
		Destroy (lifeLattice [pos].myObject);
		lifeLattice [pos].myObject = null;
		lifeLattice [pos].toUpdate = true;
	}
	public IEnumerator ForceUpNeighbors(){
		isRunning = true;
		for (int i = 0; i < lifeLattice.Keys.Count; i++) {
			//Debug.Log (cellList [i].neighbors);
			if (cellList [i].neighbors < 0) {
				//Debug.Log ("HAI");
				cellList [i].toUpdate = false;
				int neighbors = 0;
				for (int j = 0; j < directions.Length; j++){
					if (lifeLattice.ContainsKey (cellList [i].position + directions [j]) && lifeLattice [cellList [i].position + directions [j]].myObject != null) {
						neighbors++;
						if (!lifeLattice [cellList [i].position + directions [j]].isNew) {
							lifeLattice [cellList [i].position + directions [j]].neighbors++;
						}
					}else if (!lifeLattice.ContainsKey (cellList [i].position + directions [j]) && lifeLattice [cellList [i].position].myObject != null) {
						addList.Add(new Cell (cellList [i].position + directions [j]));
					}
				}
				cellList [i].neighbors = neighbors;
			}
			else if (cellList [i].toUpdate && cellList[i].neighbors >=0) {
				//Debug.Log ("OLD");
				cellList [i].toUpdate = false;
				for (int j = 0; j < directions.Length; j++) {
					if (lifeLattice.ContainsKey (cellList [i].position + directions [j]) && lifeLattice [cellList [i].position + directions [j]].neighbors >=0) {
						if (cellList [i].myObject != null) {
							lifeLattice [cellList [i].position + directions [j]].neighbors++;
						} else {
							lifeLattice [cellList [i].position + directions [j]].neighbors--;
						}
					}
					if (!lifeLattice.ContainsKey (cellList [i].position + directions [j]) && lifeLattice [cellList [i].position].myObject != null) {
						addList.Add(new Cell (cellList [i].position + directions [j]));
					}
				}
			}
			if (i % cellsProcessesed == 0) {
				yield return null;
			}
		}
		for (int i = 0; i < addList.Count; i++) {
			if (!lifeLattice.ContainsKey(addList [i].position)){
				lifeLattice.Add (addList [i].position, addList [i]);
				cellList.Add (addList [i]);
			}
			ForceUpNeighbor (lifeLattice[addList [i].position]);
			if (i % cellsProcessesed == 0) {
				yield return null;
			}
		}
		addList.Clear ();

		for (int i = 0; i < lifeLattice.Keys.Count; i++) {
			cellList [i].isNew = false;
		}

		isRunning = false;
		yield return null;
		if (autoUpdateToggle.isOn)
			CellUpdate ();
	}
	public void ForceUpNeighbor( Cell cell){
		int nieghbors = 0;
		for (int j = 0; j < directions.Length; j++){
			if (lifeLattice.ContainsKey (cell.position + directions [j]) && lifeLattice [cell.position + directions [j]].myObject != null) {
				nieghbors++;
			}
		}
		cell.neighbors = nieghbors;
	}
	public void StartRun(){
		if (autoUpdateToggle.isOn && !isRunning && !isEdit) {
			CellUpdate ();
		}
	}
	public void ToggleEdit(){
		if (isEdit) {
			isEdit = false;
			editButton.GetComponentInChildren<Text> ().text = "Sim Mode";
			StartCoroutine (ForceUpNeighbors ());
		} else {
			isEdit = true;
			editButton.GetComponentInChildren<Text> ().text = "Edit Mode";
			autoUpdateToggle.isOn = false;
			StopCoroutine (ForceUpNeighbors ());
			lifeLattice = new Dictionary<Vector3, Cell> ();
			cellList = new List<Cell> ();
			addList = new List<Cell> ();
			removeList = new List<Cell> ();
			for (int i = transform.childCount - 1; i > -1; i--) {
				Destroy (transform.GetChild (i).gameObject);
			}
			for (int i = 0; i < saveFile.Count; i++) {
				SpawnNow (saveFile [i].getValue());
			}
		}
	}
	public void Save(){
		if (isEdit) {
			BinaryFormatter bf = new BinaryFormatter ();
			FileStream file = File.Create (Application.persistentDataPath +"/" + field.text + ".dat");

			SaveFile save = new SaveFile ();
			save.save = saveFile;

			bf.Serialize (file, save);
			file.Close ();
		}
	}
	public void Open(){
		if (isEdit) {
			saveList.SetActive (true);

			DirectoryInfo info = new DirectoryInfo (Application.persistentDataPath);
			FileInfo[] fileInfo = info.GetFiles ();
			foreach (FileInfo file in fileInfo) {
				GameObject added = Instantiate (buttFab, saveList.transform.GetChild (0).GetChild (0)) as GameObject;
				added.GetComponentInChildren<Text> ().text = file.Name.Substring(0,file.Name.Length - 4);
			}
		}
	}
	public void OpenFile(string a){
		if (File.Exists (Application.persistentDataPath + "/" + a + ".dat")) {
			BinaryFormatter bf = new BinaryFormatter ();
			FileStream file = File.Open (Application.persistentDataPath + "/" + a + ".dat",FileMode.Open);
			SaveFile settings = (SaveFile)bf.Deserialize (file);
			file.Close ();

			saveFile = settings.save;

			field.text = a;

			lifeLattice = new Dictionary<Vector3, Cell> ();
			cellList = new List<Cell> ();
			addList = new List<Cell> ();
			removeList = new List<Cell> ();
			for (int i = transform.childCount - 1; i > -1; i--) {
				Destroy (transform.GetChild (i).gameObject);
			}
			for (int i = 0; i < saveFile.Count; i++) {
				SpawnNow (saveFile [i].getValue());
			}

			for (int i = saveList.transform.GetChild (0).GetChild (0).childCount - 1; i > -1; i--) {
				Destroy(saveList.transform.GetChild (0).GetChild (0).GetChild(i).gameObject);
			}
			saveList.SetActive (false);
		}
	}
	public void New(){
		if (isEdit) {
			field.text = "Untitled";
			lifeLattice = new Dictionary<Vector3, Cell> ();
			cellList = new List<Cell> ();
			addList = new List<Cell> ();
			removeList = new List<Cell> ();
			saveFile = new List<myVector3> ();
			for (int i = transform.childCount - 1; i > -1; i--) {
				Destroy (transform.GetChild (i).gameObject);
			}
			saveFile.Add (new myVector3(Vector3.zero));
			for (int i = 0; i < saveFile.Count; i++) {
				SpawnNow (saveFile [i].getValue());
			}
		}	
	}
	public void LifeClickL(int a){
		lifeBuddons [a].image.color = Color.green;
		values [a] = 0;
	}
	public void LifeClickR(int a){
		lifeBuddons [a].image.color = Color.red;
		values [a] = 2;
	}
	public void LifeClickM(int a){
		lifeBuddons [a].image.color = Color.yellow;
		values [a] = 1;
	}
	/*
	Any live cell with fewer than two live neighbours dies, as if caused by underpopulation.
	Any live cell with two or three live neighbours lives on to the next generation.
	Any live cell with more than three live neighbours dies, as if by overpopulation.
	Any dead cell with exactly three live neighbours becomes a live cell, as if by reproduction.
	*/
}

public class Cell
{
	public GameObject myObject;
	public Vector3 position;
	public int neighbors;
	public bool toUpdate;
	public bool isNew;

	public Cell (Vector3 pos)
	{
		position = pos;
		neighbors = -1;
		toUpdate = false;
		isNew = true;
	}
}
[Serializable]
class SaveFile{
	public List<myVector3> save;
}
[Serializable]
public class myVector3{
	public float x;
	public float y;
	public float z;

	public myVector3(Vector3 pos){
		x = pos.x;
		y = pos.y;
		z = pos.z;
	}
	public Vector3 getValue(){
		return new Vector3 (x, y, z);
	}
}
