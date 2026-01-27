// See https://aka.ms/new-console-template for more information

using Bakalarka;

Parser par = new Parser();
Dictionary<string, Node> obvod = new Dictionary<string, Node>(); // najit 'obvod' anglicky

Console.WriteLine("Vyber akci:");
Console.WriteLine("1. Ukazkovy priklad");
Console.WriteLine("2. Zadat vlastni");

string chosen = Console.ReadLine() ?? "1"; 


bool DFS(string nodeId)
{
    var node = obvod[nodeId];

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
    Console.WriteLine("\n--- UKÁZKOVÝ GRAF Z PAPÍRU Č. 2 ---");

    string ukazka = "x1 START 1\nx2 START 0\nG1 AND x1 x2\nG2 NEG G1";
    obvod = par.GetInput(ukazka);

    bool res = DFS("G2_orig");
    Console.WriteLine($"Výsledek G2 (-(1 AND 0)): {(res ? "1" : "0")}");
    
}
else
{
    Console.WriteLine("\nZadávej pravidla (např. 'G1 AND A B' nebo 'S -> A B | c').");
    Console.WriteLine("Pro konec a vyhodnocení napiš 'konec'.");

    string celyVstup = "";
    while (true)
    {
        Console.Write("> ");
        string radek = Console.ReadLine() ?? "";
        if (radek.ToLower().Trim() == "konec") break;
        celyVstup += radek + "\n";
    }

    // Parser zpracuje celý blok textu najednou
    obvod = par.GetInput(celyVstup);

    Console.Write("\nKteré ID chceš vyhodnotit? ");
    string final = (Console.ReadLine() ?? "").Trim();
    
    // Hledáme vždy originální větev
    string hledaneId = final + "_orig";

    if (obvod.ContainsKey(hledaneId))
    {
        bool res = DFS(hledaneId);
        Console.WriteLine($"\n VÝSLEDEK pro {final}: {(res ? "1" : "0")}!");
    }
    else
    {
        Console.WriteLine($"Chyba: ID '{final}' nebylo nalezeno.");
    }
}