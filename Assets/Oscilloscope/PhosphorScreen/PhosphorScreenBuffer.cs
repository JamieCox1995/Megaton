using System.Collections.Generic;
using UnityEngine;

using Stopwatch = System.Diagnostics.Stopwatch;

[AddComponentMenu("Oscilloscope UI/Screen Buffer")]
[RequireComponent(typeof(PhosphorScreenRenderer))]
public class PhosphorScreenBuffer : MonoBehaviour {

    public PhosphorScreenRenderer ScreenRenderer;

    private List<PhosphorCommand> commandQueue;
    private Stopwatch stopwatch;
    private Vector2 currentPosition;
    private bool isElectronBeamActive;

    private void Reset()
    {
        this.ScreenRenderer = GetComponent<PhosphorScreenRenderer>();
    }

    // Use this for initialization
    void Start()
    {
        this.stopwatch = Stopwatch.StartNew();
        this.commandQueue = new List<PhosphorCommand>(new[] { CreateEventNow() });
        this.isElectronBeamActive = true;

        this.commandQueue.Add(CreateEventNow());
    }
	
	// Update is called once per frame
	void Update()
    {
        stopwatch.Reset();

        this.ScreenRenderer.SetPhosphorCommandBuffer(this.commandQueue.ToArray());

        this.commandQueue.Clear();

        this.commandQueue.Add(CreateEventNow());
	}

    public void SetPosition(Vector2 position)
    {
        position.x = Mathf.Clamp(position.x, -1f, 1f);
        position.y = Mathf.Clamp(position.y, -1f, 1f);
        this.currentPosition = position;

        var @event = CreateEventNow();
        commandQueue.Add(@event);
    }

    public void SetElectronBeamActive(bool active)
    {
        this.isElectronBeamActive = active;

        var @event = CreateEventNow();
        commandQueue.Add(@event);
    }

    private PhosphorCommand CreateEventNow()
    {
        float currentTime = (float)(Time.time + stopwatch.Elapsed.TotalSeconds * Time.timeScale);
        var crtEvent = new PhosphorCommand(currentPosition, currentTime, isElectronBeamActive);

        return crtEvent;
    }
}
