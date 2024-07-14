using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class TestPlayerMovement : MonoBehaviour
{
    [SerializeField] private Transform _playerTrm;

    [Header("MovementSetting")]
    [SerializeField] private float _radius;
    [SerializeField] private float _speed;

    [Header("DashSetting")]
    [SerializeField] private float _dashTime;
    [SerializeField] private float _dashSpeed;
    [SerializeField] private float _dashInertia; //대쉬 끝난 후 작용하는 관성의 정도
    [SerializeField] private float _stopDashTerm;

    private float _degree;
    private float _dir;
    private bool _isDashStoped;
    private bool _isDash;
    private Coroutine _dashCoroutine;
    void Update()
    {
        PlayerInput();
    }

    private void PlayerInput()
    {
        _dir = Input.GetAxisRaw("Horizontal");

        if (Input.GetKeyDown(KeyCode.Space))
            _dashCoroutine = StartCoroutine(DashCoroutine(_degree, _dir));

        if (_isDash) return;

        Movement(_dir, _speed);
    }

    private void Movement(float dir, float speed)
    {
        _degree += dir * speed * Time.deltaTime;

        if (_degree < 360)
        {
            float radian = Mathf.Deg2Rad * _degree;

            float x = _radius * Mathf.Sin(radian);
            float y = _radius * Mathf.Cos(radian);

            _playerTrm.transform.position = transform.position + new Vector3(x, y);
            _playerTrm.transform.rotation = Quaternion.Euler(0, 0, _degree * -1);
        }
        else
        {
            _degree = 0;
        }
    }

    private IEnumerator DashCoroutine(float degree, float dir)
    {
        if (_isDash) yield break;

        _isDash = true;

        float saveDeg = degree;
        float saveDir = dir;
        float currentTime = 0;

        while(currentTime < _dashTime)
        {
            if(_dir == 0)
            {
                if (!_isDashStoped)
                {
                    _isDashStoped = true;

                    DOVirtual.DelayedCall(_stopDashTerm, () =>
                    {
                        _isDashStoped = false;

                        if (_dir == 0)
                        {
                            StartCoroutine(InertiaCoroutine(saveDir, _dashSpeed));
                            StopCoroutine(_dashCoroutine);
                        }
                    });
                }
            }
            else if(_dir != saveDir)
            {
                saveDir = _dir;
            }

            currentTime += Time.deltaTime;
            Movement(saveDir, _dashSpeed);
            yield return null;
        }

        StartCoroutine(InertiaCoroutine(saveDir, _dashSpeed));
    }

    private IEnumerator InertiaCoroutine(float saveDir, float speed) //관성 코루틴
    {
        int count = 0;
        float slowDownMultiplier = 1;

        while (count < _dashInertia)
        {
            count++;

            Movement(saveDir, speed * slowDownMultiplier);
            slowDownMultiplier -= 1 / _dashInertia;

            yield return null;
        }

        _isDash = false;

        if (_dashCoroutine != null)
            StopCoroutine(_dashCoroutine);
    }
}
