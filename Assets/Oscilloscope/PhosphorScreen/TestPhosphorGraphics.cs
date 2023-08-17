using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PhosphorGraphics))]
public class TestPhosphorGraphics : MonoBehaviour {

    public PhosphorGraphics Graphics;

    private void Reset()
    {
        this.Graphics = GetComponent<PhosphorGraphics>();
    }

	// Update is called once per frame
	void Update () {
        for (int i = 0; i < 10; i++)
        {
            float x = 0.95f - (i * 0.025f);
            float y = 0.9f - (i * 0.025f);

            this.Graphics.DrawLine(new Vector2(x, 0.95f), new Vector2(x, y));
        }

        this.Graphics.DrawPolyLine(new Vector2(-0.9f, -0.8f), new Vector2(-0.9f, -0.9f), new Vector2(-0.8f, -0.9f));

        this.Graphics.DrawCircle(new Vector2(0.75f, -0.75f), 0.2f);

        this.Graphics.DrawEllipse(new Vector2(-0.5f, 0.5f), 0.25f, 0.05f, Time.time * 180f);

        this.Graphics.Plot(new Rect(-0.55f, -0.25f, 0.5f, 0.5f), Rect.MinMaxRect(0f, -1.25f, 10f, 1.25f), x => Mathf.Sin(x + Time.time), PlotOptions.BothAxes);

        this.Graphics.Plot(new Rect(0.05f, -0.25f, 0.5f, 0.5f), Rect.MinMaxRect(-1.25f, -1.25f, 1.25f, 1.25f), t => Mathf.Sin((2f + Mathf.Sin(Time.time)) * t), t => Mathf.Cos((2f + Mathf.Cos(Time.time)) * t), 0f, 2f * Mathf.PI, PlotOptions.All);
    }
}
