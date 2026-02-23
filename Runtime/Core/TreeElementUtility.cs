using System;
using System.Collections.Generic;

namespace T2F.Tree
{
    /// <summary>
    /// 提供树形结构和线性结构之间转换的工具方法。
    /// </summary>
    public static class TreeElementUtility
    {
        /// <summary>
        /// 将树形结构转换为线性列表（深度优先前序遍历）。
        /// </summary>
        public static void TreeToList<T>(T root, IList<T> result) where T : TreeElement
        {
            if (result == null)
                throw new NullReferenceException("结果列表为空。");

            result.Clear();
            var stack = new Stack<T>();
            stack.Push(root);

            while (stack.Count > 0)
            {
                T current = stack.Pop();
                result.Add(current);

                if (current.Children != null && current.Children.Count > 0)
                {
                    for (int i = current.Children.Count - 1; i >= 0; i--)
                    {
                        stack.Push((T)current.Children[i]);
                    }
                }
            }
        }

        /// <summary>
        /// 将线性列表转换为树形结构。
        /// 列表必须按深度优先前序排列，第一个元素为根节点（depth = -1）。
        /// </summary>
        public static T ListToTree<T>(IList<T> list) where T : TreeElement
        {
            ValidateDepthValues(list);

            // 重置所有节点的父子关系
            foreach (var element in list)
            {
                element.Parent = null;
                element.Children = new List<TreeElement>();
            }

            // 重建父子关系
            for (int parentIndex = 0; parentIndex < list.Count; parentIndex++)
            {
                var parent = list[parentIndex];
                int parentDepth = parent.Depth;

                for (int i = parentIndex + 1; i < list.Count; i++)
                {
                    if (list[i].Depth <= parentDepth)
                        break;

                    if (list[i].Depth == parentDepth + 1)
                    {
                        list[i].Parent = parent;
                        parent.Children.Add(list[i]);
                    }
                }
            }

            return list[0];
        }

        /// <summary>
        /// 验证深度值是否合法。
        /// </summary>
        public static void ValidateDepthValues<T>(IList<T> list) where T : TreeElement
        {
            if (list.Count == 0)
                throw new ArgumentException("列表不能为空。");

            if (list[0].Depth != -1)
                throw new ArgumentException("根节点的深度必须为 -1。");

            for (int i = 0; i < list.Count - 1; i++)
            {
                int depth = list[i].Depth;
                int nextDepth = list[i + 1].Depth;
                if (nextDepth > depth && nextDepth - depth > 1)
                    throw new ArgumentException(
                        $"深度值非法：索引 {i} 的深度为 {depth}，而索引 {i + 1} 的深度为 {nextDepth}。");
            }

            for (int i = 1; i < list.Count; ++i)
            {
                if (list[i].Depth < 0)
                    throw new ArgumentException($"索引 {i} 的深度值非法，仅根节点可以拥有负深度。");
            }
        }

        /// <summary>
        /// 更新指定节点及其所有子节点的深度值。
        /// </summary>
        public static void UpdateDepthValues<T>(T root) where T : TreeElement
        {
            if (root == null)
                throw new ArgumentNullException(nameof(root), "根节点为空。");

            if (!root.HasChildren)
                return;

            var stack = new Stack<TreeElement>();
            stack.Push(root);
            while (stack.Count > 0)
            {
                var current = stack.Pop();
                if (current.Children != null)
                {
                    foreach (var child in current.Children)
                    {
                        child.Depth = current.Depth + 1;
                        stack.Push(child);
                    }
                }
            }
        }

        /// <summary>
        /// 检查 child 是否为 elements 中任意节点的后代。
        /// </summary>
        private static bool IsChildOf<T>(T child, IList<T> elements) where T : TreeElement
        {
            while (child != null)
            {
                child = (T)child.Parent;
                if (elements.Contains(child))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// 从列表中过滤出公共祖先节点（移除所有已被其他元素包含的后代节点）。
        /// </summary>
        public static IList<T> FindCommonAncestorsWithinList<T>(IList<T> elements) where T : TreeElement
        {
            if (elements.Count == 1)
                return new List<T>(elements);

            var result = new List<T>(elements);
            result.RemoveAll(g => IsChildOf(g, elements));
            return result;
        }

        /// <summary>
        /// 在直接子节点中按名称查找。
        /// </summary>
        public static bool TryFindChildByName(this TreeElement treeElement, string name, out TreeElement child)
        {
            child = null;
            if (treeElement.Children != null)
            {
                foreach (var element in treeElement.Children)
                {
                    if (element.Name == name)
                    {
                        child = element;
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
