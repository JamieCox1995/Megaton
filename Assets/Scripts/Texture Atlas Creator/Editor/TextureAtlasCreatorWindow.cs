using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace TotalDistraction.TextureAtlasCreator
{
    public class TextureAtlasCreatorWindow : EditorWindow
    {
        private const string WindowName = "Texture Atlas";
        private const string MenuItemPath = "Window/Texture Atlas Creator";

        private const int MinimumAtlasSize = 8; // 2 ^ 8 = 256
        private const int MaximumAtlasSize = 13; // 2 ^ 13 = 8192
        private const int AtlasSizeRange = (MaximumAtlasSize - MinimumAtlasSize) + 1;

        private static readonly string[] TextureAtlasSizes = Enumerable.Range(MinimumAtlasSize, AtlasSizeRange).Select(x => string.Format("{0} x {0}", Math.Pow(2, x))).ToArray();
        private static Dictionary<string, Rect> _guiRects;

        private bool _isInitialized;
        private Vector2 _scrollPosition;
        private TextureAtlasBundle _textureAtlasBundle;
        private string _assetPath;
        private bool _shaderReplacementIsExpanded;

        private static Dictionary<Shader, Shader> DefaultShaderReplacements;

        [MenuItem(MenuItemPath)]
        private static void Init()
        {
            var instance = GetWindow<TextureAtlasCreatorWindow>();

            instance.Show();
        }

        private void OnEnable()
        {
            Texture icon = (Texture)EditorGUIUtility.Load("Assets/Scripts/Texture Atlas Creator/Editor/Resources/WindowIcon.png");

            GUIContent titleContent = new GUIContent(WindowName, icon);

            this.titleContent = titleContent;

            if (_guiRects == null) _guiRects = new Dictionary<string, Rect>();

            if (DefaultShaderReplacements == null)
            {
                DefaultShaderReplacements = new Dictionary<Shader, Shader>
                {
                    { Shader.Find("Standard"), Shader.Find("Texture Atlas/Standard") },
                };
            }

            if (!_isInitialized)
            {
                CreateNewBundle();
                _isInitialized = true;
            }

            Repaint();
        }

        private void CreateNewBundle()
        {
            _textureAtlasBundle = CreateInstance<TextureAtlasBundle>();
            _assetPath = string.Empty;
        }

        private void OpenBundle()
        {
            string newPath = EditorUtility.OpenFolderPanel("Open Texture Atlas Bundle", "Assets", "");
            newPath = newPath.Replace(Application.dataPath, "Assets");

            if (string.IsNullOrEmpty(newPath)) return;

            _assetPath = newPath;

            _textureAtlasBundle = AssetDatabase.LoadAssetAtPath<TextureAtlasBundle>(Path.Combine(_assetPath, "TextureAtlasBundle.asset"));
        }

        private void SaveBundle(bool saveAs)
        {
            if (_textureAtlasBundle.Settings.ReplaceOriginalPrefabs)
            {
                if (!EditorUtility.DisplayDialog("Replace Prefabs?",
                    "Having the \"Replace Original Prefabs\" option selected will overwrite the selected prefabs. " +
                    "This operation is not reversible.",
                    "OK",
                    "Cancel")) return;
            }

            if (string.IsNullOrEmpty(_assetPath) || saveAs)
            {
                string newPath = EditorUtility.SaveFolderPanel("Save Texture Atlas Bundle", "Assets", "");
                newPath = newPath.Replace(Application.dataPath, "Assets");

                if (string.IsNullOrEmpty(newPath)) return;

                _assetPath = newPath;
            }

            if (!TextureAtlasCreator.Verify(_textureAtlasBundle)) return;

            TextureAtlas atlas = TextureAtlasCreator.Create(_textureAtlasBundle);

            // save texture
            string texturePath = Path.Combine(_assetPath, "atlas.asset");

            AssetDatabase.CreateAsset(atlas.Texture, texturePath);

            // save materials
            string materialFolderPath = Path.Combine(_assetPath, "Materials");
            if (!AssetDatabase.IsValidFolder(materialFolderPath)) AssetDatabase.CreateFolder(_assetPath, "Materials");

            foreach (Material material in atlas.Materials)
            {
                string materialPath = Path.Combine(materialFolderPath, string.Format("{0}-{1}.mat", material.name, material.GetInstanceID()));
                AssetDatabase.CreateAsset(material, materialPath);
            }

            // save meshes
            if (_textureAtlasBundle.Settings.UVRemapMode == UVRemapMode.BakeIntoMesh)
            {
                string meshFolderPath = Path.Combine(_assetPath, "Meshes");
                if (!AssetDatabase.IsValidFolder(meshFolderPath)) AssetDatabase.CreateFolder(_assetPath, "Meshes");

                foreach (Mesh mesh in atlas.Meshes)
                {
                    string meshPath = Path.Combine(meshFolderPath, string.Format("{0}-{1}.asset", mesh.name, mesh.GetInstanceID()));
                    AssetDatabase.CreateAsset(mesh, meshPath);
                }
            }

            // save prefabs
            if (_textureAtlasBundle.Settings.ReplaceOriginalPrefabs)
            {
                int count = _textureAtlasBundle.BundledObjects.Count;

                for (int i = 0; i < count; i++)
                {
                    PrefabUtility.ReplacePrefab(atlas.Prefabs[i], _textureAtlasBundle.BundledObjects[i], ReplacePrefabOptions.ReplaceNameBased);
                }
            }
            else
            {
                string prefabFolderPath = Path.Combine(_assetPath, "Prefabs");
                if (!AssetDatabase.IsValidFolder(prefabFolderPath)) AssetDatabase.CreateFolder(_assetPath, "Prefabs");

                foreach (GameObject prefab in atlas.Prefabs)
                {
                    string prefabPath = Path.Combine(prefabFolderPath, string.Format("{0}-{1}.prefab", prefab.name, prefab.GetInstanceID()));
                    prefabPath = prefabPath.Replace('\\', '/');
                    PrefabUtility.CreatePrefab(prefabPath, prefab);
                    GameObject.DestroyImmediate(prefab);
                }
            }

            AssetDatabase.CreateAsset(_textureAtlasBundle, Path.Combine(_assetPath, "TextureAtlasBundle.asset"));

            AssetDatabase.Refresh();
        }

        private void OnGUI()
        {
            using (var scope = new EditorGUILayout.ScrollViewScope(_scrollPosition))
            {
                _scrollPosition = scope.scrollPosition;

                DrawToolbar();

                EditorGUILayout.LabelField("Texture Atlas Settings", EditorStyles.boldLabel);
                DrawSettings();

                GUILayout.Space(8);

                EditorGUILayout.LabelField("GameObjects to Bundle", EditorStyles.boldLabel);

                EditorGUI.BeginChangeCheck();
                DrawBundleList();

                if (EditorGUI.EndChangeCheck())
                {
                    List<Shader> shaders = TextureAtlasCreator.GetShaders(_textureAtlasBundle);
                    _textureAtlasBundle.Settings.ShaderReplacements = new Dictionary<Shader, Shader>();

                    foreach (Shader shader in shaders)
                    {
                        if (DefaultShaderReplacements.ContainsKey(shader))
                        {
                            _textureAtlasBundle.Settings.ShaderReplacements.Add(shader, DefaultShaderReplacements[shader]);
                        }
                        else
                        {
                            _textureAtlasBundle.Settings.ShaderReplacements.Add(shader, null);
                        }
                    }
                }
            }
        }

        private void DrawSettings()
        {
            _textureAtlasBundle.Settings.MaximumSize = EditorGUILayout.Popup("Max Size", _textureAtlasBundle.Settings.MaximumSize - MinimumAtlasSize, TextureAtlasSizes) + MinimumAtlasSize;
            _textureAtlasBundle.Settings.KeepAspectRatio = EditorGUILayout.Toggle("Keep Aspect Ratio", _textureAtlasBundle.Settings.KeepAspectRatio);
            _textureAtlasBundle.Settings.ReplaceOriginalPrefabs = EditorGUILayout.Toggle("Replace Original Prefabs", _textureAtlasBundle.Settings.ReplaceOriginalPrefabs);
            _textureAtlasBundle.Settings.UVRemapMode = EnumToggleGroup("UV Remapping Mode", _textureAtlasBundle.Settings.UVRemapMode);

            if (_textureAtlasBundle.Settings.UVRemapMode == UVRemapMode.UseInstancedShaders)
            {
                _textureAtlasBundle.Settings.ShaderReplacements = LookupValues("Shader Replacement Lookup", _textureAtlasBundle.Settings.ShaderReplacements, ref _shaderReplacementIsExpanded);
            }
        }

        private void DrawBundleList()
        {
            for (int i = 0; i < _textureAtlasBundle.BundledObjects.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();

                if (GUILayout.Button("Λ", EditorStyles.miniButtonLeft, GUILayout.ExpandWidth(false)) && i > 0)
                {
                    GameObject temp = _textureAtlasBundle.BundledObjects[i - 1];
                    _textureAtlasBundle.BundledObjects[i - 1] = _textureAtlasBundle.BundledObjects[i];
                    _textureAtlasBundle.BundledObjects[i] = temp;
                }
                else if (GUILayout.Button("V", EditorStyles.miniButtonRight, GUILayout.ExpandWidth(false)) && i < _textureAtlasBundle.BundledObjects.Count - 1)
                {
                    GameObject temp = _textureAtlasBundle.BundledObjects[i + 1];
                    _textureAtlasBundle.BundledObjects[i + 1] = _textureAtlasBundle.BundledObjects[i];
                    _textureAtlasBundle.BundledObjects[i] = temp;
                }

                _textureAtlasBundle.BundledObjects[i] = (GameObject)EditorGUILayout.ObjectField(_textureAtlasBundle.BundledObjects[i], typeof(GameObject), false);

                if (GUILayout.Button("X", EditorStyles.miniButton, GUILayout.ExpandWidth(false)))
                {
                    _textureAtlasBundle.BundledObjects.RemoveAt(i);
                    i--;
                }

                EditorGUILayout.EndHorizontal();
            }

            if (GUILayout.Button("+", EditorStyles.miniButton))
            {
                _textureAtlasBundle.BundledObjects.Add((GameObject)EditorGUILayout.ObjectField(null, typeof(GameObject), false));
            }

            IEnumerable<GameObject> dropped = DrawDropZone<GameObject>("Drop prefabs here.");

            if (dropped != null)
            {
                _textureAtlasBundle.BundledObjects.AddRange(dropped);
            }
        }

        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            if (EditorGUILayout.DropdownButton(new GUIContent("File"), FocusType.Keyboard, EditorStyles.toolbarDropDown))
            {
                GenericMenu menu = new GenericMenu();

                menu.AddItem(new GUIContent("New"), false, CreateNewBundle);
                menu.AddItem(new GUIContent("Open..."), false, OpenBundle);
                menu.AddItem(new GUIContent("Save"), false, () => SaveBundle(false));
                
                if (string.IsNullOrEmpty(_assetPath))
                {
                    menu.AddDisabledItem(new GUIContent("Save As..."));
                }
                else
                {
                    menu.AddItem(new GUIContent("Save As..."), false, () => SaveBundle(true));
                }

                menu.DropDown(_guiRects["FileMenu"]);
            }
            else
            {
                UpdateGuiRect("FileMenu");
            }

            GUILayout.FlexibleSpace();

            EditorGUILayout.EndHorizontal();
        }

        private IEnumerable<T> DrawDropZone<T>(string title) where T : UnityEngine.Object
        {
            GUILayout.Box(title, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));

            EventType eventType = Event.current.type;
            bool isAccepted = false;

            if (eventType == EventType.DragUpdated || eventType == EventType.DragPerform)
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                if (eventType == EventType.DragPerform)
                {
                    DragAndDrop.AcceptDrag();
                    isAccepted = true;
                }
                Event.current.Use();
                GUI.changed = true;
            }

            return isAccepted ? DragAndDrop.objectReferences.OfType<T>() : null;
        }

        private TEnum EnumToggleGroup<TEnum>(string label, TEnum value) where TEnum : struct
        {
            if (!typeof(TEnum).IsEnum) throw new InvalidOperationException("The type parameter TEnum must be an enumerated type.");

            GUIStyle leftDefault = EditorStyles.miniButtonLeft;
            GUIStyle leftHighlighted = new GUIStyle(EditorStyles.miniButtonLeft);
            leftHighlighted.normal.background = leftHighlighted.active.background;

            GUIStyle middleDefault = EditorStyles.miniButtonMid;
            GUIStyle middleHighlighted = new GUIStyle(EditorStyles.miniButtonMid);
            middleHighlighted.normal.background = middleHighlighted.active.background;

            GUIStyle rightDefault = EditorStyles.miniButtonRight;
            GUIStyle rightHighlighted = new GUIStyle(EditorStyles.miniButtonRight);
            rightHighlighted.normal.background = rightHighlighted.active.background;

            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.PrefixLabel(label);

            TEnum[] values = Enum.GetValues(typeof(TEnum)).Cast<TEnum>().ToArray();

            TEnum result = value;

            for (int i = 0; i < values.Length; i++)
            {
                bool isToggled = Equals(value, values[i]);
                bool isLeftStyle = i == 0;
                bool isRightStyle = i == values.Length - 1;

                GUIStyle style;

                if (isLeftStyle)
                {
                    style = isToggled ? leftHighlighted : leftDefault;
                }
                else if (isRightStyle)
                {
                    style = isToggled ? rightHighlighted : rightDefault;
                }
                else
                {
                    style = isToggled ? middleHighlighted : middleDefault;
                }

                if (GUILayout.Button(ObjectNames.NicifyVariableName(values[i].ToString()), style))
                {
                    result = values[i];
                }
            }

            EditorGUILayout.EndHorizontal();

            return result;
        }

        private Dictionary<TKey, TValue> LookupValues<TKey, TValue>(string label, Dictionary<TKey, TValue> value, ref bool foldout) where TValue : UnityEngine.Object
        {
            if (EditorGUILayout.Foldout(foldout, label))
            {
                EditorGUI.indentLevel++;

                List<TKey> keys = value.Keys.ToList();

                bool keyIsUnityObject = typeof(UnityEngine.Object).IsAssignableFrom(typeof(TKey));

                Dictionary<TKey, TValue> result = new Dictionary<TKey, TValue>();

                foreach (TKey key in keys)
                {
                    EditorGUILayout.BeginHorizontal();

                    GUIContent itemLabel = new GUIContent(keyIsUnityObject ? (key as UnityEngine.Object).name : key.ToString(), keyIsUnityObject ? (key as UnityEngine.Object).name : key.ToString());

                    TValue val = (TValue)EditorGUILayout.ObjectField(itemLabel, value[key], typeof(TValue), false);

                    result.Add(key, val);

                    EditorGUILayout.EndHorizontal();
                }

                EditorGUI.indentLevel--;

                foldout = true;

                return result;
            }
            else
            {
                foldout = false;

                return value;
            }
        }

        private void UpdateGuiRect(string key)
        {
            if (Event.current.type == EventType.Repaint)
            {
                Rect value = GUILayoutUtility.GetLastRect();

                if (_guiRects.ContainsKey(key))
                {
                    _guiRects[key] = value;
                }
                else
                {
                    _guiRects.Add(key, value);
                }
            }
        }
    }
}