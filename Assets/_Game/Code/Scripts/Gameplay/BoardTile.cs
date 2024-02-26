using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BoardTile : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] Image m_Image;
    [SerializeField] Color m_BaseColor;
    [SerializeField] Color m_HighlightColor;
    [SerializeField] Color m_SelectedColor;

    private Vector2 m_BoardPosition;
    private PawnController m_PieceController;
    private ETeam m_PromotionalTileFor = ETeam.None;

    public Vector2 BoardPosition => m_BoardPosition;
    public PawnController PieceController => m_PieceController;

    private float m_Life;

    public void Init(Vector2 boardPosition, ETeam promotionalTileFor)
    {
        m_BoardPosition = boardPosition;
        m_PromotionalTileFor = promotionalTileFor;
    }

    public void OnPointerClick(PointerEventData pEventData)
    {
        BoardManager.Instance.ClickOnTile(this);
    }

    public void SetHighlight(bool isOn)
    {
        if (isOn)
        {
            m_Image.color = m_HighlightColor;
        }
        else
        {
            m_Image.color = m_BaseColor;
        }
    }

    public void SetSelected(bool isSelected)
    {
        if (isSelected)
        {
            m_Image.color = m_SelectedColor;
        }
        else
        {
            m_Image.color = m_BaseColor;
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

        if (m_PromotionalTileFor != ETeam.None && m_PromotionalTileFor == m_PieceController.Team)
            m_PieceController.Promote();
    }
}
