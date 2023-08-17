using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

public class FindGameObjectsEditorWindow : EditorWindow
{
    private const string WindowName = "Find GameObjects";
    private const string MenuItemPath = "Window/" + WindowName;
    private const string ButtonText = WindowName;
    private const string ObjectsSelectedHelpStringFormat = "{0} objects selected.";
    private string _helpString;
    private MessageType _helpMessageType;

    private FindByOptions _findByOptions;
    private FindBySettings _findBySettings;

    enum FindByOptions
    {
        None,
        InvalidTransform,
        DistanceFromPoint,
        SmallRigidbodyMass,
        FastRigidbody,
        MultipleMonobehaviourCopies,
    }

    abstract class FindBySettings
    {
        public abstract bool Evaluate(GameObject obj);
        public virtual void OnGUI() { }
    }

    class InvalidTransformSettings : FindBySettings
    {
        public override bool Evaluate(GameObject obj)
        {
            return IsNaN(obj.transform.position) || IsNaN(obj.transform.rotation) || IsNaN(obj.transform.localScale);
        }

        bool IsNaN(Vector3 v)
        {
            return float.IsNaN(v.x) || float.IsNaN(v.y) || float.IsNaN(v.z);
        }

        bool IsNaN(Quaternion q)
        {
            return float.IsNaN(q.w) || float.IsNaN(q.x) || float.IsNaN(q.y) || float.IsNaN(q.z);
        }
    }

    class SmallRigidbodiesSettings : FindBySettings
    {
        public float LessThanMass;

        public override bool Evaluate(GameObject obj)
        {
            Rigidbody r = obj.GetComponent<Rigidbody>();

            if (r == null) return false;

            return r.mass < this.LessThanMass;
        }

        public override void OnGUI()
        {
            this.LessThanMass = EditorGUILayout.FloatField("Mass Less Than:", this.LessThanMass);
        }
    }

    class DistanceFromPointSettings : FindBySettings
    {
        public Vector3 Point;
        public float GreaterThanDistance;

        public override bool Evaluate(GameObject obj)
        {
            return (obj.transform.position - this.Point).sqrMagnitude
                   > this.GreaterThanDistance * this.GreaterThanDistance;
        }

        public override void OnGUI()
        {
            this.Point = EditorGUILayout.Vector3Field("Point:", this.Point);
            this.GreaterThanDistance = EditorGUILayout.FloatField("Greater Than Distance:", this.GreaterThanDistance);
        }
    }

    class FastRigidbodySettings : FindBySettings
    {
        public float GreaterThanVelocity;

        public override bool Evaluate(GameObject obj)
        {
            Rigidbody rb = obj.GetComponent<Rigidbody>();

            if (rb == null) return false;

            return rb.velocity.sqrMagnitude > this.GreaterThanVelocity * this.GreaterThanVelocity;
        }

        public override void OnGUI()
        {
            this.GreaterThanVelocity = EditorGUILayout.FloatField("Greater Than Velocity:", this.GreaterThanVelocity);
        }
    }

    class MultipleMonobehaviourCopiesSettings : FindBySettings
    {
        public MonoScript Script;

        public override bool Evaluate(GameObject obj)
        {
            return obj.GetComponents(Script.GetClass()).Count() > 1;
        }

        public override void OnGUI()
        {
            this.Script = (MonoScript)EditorGUILayout.ObjectField("Script Type:", this.Script, typeof(MonoScript), false);
        }
    }

    [MenuItem(MenuItemPath)]
    static void Init()
    {
        var instance = EditorWindow.GetWindow<FindGameObjectsEditorWindow>(WindowName);
        instance.Show();
    }

    void OnGUI()
    {
        EditorGUI.BeginChangeCheck();
        _findByOptions = (FindByOptions)EditorGUILayout.EnumPopup("Find By:", _findByOptions);
        if (EditorGUI.EndChangeCheck())
        {
            switch (_findByOptions)
            {
                case FindByOptions.None:
                    _findBySettings = null;
                    break;
                case FindByOptions.InvalidTransform:
                    _findBySettings = new InvalidTransformSettings();
                    break;
                case FindByOptions.DistanceFromPoint:
                    _findBySettings = new DistanceFromPointSettings();
                    break;
                case FindByOptions.SmallRigidbodyMass:
                    _findBySettings = new SmallRigidbodiesSettings();
                    break;
                case FindByOptions.FastRigidbody:
                    _findBySettings = new FastRigidbodySettings();
                    break;
                case FindByOptions.MultipleMonobehaviourCopies:
                    _findBySettings = new MultipleMonobehaviourCopiesSettings();
                    break;
                default:
                    _helpString = "Unknown find options specified.";
                    _helpMessageType = MessageType.Error;
                    break;
            }
        }

        if (_findBySettings != null)
        {
            _findBySettings.OnGUI();
        }
        else
        {
            GUI.enabled = false;
        }

        GUILayout.FlexibleSpace();

        if (_helpString != null) EditorGUILayout.HelpBox(_helpString, MessageType.Info);

        if (GUILayout.Button(ButtonText))
        {
            List<GameObject> selectedObjects = new List<GameObject>();

            if (_findBySettings != null)
            {
                foreach (var gameObject in GameObject.FindObjectsOfType<GameObject>())
                {
                    Transform transform = gameObject.transform;

                    if (_findBySettings.Evaluate(gameObject))
                    {
                        selectedObjects.Add(gameObject);
                    }
                }
            }

            Selection.objects = selectedObjects.Cast<UnityEngine.Object>().ToArray();

            _helpString = string.Format(ObjectsSelectedHelpStringFormat, selectedObjects.Count);
        }
    }
}
