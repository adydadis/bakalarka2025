using Blazor.Diagrams.Core;
using Blazor.Diagrams.Core.Models;
using Blazor.Diagrams.Core.Geometry;

namespace bakalarka_bil0175;

public class Circuit
{
    public void Evaluate(Diagram diagram)
    {
        var allNodes = diagram.Nodes.OfType<GateNode>().ToList();
        foreach (var n in allNodes) {
            if (n.GateType == "START" && !n.IsNegatedVersion) continue;
            n.Status = NodeState.NotVisited;
        }
        foreach (var n in allNodes) {
            bool cycle = false;
            Calculate(n, diagram, ref cycle);
        }
        ApplyLayout(diagram);
    
        foreach (var node in diagram.Nodes) {
            node.Refresh();
            
        }
    }
    
    private void ApplyLayout(Diagram diagram)
    {
        var blueNodes = diagram.Nodes.OfType<GateNode>().Where(n => !n.IsNegatedVersion).ToList();
        var layers = new Dictionary<GateNode, int>();
        foreach (var n in blueNodes) layers[n] = GetNodeLevel(n, diagram);

        int colSpacing = 300; 
        int rowSpacing = 230; 

        var groups = layers.GroupBy(kv => kv.Value).OrderBy(g => g.Key);
        foreach (var group in groups)
        {
            int x = group.Key * colSpacing + 100;
            int y = 50;
            foreach (var kv in group)
            {
                var node = kv.Key;
                node.Position = new Point(x, y);
                node.Refresh();

                if (node.Partner != null)
                {
                    node.Partner.Position = new Point(x, y + 120); 
                    node.Partner.Refresh();
                }
                y += rowSpacing;
            }
        }
    }
    private int GetNodeLevel(GateNode node, Diagram diagram)
    {
        if (node.GateType == "START") return 0;
        var inputs = GetInputNodes(node, diagram);
        if (!inputs.Any()) return 0;
        return inputs.Max(i => GetNodeLevel(i, diagram)) + 1;
    }
    private bool Calculate(GateNode node, Diagram diagram, ref bool cycle)
    {
        if (node.GateType != "START") {
            if (node.Status == NodeState.True) return true;
            if (node.Status == NodeState.False) return false;
        }
        if (node.Status == NodeState.Visiting) { cycle = true; return false; }

        if (node.GateType != "START") node.Status = NodeState.Visiting;
        
        List<GateNode> inputNodes = GetInputNodes(node, diagram);
        bool result = false;

        if (!node.IsNegatedVersion) { 
            switch (node.GateType) {
                case "START": 
                    result = (node.Status == NodeState.True); break;
                case "AND":
                    result = (inputNodes.Count > 0);
                    foreach (var i in inputNodes) if (!Calculate(i, diagram, ref cycle)) { result = false; break; }
                    break;
                case "OR":
                    result = false;
                    foreach (var i in inputNodes) if (Calculate(i, diagram, ref cycle)) { result = true; break; }
                    break;
                case "NEG_AND":
                case "NEG_OR":
                case "NEG":
                    var partner = GetOrCreatePartner(node, diagram);
                    result = Calculate(partner, diagram, ref cycle);
                    break;
            }
        } else { 
            switch (node.GateType) {
                case "START": 
                    result = (node.Partner!.Status != NodeState.True); break;
                case "AND":
                case "NEG_OR": 
                    result = (inputNodes.Count > 0);
                    foreach (var i in inputNodes) if (!Calculate(i, diagram, ref cycle)) { result = false; break; }
                    break;
                case "OR":
                case "NEG_AND": 
                    result = false;
                    foreach (var i in inputNodes) if (Calculate(i, diagram, ref cycle)) { result = true; break; }
                    break;
            }
        }
        node.Status = result ? NodeState.True : NodeState.False;
        return result;
    }
    private List<GateNode> GetInputNodes(GateNode node, Diagram diagram)
    {
        List<GateNode> inputs = new List<GateNode>();
    
        foreach (var link in diagram.Links)
        {
            NodeModel? target = (link.Target.Model is PortModel tp) ? tp.Parent : link.Target.Model as NodeModel;
        
            if (target == node)
            {
                NodeModel? source = (link.Source.Model is PortModel sp) ? sp.Parent : link.Source.Model as NodeModel;
                if (source is GateNode g) inputs.Add(g);
            }
        }
        return inputs;
    }

    private GateNode GetOrCreatePartner(GateNode node, Diagram diagram)
    {
        if (node.Partner != null) return node.Partner;

        
        var partner = new GateNode(node.GateType, new Point(node.Position.X, node.Position.Y + 120), true);
        node.Partner = partner;
        partner.Partner = node;
        
        foreach (var port in node.Ports)
        {
            partner.AddPort(port.Alignment);
        }
        
        diagram.Nodes.Add(partner);

        var modreLinky = diagram.Links.ToList(); 
        foreach (var link in modreLinky)
        {
            if (link.Target.Model is PortModel tp && tp.Parent == node)
            {
                if (link.Source.Model is PortModel sp && sp.Parent is GateNode sourceGate)
                {
                    var sourcePartner = GetOrCreatePartner(sourceGate, diagram);

                    var pFrom = sourcePartner.Ports.FirstOrDefault(p => p.Alignment == PortAlignment.Right);
                    
                    int index = sourceGate.Ports.ToList().IndexOf(sp);

                    var pTo = partner.Ports.Where(p => p.Alignment == PortAlignment.Left).ElementAtOrDefault(index)
                              ?? partner.Ports.FirstOrDefault(p => p.Alignment == PortAlignment.Left);

                    if (pFrom != null && pTo != null)
                    {
                        if (!diagram.Links.Any(l => l.Source.Model == pFrom && l.Target.Model == pTo))
                        {
                            var redLink = new GateLink(pFrom, pTo, true); 
                            diagram.Links.Add(redLink);
                        }
                    }
                }
            }
        }
        partner.Refresh();
        foreach(var l in partner.Links) l.Refresh();
        return partner;
    }
}