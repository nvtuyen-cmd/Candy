﻿// // ©2015 - 2024 Candy Smith
// // All rights reserved
// // Redistribution of this software is strictly not allowed.
// // Copy of this software can be obtained from unity asset store only.
// // THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// // IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// // FITNESS FOR A PARTICULAR PURPOSE AND NON-INFRINGEMENT. IN NO EVENT SHALL THE
// // AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// // LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// // OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// // THE SOFTWARE.

using System;
using System.Linq;
using SweetSugar.Scripts.Blocks;
using SweetSugar.Scripts.Core;
using UnityEngine;

namespace SweetSugar.Scripts.Items
{
    /// <summary>
    /// Item package
    /// </summary>
    public class ItemPackage : Item, IItemInterface//, ILongDestroyable
    {
        // public bool Combinable;
        public bool ActivateByExplosion;
        public bool StaticOnStart;

        public GameObject explosion;
        public GameObject explosion2;
        public GameObject circle;

        private Action callbackDestroy;
        // private bool animationFinished;
        private int priority = 0;
        private bool canBeStarted;

        private new Item GetItem => GetComponentInParent<Item>();

        public void Destroy(Item item1, Item item2)
        {
            if (GetItem.square.type == SquareTypes.WireBlock)
            {
                GetItem.square.DestroyBlock();
                GetItem.StopDestroy();

                return;
            }

            gameObject.AddComponent<GameBlocker>();
            item1.destroying = true;
            if (LevelManager.THIS.DebugSettings.DestroyLog)
                DebugLogKeeper.Log(" pre destroy " + " type " + GetItem.currentType + GetItem.GetInstanceID() + " " + item1?.GetInstanceID() + " : " + item2?.GetInstanceID(), DebugLogKeeper.LogType.Destroying);
            GetParentItem().GetComponent<ItemDestroyAnimation>().DestroyPackage(item1);
        }


        public Item GetParentItem()
        {
            return transform.GetComponentInParent<Item>();
        }

        public override void Check(Item item1, Item item2)
        {
            if ((item2.currentType == ItemsTypes.MULTICOLOR))
            {
                item2.Check(item2, item1);
            }

            if ((item2.currentType == ItemsTypes.HORIZONTAL_STRIPED || item2.currentType == ItemsTypes.VERTICAL_STRIPED))
            {
                item2.Check(item2, item1);
            }

            if (item2.currentType == ItemsTypes.MARMALADE)
            {
                item2.Check(item2, item1);
            }

            if (item1.currentType == ItemsTypes.PACKAGE && item2.currentType == ItemsTypes.PACKAGE)
            {
                item1.GetTopItemInterface().Destroy(item1, item2);
                item2.GetTopItemInterface().Destroy(item2, item1);
            }
        }
        public GameObject GetGameobject()
        {
            return gameObject;
        }
        public bool IsCombinable()
        {
            return Combinable;
        }
        public bool IsExplodable()
        {
            return ActivateByExplosion;
        }

        public void SetExplodable(bool setExplodable)
        {
            ActivateByExplosion = setExplodable;
        }

        public bool IsStaticOnStart()
        {
            return StaticOnStart;
        }

        public void SetOrder(int i)
        {
            var spriteRenderers = GetSpriteRenderers();
            var orderedEnumerable = spriteRenderers.OrderBy(x => x.sortingOrder).ToArray();
            for (int index = 0; index < orderedEnumerable.Length; index++)
            {
                var spr = orderedEnumerable[index];
                spr.sortingOrder = i + index;
            }
        }

        public bool IsAnimationFinished()
        {
            return animationFinished;
        }

        public int GetPriority()
        {
            return priority;
        }

        public bool CanBeStarted()
        {
            return canBeStarted;
        }
    }
}
