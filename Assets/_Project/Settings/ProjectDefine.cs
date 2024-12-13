using UnityEngine;

namespace PF.PJT.Duet.Define
{
    public static class ProjectDefine
    {
        public class Layer
        {
            /// <summary>
            /// 일반적인 캐릭터.
            /// </summary>
            public static readonly int CHARACTER = LayerMask.NameToLayer("Character");
            /// <summary>
            /// 다른 캐릭터를 관통할 수 있는 캐릭터.
            /// </summary>
            public static readonly int GHOSTCHARACTER = LayerMask.NameToLayer("GhostCharacter");
            /// <summary>
            /// 다른 캐릭터를 관통하고, 공격받지 않는 캐릭터.
            /// </summary>
            public static readonly int INVISIBILITY = LayerMask.NameToLayer("Invisibility") ;
        }
    }
}
