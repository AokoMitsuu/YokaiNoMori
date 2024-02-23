using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class BoardTile : MonoBehaviour
{
    public Vector2 BoardPosition => m_BoardPosition;
    public PieceController PieceController => m_PieceController;

    [SerializeField] SpriteRenderer m_SpriteRenderer;
    [SerializeField] Color m_BaseColor;
    [SerializeField] Color m_HighlightColor;
    [SerializeField] Color m_SelectedColor;

    private Vector2 m_BoardPosition;
    private PieceController m_PieceController;
    private ETeam m_PromotionalTileFor = ETeam.None;

    public void Init(Vector2 boardPosition, ETeam promotionalTileFor)
    {
        m_BoardPosition = boardPosition;
        m_PromotionalTileFor = promotionalTileFor;
    }

    private void OnMouseDown()
    {
        BoardManager.Instance.ClickOnTile(this);
    }

    public void SetHighlight(bool isOn)
    {
        if (isOn)
        {
            m_SpriteRenderer.color = m_HighlightColor;
        }
        else
        {
            m_SpriteRenderer.color = m_BaseColor;
        }
    }

    public void SetSelected(bool isSelected)
    {
        if (isSelected)
        {
            m_SpriteRenderer.color = m_SelectedColor;
        }
        else
        {
            m_SpriteRenderer.color = m_BaseColor;
        }
    }

    public void RemovePiece()
    {
        m_PieceController = null;
    }

    public void SetPiece(PieceController pieceController)
    {
        if(m_PieceController != null)
           BoardManager.Instance.RemovePiece(m_PieceController);

        m_PieceController = pieceController;
        m_PieceController.transform.position = new Vector3(transform.position.x, transform.position.y, -2);

        if (m_PromotionalTileFor != ETeam.None && m_PromotionalTileFor == m_PieceController.Team)
            m_PieceController.Promote();
    }
}
