using UnityEngine;
using System.Collections;

namespace UltimateFracturing
{
    public class DieTimer : MonoBehaviour
    { 
        public float SecondsToDie      = Mathf.Infinity;
        public float OffscreenLifeTime = Mathf.Infinity;

        private float durationToDissolve = 0.25f;           // This is a percentage
        private float waitTime = 0, dissolveTime = 0;

        float m_fTimer = 0.0f;
        private bool dissolving = false;

        void Start()
        {
            m_fTimer = 0.0f;

            dissolveTime = durationToDissolve * SecondsToDie;
            waitTime = SecondsToDie - dissolveTime;
        }
    
        void Update()
        {
            m_fTimer += Time.deltaTime;

            if (m_fTimer >= waitTime && !dissolving)
            {
                dissolving = true;

                // Add the DissolveObject script
                DissolveObject disO = gameObject.AddComponent<DissolveObject>();
                disO.StartDissolve(dissolveTime);
            }
        }
    }
}