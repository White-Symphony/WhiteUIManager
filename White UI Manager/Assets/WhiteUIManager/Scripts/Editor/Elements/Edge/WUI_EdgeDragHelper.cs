using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace WUI.Editor.Elements
{
  public class WUI_EdgeDragHelper<TEdge> : EdgeDragHelper where TEdge : Edge, new()
  {
    protected List<Port> m_CompatiblePorts;
    private Edge m_GhostEdge;
    protected GraphView m_GraphView;
    protected static NodeAdapter s_nodeAdapter = new ();
    protected readonly IEdgeConnectorListener m_Listener;
    private IVisualElementScheduledItem m_PanSchedule;
    private Vector3 m_PanDiff = Vector3.zero;
    private bool m_WasPanned;

    public bool resetPositionOnPan { get; set; }

    public WUI_EdgeDragHelper(IEdgeConnectorListener listener)
    {
      m_Listener = listener;
      resetPositionOnPan = true;
      Reset();
    }

    public override Edge edgeCandidate { get; set; }

    public override Port draggedPort { get; set; }

    public sealed override void Reset(bool didConnect = false)
    {
      if (m_CompatiblePorts != null)
      {
        m_GraphView.ports.ForEach(p => p.OnStopEdgeDragging());
        m_CompatiblePorts = null;
      }

      if (m_GhostEdge != null && m_GraphView != null)
        m_GraphView.RemoveElement(m_GhostEdge);
      if (m_WasPanned && !resetPositionOnPan | didConnect)
        m_GraphView?.UpdateViewTransform(m_GraphView.contentViewContainer.transform.position,
          m_GraphView.contentViewContainer.transform.scale);
      
      m_PanSchedule?.Pause();
      
      if (m_GhostEdge != null)
      {
        m_GhostEdge.input = null;
        m_GhostEdge.output = null;
      }

      if (draggedPort != null && !didConnect)
      {
        draggedPort.portCapLit = false;
        draggedPort = null;
      }

      edgeCandidate?.SetEnabled(true);
      
      m_GhostEdge = null;
      edgeCandidate = null;
      m_GraphView = null;
    }

    public override bool HandleMouseDown(MouseDownEvent evt)
    {
      var mousePosition = evt.mousePosition;
      if (draggedPort == null || edgeCandidate == null)
        return false;
      m_GraphView = draggedPort.GetFirstAncestorOfType<GraphView>();
      if (m_GraphView == null)
        return false;
      if (edgeCandidate.parent == null)
        m_GraphView.AddElement(edgeCandidate);
      var flag = draggedPort.direction == Direction.Output;
      edgeCandidate.candidatePosition = mousePosition;
      edgeCandidate.SetEnabled(false);
      if (flag)
      {
        edgeCandidate.output = draggedPort;
        edgeCandidate.input = null;
      }
      else
      {
        edgeCandidate.output = null;
        edgeCandidate.input = draggedPort;
      }

      draggedPort.portCapLit = true;
      m_CompatiblePorts =
        m_GraphView.GetCompatiblePorts(draggedPort, s_nodeAdapter);
      m_GraphView.ports.ForEach(p => p.OnStartEdgeDragging());
      foreach (var compatiblePort in m_CompatiblePorts)
        compatiblePort.highlight = true;
      edgeCandidate.UpdateEdgeControl();
      if (m_PanSchedule == null)
      {
        m_PanSchedule = m_GraphView.schedule.Execute(Pan).Every(10L)
          .StartingIn(10L);
        m_PanSchedule.Pause();
      }

      m_WasPanned = false;
      edgeCandidate.layer = int.MaxValue;
      return true;
    }

    internal Vector2 GetEffectivePanSpeed(Vector2 mousePos)
    {
      var vector = Vector2.zero;
      
      if (mousePos.x <= 100.0)
        vector.x = (float)(-((100.0 - mousePos.x) / 100.0 + 0.5) * 4.0);
      else if (mousePos.x >= m_GraphView.contentContainer.layout.width - 100.0)
        vector.x = (float)(((mousePos.x - (m_GraphView.contentContainer.layout.width - 100.0)) /
          100.0 + 0.5) * 4.0);
      if (mousePos.y <= 100.0)
        vector.y = (float)(-((100.0 - mousePos.y) / 100.0 + 0.5) * 4.0);
      else if (mousePos.y >= m_GraphView.contentContainer.layout.height - 100.0)
        vector.y = (float)(((mousePos.y - (m_GraphView.contentContainer.layout.height - 100.0)) /
          100.0 + 0.5) * 4.0);
      vector = Vector2.ClampMagnitude(vector, 10f);
      return vector;
    }

    public override void HandleMouseMove(MouseMoveEvent evt)
    {
      m_PanDiff = GetEffectivePanSpeed(
        ((VisualElement)evt.target).ChangeCoordinatesTo(m_GraphView.contentContainer, evt.localMousePosition));
      if (m_PanDiff != Vector3.zero)
        m_PanSchedule.Resume();
      else
        m_PanSchedule.Pause();
      var mousePosition = evt.mousePosition;
      edgeCandidate.candidatePosition = mousePosition;
      var endPort = GetEndPort(mousePosition);
      if (endPort != null)
      {
        if (m_GhostEdge == null)
        {
          m_GhostEdge = new TEdge();
          m_GhostEdge.isGhostEdge = true;
          m_GhostEdge.pickingMode = PickingMode.Ignore;
          m_GraphView.AddElement(m_GhostEdge);
        }

        if (edgeCandidate.output == null)
        {
          m_GhostEdge.input = edgeCandidate.input;
          if (m_GhostEdge.output != null)
            m_GhostEdge.output.portCapLit = false;
          m_GhostEdge.output = endPort;
          m_GhostEdge.output.portCapLit = true;
        }
        else
        {
          if (m_GhostEdge.input != null)
            m_GhostEdge.input.portCapLit = false;
          m_GhostEdge.input = endPort;
          m_GhostEdge.input.portCapLit = true;
          m_GhostEdge.output = edgeCandidate.output;
        }
      }
      else
      {
        if (m_GhostEdge == null)
          return;
        if (edgeCandidate.input == null)
        {
          if (m_GhostEdge.input != null)
            m_GhostEdge.input.portCapLit = false;
        }
        else if (m_GhostEdge.output != null)
          m_GhostEdge.output.portCapLit = false;

        m_GraphView.RemoveElement(m_GhostEdge);
        m_GhostEdge.input = null;
        m_GhostEdge.output = null;
        m_GhostEdge = null;
      }
    }

    private void Pan(TimerState ts)
    {
      m_GraphView.viewTransform.position -= m_PanDiff;
      //edgeCandidate.ForceUpdateEdgeControl();
      m_WasPanned = true;
    }

    public override void HandleMouseUp(MouseUpEvent evt)
    {
      var didConnect = false;
      var mousePosition = evt.mousePosition;
      m_GraphView.ports.ForEach(p => p.OnStopEdgeDragging());
      if (m_GhostEdge != null)
      {
        if (m_GhostEdge.input != null)
          m_GhostEdge.input.portCapLit = false;
        if (m_GhostEdge.output != null)
          m_GhostEdge.output.portCapLit = false;
        m_GraphView.RemoveElement(m_GhostEdge);
        m_GhostEdge.input = null;
        m_GhostEdge.output = null;
        m_GhostEdge = null;
      }

      var endPort = GetEndPort(mousePosition);
      if (endPort == null && m_Listener != null)
        m_Listener.OnDropOutsidePort(edgeCandidate, mousePosition);
      edgeCandidate.SetEnabled(true);
      if (edgeCandidate.input != null)
        edgeCandidate.input.portCapLit = false;
      if (edgeCandidate.output != null)
        edgeCandidate.output.portCapLit = false;
      if (edgeCandidate.input != null && edgeCandidate.output != null)
      {
        var input = edgeCandidate.input;
        var output = edgeCandidate.output;
        m_GraphView.DeleteElements(new []
        {
          edgeCandidate
        });
        edgeCandidate.input = input;
        edgeCandidate.output = output;
      }
      else
        m_GraphView.RemoveElement(edgeCandidate);

      if (endPort != null)
      {
        if (endPort.direction == Direction.Output)
          edgeCandidate.output = endPort;
        else
          edgeCandidate.input = endPort;
        m_Listener.OnDrop(m_GraphView, edgeCandidate);
        didConnect = true;
      }
      else
      {
        edgeCandidate.output = null;
        edgeCandidate.input = null;
      }

      edgeCandidate.ResetLayer();
      edgeCandidate = null;
      m_CompatiblePorts = null;
      Reset(didConnect);
    }

    private Port GetEndPort(Vector2 mousePosition)
    {
      if (m_GraphView == null)
        return null;
      var endPort = (Port)null;
      foreach (var compatiblePort in m_CompatiblePorts)
      {
        var worldBound = compatiblePort.worldBound;
        var height = worldBound.height;
        if (compatiblePort.orientation == Orientation.Horizontal)
        {
          switch (compatiblePort.direction)
          {
            case Direction.Input:
              worldBound.x -= height;
              worldBound.width += height;
              break;
            case Direction.Output:
              worldBound.width += height;
              break;
          }
        }

        if (!worldBound.Contains(mousePosition)) continue;
        
        endPort = compatiblePort;
        break;
      }

      return endPort;
    }
  }
}