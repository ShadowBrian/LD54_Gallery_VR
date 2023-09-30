using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class CameraPhone : MonoBehaviour
{
    [SerializeField] private GameObject phoneNormal;
    [SerializeField] private GameObject phoneHollow;
    
    [SerializeField] private Transform hand;
    [SerializeField] private Transform handZoomed;

    public float zoomAlpha = 0;

    private Animator _animator;
    private int _zoomDirParam;
    private int _zoomDir = 1;
    
    private void Start()
    {
        _animator = GetComponent<Animator>();
        _zoomDirParam = Animator.StringToHash("ZoomDir");
        phoneNormal.SetActive(true);
        phoneHollow.SetActive(false);
    }

    public void UpdateTransform()
    {
        transform.position = Vector3.Lerp(hand.position, handZoomed.position, zoomAlpha);
        transform.rotation = Quaternion.Lerp(hand.rotation, handZoomed.rotation, zoomAlpha);
    }

    public void ToggleZoomAnim()
    {
        _animator.SetFloat(_zoomDirParam, _zoomDir);
        _animator.Play("PhoneZoom", 0, zoomAlpha);
        SetHollow(_zoomDir > 0);
        _zoomDir = -1 * _zoomDir;
    }

    private void SetHollow(bool hollow)
    {
        phoneNormal.SetActive(!hollow);
        phoneHollow.SetActive(hollow);
    }
}
