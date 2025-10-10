using Godot;
using System;
using System.Collections.Generic;

public partial class StateMachine : Node {
  public State CurrentState  { get; private set; }
  public State PreviousState { get; private set; }

  [Export] private State initialState;

  private Dictionary<String, State> states;

  public override void _EnterTree() {
    RegisterStates();
  }

  public override void _Process(double delta) {
    CurrentState.Process(delta);
  }

  public override void _PhysicsProcess(double delta) {
    CurrentState.PhysicsProcess(delta);
  }

  public void ChangeState(String name) {
    if (!states.ContainsKey(name)) {
      throw new NullReferenceException();
    }

    PreviousState = CurrentState;
    CurrentState  = states.GetValueOrDefault(name)!;

    PreviousState.Exit();
    CurrentState.Enter();
  }

  private void RegisterStates() {
    var parent = GetParent();
    foreach (Node child in GetChildren()) {
      if (child is not State state) {
        throw new InvalidCastException();
      }

      state.SetStateMachine(this);
      state.SetParent(parent);

      states.Add(state.Name, state);
    }
  }
}
