using UnityEngine;
using System.Collections;
using System;

public class Spaceship : NetworkItem {

	public bool isFiring;
	public int shield = 100;
	public int hp = 10;
	public int energy = 100;

	private Vector3 _targetPosition;
	private Quaternion _targetRotation;

	public GameObject _location;	//Attached to the VR camera, the player ship will always try to go there
	public GameObject _aim;

	private RaycastHit _hit;
	private bool _canShoot = true;

	//Weapons
	public Transform[] cannons;
	public GameObject bulletPrefab;

	public void Start() {
		base.Start ();
		if (_isPlayer) {
			_location = GameObject.FindGameObjectWithTag ("ShipSeat");
			_aim      = GameObject.FindGameObjectWithTag ("ShipAim");
		}	
	}

	/// <summary>
	/// Fixeds the update. Only the player controlling can write to firebase, everyone else must read and interpret
	/// </summary>
	public void FixedUpdate() {

		//Playable
		if (_isPlayer) {

			//Handle move and rotation
			transform.position = Vector3.Lerp(transform.position, _location.transform.position, Time.deltaTime*5f);
			transform.LookAt (_aim.transform.position);

			//Handle auto fire if enemy locked
			if (Physics.Linecast (transform.position, _aim.transform.position, out _hit)) {
				isFiring = _hit.collider.gameObject.tag == "Enemy";
			} else {
				isFiring = false;
			}
				
			//Set properties to propagate
			SetValues ();

		} else {
			//Non playable draw based on properties read
			InterpretValues ();
		}

		//Fire
		if (isFiring && _canShoot) {
			StartCoroutine ("Fire");
		}
	}

	/// <summary>
	/// Based on the values stored in "SetValues" use them to update this gameobject instance
	/// </summary>
	public void InterpretValues() {

		try {
			//Update position
			_targetPosition    = JsonUtility.FromJson<Vector3> (_properties["position"].ToString());
			transform.position = Vector3.Lerp(transform.position, _targetPosition, Time.deltaTime*5f);

			_targetRotation    = JsonUtility.FromJson<Quaternion> (_properties["rotation"].ToString());
			transform.rotation = Quaternion.Lerp(transform.rotation, _targetRotation, Time.deltaTime*5f);

		} catch (Exception ex) { }
	}

	/// <summary>
	/// Store in the properties dictionary all values you wish to track
	/// </summary>
	public void SetValues() {
		_properties ["position"] = JsonUtility.ToJson (transform.position);
		_properties ["rotation"] = JsonUtility.ToJson (transform.rotation);
		_properties ["fire"]     = isFiring;
		_properties ["hp"] 		 = hp;
		_properties ["shield"]   = shield;
		_properties ["energy"]   = energy;
	}

	/// <summary>
	/// Fire cannons
	/// </summary>
	IEnumerator Fire() {
		_canShoot = false;
		foreach (Transform cannon in cannons) {
			GameObject bullet = (GameObject)Instantiate (bulletPrefab, cannon.position, transform.rotation);
			bullet.GetComponent<Rigidbody> ().AddForce (transform.forward * 300f);
		}
		//wait the time defined at the delay parameter  
		yield return new WaitForSeconds(0.3f);
		_canShoot = true; 
	}
}
