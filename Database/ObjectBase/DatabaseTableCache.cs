using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;

namespace ww.Tables
{
    internal static class DatabaseTableCache
    {
        public static readonly ConcurrentDictionary<Type, DatabaseTableAttribute> TableAttributes = new ConcurrentDictionary<Type, DatabaseTableAttribute>();
        public static readonly ConcurrentDictionary<MemberInfo, DatabaseAttribute> ColumnAttributes = new ConcurrentDictionary<MemberInfo, DatabaseAttribute>();
        public static readonly ConcurrentDictionary<Type, ReadOnlyCollection<MemberInfo>> FieldMemberInfos = new ConcurrentDictionary<Type, ReadOnlyCollection<MemberInfo>>();
        public static readonly ConcurrentDictionary<Type, ReadOnlyCollection<MemberInfo>> LazyMemberInfos = new ConcurrentDictionary<Type, ReadOnlyCollection<MemberInfo>>();
        public static readonly ConcurrentDictionary<Type, ReadOnlyCollection<PropertyInfo>> DetailPropertyInfos = new ConcurrentDictionary<Type, ReadOnlyCollection<PropertyInfo>>();


        public static DatabaseTableAttribute LoadTableAttribute(Type t)
        {
            return (DatabaseTableAttribute)t.GetCustomAttributes(typeof(DatabaseTableAttribute), false).FirstOrDefault();
        }

        public static DatabaseAttribute LoadColumnAttribute(MemberInfo mi)
        {
            return (DatabaseAttribute)mi.GetCustomAttributes(typeof(DatabaseAttribute), false).FirstOrDefault() ?? new DatabaseAttribute();
        }

        public static ReadOnlyCollection<MemberInfo> LoadAllFields(Type t)
        {
            IEnumerable<MemberInfo> infos = t.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(x => Attribute.IsDefined(x, typeof(DatabaseAttribute), false));
            infos = infos.Concat(t.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(x => Attribute.IsDefined(x, typeof(DatabaseAttribute), false)));
            return infos.ToList().AsReadOnly();
        }

        public static ReadOnlyCollection<MemberInfo> LoadAllLazyMemberInfos(Type t)
        {
            IEnumerable<MemberInfo> infos = t.GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(x => x.FieldType.IsGenericType && x.FieldType.GetGenericTypeDefinition() == typeof(Lazy<>));
            infos = infos.Concat(t.GetProperties(BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(x => x.PropertyType.IsGenericType && x.PropertyType.GetGenericTypeDefinition() == typeof(Lazy<>)));
            return infos.ToList().AsReadOnly();
        }

        public static ReadOnlyCollection<PropertyInfo> LoadAllDetailPropertyInfos(Type t)
        {
            return t.GetProperties(BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(x => x.PropertyType.IsSubclassOf(typeof(DatabaseTableDetail)))
                .ToList().AsReadOnly();
        }
    }
}
