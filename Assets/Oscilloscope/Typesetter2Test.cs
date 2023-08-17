using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Typesetter2Test : MonoBehaviour {

    public Typesetter Typesetter;
    public PhosphorGraphics PhosphorGraphics;

    [TextArea(3, 10)]
    public string Text;
    //public Vector2 Position;
    public Rect Rect;

    private void Reset()
    {
        this.Typesetter.Font = Resources.Load<HersheyFont>("Fonts/futural");
        this.PhosphorGraphics = FindObjectOfType<PhosphorGraphics>();
    }

    // Use this for initialization
    void Start () {
        //Rect preferredRect = this.Typesetter.GetPreferredRect(this.Text, this.Position);
        //Debug.Log(preferredRect);
    }

    // Update is called once per frame
    void Update () {
        //Rect preferredRect = this.Typesetter.GetPreferredRect(this.Text, this.Position);
        //this.Typesetter.RenderText(this.Text, preferredRect, this.PhosphorGraphics.DrawPolyLine);
        this.Typesetter.RenderText(this.Text, this.Rect, this.PhosphorGraphics.DrawPolyLine);

        //foreach (var rect in this.Typesetter.GetRects(this.Text, this.Position))
        //{
        //    this.PhosphorGraphics.DrawRect(rect);
        //}
    }
}
