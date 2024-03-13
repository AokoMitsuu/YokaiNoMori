using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using YokaiNoMori.Enumeration;

public class Player : MonoBehaviour, IPointerDownHandler
{
    [SerializeField] private UnityEvent<Vector2> m_OnSelectPawn;

    private Board m_Board;
    private ECampType m_ECampType;

    public Board Board { get => m_Board; set => m_Board = value; }

    public void OnPointerDown(PointerEventData pEventData)
    {
        SelectPawn(pEventData.position);
    }

    public void SelectPawn(Vector2 pPosition)
    {
        m_OnSelectPawn?.Invoke(pPosition);
    }
}
