using System.Collections.Generic;
using System.Text;

public class ParseNode
{
    public string Name { get; }
    public List<ParseNode> Children { get; } = new List<ParseNode>();

    public ParseNode(string name)
    {
        Name = name;
    }

    public void AddChild(ParseNode child)
    {
        Children.Add(child);
    }

    // Рекурсивно строит текстовое представление дерева с отступами
    public string ToTreeString(int indent = 0)
    {
        var sb = new StringBuilder();
        sb.Append(' ', indent * 2)
          .AppendLine(Name);
        foreach (var child in Children)
            sb.Append(child.ToTreeString(indent + 1));
        return sb.ToString();
    }
}
