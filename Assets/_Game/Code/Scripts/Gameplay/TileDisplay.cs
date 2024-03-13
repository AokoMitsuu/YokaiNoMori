using UnityEngine;

public class TileDisplay : MonoBehaviour
{
    [SerializeField] SpriteRenderer m_HoverSprite;
    [SerializeField] Color m_BaseColor;
    [SerializeField] Color m_MoveableColor;
    [SerializeField] Color m_SelectedColor;
    [SerializeField] Color m_ReachableColor;

    private TileData m_TileData;

    public TileData TileData => m_TileData;

    public void OnMouseDown()
    {
        m_TileData.Board.OnTileClick(this);
    }

    public void SetState(TileState pType)
    {
        switch (pType)
        {
            case TileState.None:
                m_HoverSprite.color = m_BaseColor;
                break;
            case TileState.Highlighted:
                m_HoverSprite.color = m_ReachableColor;
                break;
            case TileState.Selected:
                m_HoverSprite.color = m_SelectedColor;
                break;
            case TileState.Moveable:
                m_HoverSprite.color = m_MoveableColor;
                break;
        }
    }
    public void SetTileData(TileData pTileData)
    {
        if(pTileData == null) ClearTileData();

        m_TileData = pTileData;
        m_TileData.OnPawnUpdated += UpdateDisplay;
        SetState(TileState.None);
    }

    private void ClearTileData()
    {
        m_TileData = null;
        m_TileData.OnPawnUpdated -= UpdateDisplay;
        SetState(TileState.None);
    }
    private void UpdateDisplay(Transform pPawnTransform)
    {
        pPawnTransform.SetParent(this.transform);
        pPawnTransform.SetAsFirstSibling();
        pPawnTransform.position = this.transform.position;
    }
}
