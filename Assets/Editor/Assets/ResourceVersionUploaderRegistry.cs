#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace CoinFlip.EditorTools
{
    public static class ResourceVersionUploaderRegistry
    {
        public sealed class Entry
        {
            public Type Type;
            public string DisplayName;
            public int Order;
            public IResourceVersionUploader Instance;
        }

        public static List<Entry> Discover()
        {
            var list = new List<Entry>();
            var types = TypeCache.GetTypesWithAttribute<ResourceVersionUploaderAttribute>();
            foreach (var type in types)
            {
                if (type == null || type.IsAbstract || !typeof(IResourceVersionUploader).IsAssignableFrom(type))
                {
                    continue;
                }

                var attr = (ResourceVersionUploaderAttribute)Attribute.GetCustomAttribute(
                    type, typeof(ResourceVersionUploaderAttribute));
                if (attr == null)
                {
                    continue;
                }

                IResourceVersionUploader instance;
                try
                {
                    instance = (IResourceVersionUploader)Activator.CreateInstance(type);
                }
                catch (Exception ex)
                {
                    UnityEngine.Debug.LogWarning(
                        $"[ResourceVersionUploader] Skip {type.Name}: {ex.Message}");
                    continue;
                }

                var name = !string.IsNullOrWhiteSpace(attr.DisplayName)
                    ? attr.DisplayName
                    : (!string.IsNullOrWhiteSpace(instance.DisplayName) ? instance.DisplayName : type.Name);
                list.Add(new Entry
                {
                    Type = type,
                    DisplayName = name,
                    Order = attr.Order,
                    Instance = instance
                });
            }

            return list.OrderBy(e => e.Order).ThenBy(e => e.DisplayName).ToList();
        }
    }
}
#endif
