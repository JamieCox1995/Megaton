using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightFlicker : MonoBehaviour
{
    public Vector2 toggleTimes;

    private Light light;
    private float nextToggleTime = 0.05f;

	// Use this for initialization
	void Start ()
    {
        light = GetComponent<Light>();

        StartCoroutine(FlickerLight());
	}
	

    private IEnumerator FlickerLight()
    {
        float timer = 0;

        nextToggleTime = Random.Range(toggleTimes.x, toggleTimes.y);

        while(gameObject.activeInHierarchy == true)
        {
            timer += Time.deltaTime;

            if (timer >= nextToggleTime)
            {
                light.enabled = !light.enabled;

                timer = 0f;

                nextToggleTime = Random.Range(toggleTimes.x, toggleTimes.y);
            }

            yield return new WaitForEndOfFrame();
        }
    }
}
