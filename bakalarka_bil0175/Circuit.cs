using Blazor.Diagrams.Core;
using Blazor.Diagrams.Core.Models;
using Blazor.Diagrams.Core.Geometry;

namespace bakalarka_bil0175;

public class Circuit
{
    public void Evaluate(Diagram diagram)
    {
        var blueNodes = diagram.Nodes.OfType<GateNode>().Where(n => !n.IsNegatedVersion).ToList();
        foreach (var n in blueNodes) {
            if (n.GateType == "START" || n.GateType.Contains("NEG")) {
                GetOrCreatePartner(n, diagram);
            }
        }

        var allNodesSorted = diagram.Nodes.OfType<GateNode>()
            .OrderBy(n => GetNodeLevel(n, diagram)).ToList();

        foreach (var n in allNodesSorted) {
            if (n.GateType == "START" && !n.IsNegatedVersion) continue;
            n.Status = NodeState.NotVisited;
        }

        foreach (var n in allNodesSorted) {
            CalculateNodeStatus(n, diagram);
        }

        ApplyLayout(diagram);
    }

    private string GetDualType(string type)
    {
        return type switch {
            "AND" => "OR",
            "OR" => "AND",
            "NEG_AND" => "OR",  
            "NEG_OR" => "AND",   
            _ => type 
        };
    }

    private GateNode GetOrCreatePartner(GateNode node, Diagram diagram)
    {
        if (node.Partner != null) return node.Partner;

        string partnerType = GetDualType(node.GateType);
        int offset = node.GateType.Contains("NEG") ? 160 : 120;
        
        var partner = new GateNode(partnerType, new Point(node.Position.X + offset, node.Position.Y), true, node.StartIndex);
        node.Partner = partner;
        partner.Partner = node;
        diagram.Nodes.Add(partner);
        
        var incomingLinks = diagram.Links.Where(l => {
            var target = (l.Target.Model is PortModel tp) ? tp.Parent : l.Target.Model as NodeModel;
            return target == node;
        }).ToList();

        foreach (var link in incomingLinks) {
            if (link.Source.Model is PortModel sp && sp.Parent is GateNode sourceGate) {
                var sourcePartner = GetOrCreatePartner(sourceGate, diagram);

                if (sourcePartner != null) {
                    var pFrom = sourcePartner.Ports.FirstOrDefault(p => p.Alignment == PortAlignment.Top);
                    
                    var modryTargetPort = link.Target.Model as PortModel;
                    int index = node.Ports.Where(p => p.Alignment == PortAlignment.Bottom).ToList().IndexOf(modryTargetPort);
                    var pTo = partner.Ports.Where(p => p.Alignment == PortAlignment.Bottom).ElementAtOrDefault(index);

                    if (pFrom != null && pTo != null) {
                        if (!diagram.Links.Any(l => l.Source.Model == pFrom && l.Target.Model == pTo)) {
                            diagram.Links.Add(new GateLink(pFrom, pTo, true));
                        }
                    }
                }
            }
        }
        return partner;
    }

    private void CalculateNodeStatus(GateNode node, Diagram diagram)
    {
        if (node.GateType == "START") {
            if (node.IsNegatedVersion && node.Partner != null)
                node.Status = (node.Partner.Status == NodeState.True) ? NodeState.False : NodeState.True;
            return;
        }

        var inputNodes = GetInputNodes(node, diagram);
        bool result = false;

        switch (node.GateType) {
            case "AND":
                result = inputNodes.Count > 0 && inputNodes.All(i => i.Status == NodeState.True);
                break;
            case "OR":
                result = inputNodes.Any(i => i.Status == NodeState.True);
                break;
            case "NEG_AND": 
                if (!node.IsNegatedVersion) result = !(inputNodes.Count > 0 && inputNodes.All(i => i.Status == NodeState.True));
                else result = inputNodes.Any(i => i.Status == NodeState.True); 
                break;
            case "NEG_OR": 
                if (!node.IsNegatedVersion) result = !inputNodes.Any(i => i.Status == NodeState.True);
                else result = inputNodes.Count > 0 && inputNodes.All(i => i.Status == NodeState.True); 
                break;
        }
        node.Status = result ? NodeState.True : NodeState.False;
    }

    private int GetNodeLevel(GateNode node, Diagram diagram)
    {
        if (node.GateType == "START") return 0;
        var inputs = GetInputNodes(node, diagram);
        if (!inputs.Any()) return 0;
        return inputs.Max(i => GetNodeLevel(i, diagram)) + 1;
    }

    private List<GateNode> GetInputNodes(GateNode node, Diagram diagram)
    {
        List<GateNode> inputs = new List<GateNode>();
        foreach (var link in diagram.Links) {
            NodeModel? target = (link.Target.Model is PortModel tp) ? tp.Parent : link.Target.Model as NodeModel;
            if (target == node) {
                NodeModel? source = (link.Source.Model is PortModel sp) ? sp.Parent : link.Source.Model as NodeModel;
                if (source is GateNode g) inputs.Add(g);
            }
        }
        return inputs;
    }

    private void ApplyLayout(Diagram diagram)
    {
        var blueNodes = diagram.Nodes.OfType<GateNode>().Where(n => !n.IsNegatedVersion).ToList();
        if (!blueNodes.Any()) return;

        var layers = new Dictionary<GateNode, int>();
        foreach (var n in blueNodes) layers[n] = GetNodeLevel(n, diagram);

        int maxLevel = layers.Values.Max(); 
        int rowSpacing = 160; 
        int colSpacing = 380; 

        var groups = layers.GroupBy(kv => kv.Value).OrderBy(g => g.Key);
        foreach (var group in groups) {
            int level = group.Key;
            var nodesInLevel = group.ToList();
            double startX = -((nodesInLevel.Count - 1) * colSpacing) / 2.0;
            int y = (maxLevel - level) * rowSpacing + 100;

            for (int i = 0; i < nodesInLevel.Count; i++) {
                var node = nodesInLevel[i].Key;
                double x = startX + (i * colSpacing) + 500;
                node.SetPosition(x, y);
                if (node.Partner != null) {
                    int offset = node.GateType.Contains("NEG") ? 160 : 120;
                    node.Partner.SetPosition(x + offset, y);
                }
            }
        }
        
        var allLinks = diagram.Links.ToList();
        diagram.Links.Clear(); 
        foreach (var link in allLinks) diagram.Links.Add(link);
        foreach (var node in diagram.Nodes) node.Refresh();
    }
}