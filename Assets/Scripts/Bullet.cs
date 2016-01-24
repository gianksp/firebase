using UnityEngine;
using System.Collections;

public class Bullet : MonoBehaviour {

	// Use this for initialization
	void Start () {
		Destroy (gameObject, 3);
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnCollisionEnter(Collision collision) {
		if (collision.transform.tag == "Enemy") {
			
		}
		Destroy (gameObject);
	}
}
