

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

    // Chip sürüklemenin başladığı orijinal bahis alanı
    private TableNumberPlace originPlace = null;
    // Sürükleme sırasında tespit edilen geçerli snap hedefi
    private TableNumberPlace currentSnapPlace = null;

    // Drag mesafe eşik değeri (ekran piksel cinsinden)
    [SerializeField] private float dragThreshold = 20f;
    // Sadece "place" layer'ındaki objeleri hedeflemek için layer maskesi
    [SerializeField] private LayerMask placeLayerMask;

    private void OnEnable()
    {
        EventManager.Subscribe(GameEvents.OnGameBetChanged,OnBetChanged);
    }

    private void OnBetChanged(object[] obj)
    {
        _currentSelectedChip = (Chips)obj[0];
    }


    private void OnDisable()
    {
        EventManager.Unsubscribe(GameEvents.OnGameBetChanged,OnBetChanged);
    }
    
    void Update()
    {
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
                if (Physics.Raycast(ray, out hit, Mathf.Infinity, placeLayerMask))
                {
                    TableNumberPlace tappedPlace = hit.collider.GetComponent<TableNumberPlace>();
                    if (tappedPlace != null)
                    {
                        // Eğer bu place'de chip varsa, sürükleme yapmak için en üstteki chip alınır.
                        if (tappedPlace.HasChips)
                        {
                            draggingChip = tappedPlace.RemoveChip();
                            originPlace = tappedPlace;
                            if (draggingChip != null)
                            {
                                // Offset hesaplanır
                                Vector3 chipScreenPos = Camera.main.WorldToScreenPoint(draggingChip.transform.position);
                                dragOffset = draggingChip.transform.position - Camera.main.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, chipScreenPos.z));
                            }
                        }
                        else
                        {
                            // Eğer place boşsa, tıklama olarak yorumlanır: yeni bet ekle.
                            tappedPlace.PlaceBet(_currentSelectedChip);
                        }
                    }
                }
                break;
            case TouchPhase.Moved:
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
                if (draggingChip != null)
                {
                    // Eğer yeterince sürüklenmediyse, drag yapılmamış sayılır
                    if (!isDragging)
                    {
                        // Dokunulan yerdeki TableNumberPlace'e tıklama olarak ek bet yapılır
                        if (Physics.Raycast(ray, out hit, Mathf.Infinity, placeLayerMask))
                        {
                            TableNumberPlace tappedPlace = hit.collider.GetComponent<TableNumberPlace>();
                            if (tappedPlace != null)
                            {
                                tappedPlace.PlaceBet(_currentSelectedChip);
                            }
                        }
                        // Chip orijinal alana geri konumlandırılır
                        if (originPlace != null)
                        {
                            originPlace.PlaceDraggedChip(draggingChip);
                        }
                    }
                    else
                    {
                        // Sürükleme yapıldıysa: geçerli snap hedefi varsa chip oraya bırakılır, yoksa orijinal alana geri gönderilir.
                        if (currentSnapPlace != null)
                        {
                            currentSnapPlace.PlaceDraggedChip(draggingChip);
                        }
                        else if (originPlace != null)
                        {
                            originPlace.PlaceDraggedChip(draggingChip);
                        }
                    }
                    // Temizleme
                    draggingChip = null;
                    originPlace = null;
                    currentSnapPlace = null;
                    isDragging = false;
                }
                break;
        }
    }
}
