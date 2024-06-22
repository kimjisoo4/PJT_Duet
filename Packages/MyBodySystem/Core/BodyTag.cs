using UnityEngine;

namespace StudioScor.BodySystem
{
    [CreateAssetMenu(menuName = "StudioScor/Body/new BodyTag", fileName = "Body_")]
    public class BodyTag : ScriptableObject
    {
        [Header(" [ Body ] ")]
        [SerializeField] private string _Name;
        [SerializeField, TextArea] private string _Description;

        public string Name => _Name;
        public string Description => _Description;
    }

}
