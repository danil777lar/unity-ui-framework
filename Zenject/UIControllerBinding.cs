using System;

namespace Larje.UI.Zenject
{
    public sealed class UIControllerBinding
    {
        public string Key { get; }
        public Type Type { get; }

        public UIControllerBinding(string key, Type type)
        {
            Key = key;
            Type = type;
        }
    }
}
