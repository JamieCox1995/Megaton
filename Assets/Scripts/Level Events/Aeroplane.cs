using System;
using System.Collections;
using UnityEngine;
public class Aeroplane : MonoBehaviour
{
    public MotionPath MotionPath;
    public float AirSpeed;
    public ObjectSettings ObjectSettings;
    public ExplosiveSettings ExplosiveSettings;

    public GameObject Propeller;

    public bool IsCrashing;

    public event Action<Aeroplane> OnPathComplete;

    private ExplosiveObject _explosiveObject;
    private Rigidbody _rigidbody;

    private float _pathTime;
    private float _time;

    private Vector3 _position;
    private Vector3 _velocity;
    private Vector3 _acceleration;

    private Vector3 _angularDisplacement;
    private Vector3 _angularVelocity;
    private Vector3 _angularAcceleration;

    private void Start()
    {
        _rigidbody = GetComponent<Rigidbody>() ?? this.gameObject.AddComponent<Rigidbody>();
        _rigidbody.useGravity = false;

        _rigidbody.position = this.MotionPath.ControlPoints[0].Point;
        _rigidbody.rotation = Quaternion.FromToRotation(Vector3.forward, this.MotionPath.ControlPoints[0].Tangent);

        _pathTime = this.MotionPath.Length / this.AirSpeed;
    }

    private void Update()
    {
        if (this.IsCrashing)
        {
            if (_explosiveObject == null)
            {
                StartCoroutine(SetupCrashBehaviour());
            }
        }
        else
        {
            UpdateFlight();
        }
    }

    private IEnumerator SetupCrashBehaviour()
    {
        _explosiveObject = this.gameObject.AddComponent<ExplosiveObject>();
        _explosiveObject.objSettings = this.ObjectSettings;
        _explosiveObject.settings = this.ExplosiveSettings;
        _rigidbody.useGravity = true;

        // kinda hacky: allow the ExplosiveObject to set itself up
        // before changing the rigidbody settings in the next frame
        yield return null;

        // imitate motion path behaviour
        _rigidbody.isKinematic = false;
        _rigidbody.AddForce(_velocity, ForceMode.VelocityChange);
        _rigidbody.AddTorque(_angularVelocity, ForceMode.VelocityChange);

        // get random vectors to simulate crashing behaviour
        Vector3 diveForce = Vector3.down * UnityEngine.Random.Range(0.25f, 1f);
        Vector3 rollTorque = Vector3.forward * UnityEngine.Random.Range(-3f, 3f);

        Vector3 relativeAcceleration = Quaternion.Inverse(transform.rotation) * _acceleration;
        Vector3 relativeAngularAcceleration = Quaternion.Inverse(transform.rotation) * _angularAcceleration;

        while (true)
        {
            _rigidbody.AddRelativeForce(relativeAcceleration, ForceMode.Acceleration);
            _rigidbody.AddRelativeTorque(relativeAngularAcceleration, ForceMode.Acceleration);

            _rigidbody.AddForceAtPosition(diveForce, this.Propeller.transform.position, ForceMode.Acceleration);
            _rigidbody.AddRelativeTorque(rollTorque, ForceMode.Acceleration);

            yield return null;
        }
    }

    private void UpdateFlight()
    {
        if (_time > _pathTime)
        {
            OnPathComplete(this);
            return;
        }

        _time += Time.deltaTime;

        float t = _time / _pathTime;

        // calculate position and derivatives
        Vector3 position = this.MotionPath.Evaluate(t);

        Vector3 velocity = (position - _rigidbody.position) / Time.deltaTime;
        _acceleration = (velocity - _velocity) / Time.deltaTime;
        _velocity = velocity;

        // calculate rotation and derivatives
        Quaternion rotation = Quaternion.LookRotation(position - _position);
        Vector3 rotationAxis;
        float rotationAngle;
        rotation.ToAngleAxis(out rotationAngle, out rotationAxis);
        Vector3 angularDisplacement = rotationAxis * rotationAngle * Mathf.Deg2Rad;
        Vector3 angularVelocity = (angularDisplacement - _angularDisplacement) / Time.deltaTime;
        _angularAcceleration = (angularVelocity - _angularVelocity) / Time.deltaTime;
        _angularVelocity = angularVelocity;
        _angularDisplacement = angularDisplacement;

        _position = position;
        _rigidbody.MovePosition(position);
        _rigidbody.MoveRotation(rotation);

        // rotate propeller
        this.Propeller.transform.Rotate(Vector3.forward, 360f * this.AirSpeed / Time.deltaTime, Space.Self);
    }
}
