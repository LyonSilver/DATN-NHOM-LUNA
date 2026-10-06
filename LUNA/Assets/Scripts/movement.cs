using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class movement : MonoBehaviour {

	Rigidbody RB;
	public float Speed = 20;

	// Use this for initialization
	void Start () {
		RB = this.gameObject.GetComponent<Rigidbody> ();
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKey (KeyCode.W)) {
			RB.AddForce (new Vector3(0, 0, Speed));
		}
		if (Input.GetKey (KeyCode.S)) {
			RB.AddForce (new Vector3(0, 0, -Speed));
		}
		if (Input.GetKey (KeyCode.A)) {
			RB.AddForce (new Vector3(-Speed, 0, 0));
		}
		if (Input.GetKey (KeyCode.D)) {
			RB.AddForce (new Vector3(Speed, 0, 0));
		}

		if (Input.GetKey (KeyCode.Space)) {
			transform.GetChild (0).gameObject.SetActive (true);
		} else {
			transform.GetChild (0).gameObject.SetActive (false);
		}
	}
}
