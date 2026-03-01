using Blazor.Diagrams.Core.Geometry;
using Blazor.Diagrams.Core.Models;

namespace bakalarka_bil0175;
public class GateNode : NodeModel
{
    public string GateType { get; set; } 
    public string Label { get; set; }
    public NodeState Status { get; set; } = NodeState.NotVisited;
    public bool IsNegatedVersion { get; set; } = false;
    public GateNode? Partner { get; set; }
    
    public bool StartValue { get; set; } = false; 

    public GateNode(string gateType, Point position, bool isNegated = false) : base(position)
    {
        GateType = gateType.ToUpper();
        IsNegatedVersion = isNegated;
        string sAnd = "∧"; string sOr = "∨"; string sNeg = "¬";

        if (GateType == "START") Label = isNegated ? "¬ START" : "START";
        else if (GateType == "NEG_AND") {
            Label = isNegated ? sOr : $"{sNeg} {sAnd}"; 
        }
        else if (GateType == "NEG_OR") Label = isNegated ? sAnd : $"{sNeg} {sOr}";
        else if (GateType == "AND") Label = isNegated ? sOr : sAnd;
        else if (GateType == "OR") Label = isNegated ? sAnd : sOr;
        else Label = sNeg;

        this.Changed += (m) => {
            if (!this.IsNegatedVersion && this.Partner != null) {
                this.Partner.Position = new Point(this.Position.X, this.Position.Y + 120);
                this.Partner.Refresh();
        
                foreach (var port in this.Partner.Ports) port.Refresh();
                foreach (var link in this.Partner.Links) link.Refresh();
            }
            foreach (var link in this.Links) link.Refresh();
        };
    }
}