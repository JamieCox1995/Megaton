using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Steamworks;

public class AchievementManager : MonoBehaviour
{
    public static AchievementManager _instance;

    // This stores the AppID of the game
    private CGameID m_gameID;

    // Recording whether we retreived stats from Steam, or if we want to store stats this frame
    private bool m_requestedStats;
    private bool m_statsValid;
    private bool m_storeStats;

    #region Stats
    // Storing stats for this session

    // Storing persistent stats
    private int numBronze;
    private int numSilver;
    private int numGold;
    private int numPlatinum;

    #endregion

    protected Callback<UserStatsReceived_t> m_recievedUserStats;
    protected Callback<UserStatsStored_t> m_storedUserStats;
    protected Callback<UserAchievementStored_t> m_storedAchievement;

    private bool registeredEvents;

    [RuntimeInitializeOnLoadMethod]
    private static void OnLoad()
    {
        if (_instance == null)
        {
            _instance = new GameObject("AchievementManager").AddComponent<AchievementManager>();
            _instance.OnEnable();
        }
    }

    private void OnEnable()
    {

        // First check to see if the SteamManager has successfully been initialized
        if (!SteamManager.Initialized)
        {
            return;
        }

        // Getting the AppID for the game to be used in callbacks
        m_gameID = new CGameID(SteamUtils.GetAppID());

        // We need to set up and register the methods for the callbacks
        m_recievedUserStats = Callback<UserStatsReceived_t>.Create(OnUserStatsReceived);
        m_storedUserStats = Callback<UserStatsStored_t>.Create(OnUserStatsStored);
        m_storedAchievement = Callback<UserAchievementStored_t>.Create(OnAchievementStored);

        m_requestedStats = false;
        m_statsValid = false;
            
    }

    

    private void Update()
    {
        if (!SteamManager.Initialized) return;

        if (!registeredEvents && GameEventManager.instance != null) GameEventManager.instance.onStatAchieved += CheckStatAchievements;

        if (!m_requestedStats)
        {
            if (!SteamManager.Initialized) m_requestedStats = true;

            bool success = SteamUserStats.RequestCurrentStats();
            m_requestedStats = success;
        }

        if (!m_statsValid) return;

        // Now we want to check to see if there are any stats that need to be stored
        if (m_storeStats)
        {
            // Here we want to set the game-stats before we call StoreStats();
            SteamUserStats.SetStat("NumBronze", numBronze);
            SteamUserStats.SetStat("NumSilver", numSilver);
            SteamUserStats.SetStat("NumGold", numGold);
            SteamUserStats.SetStat("NumPlatinum", numPlatinum);

            bool success = SteamUserStats.StoreStats();

            m_storeStats = !success;
        }
    }

    #region Unlocking an Achievement
    /// <summary>
    /// Unlocks an achievement.
    /// </summary>
    /// <param name="ach"></param>
    public void UnlockAchievement(Achievement achievement)
    {
        achievement.m_achieved = true;

        SteamUserStats.SetAchievement(achievement.m_achievementID.ToString());

        m_storeStats = true;
    }

    /// <summary>
    /// Unlock an achievement.
    /// </summary>
    /// <param name="type"> The ID of the achievement to be unlocked.</param>
    public void UnlockAchievement(AchievementType type)
    {
        Achievement ach = m_Achievements.First(t => t.m_achievementID == type);

        UnlockAchievement(ach);
    }
    #endregion

    public void CheckMedalAchievements()
    {
        // We want to find the total number of each medal
        numBronze = CheckMedalCount(1);
        numSilver = CheckMedalCount(2);
        numGold = CheckMedalCount(3);
        numPlatinum = CheckMedalCount(4);

        m_storeStats = true;
    }

    public void CheckIfPlayerBroke()
    {
        decimal thresehold = -10000000;              // This is the value at which the "Broke the Bank Achievement is triggered" 

        if (PlayerProgression._instance.savedProgression.playerMoney <= thresehold)
        {
            UnlockAchievement(AchievementType.ACH_BE_BROKE);
        }
    }

    private void CheckStatAchievements (StatAchievedEvent requirementEvent)
    {
        UnlockAchievement((AchievementType)requirementEvent.id);
    }

    private int CheckMedalCount(int medalIndex)
    {
        int toReturn = 0;

        List<MapResults> mapResults = PlayerProgression._instance.savedProgression.mapResults;

        if (mapResults.Count == 0) return 0;

        for (int i = 0; i < mapResults.Count; i++)
        {
            if (mapResults[i].bestMedalAchieved >= medalIndex)
            {
                toReturn++;
            }
        }

        return toReturn;
    }

