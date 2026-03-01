using Blazor.Diagrams.Core.Models;


namespace bakalarka_bil0175;

public class GateLink : LinkModel
{
    public bool IsNegated { get; set; }

    public GateLink(PortModel source, PortModel target, bool isNegated = false) : base(source, target)
    {
        IsNegated = isNegated;
        this.Color = isNegated ? "#dc3545" : "#007bff";
        this.Width = 3;
    }
}