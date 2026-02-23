# T2FTree

轻量级树形数据结构 + UGUI 树状视图组件。

## 模块结构

```
T2FTree/Runtime/
  T2FTree.asmdef        数据层程序集（零依赖）
  Core/
    TreeElement.cs       树节点基类（Id, Name, Depth, Parent, Children）
    TreeModel.cs         树模型，提供增删改查、移动、祖先/后代查询
    TreeElementUtility.cs  树 ↔ 列表互转、深度校验等工具方法
  UGUI/
    T2FTree.UGUI.asmdef  视图层程序集（依赖 T2FTree）
    UITreeView.cs        树视图控制器：数据绑定、展开/折叠、选择、节点复用
    UITreeNode.cs        节点组件：缩进、展开箭头、点击交互
    UITreeNodePool.cs    节点对象池
```

## 快速使用

### 构造数据

```csharp
var data = new List<TreeElement>
{
    new TreeElement("Root",   -1, 0),
    new TreeElement("Item A",  0, 1),
    new TreeElement("Item B",  0, 2),
    new TreeElement("Child",   1, 3),  // Item A 的子节点
};
var model = new TreeModel<TreeElement>(data);
```

### 绑定视图

1. 场景中创建 `ScrollRect`，Content 挂载 `UITreeView`
2. 制作节点 Prefab 挂载 `UITreeNode`，拖入 `UITreeView.NodePrefab`
3. 代码绑定：

```csharp
treeView.Bind(model.Root);

// 自定义节点渲染
treeView.OnNodeBind += (node, element) =>
{
    node.Label.text = element.Name;
};

// 监听选择
treeView.OnSelectionChanged += element =>
{
    Debug.Log($"Selected: {element.Name}");
};
```

### 展开/折叠

```csharp
treeView.SetExpanded(nodeId, true);
treeView.ExpandAll();
treeView.CollapseAll();
```

## 设计要点

- **数据层零依赖**：`T2FTree.asmdef` 不引用任何外部程序集，可独立用于非 UI 场景
- **视图层独立**：`T2FTree.UGUI.asmdef` 仅依赖数据层，不耦合业务框架
- **节点复用**：通过对象池避免频繁 Instantiate/Destroy
