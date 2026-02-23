namespace bakalarskaPrace;

public class Logic
{
    public static bool DFS(string nodeId, Dictionary<string, Node> circuit, ref bool cycle)
    {
        if (!circuit.ContainsKey(nodeId)) return false;
        var node = circuit[nodeId];

        if (node.Status == NodeState.False) return false;
        if (node.Status == NodeState.True) return true;
    
        if (node.Status == NodeState.Visiting)
        {
            cycle = true;
            return false;
        }

        node.Status = NodeState.Visiting;
        bool result = false;

        if (node.Type == "AND")
        {
            // OPRAVA: AND je pravda jen tehdy, pokud má aspoň jeden vstup 
            // a VŠECHNY jeho vstupy jsou pravda.
            if (node.Inputs.Count == 0) result = false; 
            else {
                result = true;
                foreach (var inId in node.Inputs)
                {
                    // DŮLEŽITÉ: Musíš volat tu stejnou funkci se VŠEMI parametry!
                    if (!DFS(inId, circuit, ref cycle))
                    {
                        result = false;
                        break;
                    }
                }
            }
        }
        else if (node.Type == "OR")
        {
            result = false;
            foreach (var inId in node.Inputs)
            {
                if (DFS(inId, circuit, ref cycle))
                {
                    result = true;
                    break;
                }
            }
        }
        // U NEG už víme, že ho Parser přetvoří na AND/OR s přehozenými dráty.

        node.Status = result ? NodeState.True : NodeState.False;
        return result;
    }

}