using System;
using Celeste.Mod.SpeedrunTool.SaveLoad.EntityIdPlus;
using Mono.Cecil.Cil;
using MonoMod.Cil;

namespace Celeste.Mod.SpeedrunTool.SaveLoad.RestoreActions.EntityActions {
    public class KeyRestoreAction : RestoreAction {
        public KeyRestoreAction() : base(typeof(Key)) { }
        public override void OnLoad() {
            IL.Celeste.Key.ctor_Player_EntityID += KeyOnCtor_Player_EntityID;
        }

        public override void OnUnload() {
            IL.Celeste.Key.ctor_Player_EntityID -= KeyOnCtor_Player_EntityID;
        }

        // 解决吃下钥匙马上保存后出现两把钥匙的问题
        private void KeyOnCtor_Player_EntityID(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            if (cursor.TryGotoNext(MoveType.After, i => i.MatchCallvirt<Leader>("GainFollower"))) {
                Instruction skipInstruction = cursor.Next;

                if (cursor.TryGotoPrev(i => i.OpCode == OpCodes.Ldarg_1)) {
                    cursor.Emit(OpCodes.Ldarg_0).Emit(OpCodes.Ldarg_0).EmitDelegate<Action<Key, EntityID>>(
                        (key, id) => {
                            key.SetEntityId2(id);
                        });
                    cursor.EmitDelegate<Func<bool>>(() => IsLoadStart);
                    cursor.Emit(OpCodes.Brtrue, skipInstruction);
                    
                }
            }
        }
    }
}