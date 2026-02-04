// See https://aka.ms/new-console-template for more information

using Bakalarka;

Parser par = new Parser();
Dictionary<string, Node> circuit = new Dictionary<string, Node>(); // najit 'obvod' anglicky

Console.WriteLine("Vyber akci:");
Console.WriteLine("1. Ukazkovy priklad");
Console.WriteLine("2. Zadat vlastni");

string chosen = Console.ReadLine() ?? "1"; 

void printTee(string nodeId, string prefix = "", bool isLast = true)
{
    if (!circuit.ContainsKey(nodeId)) 
    {
        return;
    }

    var node = circuit[nodeId];

    string vetve;
    if (isLast) 
    {
        vetve = " '--- "; //Posledni
    }
    else 
    {
        vetve = " |--- "; 
    }
    
    string statusText;
    if (node.Status == NodeState.True) 
    {
        statusText = "1";
    }
    else if (node.Status == NodeState.False) 
    {
        statusText = "0";
    }
    else 
    {
        statusText = "?"; // not visited
    }
    
    Console.Write(prefix + vetve);
    Console.WriteLine($"{nodeId} {statusText} ({node.Type})");
    
    
    for (int i = 0; i < node.Inputs.Count; i++)
    {
        // odsazeni
        string newPrefix;
        if (isLast) 
        {
            newPrefix = prefix + "      "; 
        }
        else 
        {
            newPrefix = prefix + "|   "; 
        }

        bool isLastInput = (i == node.Inputs.Count - 1);
        
        printTee(node.Inputs[i], newPrefix, isLastInput);
    }
}


bool DFS(string nodeId)
{
    var node = circuit[nodeId];

    if (node.Status == NodeState.False)
    {
        return false;
    }
    if (node.Status == NodeState.True)
    {
        return true;
    }

    bool result = false;

    if (node.Type == "AND")
    {
        result = true;
        foreach (var inId in node.Inputs)
        {
            if (!DFS(inId))
            {
                result = false;
                break;
            }
        }
        
    }else if (node.Type == "OR")
    {
        result = false;
        foreach (var inId in node.Inputs)
        {
            if (DFS(inId))
            {
                result = true;
                break;
            }
        }

    } 
    /*else if (node.Type == "NEG")
    {
        if (node.Inputs.Count > 0)
        {
            string inputId = node.Inputs[0];
        
            
            bool inputResult = DFS(inputId);
        
        
            result = !inputResult; // vysledek se zneguje

        }
        
    }*/

    if (result)
    {
        node.Status = NodeState.True;
    }
    else
    {
        node.Status = NodeState.False;
    }

    return result;

}


if (chosen == "1")
{
    Console.WriteLine("\nUkazka:");

    string ukazka = "x1 START 1\nx2 START 0\nG1 AND x1 x2\nG2 NEG G1";
    circuit = par.GetInput(ukazka);

    bool res = DFS("G2_orig");
    printTee("G2_orig"); 
    string vysledekText;
    if (res == true)
    {
        vysledekText = "1";
    }
    else
    {
        vysledekText = "0";
    }
    Console.WriteLine($"\n Vysledek pro G2: {vysledekText}!");
}
else
{
    Console.WriteLine("\nZadej pravidla ('G1 AND A B').");
    Console.WriteLine("Pro konec napiš 'konec'.");

    string celyVstup = "";
    while (true)
    {
        string radek = Console.ReadLine() ?? "";
        if (radek.ToLower().Trim() == "konec") break;
        celyVstup += radek + "\n";
    }

    circuit = par.GetInput(celyVstup);

    Console.Write("\nKtere ID vyhodnotit? ");
    string final = (Console.ReadLine() ?? "").Trim();
    
    string findId = final + "_orig";

    if (circuit.ContainsKey(findId))
    {
        bool res = DFS(findId);
        printTee(findId); 
        string vysledekText;
        if (res == true)
        {
            vysledekText = "1";
        }
        else
        {
            vysledekText = "0";
        }
        Console.WriteLine($"\n Vysledek pro {final}: {vysledekText}!");
    }
    else
    {
        Console.WriteLine($"Chyba: ID '{final}' nebylo nalezeno.");
    }
}