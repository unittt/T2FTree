using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace T2F.Tree.UGUI
{
    /// <summary>
    /// 树视图控制器，负责将 TreeElement 树结构渲染为 UGUI 节点列表。
    /// 管理展开/折叠状态、节点复用和选择。
    /// </summary>
    public class UITreeView : MonoBehaviour
    {
        [Header("引用")]
        [SerializeField] private ScrollRect _scrollRect;
        [SerializeField] private RectTransform _content;
        [SerializeField] private UITreeNode _nodePrefab;

        [Header("布局")]
        [SerializeField] private float _nodeHeight = 30f;
        [SerializeField] private float _indentWidth = 20f;

        private TreeElement _root;
        private UITreeNodePool _pool;
        private readonly List<TreeElement> _visibleList = new List<TreeElement>();
        private readonly HashSet<int> _expandedIds = new HashSet<int>();
        private int _selectedId = -1;

        /// <summary>
        /// 节点绑定回调，业务层用于自定义节点渲染。
        /// </summary>
        public event Action<UITreeNode, TreeElement> OnNodeBind;

        /// <summary>
        /// 选择变化回调。
        /// </summary>
        public event Action<TreeElement> OnSelectionChanged;

        /// <summary>
        /// 节点展开/折叠变化回调。
        /// </summary>
        public event Action<TreeElement, bool> OnNodeExpandChanged;

        /// <summary>
        /// 当前选中的元素。
        /// </summary>
        public TreeElement SelectedElement
        {
            get
            {
                if (_selectedId < 0 || _root == null) return null;
                return FindElement(_root, _selectedId);
            }
        }

        private void Awake()
        {
            if (_nodePrefab != null && _content != null)
            {
                _nodePrefab.gameObject.SetActive(false);
                _pool = new UITreeNodePool(_nodePrefab, _content);
            }
        }

        /// <summary>
        /// 绑定树数据并重建视图。
        /// </summary>
        public void Bind(TreeElement root)
        {
            _root = root;
            _expandedIds.Clear();
            _selectedId = -1;
            Rebuild();
        }

        /// <summary>
        /// 重建可见节点列表并刷新显示。
        /// </summary>
        public void Rebuild()
        {
            if (_root == null || _pool == null) return;

            // 1. 生成可见列表
            _visibleList.Clear();
            BuildVisibleList(_root);

            // 2. 回收所有活跃节点
            _pool.ReturnAll();

            // 3. 设置 Content 高度
            int count = _visibleList.Count;
            _content.sizeDelta = new Vector2(_content.sizeDelta.x, count * _nodeHeight);

            // 4. 为每个可见节点创建 UI
            for (int i = 0; i < count; i++)
            {
                var element = _visibleList[i];
                var node = _pool.Get();
                var rt = (RectTransform)node.transform;

                // 5. 定位
                rt.anchorMin = new Vector2(0, 1);
                rt.anchorMax = new Vector2(1, 1);
                rt.pivot = new Vector2(0.5f, 1);
                rt.anchoredPosition = new Vector2(0, -i * _nodeHeight);
                rt.sizeDelta = new Vector2(0, _nodeHeight);

                // 6. 配置节点
                float indent = element.Depth * _indentWidth;
                bool hasChildren = element.HasChildren;
                bool expanded = _expandedIds.Contains(element.Id);
                bool selected = element.Id == _selectedId;

                node.Setup(this, element, indent, hasChildren, expanded, selected);

                // 7. 业务自定义渲染
                OnNodeBind?.Invoke(node, element);
            }
        }

        private void BuildVisibleList(TreeElement element)
        {
            if (element.Children == null) return;

            foreach (var child in element.Children)
            {
                _visibleList.Add(child);

                if (child.HasChildren && _expandedIds.Contains(child.Id))
                {
                    BuildVisibleList(child);
                }
            }
        }

        /// <summary>
        /// 设置节点展开/折叠状态。
        /// </summary>
        public void SetExpanded(int nodeId, bool expanded)
        {
            bool changed;
            if (expanded)
                changed = _expandedIds.Add(nodeId);
            else
                changed = _expandedIds.Remove(nodeId);

            if (changed)
            {
                var element = FindElement(_root, nodeId);
                OnNodeExpandChanged?.Invoke(element, expanded);
                Rebuild();
            }
        }

        /// <summary>
        /// 查询节点是否展开。
        /// </summary>
        public bool IsExpanded(int nodeId)
        {
            return _expandedIds.Contains(nodeId);
        }

        /// <summary>
        /// 展开所有节点。
        /// </summary>
        public void ExpandAll()
        {
            CollectAllParentIds(_root, _expandedIds);
            Rebuild();
        }

        /// <summary>
        /// 折叠所有节点。
        /// </summary>
        public void CollapseAll()
        {
            _expandedIds.Clear();
            Rebuild();
        }

        /// <summary>
        /// 选中指定节点。
        /// </summary>
        public void Select(int nodeId)
        {
            if (_selectedId == nodeId) return;

            _selectedId = nodeId;
            var element = FindElement(_root, nodeId);
            OnSelectionChanged?.Invoke(element);
            Rebuild();
        }

        private static TreeElement FindElement(TreeElement root, int id)
        {
            if (root.Id == id) return root;

            if (root.Children != null)
            {
                foreach (var child in root.Children)
                {
                    var found = FindElement(child, id);
                    if (found != null) return found;
                }
            }
            return null;
        }

        private static void CollectAllParentIds(TreeElement element, HashSet<int> ids)
        {
            if (element.HasChildren)
            {
                ids.Add(element.Id);
                foreach (var child in element.Children)
                {
                    CollectAllParentIds(child, ids);
                }
            }
        }
    }
}
