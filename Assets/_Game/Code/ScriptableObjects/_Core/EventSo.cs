using System;
using UnityEngine;

[CreateAssetMenu(fileName = "New EventSo", menuName = "Core/EventSo")]
public class EventSo : ScriptableObject
{
    public event Action Event;

    public void Invoke()
    {
        Event?.Invoke();
    }
}