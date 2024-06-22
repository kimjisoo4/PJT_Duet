using UnityEngine;

namespace PF.PJT.Duet.Define
{
    public static class ProjectDefine
    {
        public class Layer
        {
            public static readonly int CHARACTER = LayerMask.NameToLayer("Character");
            public static readonly int INVISIBILITY = LayerMask.NameToLayer("Invisibility") ;
        }
    }
}
