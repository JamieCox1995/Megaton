using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chaperone : MonoBehaviour {

    public GameObject Target;

    private Renderer renderer;
    private Material material;

    private void Start()
    {
        GameEventManager.instance.onMortarFire += OnMortarFired;
        GameEventManager.instance.onProjectileDestroyed += OnProjectileDestroyed;
        GameEventManager.instance.onTurnEnd += OnTurnEnd;
        GameEventManager.instance.onPlayerReady += OnTurnEnd;

        this.renderer = GetComponent<Renderer>();

        if (renderer == null)
        {
            this.enabled = false;
            return;
        }

        this.material = this.renderer.material;
    }

    // Update is called once per frame
    void Update()
    {
		if (this.Target != null)
        {
            this.material.SetVector("_WorldPosition", this.Target.transform.position);
        }
	}

    private void OnMortarFired(MortarFireEvent eventArgs)
    {
        Target = eventArgs.projectile;
    }

    private void OnProjectileDestroyed()
    {
        Target = Camera.main.gameObject;
    }

    private void OnTurnEnd()
    {
        Target = FindObjectOfType<TargetingSystem>().GetLookAtTarget();
    }
}
