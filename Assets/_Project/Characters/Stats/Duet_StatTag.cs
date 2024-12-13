using StudioScor.StatSystem;
using StudioScor.Utilities;
using UnityEngine;
using UnityEngine.Localization;

namespace PF.PJT.Duet
{
    [CreateAssetMenu(menuName = "Project/Duet/Stat/new StatTag", fileName = "Stat_")]
    public class Duet_StatTag : StatTag, IDisplayName
    {
        [Header(" [ Duet Stat Tag ] ")]
        [SerializeField] private LocalizedString _name;
        [SerializeField] private bool _useColor = false;
        [SerializeField][SCondition(nameof(_useColor))] private Color _color = Color.white;
        string IDisplayName.Name
        {
            get
            {
                string statName = _name.GetLocalizedString();

                return _useColor ? $"<b><color=#{ColorUtility.ToHtmlStringRGBA(_color)}>{statName}</color></b>" : $"<b>{statName}</b>";
            }
        }
    }
}
