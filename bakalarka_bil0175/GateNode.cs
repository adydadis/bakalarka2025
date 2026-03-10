using Blazor.Diagrams.Core.Geometry;
using Blazor.Diagrams.Core.Models;
using Blazor.Diagrams.Core.Models.Base; 

namespace bakalarka_bil0175;

public class GateNode : NodeModel
{
    public string GateType { get; set; } 
    public string Label { get; set; } = "";
    public NodeState Status { get; set; } = NodeState.NotVisited;
    public bool IsNegatedVersion { get; set; } = false;
    public GateNode? Partner { get; set; }
    
    public bool StartValue { get; set; } = false; 
    
    public string OriginalLabel { get; set; }

    public GateNode(string gateType, Point position, bool isNegated = false, string label = "") : base(position)
    {
        GateType = gateType.ToUpper();
        IsNegatedVersion = isNegated;
        OriginalLabel = label;

        if (GateType == "START")
        {
            string displayLabel = int.TryParse(label, out _) ? $"S{label}" : label;
            
            Label = isNegated ? $"¬{displayLabel}" : displayLabel;
            this.Size = new Size(60, 40);
            this.AddPort(PortAlignment.Top);
        }
        else
        {
            this.Size = new Size(80, 60);
            this.AddPort(PortAlignment.Bottom); 
            this.AddPort(PortAlignment.Bottom); 
            this.AddPort(PortAlignment.Top);    

            string sAnd = "∧"; string sOr = "∨"; string sNeg = "¬";
        
            if (GateType == "NEG_AND") Label = $"{sNeg} {sAnd}";
            else if (GateType == "NEG_OR") Label = $"{sNeg} {sOr}";
            else if (GateType == "AND") Label = sAnd;
            else if (GateType == "OR") Label = sOr;
            else Label = sNeg;
        }

        this.Changed += (m) => {
            if (!this.IsNegatedVersion && this.Partner != null) {
                int offset = this.GateType.Contains("NEG") ? 160 : 120;
                this.Partner.SetPosition(this.Position.X + offset, this.Position.Y);
                this.Partner.Refresh();
            }
        };
    }
}