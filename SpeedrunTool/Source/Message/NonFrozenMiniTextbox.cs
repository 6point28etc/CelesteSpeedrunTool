﻿using System;
using System.Reflection;
using Celeste.Mod.SpeedrunTool.Extensions;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;

namespace Celeste.Mod.SpeedrunTool.Message {
    [Tracked]
    public class NonFrozenMiniTextbox : MiniTextbox {
        private static readonly MethodInfo RoutineMethod = typeof(MiniTextbox).GetMethodInfo("Routine").GetStateMachineTarget();
        private static ILHook routineHook;

        private NonFrozenMiniTextbox(string dialogId, string message = null) : base(dialogId) {
            RemoveTag(Tags.HUD);
            AddTag(Tags.Global | TagsExt.SubHUD | Tags.FrozenUpdate | Tags.TransitionUpdate);

            if (message != null) {
                this.SetFieldValue(
                    typeof(MiniTextbox),
                    "text",
                    FancyText.Parse($"{{portrait {(IsPlayAsBadeline() ? "BADELINE" : "MADELINE")} left normal}}{message}", 1544, 2)
                );
            }
        }

        public static void Load() {
            IL.Celeste.MiniTextbox.Render += MiniTextboxOnRender;
            routineHook = new ILHook(RoutineMethod, QuicklyClose);
        }

        public static void Unload() {
            IL.Celeste.MiniTextbox.Render -= MiniTextboxOnRender;
            routineHook?.Dispose();
            routineHook = null;
        }

        private static void QuicklyClose(ILContext il) {
            ILCursor ilCursor = new(il);
            if (ilCursor.TryGotoNext(MoveType.After, i => i.OpCode == OpCodes.Ldarg_0, i => i.MatchLdcR4(3))) {
                ilCursor.Emit(OpCodes.Ldarg_0);
                ilCursor.Emit(OpCodes.Ldfld, RoutineMethod.DeclaringType.GetField("<>4__this"));
                ilCursor.EmitDelegate<Func<float, MiniTextbox, float>>((waitTimer, textbox) => textbox is NonFrozenMiniTextbox ? 1f : waitTimer);
            }
        }

        private static void MiniTextboxOnRender(ILContext il) {
            ILCursor ilCursor = new(il);
            if (ilCursor.TryGotoNext(MoveType.After, i => i.MatchCallvirt<Level>("get_FrozenOrPaused"))) {
                ilCursor.Emit(OpCodes.Ldarg_0).EmitDelegate<Func<bool, MiniTextbox, bool>>(
                    (frozenOrPaused, textbox) => textbox is not NonFrozenMiniTextbox && frozenOrPaused);
            }
        }

        public static void Show(Level level, string dialogId, string message) {
            level.Entities.FindAll<NonFrozenMiniTextbox>().ForEach(textbox => textbox.RemoveSelf());
            level.Add(new NonFrozenMiniTextbox(ChooseDialog(dialogId), message));
        }

        private static bool IsPlayAsBadeline() {
            if (Engine.Scene.GetPlayer() is { } player) {
                return player.Sprite.Mode == PlayerSpriteMode.MadelineAsBadeline;
            } else {
                return SaveData.Instance.Assists.PlayAsBadeline;
            }
        }

        private static string ChooseDialog(string madelineDialog) {
            bool isBadeline = IsPlayAsBadeline();

            if (madelineDialog == null) {
                // 仅用于 NonFrozenMiniTextbox 父类构造函数中确定头像，具体显示内容以 message 为准
                return isBadeline ? DialogIds.ClearStateDialogBadeline : DialogIds.ClearStateDialog;
            } else {
                return isBadeline ? $"{madelineDialog}_BADELINE" : madelineDialog;
            }
        }
    }
}