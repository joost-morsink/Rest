using System;
using System.Collections.Generic;
using System.Text;

namespace Biz.Morsink.Rest.AspNetCore.Utils
{
    public class CompactRestPathContainer
    {
        #region Helpers
        public static IConfigurator Configurator(Action<CompactRestPathContainer> action)
            => new ConfiguratorByAction(action);
        public interface IConfigurator
        {
            void Configure(CompactRestPathContainer container);
        }
        private class ConfiguratorByAction : IConfigurator
        {
            private readonly Action<CompactRestPathContainer> action;

            public ConfiguratorByAction(Action<CompactRestPathContainer> action)
            {
                this.action = action;
            }
            public void Configure(CompactRestPathContainer container)
            {
                action(container);
            }
        }
        #endregion

        private readonly Dictionary<string, string> prefixes;
        private readonly Dictionary<string, string> bases;
        private int counter = 0;

        public CompactRestPathContainer(IEnumerable<IConfigurator> configurators)
        {
            prefixes = new Dictionary<string, string>();
            bases = new Dictionary<string, string>();
            foreach (var configurator in configurators)
                configurator.Configure(this);
        }
        public string NextId()
        {
            var n = counter++;
            string str = null;
            while (n > 0)
            {
                str = str + ('a' + n % 26);
                n /= 26;
            }
            return str ?? "a";
        }
        public void RegisterPrefix(string basepath, string prefix = null)
        {
            prefix = prefix ?? NextId();
            prefixes[basepath] = prefix;
            bases[prefix] = basepath;
        }
        public string GetPrefix(string basePath, string prefixSuggestion = null)
        {
            if (!prefixes.ContainsKey(basePath))
                RegisterPrefix(basePath, prefixSuggestion);
            return prefixes[basePath];
        }
        public string ToPath(RestPath restPath)
        {
            var p = restPath.PathBase;
            if (p != null && !prefixes.ContainsKey(p))
                RegisterPrefix(p);
            return restPath.PathString;
        }
        public string ToSafeCurie(RestPath restPath)
        {
            var pathBase = restPath.PathBase;
            var prefix = GetPrefix(pathBase);
            return $"[{prefix}:{restPath.ToLocal().PathString}]";
        }
    }
}
