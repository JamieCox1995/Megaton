using System.Collections;
using System.Collections.Generic;
using TotalDistraction.ServiceLocation;
using TotalDistraction.ServiceLocation.DefaultServices;
using UnityEngine;

public class InputServicesTest : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        gameObject.GetComponent<MeshRenderer>().material.color = ServiceLocator.Get<IInputProxyService>().anyKey ? Color.blue : Color.white;
        Debug.Log(ServiceLocator.Get<IInputManager>().Instance);
        Debug.Log(ServiceLocator.Get<ITestConfigurableService>().Value);
	}
}
