using UnityEngine.UIElements;

namespace Larje.UI
{
    public static class UIQuery
    {
        public static T Path<T>(VisualElement root, string path) where T : VisualElement
        {
            if (root == null || string.IsNullOrEmpty(path))
            {
                return null;
            }

            VisualElement current = root;
            string[] segments = path.Split('/');
            for (int i = 0; i < segments.Length && current != null; i++)
            {
                current = current.name == segments[i] && i == 0 ? current : current.Q(segments[i]);
            }

            return current as T;
        }
    }
}
