using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class AtomicRotator : MonoBehaviour
{
    [Header("Передвижение")]
    [SerializeField, Min(0)] private float _minSpeedRotation;
    [SerializeField, Min(0)] private float _maxSpeedRotation;
    [SerializeField, Min(0)] private float _slowdownDistance;
    [Space]
    [Header("Сортировка по слоям")]
    [SerializeField] private string _layerBehindPlayer;
    [SerializeField] private string _layerFrontPlayer;
    [SerializeField, Min(0)] private float _lightIntensityHigh;
    [SerializeField, Min(0)] private float _lightIntensityLow;
    [SerializeField, Min(0)] private float _lightIntesityChangingDuration;
    [Space]
    [SerializeField] private Light2D _lightAtackOrbBack;
    [SerializeField] private Light2D _lightShieldOrb;

    private SpiritOrb _spiritOrb;
    private Transform _pointA;
    private Transform _pointB;
    private Transform _temp;
    private TrailRenderer _trailRenderer;
    private SpriteRenderer _spriteRenderer;
    private bool _shouldAtomicRotateAroundTarget;
    private bool _isBehind;
    private bool _couroutineLerpLightStarted;
    private float _currentSpeed;
    private float _distanceToPointB;
    private float _distanceToPointA;
    private float _targetIntensityForBackLight;
    private float _elapsedTimeIntensity;
    private float _startIntensityBackLight;
    private string _layer;

    private void OnValidate()
    {
        if (_lightIntensityLow >= _lightIntensityHigh)
        {
            _lightIntensityLow = _lightIntensityHigh - 0.1f;
        }
    }

    private void Start()
    {
        _spiritOrb = GetComponent<SpiritOrb>();
        _pointA = GetComponent<SpiritOrb>().PointA;
        _pointB = GetComponent<SpiritOrb>().PointB;
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _trailRenderer = GetComponentInChildren<TrailRenderer>();

        _shouldAtomicRotateAroundTarget = true;
        StartCoroutine(MoveBetweenPoints());
    }

    private void Update()
    {
        if (_spiritOrb.ShieldMode)
        {
            _shouldAtomicRotateAroundTarget = false;
        }
    }

    IEnumerator MoveBetweenPoints()
    {
        while (_shouldAtomicRotateAroundTarget)
        {
            _distanceToPointB = Vector3.Distance(transform.position, _pointB.position);
            _distanceToPointA = Vector3.Distance(transform.position, _pointA.position);

            if (_distanceToPointB < _slowdownDistance)
            {
                _currentSpeed = Mathf.Lerp(_minSpeedRotation, _maxSpeedRotation, _distanceToPointB / _slowdownDistance);
            }
            else if (_distanceToPointA < _slowdownDistance)
            {
                _currentSpeed = Mathf.Lerp(_minSpeedRotation, _maxSpeedRotation, _distanceToPointA / _slowdownDistance);
            }
            else
            {
                _currentSpeed = _maxSpeedRotation;
            }

            transform.position = Vector3.MoveTowards(transform.position, _pointB.position, _currentSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.position, _pointB.position) < _slowdownDistance)
            {
                if (!_couroutineLerpLightStarted)
                {
                    StartCoroutine(LerpLight());
                }
            }

            if (Vector3.Distance(transform.position, _pointB.position) < 0.1f)
            {
                _temp = _pointA;
                _pointA = _pointB;
                _pointB = _temp;
                _isBehind = !_isBehind;

                ChangeSortingLayer();
            }
            yield return null;
        }
    }

    IEnumerator LerpLight()
    {
        _couroutineLerpLightStarted = true;

        if (!_isBehind)
        {
            _targetIntensityForBackLight = _lightIntensityLow;
        }
        else
        {
            _targetIntensityForBackLight = _lightIntensityHigh;
        }

        _elapsedTimeIntensity = 0;
        _startIntensityBackLight = _lightAtackOrbBack.intensity;


        while (_elapsedTimeIntensity < _lightIntesityChangingDuration)
        {
            _lightAtackOrbBack.intensity = Mathf.Lerp(_startIntensityBackLight, _targetIntensityForBackLight, _elapsedTimeIntensity / _lightIntesityChangingDuration);
            _elapsedTimeIntensity += Time.deltaTime;
            yield return null;
        }

        _lightAtackOrbBack.intensity = _targetIntensityForBackLight;

        _couroutineLerpLightStarted = false;
    }

    private void ChangeSortingLayer()
    {        
        if (_isBehind)
        {
            _layer = _layerBehindPlayer;
        }
        else
        {
            _layer = _layerFrontPlayer;
        }

        if (_spriteRenderer != null && !string.IsNullOrEmpty(_layer))
        {
            _trailRenderer.sortingLayerName = _layer;
            _spriteRenderer.sortingLayerName = _layer;
        }
    }

    public void StopRotating()
    {
        _shouldAtomicRotateAroundTarget = false;
        StopCoroutine(MoveBetweenPoints());
        //Debug.Log("Stop rotate!");
    }

    public void StartRotating()
    {
        _shouldAtomicRotateAroundTarget = true;
        StartCoroutine(MoveBetweenPoints());
        //Debug.Log("Start rotate!");
    }

    private void OnEnable()
    {
        EventBus.onLaunchSpiritProjectiles += StopRotating;
        EventBus.onOrbShieldModeActivate += StopRotating;
        EventBus.onOrbAtackModeActivate += StartRotating;
    }

    private void OnDisable()
    {
        EventBus.onLaunchSpiritProjectiles -= StopRotating;
        EventBus.onOrbShieldModeActivate -= StopRotating;
        EventBus.onOrbAtackModeActivate -= StartRotating;
    }
}
