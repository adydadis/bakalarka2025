using System.Text.Json.Serialization;

namespace Bakalarka;

public class CircuitTrans
{
    [JsonPropertyName("nodes")]
    public List<NodeTrans> Nodes { get; set; } = new();

    [JsonPropertyName("edges")]
    public List<EdgeTrans> Edges { get; set; } = new();
}

public class NodeTrans
{
    [JsonPropertyName("id")]
    public string id { get; set; } = "";

    [JsonPropertyName("data")]
    public NodeData Data { get; set; } = new();
}
public class NodeData
{
    public string label { get; set; } = "";
}
public class EdgeTrans
{
    [JsonPropertyName("source")]
    public string source { get; set; } = "";

    [JsonPropertyName("target")]
    public string target { get; set; } = "";
}

// ... NodeData třída zůstává stejná