using System;
using Celeste.Mod.SpeedrunTool.Extensions;
using Monocle;

namespace Celeste.Mod.SpeedrunTool.SaveLoad.RestoreActions.EntityActions {
    // TODO TriggerSpikesOriginal
    public class TriggerSpikesRestoreAction : RestoreAction {
        public TriggerSpikesRestoreAction() : base(typeof(TriggerSpikes)) { }

        public override void AfterEntityCreateAndUpdate1Frame(Entity loadedEntity, Entity savedEntity) {
            TriggerSpikes loaded = (TriggerSpikes) loadedEntity;
            TriggerSpikes saved = (TriggerSpikes) savedEntity;
            
            Array loadedSpikes = loaded.GetField("spikes") as Array;
            Array savedSpikes = saved.GetField("spikes") as Array;
            Array newSpikes = Activator.CreateInstance(loadedSpikes.GetType(), loadedSpikes.Length) as Array;

            for (int i = 0; i < loadedSpikes.Length; i++) {
                object spike = loadedSpikes.GetValue(i);
                object savedSpike = savedSpikes.GetValue(i);
                savedSpike.CopyFields(spike, "Parent");
                newSpikes.SetValue(savedSpike, i);
            }

            loaded.SetField("spikes", newSpikes);
        }
    }
}