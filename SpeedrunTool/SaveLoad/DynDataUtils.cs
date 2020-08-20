﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Celeste.Mod.SpeedrunTool.Extensions;
using MonoMod.Utils;

namespace Celeste.Mod.SpeedrunTool.SaveLoad {
    internal static class DynDataUtils {
        private static object CreateDynData(object obj, Type targetType) {
            string key = $"DynDataUtils-CreateDynData-{targetType.FullName}";

            ConstructorInfo constructorInfo = targetType.GetExtendedDataValue<ConstructorInfo>(key);

            if (constructorInfo == null) {
                constructorInfo = typeof(DynData<>).MakeGenericType(targetType).GetConstructor(new[] {targetType});
                targetType.SetExtendedDataValue(key, constructorInfo);
            }

            return constructorInfo?.Invoke(new[] {obj});
        }

        public static IDictionary GetDataMap(Type type) {
            string key = $"DynDataUtils-GetDataMap-{type}";

            FieldInfo fieldInfo = type.GetExtendedDataValue<FieldInfo>(key);

            if (fieldInfo == null) {
                fieldInfo = typeof(DynData<>).MakeGenericType(type)
                    .GetField("_DataMap", BindingFlags.Static | BindingFlags.NonPublic);
                type.SetExtendedDataValue(key, fieldInfo);
            }

            return fieldInfo?.GetValue(null) as IDictionary;
        }

        public static Dictionary<string, object> GetDate(object obj, Type targetType) {
            return CreateDynData(obj, targetType)?.GetPropertyValue("Data") as Dictionary<string, object>;
        }
    }
}