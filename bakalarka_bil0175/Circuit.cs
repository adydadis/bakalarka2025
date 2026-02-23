using Blazor.Diagrams.Core;
using Blazor.Diagrams.Core.Models;
using Blazor.Diagrams.Core.Geometry;

namespace bakalarka_bil0175;

public class Circuit
{
    public void Evaluate(Diagram diagram)
    {
        var nodes = diagram.Nodes.OfType<GateNode>().ToList();
        foreach (var n in nodes) n.Status = NodeState.NotVisited;

        foreach (var n in nodes)
        {
            bool cycle = false;
            Calculate(n, diagram, ref cycle);
        }
    }

    private bool Calculate(GateNode node, Diagram diagram, ref bool cycle)
    {
        if (node.Status == NodeState.True) return true;
        if (node.Status == NodeState.False) return false;
        if (node.Status == NodeState.Visiting) { cycle = true; return false; }

        node.Status = NodeState.Visiting;
        List<GateNode> inputNodes = GetInputNodes(node, diagram);
        bool result = false;

        if (!node.IsNegatedVersion)
        {
           
            switch (node.GateType)
            {
                case "START":
                    result = node.Label.Contains("(1)");
                    break;

                case "AND":
                    result = (inputNodes.Count > 0);
                    foreach (var i in inputNodes)
                    {
                        if (!Calculate(i, diagram, ref cycle)) { result = false; break; }
                    }
                    break;

                case "OR":
                    result = false;
                    foreach (var i in inputNodes)
                    {
                        if (Calculate(i, diagram, ref cycle)) { result = true; break; }
                    }
                    break;

                case "NEG_AND": 
                    bool allIn = (inputNodes.Count > 0);
                    foreach (var i in inputNodes)
                    {
                        if (!Calculate(i, diagram, ref cycle)) { allIn = false; break; }
                    }
                    result = !allIn;
                    break;

                case "NEG_OR": 
                    bool anyIn = false;
                    foreach (var i in inputNodes)
                    {
                        if (Calculate(i, diagram, ref cycle)) { anyIn = true; break; }
                    }
                    result = !anyIn;
                    break;

                case "NEG":
                    var inp = inputNodes.FirstOrDefault();
                    if (inp != null)
                        result = Calculate(GetOrCreatePartner(inp, diagram), diagram, ref cycle);
                    break;
            }
        }
        else
        {
            
            switch (node.GateType)
            {
                case "START":
                    result = !node.Partner!.Label.Contains("(1)");
                    break;

                case "AND": 
                case "NEG_OR":
                    
                    result = false;
                    foreach (var i in inputNodes)
                    {
                        if (Calculate(GetOrCreatePartner(i, diagram), diagram, ref cycle)) { result = true; break; }
                    }
                    break;

                case "OR":
                case "NEG_AND":
                    
                    result = (inputNodes.Count > 0);
                    foreach (var i in inputNodes)
                    {
                        if (!Calculate(GetOrCreatePartner(i, diagram), diagram, ref cycle)) { result = false; break; }
                    }
                    break;
            }
        }

        node.Status = result ? NodeState.True : NodeState.False;
        return result;
    }

    private List<GateNode> GetInputNodes(GateNode node, Diagram diagram)
    {
        List<GateNode> inputs = new List<GateNode>();
        
        var targetToLookFor = node.IsNegatedVersion ? node.Partner : node;

        foreach (var link in diagram.Links)
        {
            NodeModel? target = (link.Target.Model is PortModel tp) ? tp.Parent : link.Target.Model as NodeModel;
            if (target == targetToLookFor)
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
        diagram.Nodes.Add(partner);
        return partner;
    }
}