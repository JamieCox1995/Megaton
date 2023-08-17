using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireworkEvent : LevelEvent
{
	public GameObject particle;
	public Animator anim;
	public AnimationClip launchClip;
	private bool isLaunched = false;
	private bool damaged = false;
	private Rigidbody r_rigidbody;

    private Explosive explosive;

    // Use this for initialization
    void Start()
    {
        explosive = GetComponent<Explosive>();

        r_rigidbody = GetComponent<Rigidbody>();
        r_rigidbody.isKinematic = true;
    }

    protected override IEnumerator DoEvent()
    {
        particle.SetActive(true);
        anim.Play("Flying Firework");

        yield return new WaitForSeconds(launchClip.length);

        explosive.TriggerExplosion();
        GetComponent<MeshRenderer>().enabled = false;

        yield return base.DoEvent();
    }

    protected override void OnEventEnd()
    {
        Destroy(gameObject);
    }
}
