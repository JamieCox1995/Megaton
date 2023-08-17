using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProgressionStatManager : MonoBehaviour
{
    public static ProgressionStatManager _instance;

    [SerializeField] private List<StatDescriptor> statDescriptions = new List<StatDescriptor>();
    [SerializeField] private List<ProgressionStat> allGameStats = new List<ProgressionStat>();

    private bool eventRegistered = false;

	// Use this for initialization
	void Start ()
    {
        if (_instance == null)
        {
            _instance = this;
        }

        DontDestroyOnLoad(this);

        RegisterEvent();

        LoadAchievedStats();
	}

    private void Update()
    {
        if (eventRegistered == false)
        {
            RegisterEvent();
        }
    }

    private void RegisterEvent()
    {
        if (eventRegistered == false)
        {
            if (GameEventManager.instance != null)
            {
                Debug.Log("Registering to OnStatAchieved event");
                GameEventManager.instance.onStatAchieved += UnlockProgressionStat;

                eventRegistered = true;
            }
        }
    }

    private void LoadAchievedStats()
    {
        List<ProgressionStat> achieved = PlayerProgression._instance.savedProgression.completedEvents;

        foreach(ProgressionStat stat in achieved)
        {
            allGameStats.First(s => s.id == stat.id).achieved = true;
        }
    }

    private void UnlockProgressionStat(StatAchievedEvent statEvent)
    {
        int id = statEvent.id;

        // Selecting the stat we want to unlock
        ProgressionStat stat = allGameStats.FirstOrDefault(s => s.id == id);

        if (stat == null)
        {
            Debug.LogErrorFormat("Stat {0} does not exist in the game. Please consider adding it!", stat.id);
            return;
        }

        if (!stat.achieved)
        {
            // Setting the achieved stat to true, and adding it to the saved progression

            Debug.LogFormat("Unlocking StatID: {0}, {1}", id, GetDescription(id));

            stat.achieved = true;

            PlayerProgression._instance.savedProgression.completedEvents.Add(stat);
            PlayerProgression.SaveProgress();
        }
    }

    public string GetDescription(int id)
    {
        string desc = string.Empty;
            
        desc = statDescriptions.FirstOrDefault(d => d.statID == id).description;

        Debug.LogFormat("desc = {0}", desc);

        if (desc == string.Empty || desc == null)
        {
            Debug.LogErrorFormat("There was no description found for the StatID: {0}", id);

            desc = "None.";
        }

        return desc;
    }
}

[System.Serializable]
public class StatDescriptor
{
    public int statID;
    public string description = " ";
}
