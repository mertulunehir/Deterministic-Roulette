using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Camera GameObjects")]
    [SerializeField] private GameObject tableCamera;
    [SerializeField] private GameObject spinCamera;
    [SerializeField] private GameObject ballCamera;
    
    [Header("Settings")]
    [SerializeField] private float ballCamDuration = 5f; 
    
    private GameObject currentActiveCamera;
    
    private void Start()
    {
        SetInitialCameraState();
    }
    
    private void OnEnable()
    {
        EventManager.Subscribe(GameEvents.OnSpinButtonClicked, OnSpinStarted);
        EventManager.Subscribe(GameEvents.OnSpinFinished, OnSpinFinished);
    }
    
    private void OnDisable()
    {
        EventManager.Unsubscribe(GameEvents.OnSpinButtonClicked, OnSpinStarted);
        EventManager.Unsubscribe(GameEvents.OnSpinFinished, OnSpinFinished);
    }
    
    private void SetInitialCameraState()
    {
        if (tableCamera) tableCamera.SetActive(false);
        if (spinCamera) spinCamera.SetActive(false);
        if (ballCamera) ballCamera.SetActive(false);
        
        if (tableCamera)
        {
            tableCamera.SetActive(true);
            currentActiveCamera = tableCamera;
        }
        
    }
    
    private void OnSpinStarted(object[] obj)
    {
        SwitchToCamera(spinCamera);
    }
    
    private void OnSpinFinished(object[] obj)
    {
        SwitchToCamera(ballCamera);
        
        StartCoroutine(ReturnToTableCameraAfterDelay(ballCamDuration));
    }
    
    private void SwitchToCamera(GameObject targetCamera)
    {
        if (targetCamera == null)
            return;
        
        
        
        
        targetCamera.SetActive(true);
        
        if (currentActiveCamera != null)
        {
            currentActiveCamera.SetActive(false);
        }
        
        currentActiveCamera = targetCamera;
    }
    
    private IEnumerator ReturnToTableCameraAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        SwitchToCamera(tableCamera);
        
        EventManager.TriggerEvent(GameEvents.OnEnableBetCanvas);
        
    }
    
    public void SwitchToTableCamera()
    {
        SwitchToCamera(tableCamera);
    }
    
    public void SwitchToSpinCamera()
    {
        SwitchToCamera(spinCamera);
    }
    
    public void SwitchToBallCamera()
    {
        SwitchToCamera(ballCamera);
    }
}