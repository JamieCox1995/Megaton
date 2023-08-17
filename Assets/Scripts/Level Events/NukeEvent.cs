using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PostProcessing;

public class NukeEvent : LevelEvent
{
    [SerializeField]
    private GameObject effect;

    [SerializeField]
    private PostProcessingBehaviour behaviourToEffect;

    [SerializeField]
    private int statID = 11;

    private Rigidbody m_rigidbody;

    private BloomModel.Settings _bloomSettings;
    private GameObject _spawnedEffect;

	// Use this for initialization
	private void Start ()
    {
        m_rigidbody = GetComponent<Rigidbody>();
        m_rigidbody.isKinematic = true;
	}

    protected override void OnEventStart()
    {
        _bloomSettings = behaviourToEffect.profile.bloom.settings;
        
        BloomModel.Settings newSettings = _bloomSettings;
        newSettings.bloom.radius = 7f;
        
        behaviourToEffect.profile.bloom.settings = newSettings;
        
        _spawnedEffect = Instantiate(effect, transform.position, transform.rotation);

        GameEventManager.instance.TriggerEvent(new TimeScaleAdjustmentEvent(GameEventType.OnStartTimeScaleAdjustment, 0.1f));
    }

    protected override void OnEventEnd()
    {
        behaviourToEffect.profile.bloom.settings = _bloomSettings;

        GameEventManager.instance.TriggerEvent(new GameEvent(GameEventType.OnEndTimeScaleAdjustment));

        GameEventManager.instance.TriggerEvent(new StatAchievedEvent(GameEventType.OnStatAchieved, statID));

        Destroy(_spawnedEffect);
        Destroy(gameObject);
    }

    public void EnableRigidbody()
    {
        m_rigidbody.isKinematic = false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        StartLevelEvent();
    }
}
