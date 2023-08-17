using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserEmitter : MonoBehaviour
{
    [SerializeField] private float laserRange = 100f;
    [SerializeField, Tooltip("These are the layers of objects which will cause an explosion when the laser hits them.")]
    private LayerMask laserableLayers;

    [SerializeField] private Transform laserEmissionPoint;
    [SerializeField] private LineRenderer laserRenderer;
    [SerializeField]
    private float laserRetractDuration = 0.5f;
    private Color laserColour;

    [SerializeField] private ExplosiveSettings laserExplosions;
    [SerializeField] private Animation rotationAnim;

    [SerializeField] private AudioClip chargeClip;
    private AudioSource m_audio;

	// Use this for initialization
	void Start ()
    {
        laserColour = laserRenderer.material.GetColor("_EmissionColor");
        m_audio = GetComponent<AudioSource>();

        StartCoroutine("LaserEmission");
	}

    private IEnumerator LaserEmission()
    {
        float duration = 20f;
        float timer = 0f;

        if (rotationAnim)
        {
            duration = rotationAnim.clip.length;
            rotationAnim.Play();
        }
        else
        {
            duration = 10f;
        }

        while(timer <= duration)
        {
            Ray ray = new Ray(laserEmissionPoint.position, laserEmissionPoint.forward);
            timer += Time.deltaTime;

            RaycastHit hit;

            // The start of the laser does not depend on any hit functionality so we are going to update its position here
            laserRenderer.SetPosition(0, laserEmissionPoint.position);

            if(Physics.Raycast(ray, out hit, laserRange, laserableLayers))
            {
                // Update the laser renderers end position and the width
                laserRenderer.SetPosition(1, hit.point);

                // The width is proportional to the distance from the emission point
                float width = Vector3.Distance(laserEmissionPoint.position, hit.point) / laserRange;
                laserRenderer.endWidth = width;

                // We are going to want to do some stuff here so that the chunks start to "dissolve" when they are hit by the laser but for the moment we are going to destroy them
                if (hit.collider.gameObject.GetComponent<FracturedChunk>()) hit.collider.gameObject.GetComponent<FracturedChunk>().DetachFromObject();

                if (hit.collider.gameObject.GetComponent<ExplosiveObject>()) hit.collider.gameObject.GetComponent<ExplosiveObject>().Explode();

                if (hit.collider.gameObject) Destroy(hit.collider.gameObject);
            }
            else
            {
                // We need to set the end position to the correct position and the end with to 0
                Vector3 end = laserEmissionPoint.position + (laserEmissionPoint.forward * laserRange);
                laserRenderer.SetPosition(1, end);

                laserRenderer.endWidth = 0f;
            }

            yield return null;
        }

        // Now that the animation/duration has finished, we are going to retract the laser and then finally explode.
        timer = 0f;

        m_audio.Stop();

        m_audio.volume = 1;
        m_audio.clip = chargeClip;

        m_audio.Play();

        while(timer <= laserRetractDuration)
        {
            timer += Time.deltaTime;

            // Fire out another raycast to see if the laser is still hitting something
            Ray ray = new Ray(laserEmissionPoint.position, laserEmissionPoint.forward);
            RaycastHit hit;

            float lerp = timer / laserRetractDuration;

            float range = Mathf.Lerp(laserRange, 0f, lerp);

            if (Physics.Raycast(ray, out hit, range))
            {
                // Here we are checking to see which is closest, the hit position of the raycast, or the range of the retract laser
                range = Mathf.Min(Vector3.Distance(laserEmissionPoint.position, hit.point), range);
            }

            Vector3 endPosition = laserEmissionPoint.position + (laserEmissionPoint.forward * range);

            // We shall now update the end position of the line renderer
            laserRenderer.SetPosition(1, endPosition);

            yield return null;
        }

        // Once the laser has retracted, Explode like a mothafucker
        Explosive explo = GetComponent<Explosive>();
        explo.TriggerExplosion();

        GameEventManager.instance.TriggerEvent(new GameEvent(GameEventType.OnProjectileDestroyed));

        // Now that the effect has finished, we shall destroy the game object
        Destroy(gameObject.transform.parent.gameObject);
    }
}
