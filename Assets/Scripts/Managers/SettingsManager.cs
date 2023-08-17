using System.IO;
using System.Collections;
using System.Xml.Serialization;
using System.Collections.Generic;
using UnityEngine;

public class SettingsManager : MonoBehaviour {

    public static SettingsManager _instance;

    public GameSettings gameSettings;

    private string dataPath = System.Environment.ExpandEnvironmentVariables("%USERPROFILE%\\Documents\\Megaton");


    // Use this for initialization
    void Start ()
    {
        DontDestroyOnLoad(this);
	}

    private void Awake()
    {
        if(_instance != null)
        {
            return;
        }

        _instance = this;


        if (File.Exists(_instance.dataPath + "\\config.xml"))
        {
            gameSettings = Load();
        }
        else
        {
            GameSettings newGS = new GameSettings();

            gameSettings = newGS;

            Save(newGS);
        }
        
    }

    public static void Save()
    {
        XmlSerializer serial = new XmlSerializer(typeof(GameSettings));
        FileStream stream = new FileStream(_instance.dataPath + "\\config.xml", FileMode.Create);

        serial.Serialize(stream, _instance.gameSettings);
        stream.Close();
    }

    public static void Save(GameSettings toSave)
    {
        XmlSerializer serial = new XmlSerializer(typeof(GameSettings));
        FileStream stream = new FileStream(_instance.dataPath + "\\config.xml", FileMode.Create);

        serial.Serialize(stream, toSave);
        stream.Close();
    }

    public static GameSettings Load()
    {
        // Load config.xml
        XmlSerializer serial = new XmlSerializer(typeof(GameSettings));
        FileStream stream = new FileStream(_instance.dataPath + "\\config.xml", FileMode.OpenOrCreate);

        GameSettings _gameSettings = serial.Deserialize(stream) as GameSettings;
        stream.Close();

        return _gameSettings;
    }

    public static ProjectileDatabase LoadProjectileDatabase()
    {
        ProjectileDatabase db;

        XmlSerializer serial = new XmlSerializer(typeof(ProjectileDatabase));
        FileStream stream = new FileStream(Application.dataPath + "/Resources/ProjectileDatabase.xml", FileMode.OpenOrCreate);

        db = serial.Deserialize(stream) as ProjectileDatabase;
        stream.Close();

        return db;
    }
}
