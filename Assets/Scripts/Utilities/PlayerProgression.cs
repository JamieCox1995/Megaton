using System.IO;
using System.Linq;
using System.Collections;
using System.Xml.Serialization;
using System.Collections.Generic;
using UnityEngine;

public class PlayerProgression : MonoBehaviour
{
    public static PlayerProgression _instance;

    public Progression savedProgression;

    private string dataPath = System.Environment.ExpandEnvironmentVariables("%USERPROFILE%\\Documents\\Megaton");

    // Use this for initialization
    void Start ()
    {
        DontDestroyOnLoad(this);	
	}

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }

        savedProgression = LoadProgress();

        if (savedProgression.unlockedProjectiles.Count == 0)
        {
            Debug.Log("Progress does not contain any information about unlocked projectiles, adding default unlocks");

            savedProgression.unlockedProjectiles = new List<string> { "Standard", "Homing" };

            SaveProgress();
        }

        if (savedProgression.unlockedRegions.Count == 0)
        {
            savedProgression.unlockedRegions = new List<string> { "Region 1" };

            SaveProgress();
        }

        if (savedProgression.completedEvents == null)
        {
            savedProgression.completedEvents = new List<ProgressionStat>();
            SaveProgress();
        }
    }

    public void UpdateScore(float scoreToAdd)
    {
        decimal newScore = savedProgression.playerMoney + (decimal)scoreToAdd;

        savedProgression.playerMoney = newScore;

        // Save
        SaveProgress();
    } 

    public decimal GetScore()
    {
        return savedProgression.playerMoney;
    }

    public void SetEquippedProjectile(ProjectileType type, string name)
    {
        if (type == ProjectileType.Primary)
        {
            savedProgression.primaryProjectile = name;
        }
        else if (type == ProjectileType.Pickup)
        {
            savedProgression.pickupProjectile = name;
        }

        // Now we want to call Save
        SaveProgress();
    }

    public string GetEquippedProjectile(ProjectileType type)
    {
        if (type == ProjectileType.Primary)
        {
            return savedProgression.primaryProjectile;
        }
        else
        {
            return savedProgression.pickupProjectile;
        }
    }

    public void AddNewUnlock(string newUnlock, float cost)
    {
        //if (!savedProgression.unlockedProjectiles.Contains(newUnlock))
        //{
            savedProgression.unlockedProjectiles.Add(newUnlock);

            decimal newScore = savedProgression.playerMoney - (decimal)cost;

            savedProgression.playerMoney = newScore;

            SaveProgress();
        //}
    }

    public void SetLevelResults(string levelName, float score, int medalAchieved)
    {
        MapResults toUpdate = savedProgression.mapResults.SingleOrDefault(r => r.levelName == levelName);

        if (toUpdate != null)
        {
            MapResults old = toUpdate;

            // Now we shall check to see if the we should update the score
            toUpdate.highestScore = Mathf.Max(toUpdate.highestScore, score);
            toUpdate.bestMedalAchieved = Mathf.Max(toUpdate.bestMedalAchieved, medalAchieved);

            savedProgression.mapResults[savedProgression.mapResults.IndexOf(old)] = toUpdate;
        }
        else
        {
            // Add the level and the results to the dictionary
            MapResults resultsToSave = new MapResults(levelName, score, medalAchieved);

            savedProgression.mapResults.Add(resultsToSave);
        }

        // Save
        SaveProgress();
    }

    public MapResults GetMapResults(string levelName)
    {
        for(int i = 0; i < savedProgression.mapResults.Count; i++)
        {
            if (savedProgression.mapResults[i].levelName == levelName)
            {
                return savedProgression.mapResults[i];
            }
        }

        return null;
    }

    public static void SaveProgress()
    {

        if (File.Exists(_instance.dataPath + "\\Progression.xml"))
        {
            FileInfo i = new FileInfo(_instance.dataPath + "\\Progression.xml");
            i.Attributes = FileAttributes.Normal;
        }

        XmlSerializer serial = new XmlSerializer(typeof(Progression));
        FileStream stream = new FileStream(_instance.dataPath + "\\Progression.xml", FileMode.Create);

        serial.Serialize(stream, _instance.savedProgression);
        stream.Close();

        FileInfo info = new FileInfo(_instance.dataPath + "\\Progression.xml");
        info.Attributes = FileAttributes.Hidden;
    }

    public static Progression LoadProgress()
    {
        if (!File.Exists(_instance.dataPath + "\\Progression.xml"))
        {
            SaveProgress();
            return _instance.savedProgression;
        }

        Progression progress;

        XmlSerializer serial = new XmlSerializer(typeof(Progression));
        FileStream stream = new FileStream(_instance.dataPath + "\\Progression.xml", FileMode.Open);

        progress = serial.Deserialize(stream) as Progression;
        stream.Close();

        if (progress.completedEvents == null)
        {
            progress.completedEvents = new List<ProgressionStat>();
        }

        return progress;
    }
}

[System.Serializable]
public class Progression
{
    public decimal playerMoney = 0;
     
    public string primaryProjectile = "Standard";
    public string pickupProjectile = "Homing";

    /// <summary>
    /// Stores a list of the names of projectiles we have unlocked. Starts with Standard and Homing already unlocked.
    /// </summary>
    public List<string> unlockedProjectiles;

    /// <summary>
    /// This will be used to store all of the regions the player has unlocked
    /// </summary>
    public List<string> unlockedRegions;

    /// <summary>
    /// Storing the region the player is currently in. This will be used in the future to apply travel costs to
    /// switching to a new region
    /// </summary>
    public int currentRegion;

    /// <summary>
    /// Stores a list of the maps the player has completed
    /// </summary>
    public List<MapResults> mapResults = new List<MapResults>();

    public List<ProgressionStat> completedEvents;
}

public enum ProjectileType
{
    Primary, Pickup
}

[System.Serializable]
public class MapResults
{
    public string levelName;

    public float highestScore;
    public int bestMedalAchieved;

    public MapResults()
    {

    }

    public MapResults(string name, float score, int medal)
    {
        levelName = name;
        highestScore = score;
        bestMedalAchieved = medal;
    }
}

[System.Serializable]
public class ProgressionStat
{
    public int id = -1;
    public bool achieved = false;

    public ProgressionStat()
    {

    }

    public ProgressionStat(int id)
    {
        this.id = id;
    }

    public void SetStatAchieved()
    {
        achieved = true;
    }
}