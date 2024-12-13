using StudioScor.Utilities;
using UnityEngine;
using UnityEngine.Localization;

namespace PF.PJT.Duet
{

    [CreateAssetMenu(menuName = "Project/Duet/Data/new DamageType", fileName = "DamageType_")]
    public class Duet_DamageType : DamageType, IDisplayName
    {
        [Header(" [ Duet Damage Type ] ")]
        [SerializeField] private LocalizedString _name;
        [SerializeField] private bool _useColor = true;
        [SerializeField][SCondition(nameof(_useColor))] private Color _color = Color.white;
        
        string IDisplayName.Name
        {
            get
            {
                string name = _name.GetLocalizedString().ToBold();

                return _useColor ? name.ToColor(_color) : name;
            }
        }
    }
}
