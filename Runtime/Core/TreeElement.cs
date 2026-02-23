using System;
using System.Collections.Generic;
using UnityEngine;

namespace T2F.Tree
{
    /// <summary>
    /// 树结构的基本元素类，用于定义树形节点的基本属性。
    /// </summary>
    [Serializable]
    public class TreeElement
    {
        [SerializeField] private int _id;
        [SerializeField] private string _name;
        [SerializeField] private int _depth;

        [NonSerialized] private TreeElement _parent;
        [NonSerialized] private List<TreeElement> _children = new();

        /// <summary>
        /// 节点的深度，用于表示节点层级。
        /// </summary>
        public int Depth
        {
            get => _depth;
            set => _depth = value;
        }

        /// <summary>
        /// 节点的父节点（非序列化，仅用于运行时）。
        /// </summary>
        public TreeElement Parent
        {
            get => _parent;
            set => _parent = value;
        }

        /// <summary>
        /// 节点的子节点列表（非序列化，仅用于运行时）。
        /// </summary>
        public List<TreeElement> Children
        {
            get => _children;
            set => _children = value;
        }

        /// <summary>
        /// 是否包含子节点。
        /// </summary>
        public bool HasChildren => _children is { Count: > 0 };

        /// <summary>
        /// 节点的名称。
        /// </summary>
        public string Name
        {
            get => _name;
            set => _name = value;
        }

        /// <summary>
        /// 节点的唯一 ID。
        /// </summary>
        public int Id
        {
            get => _id;
            set => _id = value;
        }

        public TreeElement()
        {
        }

        /// <summary>
        /// 带参数的构造函数，用于初始化节点属性。
        /// </summary>
        /// <param name="name">节点名称</param>
        /// <param name="depth">节点深度</param>
        /// <param name="id">节点唯一 ID</param>
        public TreeElement(string name, int depth, int id)
        {
            _name = name;
            _id = id;
            _depth = depth;
        }
    }
}
