// See https://aka.ms/new-console-template for more information

using Bakalarka;
using Microsoft.AspNetCore.Builder;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(obj => obj.AddDefaultPolicy(p =>
    p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();
app.UseCors();

app.MapPost("/getResult", (CircuitTrans input) =>
{
    Console.WriteLine($"\n--- KONTROLA HRAN ---");
    foreach (var e in input.Edges)
    {
        Console.WriteLine($"Hrana: z {e.source} do {e.target}");
    }
    Parser par = new Parser();
    var circuit = par.GetInput(input);

    Console.WriteLine("\n--- DIAGNOSTIKA OBVODU ---");
    foreach (var nodeKey in circuit.Keys)
    {
        var n = circuit[nodeKey];
        // Vypíše: ID uzlu, jeho typ, status a na co je napojený
        Console.WriteLine($"Uzel: {nodeKey} | Typ: {n.Type} | Status: {n.Status} | Vstupy: {string.Join(", ", n.Inputs)}");
    }

    foreach (var key in circuit.Keys)
    {
        bool cycle = false;
        DFS(key, circuit, ref cycle);
    }

    foreach (var n in input.Nodes)
    {
        string findId = n.id + "_orig";
        if (circuit.ContainsKey(findId))
        {
            var res = circuit[findId].Status;
            string value = (res == NodeState.True) ? "(1)" : "(0)";
            string justName = n.Data.label.Split(' ')[0];
            n.Data.label = $"{justName} {value}";
        }
    }

    return Results.Ok(input);
});
app.Run();

bool DFS(string nodeId, Dictionary<string, Node> circuit, ref bool cycle)
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
