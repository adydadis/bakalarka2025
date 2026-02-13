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
  
  const deleteSelected = () => {
  
    setNodes((nds) => nds.filter((node) => !node.selected));
    setEdges((eds) => eds.filter((edge) => !edge.selected));
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