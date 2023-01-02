using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using WUI.Editor.Data.Save;
using WUI.Editor.Enumerations;

namespace WUI.Editor.Elements
{
    public class WUI_Port : Port
    {
        protected WUI_Port(Orientation portOrientation, Direction portDirection, Capacity portCapacity, Type type) : base(portOrientation, portDirection, portCapacity, type)
        {
            
        }

        public void OnSetupPorts(Port draggedPort)
        {
          if(draggedPort != this) return;
          
          OnRemovePorts(draggedPort, false, 0);

          switch (draggedPort.direction)
          {
            case Direction.Input:  AddOutputs(draggedPort); break;
            case Direction.Output: AddInputs(draggedPort);  break;
            default: return;
          }
        }

        public async void OnRemovePorts(Port draggedPort, bool isRemoveDraggedPort = true, int delayTime = 100)
        {
          if(draggedPort != this) return;
          
          await Task.Delay(delayTime);

          switch (draggedPort.direction)
          {
            case Direction.Input:  RemoveOutputs(); break;
            case Direction.Output: RemoveInputs();  break;
            default: return;
          }

          if(isRemoveDraggedPort) edgeConnector.edgeDragHelper.draggedPort = null;
        }

        public override void OnStopEdgeDragging()
        {
          OnRemovePorts(edgeConnector.edgeDragHelper.draggedPort);

          base.OnStopEdgeDragging();
        }

        public new WUI_Edge ConnectTo(Port other) => ConnectTo<WUI_Edge>(other);
        
        public new static WUI_Port Create<TEdge>(
          Orientation orientation,
          Direction direction,
          Capacity capacity,
          Type type) where TEdge : Edge, new()
        {
          var listener = new WUI_DefaultEdgeConnectorListener();
          var ele = new WUI_Port(orientation, direction, capacity, type)
          {
            m_EdgeConnector = new EdgeConnector<TEdge>(listener)
          };
          ele.AddManipulator(ele.edgeConnector);
          return ele;
        }

        private void AddInputs(Port draggedPort)
        {
          if (m_GraphView == null || m_GraphView.nodes.ToArray().Length < 1) return;
          
          var nodes = m_GraphView.nodes;

          foreach (var nodeData in nodes.OfType<WUI_Node>().Where(nodeData => nodeData.NodeType != WUI_NodeType.HomeUI))
          {
            if (nodeData == draggedPort.node) continue;

            if (draggedPort.node is not WUI_Node thisNode) continue;

            if (nodeData.PreviousNodes.Any(n => n.NodeID == thisNode.ID)) continue;

            var inputPorts = nodeData.inputContainer.Children().Select(e => e as Port).ToArray();
            
            if (nodeData.inputContainer.childCount == 1)
            {
              if (inputPorts.Any(port => !port.connected)) continue;
            }

            var inputData = new WUI_NodeData()
            {
              NodeName = thisNode.UIName,
              NodeID = thisNode.ID
            };
            
            nodeData.AddInputWithData("", Capacity.Single, inputData);
          }
        }

        private void AddOutputs(Port draggedPort)
        {
          if (m_GraphView == null || m_GraphView.nodes.ToArray().Length < 1) return;
          
          var nodes = m_GraphView.nodes;

          foreach (var nodeData in nodes.OfType<WUI_Node>().Where(nodeData => nodeData.NodeType != WUI_NodeType.LastUI))
          {
            if (nodeData == draggedPort.node) continue;

            if (draggedPort.node is not WUI_Node thisNode) continue;

            if (nodeData.NextNodes.Any(n => n.NodeID == thisNode.ID)) continue;
            
            var outputPorts = nodeData.outputContainer.Children().Select(e => e as Port).ToArray();
            
            if (nodeData.outputContainer.childCount == 1)
            {
              if (outputPorts.Any(port => !port.connected)) continue;
            }

            var outputData = new WUI_NodeData()
            {
              NodeName = thisNode.UIName,
              NodeID = thisNode.ID
            };
            
            nodeData.AddOutputWithData("", Capacity.Single, outputData);
          }
        }

        private void RemoveInputs()
        {
          if (m_GraphView == null || m_GraphView.nodes.ToArray().Length < 1) return;
          
          var nodes = m_GraphView.nodes.Select(n => n as WUI_Node);

          foreach (var wuiNode in nodes)
          {
            var inputPorts = wuiNode?.inputContainer.Children().Select(e => e as Port).ToArray();

            if (inputPorts == null) continue;

            if (inputPorts.Length < 2) continue;

            var emptyPorts = inputPorts.Where(p => !p.connected).ToArray();
            
            foreach (var emptyPort in emptyPorts)
            {
              wuiNode.RemoveInput(emptyPort);
            }
          }
        }

        private void RemoveOutputs()
        {
          if (m_GraphView == null || m_GraphView.nodes.ToArray().Length < 1) return;

          var nodes = m_GraphView.nodes.Select(n => n as WUI_Node);

          foreach (var wuiNode in nodes)
          {
            var outputPorts = wuiNode?.outputContainer.Children().Select(e => e as Port).ToArray();

            if (outputPorts == null) continue;

            if (outputPorts.Length < 2) continue;

            var emptyPorts = outputPorts.Where(p => !p.connected).ToArray();

            foreach (var emptyPort in emptyPorts)
            {
              wuiNode.RemoveOutput(emptyPort);
            }
          }
        }
        
        private class WUI_DefaultEdgeConnectorListener : IEdgeConnectorListener
        {
          private readonly GraphViewChange m_GraphViewChange;
          private readonly List<Edge> m_EdgesToCreate;
          private readonly List<GraphElement> m_EdgesToDelete;

          public WUI_DefaultEdgeConnectorListener()
          {
            m_EdgesToCreate = new List<Edge>();
            m_EdgesToDelete = new List<GraphElement>();
            m_GraphViewChange.edgesToCreate = m_EdgesToCreate;
          }

          public void OnDropOutsidePort(Edge edge, Vector2 position){}

          public void OnDrop(GraphView graphView, Edge edge)
          {
            m_EdgesToCreate.Clear();
            
            m_EdgesToCreate.Add(edge);
            
            m_EdgesToDelete.Clear();
            
            if (edge.input.capacity == Capacity.Single)
            {
              foreach (var connection in edge.input.connections)
              {
                if (connection != edge) m_EdgesToDelete.Add(connection);
              }
            }

            if (edge.output.capacity == Capacity.Single)
            {
              foreach (var connection in edge.output.connections)
              {
                if (connection != edge) m_EdgesToDelete.Add(connection);
              }
            }

            if (m_EdgesToDelete.Count > 0) graphView.DeleteElements(m_EdgesToDelete);
            
            var edgesToCreate = m_EdgesToCreate;
            
            if (graphView.graphViewChanged != null)
              edgesToCreate = graphView.graphViewChanged(m_GraphViewChange).edgesToCreate;
            
            foreach (var edge1 in edgesToCreate)
            {
              graphView.AddElement(edge1);
              edge.input.Connect(edge1);
              edge.output.Connect(edge1);
            }
          }
        }
    }
}