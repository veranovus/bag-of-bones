using Godot;
using System;

public partial class Player : CharacterBody2D {
  public AnimatedSprite2D Sprite2D        { get; private set; }
  public AnimationPlayer  AnimationPlayer { get; private set; }
  public StateMachine     StateMachine    { get; private set; }

  public Vector2 Direction { get; private set; }

  private float gravity;

  public override void _Ready() {
    Sprite2D        = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
    AnimationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
    StateMachine    = GetNode<StateMachine>("StateMachine");

    gravity = (float)ProjectSettings.GetSetting("physics/2d/default_gravity");
  }

  public override void _Process(double delta) {
    FlipSprite();
  }

  public override void _PhysicsProcess(double delta) {
    ApplyGravity(delta);
    MoveAndSlide();
  }

  public Vector2 CollectDirectionalInput() {
    var input = Input.GetVector("action_left", "action_right", "action_up", "action_down");
    input.X   = MathF.Sign(input.X);
    input.Y   = MathF.Sign(input.Y);

    Direction = input;
    return input;
  }

  private void ApplyGravity(double delta) {
    Velocity = Velocity with { Y = Velocity.Y + (gravity * (float)delta) }; 
  }

  private void FlipSprite() {
    Sprite2D.FlipH = Direction.X > 0.0f;
  }
}
