using Godot;
using System;

public partial class State<T>: Node where T : Node {
  protected StateMachine<T> StateMachine { get; private set; }
  protected T               Parent       { get; private set; }

  public virtual void OnEnter() {}
  public virtual void OnExit() {}
  public virtual void OnProcess(double delta) {}
  public virtual void OnPhysicsProcess(double delta) {}

  public void SetParent(Node parent) => Parent = (T)parent;
  public void SetStateMachine(StateMachine<T> stateMachine) => StateMachine = stateMachine;
}
