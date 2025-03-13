using System;
using UnityEngine;

public class RouletteBetInputController : MonoBehaviour
{
    // UI'dan veya başka yerden güncellenebilecek seçili chip değeri
    private Chips _currentSelectedChip = Chips.Ten;

    // Sürüklenen chip referansı
    private Chip draggingChip = null;
    private Vector3 dragOffset;
    private Vector3 initialTouchPosition;
    private bool isDragging = false;
    
    // Long press detection for chip removal
    private float longPressTime = 0.5f; // Seconds to hold for long press
    private float pressStartTime = 0f;  // When the press started
    private bool isLongPressing = false;
    private TableNumberPlace pressedPlace = null;

    // Chip sürüklemenin başladığı orijinal bahis alanı
    private TableNumberPlace originPlace = null;
    // Sürükleme sırasında tespit edilen geçerli snap hedefi
    private TableNumberPlace currentSnapPlace = null;

    // Drag mesafe eşik değeri (ekran piksel cinsinden)
    [SerializeField] private float dragThreshold = 20f;
    // Sadece "place" layer'ındaki objeleri hedeflemek için layer maskesi
    [SerializeField] private LayerMask placeLayerMask;
    
    // Reference to MoneyCanvasController to check total balance
    private MoneyCanvasController moneyController;
    
    private void Awake()
    {
        // Find the MoneyCanvasController in the scene
        moneyController = FindObjectOfType<MoneyCanvasController>();
        if (moneyController == null)
        {
            Debug.LogError("MoneyCanvasController could not be found in the scene!");
        }
    }

    private void OnEnable()
    {
        EventManager.Subscribe(GameEvents.OnGameBetChanged, OnBetChanged);
    }

    private void OnBetChanged(object[] obj)
    {
        _currentSelectedChip = (Chips)obj[0];
    }

    private void OnDisable()
    {
        EventManager.Unsubscribe(GameEvents.OnGameBetChanged, OnBetChanged);
    }
    
    void Update()
    {
        // Check for long press if we're tracking a press
        if (pressedPlace != null && !isLongPressing && !isDragging)
        {
            if (Time.time - pressStartTime > longPressTime)
            {
                isLongPressing = true;
                
                // Long press detected - remove chip
                if (pressedPlace.HasChips)
                {
                    draggingChip = pressedPlace.RemoveChip();
                    originPlace = pressedPlace;
                    
                    if (draggingChip != null)
                    {
                        // Calculate drag offset
                        Vector3 screenPos = Input.mousePosition;
                        if (Input.touchCount > 0)
                        {
                            screenPos = Input.GetTouch(0).position;
                        }
                        
                        Vector3 chipScreenPos = Camera.main.WorldToScreenPoint(draggingChip.transform.position);
                        dragOffset = draggingChip.transform.position - Camera.main.ScreenToWorldPoint(
                            new Vector3(screenPos.x, screenPos.y, chipScreenPos.z));
                        
                        isDragging = true;
                        Debug.Log("Long press detected, starting chip drag");
                    }
                }
            }
        }
        
        // Mobil dokunma kontrolü
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            ProcessTouch(touch.phase, touch.position);
        }
        else // PC/Editor için mouse kontrolü
        {
            if (Input.GetMouseButtonDown(0))
                ProcessTouch(TouchPhase.Began, Input.mousePosition);
            else if (Input.GetMouseButton(0))
                ProcessTouch(TouchPhase.Moved, Input.mousePosition);
            else if (Input.GetMouseButtonUp(0))
                ProcessTouch(TouchPhase.Ended, Input.mousePosition);
        }
    }

    private void ProcessTouch(TouchPhase phase, Vector3 screenPos)
    {
        Ray ray = Camera.main.ScreenPointToRay(screenPos);
        RaycastHit hit;
        
        switch (phase)
        {
            case TouchPhase.Began:
                initialTouchPosition = screenPos;
                pressStartTime = Time.time;
                isLongPressing = false;
                isDragging = false;
                
                if (Physics.Raycast(ray, out hit, Mathf.Infinity, placeLayerMask))
                {
                    TableNumberPlace tappedPlace = hit.collider.GetComponent<TableNumberPlace>();
                    if (tappedPlace != null)
                    {
                        // Store the tapped place for long press detection
                        pressedPlace = tappedPlace;
                    }
                }
                break;
                
            case TouchPhase.Moved:
                // If we're already dragging a chip, move it
                if (draggingChip != null)
                {
                    // Drag mesafesini kontrol et
                    if (!isDragging && Vector3.Distance(screenPos, initialTouchPosition) > dragThreshold)
                    {
                        isDragging = true;
                    }
                    
                    if (isDragging)
                    {
                        // Chip pointer'ı takip etsin
                        Plane plane = new Plane(Vector3.up, new Vector3(0, draggingChip.transform.position.y, 0));
                        if (plane.Raycast(ray, out float distance))
                        {
                            Vector3 worldPos = ray.GetPoint(distance);
                            worldPos.y = draggingChip.transform.position.y;
                            draggingChip.transform.position = worldPos;
                        }
                        
                        // Snap için: pointer'ın altındaki TableNumberPlace tespit edilsin
                        if (Physics.Raycast(ray, out hit, Mathf.Infinity, placeLayerMask))
                        {
                            TableNumberPlace snapPlace = hit.collider.GetComponent<TableNumberPlace>();
                            if (snapPlace != null)
                            {
                                currentSnapPlace = snapPlace;
                            }
                        }
                        else
                        {
                            currentSnapPlace = null;
                        }
                    }
                }
                break;
                
            case TouchPhase.Ended:
                // Handle tapping (short press)
                bool wasDragging = isDragging;
                bool wasLongPressing = isLongPressing;
                
                // Reset tracking variables
                isDragging = false;
                isLongPressing = false;
                
                // If this was a simple tap (not a drag or long press)
                if (!wasDragging && !wasLongPressing && pressedPlace != null)
                {
                    // This was a simple tap, place a chip
                    pressedPlace.PlaceBet(_currentSelectedChip);
                    Debug.Log("Simple tap detected, placing new chip");
                }
                // If we were dragging a chip
                else if (draggingChip != null)
                {
                    // Sürükleme yapıldıysa: geçerli snap hedefi varsa chip oraya bırakılır, yoksa orijinal alana geri gönderilir.
                    bool chipPlaced = false;
                    if (currentSnapPlace != null)
                    {
                        // Try to place chip and check if it was successful
                        chipPlaced = currentSnapPlace.PlaceDraggedChip(draggingChip);
                    }
                    
                    // If chip couldn't be placed at the snap place, return it to the origin
                    if (!chipPlaced && originPlace != null)
                    {
                        originPlace.PlaceDraggedChip(draggingChip);
                    }
                }
                
                // Temizleme
                draggingChip = null;
                originPlace = null;
                currentSnapPlace = null;
                pressedPlace = null;
                break;
        }
    }
}