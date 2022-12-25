using System;
using System.Collections.Generic;
using System.Linq;
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

        public override void OnStartEdgeDragging()
        {
          var draggedPort = edgeConnector.edgeDragHelper.draggedPort;
          
          if(draggedPort != this) return;
          
          if (m_GraphView == null || m_GraphView.nodes.ToArray().Length < 1) return;
          
          var nodes = m_GraphView.nodes;

          foreach (var nodeData in nodes.OfType<WUI_Node>().Where(nodeData => nodeData.NodeType != WUI_NodeType.HomeUI))
          {
            if (nodeData == draggedPort.node) continue;

            var inputPorts = nodeData.inputContainer.Children().Select(e => e as Port).ToArray();
            
            if (nodeData.inputContainer.childCount == 1)
            {
              if (inputPorts[^1].connected == false) continue;
            }

            nodeData.PreviousNodes.Add(new WUI_NodeData());
            nodeData.AddInput("", new WUI_NodeData());
            return;
          }
          
          base.OnStartEdgeDragging();
        }
        
        public override void OnStopEdgeDragging()
        {
          base.OnStopEdgeDragging();

          var draggedPort = edgeConnector.edgeDragHelper.draggedPort;
          
          if(draggedPort != this) return;

          if (m_GraphView == null || m_GraphView.nodes.ToArray().Length < 1) return;
          
          var nodes = m_GraphView.nodes.Select(n => n as WUI_Node);

          foreach (var n in nodes)
          {
            var inputPorts = n?.inputContainer.Children().Select(e => e as Port).ToArray();

            if (inputPorts == null) continue;

            if (inputPorts.Length < 2) continue;
            
            for (var i = 1; i < inputPorts.Length; i++)
            {
              if (inputPorts[i].connected) continue;
              
              n.RemoveLastInput();
            }
          }
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

          public void OnDropOutsidePort(Edge edge, Vector2 position)
          {
          }

          public void OnDrop(GraphView graphView, Edge edge)
          {
            m_EdgesToCreate.Clear();
            m_EdgesToCreate.Add(edge);
            m_EdgesToDelete.Clear();
            if (edge.input.capacity == Capacity.Single)
            {
              foreach (var connection in edge.input.connections)
              {
                if (connection != edge)
                  m_EdgesToDelete.Add(connection);
              }
            }

            if (edge.output.capacity == Capacity.Single)
            {
              foreach (var connection in edge.output.connections)
              {
                if (connection != edge)
                  m_EdgesToDelete.Add(connection);
              }
            }

            if (m_EdgesToDelete.Count > 0)
              graphView.DeleteElements(m_EdgesToDelete);
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