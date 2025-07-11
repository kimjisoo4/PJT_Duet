﻿using StudioScor.Utilities;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace StudioScor.GameplayTagSystem
{
    [DefaultExecutionOrder(GameplayTagSystemUtility.MAIN_ORDER)]
    [AddComponentMenu("StudioScor/GameplayTagSystem/GameplayTagSystem Component", order:0)]
    public class GameplayTagSystemComponent : BaseMonoBehaviour, IGameplayTagSystem
    {
        protected readonly Dictionary<GameplayTag, int> _ownedTags = new();
        protected readonly Dictionary<GameplayTag, int> _blockTags = new();

        public IReadOnlyDictionary<GameplayTag, int> OwnedTags => _ownedTags;
        public IReadOnlyDictionary<GameplayTag, int> BlockTags => _blockTags;


        public event IGameplayTagSystem.GameplayTagEventHandler OnGrantedOwnedTag;
        public event IGameplayTagSystem.GameplayTagEventHandler OnRemovedOwnedTag;
        public event IGameplayTagSystem.GameplayTagEventHandler OnAddedOwnedTag;
        public event IGameplayTagSystem.GameplayTagEventHandler OnSubtractedOwnedTag;
        
        public event IGameplayTagSystem.GameplayTagEventHandler OnGrantedBlockTag;
        public event IGameplayTagSystem.GameplayTagEventHandler OnRemovedBlockTag;
        public event IGameplayTagSystem.GameplayTagEventHandler OnAddedBlockTag;
        public event IGameplayTagSystem.GameplayTagEventHandler OnSubtractedBlockTag;
        
        public event IGameplayTagSystem.GameplayTagTriggerEventHandler OnTriggeredTag;

        public void ClearAllGameplayTags()
        {
            Log(nameof(ClearAllGameplayTags));

            if (_ownedTags.Count > 0)
            {
                var ownedTags = _ownedTags.Keys;

                for (int i = ownedTags.LastIndex(); i >= 0; i--)
                {
                    var ownedTag = ownedTags.ElementAt(i);

                    ClearOwnedTag(ownedTag);
                }

                _ownedTags.Clear();
            }

            if(_blockTags.Count > 0)
            {
                var blockTags = _blockTags.Keys;

                for (int i = blockTags.LastIndex(); i >= 0; i--)
                {
                    var blockTag = blockTags.ElementAt(i);

                    ClearBlockTag(blockTag);
                }

                _blockTags.Clear();
            }
            

            OnReset();
        }

        protected virtual void OnReset() { }

        public void TriggerTag(GameplayTag triggerTag, object data = null)
        {
            if (!triggerTag)
                return;

            Invoke_OnTriggeredTag(triggerTag, data);
        }
        
        public void AddOwnedTag(GameplayTag addTag)
        {
            if (!addTag)
                return;

            if (_ownedTags.ContainsKey(addTag))
            {
                _ownedTags[addTag] += 1;

                if (_ownedTags[addTag] == 1)
                {
                    Invoke_OnGrantedOwnedTag(addTag);
                }
            }
            else
            {
                _ownedTags.TryAdd(addTag, 1);

                Invoke_OnGrantedOwnedTag(addTag);
            }

            Invoke_OnAddedOwnedTag(addTag);
        }

        public void RemoveOwnedTag(GameplayTag removeTag)
        {
            if (!removeTag)
                return;

            if (_ownedTags.ContainsKey(removeTag))
            {
                _ownedTags[removeTag] -= 1;
            }
            else
            {
                _ownedTags.Add(removeTag, -1);
            }

            Invoke_OnSubtractedOwnedTag(removeTag);

            if (_ownedTags[removeTag] == 0)
            {
                Invoke_OnRemovedOwnedTag(removeTag);
            }
        }

        public void ClearOwnedTag(GameplayTag clearTag)
        {
            if (!clearTag)
                return;

            if (_ownedTags.TryGetValue(clearTag, out int count) && count != 0)
            {
                _ownedTags[clearTag] = 0;

                if (count > 0)
                    Invoke_OnSubtractedOwnedTag(clearTag);
                else
                    Invoke_OnAddedOwnedTag(clearTag);

                Invoke_OnRemovedOwnedTag(clearTag);
            }
        }
        public void AddBlockTag(GameplayTag addTag)
        {
            if (!addTag)
                return;

            if (_blockTags.ContainsKey(addTag))
            {
                _blockTags[addTag] += 1;

                if (_blockTags[addTag] == 1)
                {
                    Invoke_OnGrantedBlockTag(addTag);
                }
            }
            else
            {
                _blockTags.TryAdd(addTag, 1);

                Invoke_OnGrantedBlockTag(addTag);
            }

            Invoke_OnAddedBlockTag(addTag);
        }

        public void RemoveBlockTag(GameplayTag removeTag)
        {
            if (!removeTag)
                return;

            if (_blockTags.ContainsKey(removeTag))
            {
                _blockTags[removeTag] -= 1;
            }
            else
            {
                _blockTags.Add(removeTag, -1);
            }

            Invoke_OnSubtractedBlockTag(removeTag);

            if (_blockTags[removeTag] == 0)
            {
                Invoke_OnRemovedBlockTag(removeTag);
            }
        }

        public void ClearBlockTag(GameplayTag clearTag)
        {
            if (!clearTag)
                return;

            if(_blockTags.TryGetValue(clearTag, out int count) && count != 0)
            {
                _blockTags[clearTag] = 0;

                if (count > 0)
                    Invoke_OnSubtractedBlockTag(clearTag);
                else
                    Invoke_OnAddedBlockTag(clearTag);

                Invoke_OnRemovedBlockTag(clearTag);
            }
        }


        #region Invoke
        private void Invoke_OnTriggeredTag(GameplayTag triggerTag, object data = null)
        {
            Log($"{nameof(OnTriggeredTag)} :: {triggerTag}");

            OnTriggeredTag?.Invoke(this, triggerTag, data);
        }
        private void Invoke_OnAddedOwnedTag(GameplayTag addOwnedTag)
        {
            Log($"{nameof(OnAddedOwnedTag)} :: {addOwnedTag}");

            OnAddedOwnedTag?.Invoke(this, addOwnedTag);
        }
        private void Invoke_OnSubtractedOwnedTag(GameplayTag addOwnedTag)
        {
            Log($"{nameof(OnSubtractedOwnedTag)} :: {addOwnedTag}");

            OnSubtractedOwnedTag?.Invoke(this, addOwnedTag);
        }
        private void Invoke_OnGrantedOwnedTag(GameplayTag addNewOwnedTag)
        {
            Log($"{nameof(OnGrantedOwnedTag)} :: {addNewOwnedTag}");

            OnGrantedOwnedTag?.Invoke(this, addNewOwnedTag);
        }
        private void Invoke_OnRemovedOwnedTag(GameplayTag removeOwnedTag)
        {
            Log($"{nameof(OnRemovedOwnedTag)} :: {removeOwnedTag}");

            OnRemovedOwnedTag?.Invoke(this, removeOwnedTag);
        }
        private void Invoke_OnAddedBlockTag(GameplayTag addBlockTag)
        {
            Log($"{nameof(OnAddedBlockTag)} :: {addBlockTag}");

            OnAddedBlockTag?.Invoke(this, addBlockTag);
        }
        private void Invoke_OnSubtractedBlockTag(GameplayTag addBlockTag)
        {
            Log($"{nameof(OnSubtractedBlockTag)} :: {addBlockTag}");

            OnSubtractedBlockTag?.Invoke(this, addBlockTag);
        }
        private void Invoke_OnGrantedBlockTag(GameplayTag addNewBlockTag)
        {
            Log($"{nameof(OnGrantedBlockTag)} :: {addNewBlockTag}");

            OnGrantedBlockTag?.Invoke(this, addNewBlockTag);
        }
        private void Invoke_OnRemovedBlockTag(GameplayTag removeBlockTag)
        {
            Log($"{nameof(OnRemovedBlockTag)} :: {removeBlockTag}");

            OnRemovedBlockTag?.Invoke(this, removeBlockTag);
        }
        #endregion
    }
}
