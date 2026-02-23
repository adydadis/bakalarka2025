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

    public GateNode(string gateType, Point position, bool isNegated = false) : base(position)
    {
        GateType = gateType.ToUpper();
        IsNegatedVersion = isNegated;

        
        string sAnd = "∧";
        string sOr = "∨";
        string sNeg = "¬";

        if (GateType == "START")
        {
            Label = isNegated ? $"{sNeg} START" : "START";
        }
        else if (GateType == "NEG_AND")
        {
            
            Label = isNegated ? sAnd : $"{sNeg} {sAnd}";
        }
        else if (GateType == "NEG_OR")
        {
            Label = isNegated ? sOr : $"{sNeg} {sOr}";
        }
        else if (GateType == "AND")
        {
            Label = isNegated ? $"{sNeg} {sAnd}" : sAnd;
        }
        else if (GateType == "OR")
        {
            Label = isNegated ? $"{sNeg} {sOr}" : sOr;
        }
        else if (GateType == "NEG")
        {
            Label = sNeg;
        }
    }
}