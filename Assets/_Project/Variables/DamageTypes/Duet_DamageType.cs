using StudioScor.Utilities;
using UnityEngine;

namespace PF.PJT.Duet
{

    [CreateAssetMenu(menuName = "Project/Duet/Data/new DamageType", fileName = "DamageType_")]
    public class Duet_DamageType : DamageType, IDisplayName
    {
        [Header(" [ Duet Damage Type ] ")]
        [SerializeField] private bool _useColor = true;
        [SerializeField][SCondition(nameof(_useColor))] private Color _color = Color.white;
        
        string IDisplayName.Name => _useColor ? $"<b><color=#{ColorUtility.ToHtmlStringRGBA(_color)}> {Name} </color></b>" : $"<b>{Name}</b>";
    }
}
