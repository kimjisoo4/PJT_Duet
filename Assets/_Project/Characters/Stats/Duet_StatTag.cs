using StudioScor.StatSystem;
using StudioScor.Utilities;
using UnityEngine;

namespace PF.PJT.Duet
{
    [CreateAssetMenu(menuName = "Project/Duet/Stat/new StatTag", fileName = "Stat_")]
    public class Duet_StatTag : StatTag, IDisplayName
    {
        [Header(" [ Duet Stat Tag ] ")]
        [SerializeField] private bool _useColor = false;
        [SerializeField][SCondition(nameof(_useColor))] private Color _color = Color.white;
        string IDisplayName.Name => _useColor ? $"<b><color=#{ColorUtility.ToHtmlStringRGBA(_color)}> {Name} </color></b>" : $"<b>{Name}</b>";
    }
}
