using System.Collections.Generic;
using UnityEngine;

namespace T2F.Tree.UGUI
{
    /// <summary>
    /// UITreeNode 对象池，避免频繁 Instantiate/Destroy。
    /// </summary>
    public class UITreeNodePool
    {
        private readonly UITreeNode _prefab;
        private readonly RectTransform _parent;
        private readonly Queue<UITreeNode> _pool = new Queue<UITreeNode>();
        private readonly List<UITreeNode> _active = new List<UITreeNode>();

        public IReadOnlyList<UITreeNode> ActiveNodes => _active;

        public UITreeNodePool(UITreeNode prefab, RectTransform parent)
        {
            _prefab = prefab;
            _parent = parent;
        }

        /// <summary>
        /// 从池中取出或新建一个节点。
        /// </summary>
        public UITreeNode Get()
        {
            UITreeNode node;
            if (_pool.Count > 0)
            {
                node = _pool.Dequeue();
            }
            else
            {
                node = Object.Instantiate(_prefab, _parent);
            }

            node.gameObject.SetActive(true);
            _active.Add(node);
            return node;
        }

        /// <summary>
        /// 回收单个节点。
        /// </summary>
        public void Return(UITreeNode node)
        {
            node.gameObject.SetActive(false);
            _active.Remove(node);
            _pool.Enqueue(node);
        }

        /// <summary>
        /// 回收所有活跃节点。
        /// </summary>
        public void ReturnAll()
        {
            for (int i = _active.Count - 1; i >= 0; i--)
            {
                var node = _active[i];
                node.gameObject.SetActive(false);
                _pool.Enqueue(node);
            }
            _active.Clear();
        }
    }
}
