using System.Collections.Generic;
using System.Xml;
using UnityEngine;

namespace SeaWarsOnline.Core.Localization
{
    public class LocalizationManager
    {

        private readonly Dictionary<string, string> _cached = new Dictionary<string, string>();

        private static LocalizationManager _instance;

        private LocalizationManager()
        {
            Language = "English";
        }

        public static LocalizationManager Instance
        {
            get { return _instance ?? (_instance = new LocalizationManager()); }
        }

        private string _language;
        private XmlDocument _currentResource;

        public string Language
        {
            get { return _language; }
            set
            {
                if (_language != value)
                {
                    LoadResources(value);

                    _language = value;
                }
            }
        }

        private void LoadResources(string resourceName)
        {
            _cached.Clear();

            var xmlData = (TextAsset)Resources.Load("Localization/" + resourceName, typeof(TextAsset));

            _currentResource = new XmlDocument();
            _currentResource.LoadXml(xmlData.text);
        }

        public string GetString(string category, string name)
        {
            if (_currentResource == null)
                return "??LANG??";

            var cacheId = category + "/" + name;

            if (_cached.ContainsKey(cacheId))
                return _cached[cacheId];

            var nodes = _currentResource.SelectNodes("/Localization/" + category + "/String[@name='" + name + "']/@value");

            if (nodes == null || nodes.Count == 0)
                return "?" + name + "?";

            var value = nodes[0].Value.Replace("\\n", "\n");

            _cached.Add(cacheId, value);

            return value;
        }

        public bool HasString(string category, string name)
        {
            if (_currentResource == null)
                return false;

            var cacheId = category + "/" + name;

            if (_cached.ContainsKey(cacheId))
                return true;

            var nodes = _currentResource.SelectNodes("/Localization/" + category + "/String[@name='" + name + "']/@value");

            if (nodes == null || nodes.Count == 0)
                return false;

            var value = nodes[0].Value.Replace("\\n", "\n");

            _cached.Add(cacheId, value);

            return true;
        }

    }
}
