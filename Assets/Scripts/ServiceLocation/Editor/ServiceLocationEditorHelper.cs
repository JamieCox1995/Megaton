using System.IO;
using System.Text;
using UnityEngine;
using UnityEditor;
using UnityEditor.ProjectWindowCallback;

namespace TotalDistraction.ServiceLocation.Editor
{
    /// <summary>
    /// Provides menu items in the Assets/Create menu.
    /// </summary>
    public static class ServiceLocationEditorHelper
    {
        private static readonly string DefaultServiceName = "NewUnityService";
        private static readonly string DefaultStartupScriptName = "Startup";
        private static readonly string ScriptExtension = ".cs";
        private static readonly string InterfacePrefix = "I";
        private static readonly string ConfigurationSuffix = "Configuration";

        private static TextAsset InterfaceTemplate;
        private static TextAsset MonoBehaviourTemplate;
        private static TextAsset StandardCsTemplate;
        private static TextAsset StartupScriptTemplate;
        private static TextAsset ServiceConfigurationTemplate;

        [MenuItem("Assets/Create/Service Locator/Service", false, 0)]
        private static void CreateStandardCsService()
        {
            if (InterfaceTemplate == null) InterfaceTemplate = Resources.Load<TextAsset>("_IUnityService.cs");
            if (StandardCsTemplate == null) StandardCsTemplate = Resources.Load<TextAsset>("_UnityService_Standard.cs");

            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(
                0,
                ScriptableObject.CreateInstance<DoCreateStandardService>(),
                DefaultServiceName + ScriptExtension,
                (Texture2D)EditorGUIUtility.IconContent("cs Script Icon").image,
                null);
        }

        [MenuItem("Assets/Create/Service Locator/MonoBehaviour Service", false, 1)]
        private static void CreateMonoBehaviourService()
        {
            if (InterfaceTemplate == null) InterfaceTemplate = Resources.Load<TextAsset>("_IUnityService.cs");
            if (MonoBehaviourTemplate == null) MonoBehaviourTemplate = Resources.Load<TextAsset>("_UnityService_MonoBehaviour.cs");

            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(
                0,
                ScriptableObject.CreateInstance<DoCreateMonoBehaviourService>(),
                DefaultServiceName + ScriptExtension,
                (Texture2D)EditorGUIUtility.IconContent("cs Script Icon").image,
                null);
        }

        [MenuItem("Assets/Create/Service Locator/Service Configuration", false, 2)]
        private static void CreateServiceConfiguration()
        {
            if (ServiceConfigurationTemplate == null) ServiceConfigurationTemplate = Resources.Load<TextAsset>("_ServiceConfiguration.cs");

            string resourcePath = AssetDatabase.GetAssetPath(ServiceConfigurationTemplate);

            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(
                0,
                ScriptableObject.CreateInstance<DoCreateStartupScript>(),
                DefaultServiceName + ConfigurationSuffix + ScriptExtension,
                (Texture2D)EditorGUIUtility.IconContent("cs Script Icon").image,
                resourcePath);
        }

        [MenuItem("Assets/Create/Service Locator/Startup Script", false, 100)]
        private static void CreateStartupScript()
        {
            if (StartupScriptTemplate == null) StartupScriptTemplate = Resources.Load<TextAsset>("_Startup.cs");

            string resourcePath = AssetDatabase.GetAssetPath(StartupScriptTemplate);

            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(
                0,
                ScriptableObject.CreateInstance<DoCreateStartupScript>(),
                DefaultStartupScriptName + ScriptExtension,
                (Texture2D)EditorGUIUtility.IconContent("cs Script Icon").image,
                resourcePath);
        }

        private static UnityEngine.Object CreateScriptAssetFromTemplate(string pathName, string resourceFile)
        {
            string text = File.ReadAllText(resourceFile);
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(pathName);

            string niceName = ObjectNames.NicifyVariableName(fileNameWithoutExtension);

            string scriptName = fileNameWithoutExtension.Replace(" ", "");

            text = text.Replace("#SCRIPTNAME#", scriptName);
            text = text.Replace("#NICENAME#", niceName);

            return CreateScriptAssetWithContent(pathName, text);
        }

        private static UnityEngine.Object CreateScriptAssetWithContent(string pathName, string templateContent)
        {
            string fullPath = Path.GetFullPath(pathName);
            UTF8Encoding encoding = new UTF8Encoding(true);
            File.WriteAllText(fullPath, templateContent, encoding);
            AssetDatabase.ImportAsset(pathName);
            return AssetDatabase.LoadAssetAtPath(pathName, typeof(UnityEngine.Object));
        }

        private static string FormatTemplate(TextAsset template, params object[] args)
        {
            return string.Format(template.text, args);
        }

        private class DoCreateMonoBehaviourService : EndNameEditAction
        {
            public override void Action(int instanceId, string pathName, string resourceFile)
            {
                string interfaceTemplateLocation = AssetDatabase.GetAssetPath(InterfaceTemplate);
                string classTemplateLocation = AssetDatabase.GetAssetPath(MonoBehaviourTemplate);

                string interfacePath = Path.Combine(Path.GetDirectoryName(pathName), InterfacePrefix + Path.GetFileName(pathName));

                UnityEngine.Object interfaceAsset = CreateScriptAssetFromTemplate(interfacePath, interfaceTemplateLocation);
                UnityEngine.Object classAsset = CreateScriptAssetFromTemplate(pathName, classTemplateLocation);
                ProjectWindowUtil.ShowCreatedAsset(classAsset);
                ProjectWindowUtil.ShowCreatedAsset(interfaceAsset);
            } 
        }

        private class DoCreateStandardService : EndNameEditAction
        {
            public override void Action(int instanceId, string pathName, string resourceFile)
            {
                string interfaceTemplateLocation = AssetDatabase.GetAssetPath(InterfaceTemplate);
                string classTemplateLocation = AssetDatabase.GetAssetPath(StandardCsTemplate);

                string interfacePath = Path.Combine(Path.GetDirectoryName(pathName), InterfacePrefix + Path.GetFileName(pathName));

                UnityEngine.Object interfaceAsset = CreateScriptAssetFromTemplate(interfacePath, interfaceTemplateLocation);
                UnityEngine.Object classAsset = CreateScriptAssetFromTemplate(pathName, classTemplateLocation);
                ProjectWindowUtil.ShowCreatedAsset(classAsset);
                ProjectWindowUtil.ShowCreatedAsset(interfaceAsset);
            }
        }

        private class DoCreateStartupScript : EndNameEditAction
        {
            public override void Action(int instanceId, string pathName, string resourceFile)
            {
                UnityEngine.Object scriptAsset = CreateScriptAssetFromTemplate(pathName, resourceFile);
                ProjectWindowUtil.ShowCreatedAsset(scriptAsset);
            }
        }

        private class DoCreateServiceConfiguration : EndNameEditAction
        {
            public override void Action(int instanceId, string pathName, string resourceFile)
            {
                UnityEngine.Object scriptAsset = CreateScriptAssetFromTemplate(pathName, resourceFile);
                ProjectWindowUtil.ShowCreatedAsset(scriptAsset);
            }
        }
    }
}
