using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TotalDistraction.ServiceLocation;
using TotalDistraction.ServiceLocation.DefaultServices;
using TeamUtility.IO;

public class ControlValidator : MonoBehaviour
{
	// Use this for initialization
	void Start ()
    {
        string dir = System.Environment.ExpandEnvironmentVariables("%USERPROFILE%\\Documents\\Megaton");

        // Here we are checking to see if the folder containing the controls is present
        if (!Directory.Exists(dir))
        {
            // If not, we are going to create the directory and save the input config
            Directory.CreateDirectory(dir);

            ServiceLocator.Get<IInputManager>().Save(dir + "\\input_config.xml");

            // Return as we do not need to validate the control
            return;
        }

        ValidateControls();	
	}
	
    private void ValidateControls()
    {
        IInputManager input = ServiceLocator.Get<IInputManager>();
        string dataPath = System.Environment.ExpandEnvironmentVariables("%USERPROFILE%\\Documents\\Megaton");

        if (input != null)
        {

            // First we want to load the player's saved Input Configuration
            SaveLoadParameters players;
            InputLoaderXML loader = new InputLoaderXML(dataPath + "\\input_config.xml");

            // We create a copy of the players control config so that any custom key bindings can be carried into the
            // updated version.
            players = loader.Load();

            // This stores all of the Control Configurations and Axes which the game has set up
            List<string> allGameConfigs = new List<string>();
            
            // Iterating over all of the different configurations
            for (int config = 0; config < input.Instance.inputConfigurations.Count; config++)
            {
                // Iterating over all of the axes in the current control configuration
                for (int axis = 0; axis < input.Instance.inputConfigurations[config].axes.Count; axis++)
                {
                    // Creating a string that will store all of the axes and the corresponding configuration they are paired with
                    allGameConfigs.Add(input.Instance.inputConfigurations[config].axes[axis].name + "*" + input.Instance.inputConfigurations[config].name);
                }
            }

            // Sorting the list alphabetically
            allGameConfigs.Sort();

            // Here we do the same as above, but instead for the controls loaded from the player's control XML
            List<string> playersConfigs = new List<string>();

            for (int config = 0; config < players.inputConfigurations.Count; config++)
            {
                for (int axis = 0; axis < players.inputConfigurations[config].axes.Count; axis++)
                {
                    playersConfigs.Add(players.inputConfigurations[config].axes[axis].name + "*" + players.inputConfigurations[config].name);
                }
            }

            playersConfigs.Sort();

            // Before we do any comparisons, we check to see if the 2 lists are exactly the same.
            // If they are, that means that the player's control config is up-to-date and we return out of the method.
            if (allGameConfigs.SequenceEqual(playersConfigs))
            {
                return;
            }

            // Getting the strings which appear in the games controls, but not in the players.
            // These axes will be added to the players config.
            IEnumerable<string> difference = allGameConfigs.Except(playersConfigs);

            // Iterating over each of the strings which did not appear in both lists.
            foreach (string config in difference)
            {
                // Spliting the stored string into an axis name, and a configuration name.
                string[] axisconfig = config.Split('*');

                // Getting the input configuration the axis belongs to in the game's controls
                InputConfiguration needed = input.Instance.inputConfigurations.First(c => axisconfig[1] == c.name);

                // Getting the correct axis from the games configuration
                AxisConfiguration configAS = needed.axes.FirstOrDefault(a => axisconfig[0] == a.name);

                // Adding the axis to the configuration in the players config.
                players.inputConfigurations.First(c => axisconfig[1] == c.name).axes.Add(configAS);
            }

            // Here we're applying the controls in the player's config to the games controls. This is incase the player had
            // any custom controls that need to be kept.
            for (int i = 0; i < input.Instance.inputConfigurations.Count; i++)
            {
                input.Instance.inputConfigurations[i] = players.inputConfigurations[i];
            }

            // Finally we tell the ServiceLocator.Get<IInputProxyService>() to save the controls to the XML so that the saved version is up-to-date.
            input.Save(dataPath + "\\input_config.xml");
        }
    }
}
