using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace AssetStudio
{
    public sealed class Prefab : NamedObject
    {
        public Prefab(ObjectReader reader) : base(reader)
        {
        }
    }

    public class PrefabNode
    {
        public PrefabNode parent;

        public List<PrefabNode> children = new List<PrefabNode>();

        public Transform transform;

        public long id => transform.m_PathID;

        public PrefabNode(Transform transform)
        {
            this.transform = transform;
        }

        public string Dump()
        {
            StringBuilder sb = new StringBuilder();

            if (parent != null)
            {
                sb.AppendLine("parent:");
                sb.AppendLine($"\t{parent.id}");
            }

            sb.AppendLine("children:");

            Stack<(PrefabNode node, int indent)> stack = new Stack<(PrefabNode, int)>();

            PrefabNode mostParent = this;
            while (mostParent.parent != null)
            {
                mostParent = mostParent.parent;
            }
            stack.Push((mostParent, 0));

            while (stack.Count > 0)
            {
                var top = stack.Pop();

                for (int i = 0; i < top.indent; ++i)
                {
                    sb.Append("\t");
                }

                top.node.transform.m_GameObject.TryGet(out var gameObject);
                sb.AppendLine($"{gameObject.m_Name}:{top.node.id}");

                for (int i = top.node.children.Count - 1; i >= 0; --i)
                {
                    var child = top.node.children[i];
                    stack.Push((child, top.indent + 1));
                }

            }

            return sb.ToString();
        }

    }

    public class PrefabUtil
    {
        private static PrefabUtil _instance;

        public static PrefabUtil Instance
        { 
            get
            {
                if (_instance == null) _instance = new PrefabUtil();
                return _instance;
            }
        }

        private List<PrefabNode> m_gcCache = new List<PrefabNode>();

        public Dictionary<long, PrefabNode> m_prefabNodes = new Dictionary<long, PrefabNode>();

        public PrefabNode FillTransform(Transform transform)
        {
            var node = new PrefabNode(transform);
            m_prefabNodes.Add(node.id, node);
            m_gcCache.Add(node);

            if (transform.m_Father != null && m_prefabNodes.TryGetValue(transform.m_Father.m_PathID, out var fatherNode))
            {
                fatherNode.children.Add(node);
                node.parent = fatherNode;
            }

            foreach (var child in transform.m_Children)
            {
                if (child != null && m_prefabNodes.TryGetValue(child.m_PathID, out var childNode))
                {
                    node.children.Add(childNode);
                    childNode.parent = node;
                }
            }

            return node;
        }

        public void Clear()
        {
            m_prefabNodes.Clear();
        }
    }
}
