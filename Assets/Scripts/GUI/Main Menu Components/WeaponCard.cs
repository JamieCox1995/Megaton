using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class WeaponCard : MonoBehaviour
{
    [Header("Stats:")]
    public ProjectileStats projectileStats;

    [Header("Preview:")]
    public Mesh projectilePreviewMesh;
    public VideoClip previewVideo;
    public Vector3 offset;

    public WeaponSelector weaponSelector;

    public UnlockButton.ButtonState buttonState = UnlockButton.ButtonState.Locked;

    public void SetEquippedText()
    {
        OscilloscopeText text = GetComponentInChildren<OscilloscopeText>();
        text.Text = string.Format(">{0} Projectile<", projectileStats.name);
    }

    public void SetUnequippedText()
    {
        OscilloscopeText text = GetComponentInChildren<OscilloscopeText>();
        text.Text = string.Format("{0} Projectile", projectileStats.name); 
    }

    public void OnPressed()
    {
        weaponSelector.OnCardSelected(this);
    }
}
