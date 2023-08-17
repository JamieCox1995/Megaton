using UnityEngine;

namespace TotalDistraction.CameraSystem.Animation
{
    [RequireComponent(typeof(CameraSystem))]
    public class CameraSystemController : MonoBehaviour
    {
        public RuntimeAnimatorController Controller;

        private Animator _animator;
        private CameraSystem _cameraSystem;

        private CameraMode _mainLoopMode;
        private CameraMode _pauseMode;

        private bool _transitionQueued;

        private void Start()
        {
            if (this.Controller == null)
            {
                Debug.LogError("No RuntimeAnimatorController supplied to the Camera System Animator.");
                this.enabled = false;
                return;
            }

            _cameraSystem = GetComponent<CameraSystem>();

            _animator = GetComponent<Animator>();
            if (_animator == null)
            {
                _animator = gameObject.AddComponent<Animator>();
                _animator.runtimeAnimatorController = this.Controller;
                _animator.updateMode = AnimatorUpdateMode.UnscaledTime;
            }

            RegisterEvents();
        }

        private void Update()
        {
            _transitionQueued = false;
        }

        private void RegisterEvents()
        {
            if (GameEventManager.instance == null) return;

            GameEventManager.instance.onMortarFire += OnMortarFire;
            GameEventManager.instance.onPlayerReady += OnPlayerReady;
            GameEventManager.instance.onProjectileDestroyed += OnProjectileDestroyed;
            GameEventManager.instance.onTurnEnd += OnTurnEnd;
            GameEventManager.instance.onLevelEventStarted += OnLevelEventStarted;
            GameEventManager.instance.onLevelEventEnded += OnLevelEventEnded;
            GameEventManager.instance.onGamePaused += OnGamePaused;
            GameEventManager.instance.onGameUnpaused += OnGameUnpaused;
            GameEventManager.instance.onGameOver += OnGamePaused;
        }

        public void SetCameraMode(CameraMode cameraMode)
        {
            _cameraSystem.Current = cameraMode;
        }

        private void SetTrigger(CameraMode mode)
        {
            if (!_transitionQueued)
            {
                _animator.SetTrigger(string.Format("Trigger{0}", mode));
                _transitionQueued = true;
            }
        }

        #region Event handlers
        private void OnPlayerReady()
        {
            if (_cameraSystem.Current != CameraMode.LevelEvent) SetTrigger(CameraMode.Targeting);
            _mainLoopMode = CameraMode.Targeting;
        }

        private void OnMortarFire(MortarFireEvent obj)
        {
            if (_cameraSystem.Current != CameraMode.LevelEvent) SetTrigger(CameraMode.Following);
            _mainLoopMode = CameraMode.Following;
        }

        private void OnProjectileDestroyed()
        {
            if (_cameraSystem.Current != CameraMode.LevelEvent) SetTrigger(CameraMode.Free);
            _mainLoopMode = CameraMode.Free;
        }

        private void OnTurnEnd()
        {
            if (_cameraSystem.Current != CameraMode.LevelEvent) SetTrigger(CameraMode.Targeting);
            _mainLoopMode = CameraMode.Targeting;
        }

        private void OnLevelEventEnded()
        {
            switch (_mainLoopMode)
            {
                case CameraMode.Targeting:
                    SetTrigger(CameraMode.Targeting);
                    break;
                case CameraMode.Following:
                    SetTrigger(CameraMode.Following);
                    break;
                case CameraMode.Free:
                    SetTrigger(CameraMode.Free);
                    break;
                default:
                    Debug.LogWarningFormat("Unexpected camera mode {0} after Level Event", _mainLoopMode);
                    break;
            }
        }

        private void OnLevelEventStarted()
        {
            SetTrigger(CameraMode.LevelEvent);
        }

        private void OnGameUnpaused()
        {
            SetTrigger(_pauseMode);
        }

        private void OnGamePaused()
        {
            _pauseMode = _cameraSystem.Current;
            SetTrigger(CameraMode.Default);
        }
        #endregion
    }
}
