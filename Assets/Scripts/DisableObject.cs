using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisableObject : MonoBehaviour
{
    public FracturedObject Target;

	// Use this for initialization
	void Start()
    {
        GameEventManager.instance.onObjectDamaged += HandleObjectDamaged;
	}

    private void HandleObjectDamaged(ObjectDamagedEvent obj)
    {
        if (obj.target == this.Target)
        {
            gameObject.SetActive(false);
        }
    }
}
