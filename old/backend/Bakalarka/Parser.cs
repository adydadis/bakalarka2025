namespace Bakalarka;

public class Parser
{
    public Dictionary<string, Node> GetInput(CircuitTrans input)
    {
        var obvod = new Dictionary<string, Node>();

        foreach (var n in input.Nodes)
        {
            string id = n.id;
            string type = n.Data.label.Split(' ')[0].ToUpper();

            var parentIds = input.Edges
                .Where(e => e.source == id)
                .Select(e => e.target)
                .ToList();

            ProcessNode(id, type, parentIds, n.Data.label, obvod);
        }

        return obvod;
    }
    private void ProcessNode(string id, string type, List<string> inputs, string fullLabel, Dictionary<string, Node> obvod)
    {
        if (type == "START")
        {
            bool isOne = fullLabel.Contains("(1)");
            obvod.Add(id + "_orig", new Node(id + "_orig", "START", new List<string>(), isOne ? NodeState.True : NodeState.False));
            obvod.Add(id + "_neg", new Node(id + "_neg", "START", new List<string>(), isOne ? NodeState.False : NodeState.True));
        }
        else if (type == "AND")
        {
            
            obvod[id + "_orig"] = new Node(id + "_orig", "AND", inputs.Select(v => v + "_orig").ToList());
            obvod[id + "_neg"] = new Node(id + "_neg", "OR", inputs.Select(v => v + "_neg").ToList());
        }
        else if (type == "OR")
        {
            obvod[id + "_orig"] = new Node(id + "_orig", "OR", inputs.Select(v => v + "_orig").ToList());
            obvod[id + "_neg"] = new Node(id + "_neg", "AND", inputs.Select(v => v + "_neg").ToList());
        }
        else if (type == "NEG")
        {
            obvod[id + "_orig"] = new Node(id + "_orig", "AND", inputs.Select(v => v + "_neg").ToList());
            obvod[id + "_neg"] = new Node(id + "_neg", "OR", inputs.Select(v => v + "_orig").ToList());
        }
    }
}