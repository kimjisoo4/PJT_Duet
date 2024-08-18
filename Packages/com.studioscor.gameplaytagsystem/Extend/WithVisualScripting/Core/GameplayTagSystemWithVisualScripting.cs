﻿#if SCOR_ENABLE_VISUALSCRIPTING

namespace StudioScor.GameplayTagSystem
{
    public class TriggerTagData
    {
        public TriggerTagData(GameplayTag gameplayTag, object data = null)
        {
            this.triggerTag = gameplayTag;
            this.data = data;
        }

        private readonly GameplayTag triggerTag;
        private readonly object data;

        public GameplayTag TriggerTag => triggerTag;
        public object Data => data;
    }
    public static class GameplayTagSystemWithVisualScripting
    {
        public const string TRIGGER_TAG = "TriggerTag";

        public const string ADD_OWNED_TAG = "AddOwnedTag";
        public const string SUBTRACT_OWNED_TAG = "SubtractOwnedTag";
        public const string REMOVE_OWNED_TAG = "RemoveOwnedTag";

        public const string ADD_BLOCK_TAG = "AddBlockTag";
        public const string SUBTRACT_BLOCK_TAG = "SubtractBlockTag";
        public const string REMOVE_BLOCK_TAG = "RemoveBlockTag";

        public const string GRANT_OWNED_TAG = "GrantOwnedTag";
        public const string GRANT_BLOCK_TAG = "GrantBlockTag";
    }
}
#endif