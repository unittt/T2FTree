using System;
using System.Collections.Generic;
using System.Linq;

namespace T2F.Tree
{
    /// <summary>
    /// TreeModel 用于管理树形数据结构。
    /// 通过维护一个线性列表和根节点，提供对树形结构的增删改查操作。
    /// </summary>
    /// <typeparam name="T">继承自 TreeElement 的数据类型。</typeparam>
    public class TreeModel<T> where T : TreeElement
    {
        private IList<T> _data;
        private T _root;
        private int _maxID;

        public T Root
        {
            get => _root;
            set => _root = value;
        }

        /// <summary>
        /// 数据模型发生变化时触发。
        /// </summary>
        public event Action ModelChanged;

        /// <summary>
        /// 数据总量。
        /// </summary>
        public int NumberOfDataElements => _data.Count;

        public TreeModel(IList<T> data)
        {
            SetData(data);
        }

        /// <summary>
        /// 根据 ID 查找树元素。
        /// </summary>
        public T Find(int id)
        {
            return _data.FirstOrDefault(element => element.Id == id);
        }

        /// <summary>
        /// 设置数据并重新初始化树模型。
        /// </summary>
        public void SetData(IList<T> data)
        {
            Init(data);
        }

        private void Init(IList<T> data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data), "输入数据不能为空。");

            _data = data;
            if (_data.Count > 0)
            {
                _root = TreeElementUtility.ListToTree(data);
                _maxID = _data.Max(e => e.Id);
            }
            else
            {
                _maxID = 0;
            }
        }

        /// <summary>
        /// 生成唯一 ID。
        /// </summary>
        public int GenerateUniqueID()
        {
            return ++_maxID;
        }

        /// <summary>
        /// 获取指定节点的所有祖先节点的 ID。
        /// </summary>
        public IList<int> GetAncestors(int id)
        {
            var parents = new List<int>();
            TreeElement current = Find(id);
            while (current?.Parent != null)
            {
                parents.Add(current.Parent.Id);
                current = current.Parent;
            }
            return parents;
        }

        /// <summary>
        /// 获取指定节点的所有后代中有子节点的节点 ID。
        /// </summary>
        public IList<int> GetDescendantsThatHaveChildren(int id)
        {
            T element = Find(id);
            return element != null ? GetParentsBelowStackBased(element) : new List<int>();
        }

        private IList<int> GetParentsBelowStackBased(TreeElement element)
        {
            var stack = new Stack<TreeElement>();
            stack.Push(element);

            var parentsWithChildren = new List<int>();
            while (stack.Count > 0)
            {
                var current = stack.Pop();
                if (current.HasChildren)
                {
                    parentsWithChildren.Add(current.Id);
                    foreach (var child in current.Children)
                    {
                        stack.Push(child);
                    }
                }
            }
            return parentsWithChildren;
        }

        /// <summary>
        /// 移除指定 ID 的节点。
        /// </summary>
        public void RemoveElements(IList<int> elementIDs)
        {
            var elements = _data.Where(element => elementIDs.Contains(element.Id)).ToList();
            RemoveElements(elements);
        }

        /// <summary>
        /// 移除指定的节点（自动过滤公共祖先，避免重复移除）。
        /// </summary>
        public void RemoveElements(IList<T> elements)
        {
            // 先过滤到公共祖先，避免删除父节点后子节点已脱离导致异常
            var ancestors = TreeElementUtility.FindCommonAncestorsWithinList(elements);

            foreach (var element in ancestors)
            {
                if (element == _root)
                    throw new ArgumentException("不能移除根节点。");

                element.Parent.Children.Remove(element);
                element.Parent = null;
            }

            TreeElementUtility.TreeToList(_root, _data);
            NotifyModelChanged();
        }

        /// <summary>
        /// 添加多个节点到指定父节点。
        /// </summary>
        public void AddElements(IList<T> elements, TreeElement parent, int insertPosition)
        {
            if (elements == null || parent == null)
                throw new ArgumentNullException("父节点或子节点不能为空。");

            if (parent.Children == null)
                parent.Children = new List<TreeElement>();

            parent.Children.InsertRange(insertPosition, elements);
            foreach (var element in elements)
            {
                element.Parent = parent;
                element.Depth = parent.Depth + 1;
                TreeElementUtility.UpdateDepthValues(element);
            }

            TreeElementUtility.TreeToList(_root, _data);
            NotifyModelChanged();
        }

        /// <summary>
        /// 添加根节点（仅在数据为空时使用）。
        /// </summary>
        public void AddRoot(T root)
        {
            if (_data.Count > 0)
                throw new InvalidOperationException("根节点只能在数据为空时添加。");

            root.Id = GenerateUniqueID();
            root.Depth = -1;
            _data.Add(root);
            _root = root;
        }

        /// <summary>
        /// 添加单个节点。
        /// </summary>
        public void AddElement(T element, TreeElement parent, int insertPosition)
        {
            if (element == null)
                throw new ArgumentNullException(nameof(element), "元素不能为空。");

            if (parent == null)
            {
                if (_root == null)
                    throw new InvalidOperationException("根节点未设置，无法添加子节点。");

                if (_root.Children == null)
                    _root.Children = new List<TreeElement>();

                _root.Children.Insert(insertPosition, element);
                element.Parent = _root;
            }
            else
            {
                if (parent.Children == null)
                    parent.Children = new List<TreeElement>();

                parent.Children.Insert(insertPosition, element);
                element.Parent = parent;
            }

            element.Depth = parent == null ? 0 : parent.Depth + 1;
            TreeElementUtility.UpdateDepthValues(_root);
            TreeElementUtility.TreeToList(_root, _data);

            NotifyModelChanged();
        }

        /// <summary>
        /// 移动节点到指定父节点（原子操作）。
        /// </summary>
        public void MoveElements(TreeElement newParent, int insertIndex, List<TreeElement> elements)
        {
            if (newParent.Children == null)
                newParent.Children = new List<TreeElement>();

            // 修正 insertIndex：如果从同一个父节点移动，
            // 移除元素后原来在 insertIndex 之前的元素减少，需要修正
            foreach (var element in elements)
            {
                if (element.Parent == newParent)
                {
                    int currentIndex = newParent.Children.IndexOf(element);
                    if (currentIndex >= 0 && currentIndex < insertIndex)
                    {
                        insertIndex--;
                    }
                }
                element.Parent.Children.Remove(element);
                element.Parent = newParent;
            }

            newParent.Children.InsertRange(insertIndex, elements);
            TreeElementUtility.UpdateDepthValues(_root);
            TreeElementUtility.TreeToList(_root, _data);
            NotifyModelChanged();
        }

        private void NotifyModelChanged()
        {
            ModelChanged?.Invoke();
        }
    }
}
