using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class LanternSwitch : MonoBehaviour
{
    [SerializeField, Min(0)] private float _timeTurnOn;
    [SerializeField, Min(0)] private float _targetIntensity;
    [SerializeField] private bool _isActiveAlways;
    [SerializeField] private bool _isFlickering;
    [SerializeField, Min(0)] private float _minFlickeringDelay;
    [SerializeField, Min(0)] private float _maxFlickeringDelay;
    [SerializeField, Min(0)] private float _differenceFlickIntensity;

    private Light2D _light;
    private ParticleSystem _ps;
    private float _timer = 0;
    private float _tmpIntensity;
    private bool _hasArsonist;
    private bool _isFired;

    private void OnValidate()
    {
        if (_minFlickeringDelay >= _maxFlickeringDelay)
        {
            _minFlickeringDelay = _maxFlickeringDelay - 0.1f;
        }
    }

    private void Awake()
    {
        _light = GetComponentInChildren<Light2D>();
        _ps = GetComponentInChildren<ParticleSystem>();

        if (_isActiveAlways)
        {
            TurnOnFast();
        }
        else
        {
            TurnOff();
        }
    }

    private void Update()
    {
        if (_isFired)
        {
            return;
        }

        if (_hasArsonist && Input.GetKeyDown(KeyCode.E))
        {
            TurnOn();
        }
    }

    private void TurnOff()
    {
        _light.intensity = 0;
        _ps.Pause();
    }

    private void TurnOnFast()
    {
        _light.intensity = _targetIntensity;
        _isFired = true;
        if (_isFlickering)
        {
            StartCoroutine(Flickering());
        }
    }

    private void TurnOn()
    {
        _ps.Play();
        _isFired = true;
        StartCoroutine(LerpIntensity());
        if (_isFlickering)
        {
            StartCoroutine(Flickering());
        }
    }

    IEnumerator LerpIntensity()
    {
        while (_timer < _timeTurnOn)
        {
            _light.intensity = Mathf.Lerp(0, _targetIntensity, _timer / _timeTurnOn);
            _tmpIntensity = _light.intensity;
            _timer += Time.deltaTime;
            yield return null;
        }

        yield break;
    }

    IEnumerator Flickering()
    {
        while (true)
        {
            _tmpIntensity = _light.intensity;
            _light.intensity = Random.Range(_light.intensity - _differenceFlickIntensity, _light.intensity + _differenceFlickIntensity);
            yield return new WaitForSeconds(Random.Range(_minFlickeringDelay, _maxFlickeringDelay));
            _light.intensity = _tmpIntensity;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.GetComponent<Player>())
        {
            _hasArsonist = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.GetComponent<Player>())
        {
            _hasArsonist = true;
        }
    }
}
