using System.Collections.Generic;
using Celeste.Mod.SpeedrunTool.Extensions;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.SpeedrunTool.SaveLoad.Actions {
    public class BounceBlockAction : AbstractEntityAction {
        private Dictionary<EntityID, BounceBlock> savedBounceBlocks = new Dictionary<EntityID, BounceBlock>();

        public override void OnQuickSave(Level level) {
            savedBounceBlocks = level.Entities.GetDictionary<BounceBlock>();
        }

        private void RestoreBounceBlockState(On.Celeste.BounceBlock.orig_ctor_EntityData_Vector2 orig, BounceBlock self,
            EntityData data,
            Vector2 offset) {
            EntityID entityId = data.ToEntityId();
            self.SetEntityId(entityId);
            orig(self, data, offset);

            if (IsLoadStart && savedBounceBlocks.ContainsKey(entityId)) {
                BounceBlock savedBounceBlock = savedBounceBlocks[entityId];
                self.Position = savedBounceBlock.Position;
                self.Collidable = savedBounceBlock.Collidable;
                self.CopyFields(typeof(BounceBlock), savedBounceBlock, "bounceDir");
                self.CopyFields(typeof(BounceBlock), savedBounceBlock, "state");
                self.CopyFields(typeof(BounceBlock), savedBounceBlock, "moveSpeed");
                self.CopyFields(typeof(BounceBlock), savedBounceBlock, "windUpStartTimer");
                self.CopyFields(typeof(BounceBlock), savedBounceBlock, "windUpProgress");
                self.CopyFields(typeof(BounceBlock), savedBounceBlock, "bounceEndTimer");
                self.CopyFields(typeof(BounceBlock), savedBounceBlock, "bounceLift");
                self.CopyFields(typeof(BounceBlock), savedBounceBlock, "reappearFlash");
                self.CopyFields(typeof(BounceBlock), savedBounceBlock, "debrisDirection");
                self.CopyFields(typeof(BounceBlock), savedBounceBlock, "iceMode");
                self.CopyFields(typeof(BounceBlock), savedBounceBlock, "iceModeNext");
                self.CopyFields(typeof(BounceBlock), savedBounceBlock, "respawnTimer");
            }
        }

        public override void OnClear() {
            savedBounceBlocks.Clear();
        }

        public override void OnLoad() {
            On.Celeste.BounceBlock.ctor_EntityData_Vector2 += RestoreBounceBlockState;
        }

        public override void OnUnload() {
            On.Celeste.BounceBlock.ctor_EntityData_Vector2 -= RestoreBounceBlockState;
        }
    }
}