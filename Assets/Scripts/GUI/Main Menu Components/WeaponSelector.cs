using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class WeaponSelector : MonoBehaviour
{
    [Header("Weapon Cards: ")]
    public WeaponCard[] primaryWeapons;
    public WeaponCard[] pickupWeapons;

    [Space]
    public ProjectileDatabase projectileDatabase;

    [Header("GUI Variables: ")]
    public OscilloscopeText weaponName;
    public OscilloscopeText weaponCost;
    public OscilloscopeText weaponDescription;
    public OscilloscopeText weaponInfoDescription;
    public OscilloscopeText weaponRequirements;
    public OscilloscopeText playerFunds;
    public OscilloscopeButton unlockButton;

    [Space]
    public Animator menuAnimator;
    public MeshFilter previewMesh;
    public VideoPlayer videoPlayer;

    // Storing all of the saved data for progression.
    private List<string> unlockedProjectiles;
    private WeaponCard currentCard;
    private string primary, pickup;
    private decimal savedMoney;

	// Use this for initialization
	void Start ()
    {
        RetrieveSavedProgress();
        SetupWeaponCards();

        SetProjectileCosts();
	}

    private void SetProjectileCosts()
    {
        float pCost, sCost;

        pCost = projectileDatabase.primaryStats.First(x => x.name == primary).cost;
        sCost = projectileDatabase.pickupStats.First(x => x.name == pickup).cost;

        PlayerPrefs.SetFloat("Primary Cost", pCost);
        PlayerPrefs.SetFloat("Pickup Cost", sCost);
    }

    private void RetrieveSavedProgress()
    {
        unlockedProjectiles = PlayerProgression._instance.savedProgression.unlockedProjectiles;

        primary = PlayerProgression._instance.GetEquippedProjectile(ProjectileType.Primary);
        pickup = PlayerProgression._instance.GetEquippedProjectile(ProjectileType.Pickup);

        savedMoney = PlayerProgression._instance.GetScore();
    }
	
	private void SetupWeaponCards()
    {
        for(int index = 0; index < primaryWeapons.Length; index++)
        {
            primaryWeapons[index].projectileStats = projectileDatabase.primaryStats[index];

            if (primaryWeapons[index].projectileStats.name == primary) primaryWeapons[index].SetEquippedText();
        }

        for (int index = 0; index < primaryWeapons.Length; index++)
        {
            pickupWeapons[index].projectileStats = projectileDatabase.pickupStats[index];

            if (pickupWeapons[index].projectileStats.name == pickup) pickupWeapons[index].SetEquippedText();
        }
    }

    public void OnCardSelected(WeaponCard card)
    {
        SetGUIText(card);
        SetInfoPanelText(card);
        CheckCardState(card);
        UpdateUnlockButton(card);
        UpdateVideoplayer(card);

        menuAnimator.SetTrigger("Weapon Selected");
        currentCard = card;
    }

    private void UpdateVideoplayer(WeaponCard card)
    {
        videoPlayer.Stop();
        videoPlayer.clip = card.previewVideo;
    }

    public void OnWeaponSelect()
    {
        if (currentCard.buttonState == UnlockButton.ButtonState.Equippable)
        {
            EquipSelectedCard();
        }
        else if (currentCard.buttonState == UnlockButton.ButtonState.Purchasable)
        {
            PurchaseSelectedCard();
        }

        OnCardSelected(currentCard);
    }

    private void EquipSelectedCard()
    {
        if (primaryWeapons.Contains(currentCard))
        {
            WeaponCard card = primaryWeapons.FirstOrDefault(c => c.projectileStats.name == primary);
            card.SetUnequippedText();

            PlayerProgression._instance.SetEquippedProjectile(ProjectileType.Primary, currentCard.projectileStats.name);
            primary = currentCard.projectileStats.name;

            SetProjectileCosts();
        }
        else if (pickupWeapons.Contains(currentCard))
        {
            WeaponCard card = pickupWeapons.FirstOrDefault(c => c.projectileStats.name == pickup);
            card.SetUnequippedText();

            PlayerProgression._instance.SetEquippedProjectile(ProjectileType.Pickup, currentCard.projectileStats.name);
            pickup = currentCard.projectileStats.name;

            SetProjectileCosts();
        }

        currentCard.SetEquippedText();

        PlayerProgression.SaveProgress();
    }

    private void PurchaseSelectedCard()
    {
        PlayerProgression._instance.AddNewUnlock(currentCard.projectileStats.name, currentCard.projectileStats.cost);

        savedMoney -= (decimal)currentCard.projectileStats.cost;
        playerFunds.Text = string.Format("Funds: ${0}", savedMoney.ToString("N0"));

        EquipSelectedCard();
    }

    private void SetGUIText(WeaponCard card)
    {
        // Here we want to set the GUI Text to display the name and description of the projectile.
        playerFunds.Text = string.Format("Funds: ${0}", savedMoney.ToString("N0"));
        // Set Name and Cost
        weaponName.Text = card.projectileStats.name + " Projectile";
        weaponCost.Text = string.Format("Cost: ${0}", card.projectileStats.cost.ToString("N0"));

        // Set Description.
        weaponDescription.Text = string.Format("{0}", card.projectileStats.shortDescription);

        // Now we want to set the requirements panel.
        if (card.projectileStats.eventRequirements.Length == 0)
        {
            string requirements = string.Empty;

            requirements += "- No Reqiurements";

            weaponRequirements.Text = requirements;
        }
        else
        {
            string requirements = string.Empty;

            foreach (ProgressionStat stat in card.projectileStats.eventRequirements)
            {
                requirements += string.Format("- {0}\n", ProgressionStatManager._instance.GetDescription(stat.id));
            }

            weaponRequirements.Text = requirements;
        }

        // Finally we are going to update the Preview Mesh
        previewMesh.mesh = card.projectilePreviewMesh;
    }

    private void SetInfoPanelText(WeaponCard card)
    {
        weaponInfoDescription.Text = card.projectileStats.description;
    }

    private void CheckCardState(WeaponCard card)
    {
        // We are going to check to see if the current state of the card is correct, as it may have changed since the last time we updated it

        // First of all, checking to see if the card is already equipped.
        if (primary == card.projectileStats.name || pickup == card.projectileStats.name)
        {
            card.buttonState = UnlockButton.ButtonState.Active;
            return;
        }

        // Now we are checking to see if the card is unlocked. We are assuming that if it has been equipped, it is already set correctly.
        if (unlockedProjectiles.Contains(card.projectileStats.name))
        {
            card.buttonState = UnlockButton.ButtonState.Equippable;
            return;
        }

        if ((decimal)card.projectileStats.cost > savedMoney)
        {
            card.buttonState = UnlockButton.ButtonState.Locked;
            return;
        }

        // Now we want to check to see if we can Buy the Projectile. To do this we need to check to see if all the requirements are met (if there are any), and if the player has enough money. If neither of these has been met, then we are going to set the state to Locked
        if (card.projectileStats.eventRequirements.Length != 0)
        {
            List<ProgressionStat> completedStats = PlayerProgression._instance.savedProgression.completedEvents;

            foreach (ProgressionStat stat in card.projectileStats.eventRequirements)
            {
                if (completedStats.FirstOrDefault(req => req.id == stat.id) == null)
                {
                    card.buttonState = UnlockButton.ButtonState.Locked;
                    return;
                }
            }
        }

        card.buttonState = UnlockButton.ButtonState.Purchasable;
    }

    private void UpdateUnlockButton(WeaponCard card)
    {
        // We want to check to see if the Unlock Button should be deactivated, or the text set to "Buy"/"Equip"
        switch (card.buttonState)
        {
            case UnlockButton.ButtonState.Locked:
                unlockButton.interactable = false;
                unlockButton.GetComponentInChildren<OscilloscopeText>().Text = "Locked";
                break;
            case UnlockButton.ButtonState.Purchasable:
                unlockButton.interactable = true;
                unlockButton.GetComponentInChildren<OscilloscopeText>().Text = "Buy";
                break;
            case UnlockButton.ButtonState.Equippable:
                unlockButton.interactable = true;
                unlockButton.GetComponentInChildren<OscilloscopeText>().Text = "Equip";
                break;
            case UnlockButton.ButtonState.Active:
                unlockButton.interactable = false;
                unlockButton.GetComponentInChildren<OscilloscopeText>().Text = "Equipped";
                break;
        }
    }
}
