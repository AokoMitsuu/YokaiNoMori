using UnityEngine;
using UnityEngine.UI;

public class Tile : MonoBehaviour
{
    [SerializeField] Image m_HoverImage;
    [SerializeField] Color m_BaseColor;
    [SerializeField] Color m_MoveableColor;
    [SerializeField] Color m_SelectedColor;
    [SerializeField] Color m_ReachableColor;

    private Team m_TeamBackRow = Team.None;

    public Team TeamBackRow => m_TeamBackRow;

    public void Init(Team pTeamBackRow)
    {
        m_TeamBackRow = pTeamBackRow;
        SetState(TileState.None);
    }

    public void SetState(TileState pType)
    {
        switch (pType)
        {
            case TileState.None:
                m_HoverImage.color = m_BaseColor;
                break;
            case TileState.Highlighted:
                m_HoverImage.color = m_ReachableColor;
                break;
            case TileState.Selected:
                m_HoverImage.color = m_SelectedColor;
                break;
            case TileState.Moveable:
                m_HoverImage.color = m_MoveableColor;
                break;
        }
    }

    public void Clear()
    {
        SetState(TileState.None);
    }
}

public enum TileState
{
    None,
    Highlighted,
    Selected,
    Moveable
}