using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BoardTile : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] Image m_HoverImage;
    [SerializeField] Color m_BaseColor;
    [SerializeField] Color m_HighlightColor;
    [SerializeField] Color m_MoveableColor;
    [SerializeField] Color m_SelectedColor;

    private Vector2 m_BoardPosition;
    private PawnController m_PieceController;
    private ETeam m_PromotionalTileFor = ETeam.None;

    public Vector2 BoardPosition => m_BoardPosition;
    public PawnController PieceController => m_PieceController;

    public void Init(Vector2 boardPosition, ETeam promotionalTileFor)
    {
        m_BoardPosition = boardPosition;
        m_PromotionalTileFor = promotionalTileFor;
        SetColor(EColorType.None);
    }

    public void OnPointerClick(PointerEventData pEventData)
    {
        BoardManager.Instance.ClickOnTile(this);
    }

    public void SetColor(EColorType pType)
    {
        switch (pType)
        {
            case EColorType.None:
                m_HoverImage.color = m_BaseColor;
                break;
            case EColorType.Highlight:
                m_HoverImage.color = m_HighlightColor;
                break;
            case EColorType.Select:
                m_HoverImage.color = m_SelectedColor;
                break;
            case EColorType.Moveable:
                m_HoverImage.color = m_MoveableColor;
                break;

        }
    }

    public void RemovePiece()
    {
        m_PieceController = null;
    }

    public void SetPiece(PawnController pieceController)
    {
        if(m_PieceController != null)
           BoardManager.Instance.RemovePiece(m_PieceController);

        m_PieceController = pieceController;
        m_PieceController.transform.position = new Vector3(transform.position.x, transform.position.y, -2);
        m_PieceController.transform.SetParent(gameObject.transform);
        m_PieceController.transform.SetAsFirstSibling();
        m_PieceController.SetPosition(m_BoardPosition);

        if (m_PromotionalTileFor != ETeam.None && m_PromotionalTileFor == m_PieceController.Team)
            m_PieceController.Promote();
    }

    public void Clear()
    {
        SetColor(EColorType.None);
        m_PieceController = null;
    }
}

public enum EColorType
{
    None,
    Highlight,
    Select,
    Moveable
}