    private void OnUserStatsReceived(UserStatsReceived_t callback)
    {
        if (!SteamManager.Initialized) return;

        if ((ulong)m_gameID == callback.m_nGameID)
        {
            if (EResult.k_EResultOK == callback.m_eResult)
            {
                m_statsValid = true;

                // loading the achievements
                foreach(Achievement ach in m_Achievements)
                {
                    bool retrieved = SteamUserStats.GetAchievement(ach.m_achievementID.ToString(), out ach.m_achieved);

                    if (retrieved)
                    {
                        ach.m_name = SteamUserStats.GetAchievementDisplayAttribute(ach.m_achievementID.ToString(), "name");
                        ach.m_description = SteamUserStats.GetAchievementDisplayAttribute(ach.m_achievementID.ToString(), "desc");
                    }
                    else
                    {
                        Debug.LogWarning("Achievement " + ach.m_achievementID.ToString() + " is not registered with Steamworks");
                    }
                }

                // loading the player's stats
                SteamUserStats.GetStat("NumBronze", out numBronze);
            }
        }
    }

    private void OnUserStatsStored(UserStatsStored_t callback)
    {
        if ((ulong)m_gameID == callback.m_nGameID)
        {
            if (EResult.k_EResultOK == callback.m_eResult)
            {
                Debug.Log("StoreStats - success");
            }
            else if (EResult.k_EResultInvalidParam == callback.m_eResult)
            {
                // One or more stats we set broke a constraint. They've been reverted,
                // and we should re-iterate the values now to keep in sync.
                Debug.Log("StoreStats - some failed to validate");
                // Fake up a callback here so that we re-load the values.
                UserStatsReceived_t pCallback = new UserStatsReceived_t();
                callback.m_eResult = EResult.k_EResultOK;
                callback.m_nGameID = (ulong)m_gameID;
                OnUserStatsReceived(pCallback);
            }
        }
    }

    private void OnAchievementStored(UserAchievementStored_t callback)
    {
        if ((ulong)m_gameID == callback.m_nGameID)
        {
            if (0 == callback.m_nMaxProgress)
            {
                Debug.Log(callback.m_rgchAchievementName + " unlocked!");
            }
            else
            {

            }
        }
    }

    private static Achievement[] GetAchievements()
    {
        List<Achievement> ach = new List<Achievement>();

        foreach(AchievementType id in Enum.GetValues(typeof(AchievementType)))
        {
            ach.Add(new Achievement(id));
        }

        return ach.ToArray();
    }

    // This is the array which will create and hold all of the achievement information
    private Achievement[] m_Achievements = GetAchievements();
}

// Here we are creating an enumeration for our Achievements
public enum AchievementType : int
{
    #region Medal Achievements
    // These are the achievements to be earned by earning one of each medal type
    ACH_EARN_ONE_BRONZE,
    ACH_EARN_ONE_SILVER,
    ACH_EARN_ONE_GOLD,
    ACH_EARN_ONE_PLATINUM,

    // These are for earning each medal type on all levels
    ACH_EARN_ALL_BRONZE,
    ACH_EARN_ALL_SILVER,
    ACH_EARN_ALL_GOLD,
    ACH_EARN_ALL_PLATINUM,
    #endregion

    #region Gameplay Achievements
    ACH_DAMAGE_LOTS_BUILDINGS,              // Earned by causing destroying a large number of buildings in a single shot
    ACH_ONE_MINUTE_FLIGHT,                  // Earned by keeping the Homing projectile in the air for longer than 1 minute
    ACH_BE_BROKE,                           // This is a joke achievement given to the player when they reach -1,000,000 USD
    #endregion

    // These are used unlocking pick-up projectiles
    #region Level/Unlock Achievements
    ACH_UNLOCK_NUKE,

    #endregion
}

[Serializable]
// The class which stores information about an achievement such as the ID, name, description, and achieved state
public class Achievement
{
    public AchievementType m_achievementID;
    public string m_name;
    public string m_description;
    public bool m_achieved;

    public Achievement(AchievementType id)
    {
        m_achievementID = id;
        m_name = string.Empty;
        m_description = string.Empty;
        m_achieved = false;
    }

    public Achievement(AchievementType id, string name, string desc)
    {
        m_achievementID = id;
        m_name = name;
        m_description = desc;
        m_achieved = false;
    }
}

