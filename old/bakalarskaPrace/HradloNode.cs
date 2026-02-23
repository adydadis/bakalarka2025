
 
using Blazor.Diagrams.Core.Geometry;
using Blazor.Diagrams.Core.Models;

namespace bakalarskaPrace;

public class AddTwoNumbersNode : NodeModel
{
    public AddTwoNumbersNode(Point? position = null) : base(position) { }

    public double FirstNumber { get; set; }
    public double SecondNumber { get; set; }

    // Here, you can put whatever you want, such as a method that does the addition
}