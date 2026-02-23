using UnityEngine;
using UnityEngine.UI;

namespace T2F.Tree.UGUI
{
    /// <summary>
    /// 树节点 UI 组件，挂载在节点 Prefab 上。
    /// 负责显示单个节点并处理点击/展开交互。
    /// </summary>
    public class UITreeNode : MonoBehaviour
    {
        [Header("节点结构")]
        [SerializeField] private RectTransform _indentRect;
        [SerializeField] private Button _expandButton;
        [SerializeField] private RectTransform _expandArrow;
        [SerializeField] private Text _label;
        [SerializeField] private Image _background;

        private TreeElement _element;
        private UITreeView _treeView;
        private bool _expanded;

        public RectTransform IndentRect => _indentRect;
        public Button ExpandButton => _expandButton;
        public RectTransform ExpandArrow => _expandArrow;
        public Text Label => _label;
        public Image Background => _background;

        /// <summary>
        /// 当前绑定的数据元素。
        /// </summary>
        public TreeElement Element => _element;

        /// <summary>
        /// 当前节点 ID。
        /// </summary>
        public int NodeId => _element != null ? _element.Id : -1;

        private void Awake()
        {
            if (_expandButton != null)
                _expandButton.onClick.AddListener(OnExpandClicked);

            // 点击节点背景选中
            var bgButton = _background != null ? _background.GetComponent<Button>() : null;
            if (bgButton != null)
                bgButton.onClick.AddListener(OnNodeClicked);
        }

        /// <summary>
        /// 由 UITreeView 调用，配置节点显示。
        /// </summary>
        public void Setup(UITreeView treeView, TreeElement element, float indent, bool hasChildren, bool expanded, bool selected)
        {
            _treeView = treeView;
            _element = element;
            _expanded = expanded;

            // 缩进
            if (_indentRect != null)
                _indentRect.sizeDelta = new Vector2(indent, _indentRect.sizeDelta.y);

            // 展开箭头
            if (_expandArrow != null)
            {
                _expandArrow.gameObject.SetActive(hasChildren);
                if (hasChildren)
                    _expandArrow.localRotation = Quaternion.Euler(0, 0, expanded ? -90f : 0f);
            }

            // 默认文本
            if (_label != null)
                _label.text = element.Name ?? string.Empty;
        }

        private void OnExpandClicked()
        {
            if (_treeView != null && _element != null)
            {
                _treeView.SetExpanded(_element.Id, !_expanded);
            }
        }

        private void OnNodeClicked()
        {
            if (_treeView != null && _element != null)
            {
                _treeView.Select(_element.Id);
            }
        }
    }
}
