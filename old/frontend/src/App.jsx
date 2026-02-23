import React from 'react';
import ReactFlow, {useNodesState, useEdgesState, addEdge} from 'reactflow';
import 'reactflow/dist/style.css';

export default function App() {
  
  //seznamy
  const [nodes, setNodes, onNodesChange] = useNodesState([]);
  const [edges, setEdges, onEdgesChange] = useEdgesState([]);
  
  const addNode = (type) => {

    const newNode = {

      id: Math.random().toString(),   //random cislo
      type: 'default',
      data: { label: type === 'START' ? 'START (0)' : type },
      position: {x: 50, y:50 },

    };

    setNodes((listNode) => listNode.concat(newNode));
  };

  const makeRelationship = (params) => {

    setEdges((listEdge) => addEdge(params, listEdge));
  };

  // zmena START value

  const changeStartValue = (event, node) => {

    if (node.data.label.includes('START')) {

      setNodes((nds) => 
        nds.map((n) => {

          if (n.id == node.id) {

            const newValue = n.data.label === 'START (0)' ? 'START (1)' : 'START (0)';
            return {...n, data: {label: newValue}};
          }
          return n;
        })
      );
    }
  };

  // smazani
  
  const deleteSelected = () => {
  
    setNodes((nds) => nds.filter((node) => !node.selected));
    setEdges((eds) => eds.filter((edge) => !edge.selected));
  };

  const getResult = async () => {
    const backendData = { nodes, edges };

    try{

      const response = await fetch('http://localhost:5000/getResult', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(backendData),
      });

      if (response.ok) {

        const result = await response.json();

      const updatedNodes = nodes.map((existingNode) => {
        
        const nodeBackend = result.nodes.find((n) => n.id === existingNode.id);

        if (nodeBackend) {
          const isOne = nodeBackend.data.label.includes("(1)");
          return {
            ...existingNode, 
            data: { 
              ...existingNode.data, 
              label: nodeBackend.data.label 
            },
            style: { 
              ...existingNode.style, 
              background: isOne ? '#95ff95' : '#ff9595', 
              transition: 'background 0.5s ease'
            }
          };
        }
        return existingNode;
      });

        setNodes(updatedNodes);
      }
    } catch (error){
      console.error("Chyba spojeni: ", error);
    }
  };
   
  
  return (
    <div style={{ width: '100vw', height: '100vh', position: 'relative' }}>
      
      {/* nahore cudliky */}
      <div style = {{position: 'absolute', top: '20px', left: '50%', transform: 'translateX(-50%)',
                     display: 'flex', gap: '10px', zIndex: 10 }}>
        <button onClick={() => addNode('START')}>START</button>
        <button onClick={() => addNode('OR')}>OR</button>
        <button onClick={() => addNode('AND')}>AND</button>
        <button onClick={() => addNode('NEG')}>NEG</button>
        <button onClick={deleteSelected} style={{ background: '#c31111', fontWeight: 'bold' }}>SMAZAT VYBRANÉ</button>
        <button onClick={getResult} style={{ background: '#27ae60', color: 'white', fontWeight: 'bold', border: '2px solid #2ecc71'}}>VYPOCITAT</button>
       </div>

      {/* graf */}
      <div style={{ width: '100%', height: '100%', background: '#f2f2f2', border: '1px solid #ccc' }}>
        <ReactFlow
          nodes={nodes}             
          edges={edges}             
          onNodesChange={onNodesChange} 
          onEdgesChange={onEdgesChange} 
          onConnect={makeRelationship}
          onNodeClick={changeStartValue}

        />
      </div>
    </div>
  );
}