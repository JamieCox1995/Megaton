using System.Collections.Generic;
using System.Linq;
using TotalDistraction.CameraSystem.Behaviours;
using UnityEngine;

namespace TotalDistraction.CameraSystem
{
    public class CameraSystem : MonoBehaviour, ISerializationCallbackReceiver
    {
        public static GameObject Target { get; set; }

        private Camera _camera;
        private CameraMode _cameraMode;

        private Dictionary<CameraMode, CameraBehaviour> _behaviours;

        [SerializeField]
        private List<CameraMode> _behavioursKeys;
        [SerializeField]
        private List<CameraBehaviour> _behavioursValues;

        private CameraBehaviour _previousFrameBehaviour;

        public CameraMode Current { get { return _cameraMode; } set { _cameraMode = value; } }

        public void OnBeforeSerialize()
        {
            if (_behaviours != null)
            {
                var serialize = _behaviours;

                _behavioursKeys = new List<CameraMode>(serialize.Select(kvp => kvp.Key));
                _behavioursValues = new List<CameraBehaviour>(serialize.Select(kvp => kvp.Value));
            }
        }

        public void OnAfterDeserialize()
        {
            _behaviours = new Dictionary<CameraMode, CameraBehaviour>();

            if (_behavioursKeys != null && _behavioursValues != null)
            {
                int count = Mathf.Min(_behavioursKeys.Count, _behavioursValues.Count);
                for (int i = 0; i < count; i++)
                {
                    CameraMode key = _behavioursKeys[i];

                    CameraBehaviour behaviour = _behavioursValues[i];

                    _behaviours.Add(key, behaviour);
                }
            }
        }

        private void Start()
        {
            _camera = Camera.main;

            if (_behaviours == null) return;

            var cameraModes = _behaviours.Keys.ToList();
            foreach (var cameraMode in cameraModes)
            {
                _behaviours[cameraMode] = ScriptableObject.Instantiate(_behaviours[cameraMode]);
            }
        }

        private void Update()
        {
            if (_behaviours == null) return;

            CameraBehaviour behaviour;

            if (!_behaviours.TryGetValue(_cameraMode, out behaviour)) _behaviours.TryGetValue(CameraMode.Default, out behaviour);

            if (behaviour != null)
            {
                if (behaviour != _previousFrameBehaviour)
                {
                    if (_previousFrameBehaviour != null) _previousFrameBehaviour.EndBehaviour(_camera);
                    behaviour.StartBehaviour(_camera);
                }

                behaviour.UpdateBehaviour(_camera, CameraSystem.Target);

                _previousFrameBehaviour = behaviour;
            }
            else
            {
                Debug.LogWarningFormat("No suitable camera behaviour for mode {0} was found, and no default behaviour exists.", _cameraMode);
            }
        }

        public CameraBehaviour this[CameraMode cameraMode]
        {
            get
            {
                CameraBehaviour value;

                if (_behaviours != null && _behaviours.TryGetValue(cameraMode, out value))
                {
                    if (value != null)
                    {
                        return value;
                    }
                }

                return null;
            }
        }

        public void Register(CameraMode cameraMode, CameraBehaviour behaviour)
        {
            if (_behaviours == null) _behaviours = new Dictionary<CameraMode, CameraBehaviour>();

            _behaviours[cameraMode] = behaviour;
        }
    }
}
