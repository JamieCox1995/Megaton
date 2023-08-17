using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MotionPathRunner : MonoBehaviour
{
    public float TravelTime;
    public bool RotateToPath;

    [SerializeField]private MotionPath _motionPath;
    private Vector3 _previousPosition;

    private float timer = 0f;
    private bool _started = false;

	// Use this for initialization
	void Start()
    {
        if (_motionPath == null)
        _motionPath = GetComponent<MotionPath>();
	}
	
	// Update is called once per frame
	void Update()
    {
        if (_started)
        {
            _previousPosition = this.transform.position;

            timer += Time.deltaTime;

            float t = (timer / this.TravelTime) % 1;

            Vector3 position = _motionPath.Evaluate(t);

            if (this.RotateToPath)
            {
                Quaternion rotation = Quaternion.LookRotation(position - _previousPosition);

                this.transform.rotation = rotation;
            }

            this.transform.position = position;
        }
	}

    public void StartRunner()
    {
        _started = true;
    }

    public void StopRunner()
    {
        _started = false;
    }
}
