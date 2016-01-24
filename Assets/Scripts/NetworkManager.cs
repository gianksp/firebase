using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class NetworkManager : MonoBehaviour {

	public static IFirebase firebase;			 //Firebase instance
	public static string identifier;			 //This network item id
	public GameObject prefab;					 //Ship prefab

	public readonly Queue<Action> ExecuteOnMainThread = new Queue<Action>(); 	//Actions to be performed in main thread
	public static IDictionary <string,object> items;

	/// <summary>
	/// Initialise this player
	/// </summary>
	void Start () {
		
		//Initialise this object in map
		Vector3 initialPosition = new Vector3 (UnityEngine.Random.Range (-10, 10), 5, (UnityEngine.Random.Range (-10, 10)));
		firebase = Firebase.CreateNew ("https://gdg2015.firebaseio.com/");
		firebase.UnAuth ();
		firebase.AuthAnonymously ((AuthData auth) => {
			InitFirebasePlayer(auth.Uid,initialPosition);
		}, (FirebaseError e) => {
			Debug.Log ("auth failure!! "+e);
		});
			
		//Listeners
		firebase.ChildAdded   += (object sender, ChangedEventArgs e) => { ExecuteOnMainThread.Enqueue(() => { StartCoroutine(Add(e.DataSnapshot));    }); };
		firebase.ChildRemoved += (object sender, ChangedEventArgs e) => { ExecuteOnMainThread.Enqueue(() => { StartCoroutine(Remove(e.DataSnapshot)); }); };
		firebase.ValueUpdated += (object sender, ChangedEventArgs e) => { ExecuteOnMainThread.Enqueue(() => { StartCoroutine(Modify(e.DataSnapshot)); }); };
	}
	
	/// <summary>
	/// Execute items in main thread
	/// </summary>
	public void Update() {
		// dispatch stuff on main thread
		while (ExecuteOnMainThread.Count > 0) {
			ExecuteOnMainThread.Dequeue().Invoke();
		}
	}

	/// <summary>
	/// Update this firebase instance
	/// </summary>
	/// <param name="snapshot">Snapshot.</param>
	IEnumerator Modify (IDataSnapshot snapshot) {
		try {
			items = snapshot.DictionaryValue;
		} catch (ArgumentNullException) {         }
		yield return null;
	}

	/// <summary>
	/// This instance was created, init it
	/// </summary>
	/// <param name="snapshot">Snapshot.</param>
	IEnumerator Add (IDataSnapshot snapshot) {
		try {
			if (items == null) {
				items = new Dictionary<string,object>();
			}
			items [snapshot.Key] = snapshot.DictionaryValue;
			Vector3 initPos = JsonUtility.FromJson<Vector3>(snapshot.DictionaryValue["position"].ToString());
			GameObject ship = (GameObject)Instantiate(prefab,initPos,Quaternion.identity);
			ship.GetComponent<NetworkItem>().identifier = snapshot.Key;
		} catch (ArgumentNullException) {         }
		yield return null;
	}

	/// <summary>
	/// This instance died, explode it locally
	/// </summary>
	/// <param name="snapshot">Snapshot.</param>
	IEnumerator Remove (IDataSnapshot snapshot) {
		try {
			items.Remove (snapshot.Key);
		} catch (ArgumentNullException) {         }
		yield return null;
	}

	/// <summary>
	/// Create a Firebase object based on this current player logged in
	/// </summary>
	void InitFirebasePlayer(string id,Vector3 initialPosition) {
		identifier = id;
		IDictionary<string, object> data = new Dictionary<string, object>();
		data.Add ("position", JsonUtility.ToJson(initialPosition));
		firebase.Child(identifier).SetValue(data);
	}
}
