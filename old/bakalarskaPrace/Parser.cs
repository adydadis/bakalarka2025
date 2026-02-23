using Blazor.Diagrams.Core.Models;
using Blazor.Diagrams.Core.Models.Base;

namespace bakalarskaPrace;

public class Parser
{
    public Dictionary<string, Node> GetInput(IEnumerable<NodeModel> nodes, IEnumerable<BaseLinkModel> links)
    {
        var obvod = new Dictionary<string, Node>();

        foreach (var diagNode in nodes)
        {
            string id = diagNode.Id;
            string type = diagNode.Title?.Split(' ')[0].ToUpper() ?? "UNKNOWN";
            var parentIds = new List<string>();

            foreach (var link in links)
            {
                // Triky s 'dynamic' - Rider přestane kontrolovat přesná jména tříd
                dynamic l = link;
                try 
                {
                    // Jen se zeptáme: "Máš na konci tenhle uzel?"
                    if (l.Target.Node == diagNode)
                    {
                        // Pokud ano, vezmi ID z druhého konce
                        parentIds.Add(l.Source.Node.Id);
                    }
                }
                catch { /* Čára není připojená, to nás nezajímá */ }
            }

            ProcessNode(id, type, parentIds, diagNode.Title ?? "", obvod);
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
        else if (type == "AND" || type == "OR")
        {
            // Pro AND/OR v Dual-Railu vytvoříme originál i negaci
            obvod[id + "_orig"] = new Node(id + "_orig", type, inputs.Select(v => v + "_orig").ToList());
            string negType = (type == "AND") ? "OR" : "AND";
            obvod[id + "_neg"] = new Node(id + "_neg", negType, inputs.Select(v => v + "_neg").ToList());
        }
        else if (type == "NEG")
        {
            // NEG jen prohodí dráty
            obvod[id + "_orig"] = new Node(id + "_orig", "AND", inputs.Select(v => v + "_neg").ToList());
            obvod[id + "_neg"] = new Node(id + "_neg", "OR", inputs.Select(v => v + "_orig").ToList());
        }
    }
}