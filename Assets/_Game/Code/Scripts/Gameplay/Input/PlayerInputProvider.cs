using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class PlayerInputProvider : MonoBehaviour, IInputProvider, IPointerDownHandler
{
    [SerializeField] private UnityEvent<Vector2> m_OnSelectPawn;

    public void OnPointerDown(PointerEventData pEventData)
    {
        SelectPawn(pEventData.position);
    }

    public void SelectPawn(Vector2 pPosition)
    {
        m_OnSelectPawn?.Invoke(pPosition);
    }
}
