using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Cocona.Filters.Internal
{
    internal class FilterHelper
    {
        public static IReadOnlyList<IFilterFactory> GetFilterFactories(MethodInfo methodInfo)
        {
            return GetFilterFactoriesFromCustomAttributes(methodInfo.GetCustomAttributes(true).OfType<Attribute>())
                .Concat(GetFilterFactoriesFromCustomAttributes(methodInfo.DeclaringType.GetCustomAttributes(true).OfType<Attribute>()))
                .ToArray()!;
        }

        private static IEnumerable<IFilterFactory> GetFilterFactoriesFromCustomAttributes(IEnumerable<Attribute> attributes)
        {
            return attributes
                .Select(x =>
                {
                    if (x is IFilterFactory factory) return factory;
                    if (x is IFilterMetadata filter) return new InstancedFilterFactory(filter, (filter is IOrderedFilter orderedFilter ? orderedFilter.Order : 0));
                    return null;
                })
                .Where(x => x != null)!;
        }

        public static IReadOnlyList<T> GetFilters<T>(MethodInfo methodInfo, IServiceProvider serviceProvider)
            where T: IFilterMetadata
        {
            return GetFilterFactories(methodInfo)
                .OrderBy(x => x is IOrderedFilter orderedFilter ? orderedFilter.Order : 0)
                .Select(x => x.CreateInstance(serviceProvider))
                .OfType<T>()
                .ToArray();
        }

        public static IReadOnlyList<T> GetFilters<T>(Type t, IServiceProvider serviceProvider)
            where T : IFilterMetadata
        {
            return GetFilterFactoriesFromCustomAttributes(t.GetCustomAttributes(true).OfType<Attribute>())
                .OrderBy(x => x is IOrderedFilter orderedFilter ? orderedFilter.Order : 0)
                .Select(x => x.CreateInstance(serviceProvider))
                .OfType<T>()
                .ToArray();
        }
    }
}
