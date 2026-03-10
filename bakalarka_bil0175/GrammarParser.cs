using Blazor.Diagrams;
using Blazor.Diagrams.Core.Geometry;
using Blazor.Diagrams.Core.Models;

namespace bakalarka_bil0175;

public static class GrammarParser
{
    private static Dictionary<string, string> _rules = new();
    private static Dictionary<string, NodeModel> _existingNodes = new();

    public static void Parse(string input, BlazorDiagram diagram)
    {
        _rules.Clear();
        _existingNodes.Clear();

        var lines = input.Split(new[] { '\n', '\r', ';' }, StringSplitOptions.RemoveEmptyEntries);
        
        foreach (var line in lines)
        {
            if (!line.Contains("->")) continue;
            var parts = line.Split("->");
            _rules[parts[0].Trim()] = parts[1].Trim();
        }

        if (!_rules.Any()) return;

        
        string rootSymbol = _rules.Keys.First();
        BuildSymbol(rootSymbol, diagram);
    }

    private static NodeModel? BuildSymbol(string symbol, BlazorDiagram diagram)
    {
        symbol = symbol.Trim();

        
        if (char.IsLower(symbol[0]))
        {
            var startNode = new GateNode("START", new Point(0, 0), false, symbol);
            diagram.Nodes.Add(startNode);
            return startNode;
        }
        
        if (_rules.ContainsKey(symbol))
        {
            return BuildExpression(_rules[symbol], diagram);
        }

        var unknownNode = new GateNode("START", new Point(0, 0), false, symbol);
        diagram.Nodes.Add(unknownNode);
        return unknownNode;
    }

    private static NodeModel? BuildExpression(string expr, BlazorDiagram diagram)
    {
        expr = expr.Trim();

        var orParts = expr.Split('|', StringSplitOptions.RemoveEmptyEntries);

        if (orParts.Length > 1)
        {
            var orGate = new GateNode("OR", new Point(0, 0));
            diagram.Nodes.Add(orGate);

            for (int i = 0; i < orParts.Length; i++)
            {
                var partNode = BuildAndSequence(orParts[i].Trim(), diagram);
                Connect(partNode, orGate, diagram, i);
            }
            return orGate;
        }

        return BuildAndSequence(expr, diagram);
    }

    private static NodeModel? BuildAndSequence(string sequence, BlazorDiagram diagram)
    {
        var symbols = sequence.ToCharArray().Select(c => c.ToString()).ToList();

        if (symbols.Count > 1)
        {
            var andGate = new GateNode("AND", new Point(0, 0));
            diagram.Nodes.Add(andGate);

            for (int i = 0; i < symbols.Count; i++)
            {
                var symbolNode = BuildSymbol(symbols[i], diagram);
                Connect(symbolNode, andGate, diagram, i);
            }
            return andGate;
        }

        return BuildSymbol(symbols[0], diagram);
    }

    private static void Connect(NodeModel? source, NodeModel? target, BlazorDiagram diagram, int idx)
    {
        if (source == null || target == null) return;

        var pFrom = source.Ports.FirstOrDefault(p => p.Alignment == PortAlignment.Top);
        
        var pTo = target.Ports.Where(p => p.Alignment == PortAlignment.Bottom)
                         .OrderBy(p => p.Position.X)
                         .ElementAtOrDefault(idx);

        if (pTo == null) pTo = target.Ports.LastOrDefault(p => p.Alignment == PortAlignment.Bottom);

        if (pFrom != null && pTo != null)
        {
            diagram.Links.Add(new GateLink(pFrom, pTo));
        }
    }
}