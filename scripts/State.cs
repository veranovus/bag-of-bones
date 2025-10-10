using Godot;
using System;

public partial class State : Node {
  protected StateMachine StateMachine { get; private set; }
  protected Node         Parent       { get; private set; }

  public virtual void Enter() {}
  public virtual void Exit() {}
  public virtual void Process(double delta) {}
  public virtual void PhysicsProcess(double delta) {}

  public void SetParent(Node parent) => Parent = parent;
  public void SetStateMachine(StateMachine stateMachine) => StateMachine = stateMachine;
}
