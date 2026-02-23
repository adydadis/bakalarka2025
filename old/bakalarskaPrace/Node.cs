namespace bakalarskaPrace;

public enum NodeState {NotVisited, True, False, Visiting}

public class Node
{
    public string Id { get; set; }
    public string Type { get; set; } 
    public List<string> Inputs { get; set; } = new();
    public NodeState Status { get; set; } = NodeState.NotVisited;

    public Node(string id, string type, List<string> inputs, NodeState status)
    {
        Id = id;
        Type = type;
        Inputs = inputs;
        Status = status;
        
    }
    
    public Node(string id, string type, List<string> inputs)
    {
        Id = id;
        Type = type;
        Inputs = inputs;
        Status = NodeState.NotVisited;
        
    }
        
}