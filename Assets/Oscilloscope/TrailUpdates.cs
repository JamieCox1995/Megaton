using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrailUpdates : MonoBehaviour {

    public float CycleRadius = 3f;
    public float EpicycleRadius = 0.5f;

    public float CycleSpeed = 1f;

    //public ScopeRenderer ScopeRenderer;
    public PhosphorGraphics PhosphorGraphics;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        float x = this.CycleRadius * Mathf.Sin(2f * Mathf.PI * Time.time * this.CycleSpeed);
        float y = this.CycleRadius * Mathf.Cos(2f * Mathf.PI * Time.time * this.CycleSpeed);

        //for (int i = 0; i < 32; i++)
        //{
        //    float ex = this.EpicycleRadius * Mathf.Sin(2f * Mathf.PI * i / 32f);
        //    float ey = this.EpicycleRadius * Mathf.Cos(2f * Mathf.PI * i / 32f);

        //    //gameObject.transform.position = new Vector3(x + ex, y + ey, 0f);

        //    //this.ScopeRenderer.SetPosition(new Vector3(x + ex, y + ey, 0f));
        //}

        this.PhosphorGraphics.DrawCircle(new Vector2(x, y), this.EpicycleRadius);
	}
}
