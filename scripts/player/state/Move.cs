using Godot;
using System;

public partial class Move : State<Player> {
  public override void OnPhysicsProcess(double delta) {
    var input = Parent.CollectDirectionalInput();

    Parent.Velocity = Parent.Velocity with { X = input.X * 200.0f };
  }
}
