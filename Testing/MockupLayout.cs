using System;

namespace Larje.UI.Testing
{
    [Serializable]
    public sealed class MockupLayout
    {
        public string id;
        public string kind;
        public string state;
        public int[] referenceResolution;
        public string[] warnings;
        public MockupNode[] nodes;
    }

    [Serializable]
    public sealed class MockupNode
    {
        public string path;
        public string type;
        public string text;
        public MockupRect rect;
    }

    [Serializable]
    public sealed class MockupRect
    {
        public float x;
        public float y;
        public float w;
        public float h;
    }

    [Serializable]
    public sealed class MockupManifest
    {
        public float tolerance = 16f;
        public MockupItem[] items;
    }

    [Serializable]
    public sealed class MockupItem
    {
        public string id;
        public string kind;
        public string key;
        public string uxml;
        public string unity;
        public string[] states;
    }
}
