using Godot;
using System;
using System.Collections.Generic;

public partial class StateMachine<T> : Node where T: Node {
  public State<T> CurrentState  { get; private set; }
  public State<T> PreviousState { get; private set; }

  [Export] private State<T> initialState;

  private readonly Dictionary<String, State<T>> states = [];

  public override void _EnterTree() {
    if (initialState == null) {
      throw new NullReferenceException();
    }
    CurrentState = initialState;
    CurrentState.OnEnter();

    RegisterStates();
  }

  public override void _Process(double delta) {
    CurrentState.OnProcess(delta);
  }

  public override void _PhysicsProcess(double delta) {
    CurrentState.OnPhysicsProcess(delta);
  }

  public void ChangeState(String name) {
    if (!states.ContainsKey(name)) {
      throw new NullReferenceException();
    }

    PreviousState = CurrentState;
    CurrentState  = states.GetValueOrDefault(name)!;

    PreviousState.OnExit();
    CurrentState.OnEnter();
  }

  private void RegisterStates() {
    var parent = GetParent();
    foreach (Node child in GetChildren()) {
      if (child is not State<T> state) {
        throw new InvalidCastException();
      }

      state.SetStateMachine(this);
      state.SetParent(parent);

      states.Add(state.Name, state);
    }
  }
}
