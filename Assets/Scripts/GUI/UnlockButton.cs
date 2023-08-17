using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class UnlockButton : MonoBehaviour
{
    public enum ButtonState
    {
        // Requirements not met
        Locked,
        // Requirements are met and the player can buy it 
        Purchasable,
        // Player has bought the projectile but has not been set as an active projectile
        Equippable,
        // Is being used.
        Active
    };

    [SerializeField]
    private ButtonState currentState = ButtonState.Locked;

    [SerializeField]
    private UnlockManager unlockManager;

    [SerializeField]
    private Text stateText;

    public Text costText;

    public ProjectileStats stats;

    public VideoClip previewClip;

    public void SetNameAndDescription()
    {
        unlockManager.UpdateProjectileInformation(stats.name, stats.description, previewClip, stats.eventRequirements);
    }

    public void SetButtonAltText(string text, bool resize = false)
    {
        stateText.text = text;
    }

    public void SetState(ButtonState newState)
    {
            currentState = newState;

        switch (currentState)
        {
            case ButtonState.Locked:
                SetButtonAltText("Requirements Not Met", true);
                break;

            case ButtonState.Purchasable:
                SetButtonAltText("Purchase Now");
                break;

            case ButtonState.Equippable:
                SetButtonAltText("Equip Now");
                break;

            case ButtonState.Active:
                SetButtonAltText("Equipped");
                break;
        }
    }

    public void OnPress()
    {
        unlockManager.UpdateProjectile(stats, currentState);
    }
}
