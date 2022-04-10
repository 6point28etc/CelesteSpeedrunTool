﻿using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;

namespace Celeste.Mod.SpeedrunTool.SaveLoad;

internal static class DynDataUtils {
    // DynData
    private static readonly Dictionary<Type, object> CachedDataMaps = new();
    public static ConditionalWeakTable<object, object> IgnoreObjects = new();
    private static readonly HashSet<Type> IgnoreTypes = new();

    private static readonly Lazy<int> EmptyTableEntriesLength =
        new(() => new ConditionalWeakTable<object, object>().GetFieldValue<Array>("_entries").Length);

    private static readonly Lazy<int> EmptyTableFreeList = new(() => new ConditionalWeakTable<object, object>().GetFieldValue<int>("_freeList"));

    // DynamicData
    public static readonly object DynamicDataMap = typeof(DynamicData).GetFieldValue("_DataMap");
    private static readonly ConditionalWeakTable<object, object> DynamicDataObjects = new();
    private static ILHook dynamicDataHook;
    private static readonly bool RunningOnMono = Type.GetType("Mono.Runtime") != null;
    private static FastReflectionDelegate TryGetValueDelegate;
    private static FastReflectionDelegate AddDelegate;

    [Load]
    private static void Load() {
        dynamicDataHook = new ILHook(typeof(DynamicData).GetConstructor(new[] {typeof(Type), typeof(object), typeof(bool)}), il => {
            ILCursor ilCursor = new(il);
            ilCursor.Emit(OpCodes.Ldarg_2).EmitDelegate<Action<object>>(RecordDynamicDataObject);
        });
    }

    [Unload]
    private static void Unload() {
        dynamicDataHook?.Dispose();
    }

    public static void ClearCached() {
        IgnoreObjects = new ConditionalWeakTable<object, object>();
        IgnoreTypes.Clear();
    }

    public static void RecordDynamicDataObject(object target) {
        if (target != null && !DynamicDataObjects.TryGetValue(target, out object _)) {
            DynamicDataObjects.Add(target, null);
        }
    }

    public static bool ExistDynamicData(object target) {
        return DynamicDataObjects.TryGetValue(target, out object _);
    }

    public static bool NotExistDynData(Type type, out object dataMap) {
        if (IgnoreTypes.Contains(type)) {
            dataMap = null;
            return true;
        }

        dataMap = GetDataMap(type);

        bool result;
        if (RunningOnMono) {
            result = dataMap.GetFieldValue<int>("size") == 0;
        } else {
            result = dataMap.GetFieldValue<Array>("_entries").Length == EmptyTableEntriesLength.Value &&
                     dataMap.GetFieldValue<int>("_freeList") == EmptyTableFreeList.Value;
        }

        if (result) {
            IgnoreTypes.Add(type);
        }

        return result;
    }

    public static bool DataMapTryGetValue(object[] parameters) {
        TryGetValueDelegate ??= DynamicDataMap.GetType().GetMethodDelegate("TryGetValue");
        return (bool)TryGetValueDelegate(DynamicDataMap, parameters);
    }

    public static void DataMapAdd(object key, object value) {
        AddDelegate ??= DynamicDataMap.GetType().GetMethodDelegate("Add");
        AddDelegate(DynamicDataMap, key, value);
    }

    private static object GetDataMap(Type type) {
        if (CachedDataMaps.TryGetValue(type, out var result)) {
            return result;
        } else {
            result = typeof(DynData<>).MakeGenericType(type).GetFieldValue("_DataMap");
            return CachedDataMaps[type] = result;
        }
    }
}