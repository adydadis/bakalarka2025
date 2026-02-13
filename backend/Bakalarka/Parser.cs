namespace Bakalarka;

public class Parser
{
    public Dictionary<string, Node> GetInput(string input)
    {
        var obvod = new Dictionary<string, Node>();
        var lines = input.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

        foreach (var l in lines)
        {
            var getWords = l.Trim();
            if (string.IsNullOrWhiteSpace(getWords))
            {
                continue;
            }

            if (getWords.Contains("->"))
            {
                GetGrammar(getWords, obvod);
            }
            else
            {
                GetNode(getWords, obvod);
            }
        }

        return obvod;

    }

    private void GetNode(string line, Dictionary<string, Node> obvod)
    {
        var splitLine = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (splitLine.Length < 2)
        {
            return;
        }

        string id = splitLine[0];
        string type = splitLine[1].ToUpper();

        NodeState original;
        NodeState negative;

        if (type == "START")
        {
            if (splitLine.Length > 2 && splitLine[2] == "1")
            {
                original = NodeState.True;
                negative = NodeState.False;
            }
            else
            {
                original = NodeState.False;
                negative = NodeState.True;
            }
            
            obvod.Add(id + "_orig", new Node(id + "_orig", "START", new List<string>(), original));
            obvod.Add(id + "_neg", new Node(id + "_neg", "START", new List<string>(), negative));
        } else if (type == "AND")
        {
            obvod[$"{id}_orig"] = new Node($"{id}_orig", "AND", splitLine.Skip(2).Select(v => v + "_orig").ToList());
            obvod[$"{id}_neg"] = new Node($"{id}_neg", "OR", splitLine.Skip(2).Select(v => v + "_neg").ToList());
        }else if (type == "OR")
        {
            List<string> inputsOrig = new List<string>();
            List<string> inputsNeg = new List<string>();

            for (int i = 2; i < splitLine.Length; i++)
            {
                inputsOrig.Add(splitLine[i] + "_orig");
                inputsNeg.Add(splitLine[i] + "_neg");
            }

            obvod.Add(id + "_orig", new Node(id + "_orig", "OR", inputsOrig, NodeState.NotVisited));
    
            obvod.Add(id + "_neg", new Node(id + "_neg", "AND", inputsNeg, NodeState.NotVisited));
        }else if (type == "NEG" || type == "NOR" || type == "NEGOR")
        {
            List<string> inputsOrig = new List<string>();
            List<string> inputsNeg = new List<string>();

            for (int i = 2; i < splitLine.Length; i++)
            {
                inputsOrig.Add(splitLine[i] + "_orig");
                inputsNeg.Add(splitLine[i] + "_neg");
            }

            obvod.Add(id + "_orig", new Node(id + "_orig", "AND", inputsNeg, NodeState.NotVisited));
    
            obvod.Add(id + "_neg", new Node(id + "_neg", "OR", inputsOrig, NodeState.NotVisited));
        }else if (type == "NAND" || type == "NEGAND")
        {
            List<string> inputsOrig = new List<string>();
            List<string> inputsNeg = new List<string>();

            for (int i = 2; i < splitLine.Length; i++)
            {
                inputsOrig.Add(splitLine[i] + "_orig");
                inputsNeg.Add(splitLine[i] + "_neg");
            }

            obvod.Add(id + "_orig", new Node(id + "_orig", "OR", inputsNeg, NodeState.NotVisited));
            obvod.Add(id + "_neg", new Node(id + "_neg", "AND", inputsOrig, NodeState.NotVisited));
        }

    }

    private void GetGrammar(string line, Dictionary<string, Node> obvod)
    {
        var sides = line.Split("->");
        string head = sides[0].Trim();
        var choices = sides[1].Split('|', StringSplitOptions.RemoveEmptyEntries);

        var mainOR = new List<string>();
        int i = 1;

        foreach (var ch in choices)
        {
            var symbols = ch.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            string partId;
            
            if (symbols.Length > 1)
            {
                partId = $"{head}_p{i}_orig";
                var andInputs = new List<string>();

                foreach (var s in symbols)
                {
                    andInputs.Add(s + "_orig");
                    CheckAndAddTerminal(s, obvod);
                }
                
                obvod.Add(partId, new Node(partId, "AND", andInputs));
                i++;
            }
            else
            {
                partId = symbols[0] + "_orig";
                CheckAndAddTerminal(symbols[0], obvod);
            }
            
            mainOR.Add(partId);

            
        }

        string finalId = head + "_orig";
        if (!obvod.ContainsKey(finalId))
        {
            obvod.Add(finalId, new Node(finalId, "OR", mainOR));
        }

    }

    private void CheckAndAddTerminal(string symbol, Dictionary<string, Node> obvod)
    {
        if (symbol == "e")
        {
            string idOrig = "e_orig";
            string idNeg = "e_neg";
            
            if (!obvod.ContainsKey(idOrig))
            {
                obvod.Add(idOrig, new Node(idOrig, "START", new List<string>(), NodeState.True));
                obvod.Add(idNeg, new Node(idNeg, "START", new List<string>(), NodeState.False));
            }
            return;

        }
        
        if (char.IsLower(symbol[0]))
        {
            string idOrig = symbol + "_orig";
            string idNeg = symbol + "_neg";

            if (!obvod.ContainsKey(idOrig))
            {
                obvod.Add(idOrig, new Node(idOrig, "START", new List<string>(), NodeState.True));
                obvod.Add(idNeg, new Node(idNeg, "START", new List<string>(), NodeState.False));                
            }
        }
    }
    
    
}