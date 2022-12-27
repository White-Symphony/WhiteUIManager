using System;
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
    protected static NodeAdapter s_nodeAdapter = new NodeAdapter();
    protected readonly IEdgeConnectorListener m_Listener;
    private IVisualElementScheduledItem m_PanSchedule;
    private Vector3 m_PanDiff = Vector3.zero;
    private bool m_WasPanned;

    public bool resetPositionOnPan { get; set; }

    public WUI_EdgeDragHelper(IEdgeConnectorListener listener)
    {
      this.m_Listener = listener;
      this.resetPositionOnPan = true;
      this.Reset(false);
    }

    public override Edge edgeCandidate { get; set; }

    public override Port draggedPort { get; set; }

    public override void Reset(bool didConnect = false)
    {
      if (this.m_CompatiblePorts != null)
      {
        this.m_GraphView.ports.ForEach((Action<Port>)(p => p.OnStopEdgeDragging()));
        this.m_CompatiblePorts = (List<Port>)null;
      }

      if (this.m_GhostEdge != null && this.m_GraphView != null)
        this.m_GraphView.RemoveElement((GraphElement)this.m_GhostEdge);
      if (this.m_WasPanned && !this.resetPositionOnPan | didConnect)
        this.m_GraphView.UpdateViewTransform(this.m_GraphView.contentViewContainer.transform.position,
          this.m_GraphView.contentViewContainer.transform.scale);
      if (this.m_PanSchedule != null)
        this.m_PanSchedule.Pause();
      if (this.m_GhostEdge != null)
      {
        this.m_GhostEdge.input = (Port)null;
        this.m_GhostEdge.output = (Port)null;
      }

      if (this.draggedPort != null && !didConnect)
      {
        this.draggedPort.portCapLit = false;
        this.draggedPort = (Port)null;
      }

      if (this.edgeCandidate != null)
        this.edgeCandidate.SetEnabled(true);
      this.m_GhostEdge = (Edge)null;
      this.edgeCandidate = (Edge)null;
      this.m_GraphView = (UnityEditor.Experimental.GraphView.GraphView)null;
    }

    public override bool HandleMouseDown(MouseDownEvent evt)
    {
      Vector2 mousePosition = evt.mousePosition;
      if (this.draggedPort == null || this.edgeCandidate == null)
        return false;
      this.m_GraphView = this.draggedPort.GetFirstAncestorOfType<UnityEditor.Experimental.GraphView.GraphView>();
      if (this.m_GraphView == null)
        return false;
      if (this.edgeCandidate.parent == null)
        this.m_GraphView.AddElement((GraphElement)this.edgeCandidate);
      bool flag = this.draggedPort.direction == Direction.Output;
      this.edgeCandidate.candidatePosition = mousePosition;
      this.edgeCandidate.SetEnabled(false);
      if (flag)
      {
        this.edgeCandidate.output = this.draggedPort;
        this.edgeCandidate.input = (Port)null;
      }
      else
      {
        this.edgeCandidate.output = (Port)null;
        this.edgeCandidate.input = this.draggedPort;
      }

      this.draggedPort.portCapLit = true;
      this.m_CompatiblePorts =
        this.m_GraphView.GetCompatiblePorts(this.draggedPort, WUI_EdgeDragHelper<TEdge>.s_nodeAdapter);
      this.m_GraphView.ports.ForEach((Action<Port>)(p => p.OnStartEdgeDragging()));
      foreach (Port compatiblePort in this.m_CompatiblePorts)
        compatiblePort.highlight = true;
      this.edgeCandidate.UpdateEdgeControl();
      if (this.m_PanSchedule == null)
      {
        this.m_PanSchedule = this.m_GraphView.schedule.Execute(new Action<TimerState>(this.Pan)).Every(10L)
          .StartingIn(10L);
        this.m_PanSchedule.Pause();
      }

      this.m_WasPanned = false;
      this.edgeCandidate.layer = int.MaxValue;
      return true;
    }

    internal Vector2 GetEffectivePanSpeed(Vector2 mousePos)
    {
      Vector2 vector = Vector2.zero;
      if ((double)mousePos.x <= 100.0)
        vector.x = (float)(-((100.0 - (double)mousePos.x) / 100.0 + 0.5) * 4.0);
      else if ((double)mousePos.x >= (double)this.m_GraphView.contentContainer.layout.width - 100.0)
        vector.x = (float)((((double)mousePos.x - ((double)this.m_GraphView.contentContainer.layout.width - 100.0)) /
          100.0 + 0.5) * 4.0);
      if ((double)mousePos.y <= 100.0)
        vector.y = (float)(-((100.0 - (double)mousePos.y) / 100.0 + 0.5) * 4.0);
      else if ((double)mousePos.y >= (double)this.m_GraphView.contentContainer.layout.height - 100.0)
        vector.y = (float)((((double)mousePos.y - ((double)this.m_GraphView.contentContainer.layout.height - 100.0)) /
          100.0 + 0.5) * 4.0);
      vector = Vector2.ClampMagnitude(vector, 10f);
      return vector;
    }

    public override void HandleMouseMove(MouseMoveEvent evt)
    {
      this.m_PanDiff = (Vector3)this.GetEffectivePanSpeed(
        ((VisualElement)evt.target).ChangeCoordinatesTo(this.m_GraphView.contentContainer, evt.localMousePosition));
      if (this.m_PanDiff != Vector3.zero)
        this.m_PanSchedule.Resume();
      else
        this.m_PanSchedule.Pause();
      Vector2 mousePosition = evt.mousePosition;
      this.edgeCandidate.candidatePosition = mousePosition;
      Port endPort = this.GetEndPort(mousePosition);
      if (endPort != null)
      {
        if (this.m_GhostEdge == null)
        {
          this.m_GhostEdge = (Edge)new TEdge();
          this.m_GhostEdge.isGhostEdge = true;
          this.m_GhostEdge.pickingMode = PickingMode.Ignore;
          this.m_GraphView.AddElement((GraphElement)this.m_GhostEdge);
        }

        if (this.edgeCandidate.output == null)
        {
          this.m_GhostEdge.input = this.edgeCandidate.input;
          if (this.m_GhostEdge.output != null)
            this.m_GhostEdge.output.portCapLit = false;
          this.m_GhostEdge.output = endPort;
          this.m_GhostEdge.output.portCapLit = true;
        }
        else
        {
          if (this.m_GhostEdge.input != null)
            this.m_GhostEdge.input.portCapLit = false;
          this.m_GhostEdge.input = endPort;
          this.m_GhostEdge.input.portCapLit = true;
          this.m_GhostEdge.output = this.edgeCandidate.output;
        }
      }
      else
      {
        if (this.m_GhostEdge == null)
          return;
        if (this.edgeCandidate.input == null)
        {
          if (this.m_GhostEdge.input != null)
            this.m_GhostEdge.input.portCapLit = false;
        }
        else if (this.m_GhostEdge.output != null)
          this.m_GhostEdge.output.portCapLit = false;

        this.m_GraphView.RemoveElement((GraphElement)this.m_GhostEdge);
        this.m_GhostEdge.input = (Port)null;
        this.m_GhostEdge.output = (Port)null;
        this.m_GhostEdge = (Edge)null;
      }
    }

    private void Pan(TimerState ts)
    {
      this.m_GraphView.viewTransform.position -= this.m_PanDiff;
      this.edgeCandidate.ForceUpdateEdgeControl();
      this.m_WasPanned = true;
    }

    public override void HandleMouseUp(MouseUpEvent evt)
    {
      bool didConnect = false;
      Vector2 mousePosition = evt.mousePosition;
      this.m_GraphView.ports.ForEach((Action<Port>)(p => p.OnStopEdgeDragging()));
      if (this.m_GhostEdge != null)
      {
        if (this.m_GhostEdge.input != null)
          this.m_GhostEdge.input.portCapLit = false;
        if (this.m_GhostEdge.output != null)
          this.m_GhostEdge.output.portCapLit = false;
        this.m_GraphView.RemoveElement((GraphElement)this.m_GhostEdge);
        this.m_GhostEdge.input = (Port)null;
        this.m_GhostEdge.output = (Port)null;
        this.m_GhostEdge = (Edge)null;
      }

      Port endPort = this.GetEndPort(mousePosition);
      if (endPort == null && this.m_Listener != null)
        this.m_Listener.OnDropOutsidePort(this.edgeCandidate, mousePosition);
      this.edgeCandidate.SetEnabled(true);
      if (this.edgeCandidate.input != null)
        this.edgeCandidate.input.portCapLit = false;
      if (this.edgeCandidate.output != null)
        this.edgeCandidate.output.portCapLit = false;
      if (this.edgeCandidate.input != null && this.edgeCandidate.output != null)
      {
        Port input = this.edgeCandidate.input;
        Port output = this.edgeCandidate.output;
        this.m_GraphView.DeleteElements((IEnumerable<GraphElement>)new Edge[1]
        {
          this.edgeCandidate
        });
        this.edgeCandidate.input = input;
        this.edgeCandidate.output = output;
      }
      else
        this.m_GraphView.RemoveElement((GraphElement)this.edgeCandidate);

      if (endPort != null)
      {
        if (endPort.direction == Direction.Output)
          this.edgeCandidate.output = endPort;
        else
          this.edgeCandidate.input = endPort;
        this.m_Listener.OnDrop(this.m_GraphView, this.edgeCandidate);
        didConnect = true;
      }
      else
      {
        this.edgeCandidate.output = (Port)null;
        this.edgeCandidate.input = (Port)null;
      }

      this.edgeCandidate.ResetLayer();
      this.edgeCandidate = (Edge)null;
      this.m_CompatiblePorts = (List<Port>)null;
      this.Reset(didConnect);
    }

    private Port GetEndPort(Vector2 mousePosition)
    {
      if (this.m_GraphView == null)
        return (Port)null;
      Port endPort = (Port)null;
      foreach (Port compatiblePort in this.m_CompatiblePorts)
      {
        Rect worldBound = compatiblePort.worldBound;
        float height = worldBound.height;
        if (compatiblePort.orientation == Orientation.Horizontal)
        {
          if (compatiblePort.direction == Direction.Input)
          {
            worldBound.x -= height;
            worldBound.width += height;
          }
          else if (compatiblePort.direction == Direction.Output)
            worldBound.width += height;
        }

        if (worldBound.Contains(mousePosition))
        {
          endPort = compatiblePort;
          break;
        }
      }

      return endPort;
    }
  }
}