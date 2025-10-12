using Godot;
using System;
using System.Collections.Generic;

public partial class Player : CharacterBody2D {
  public AnimatedSprite2D Sprite2D        { get; private set; }
  public AnimationPlayer  AnimationPlayer { get; private set; }
  public StateMachine     StateMachine    { get; private set; }
  public Node2D           AttacksRoot     { get; private set; }

  [Export] public float Speed     { get; private set; }
  [Export] public float JumpSpeed { get; private set; }

  public bool     Jump           { get; private set; } = true;
  public Vector2  Direction      { get; private set; } = Vector2.Right;
  public Marker2D AttackPosition { get; private set; }
  #pragma warning disable
  public string?  Attack         { get; private set; }
  #pragma warning restore

  private readonly float                        Gravity         = 980.0f;
  private readonly Dictionary<String, Marker2D> AttackPositions = [];

  public override void _Ready() {
    Sprite2D        = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
    AnimationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
    StateMachine    = GetNode<StateMachine>("StateMachine");
    AttacksRoot     = GetNode<Node2D>("Actions");

    RegisterAttackPositions();
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

  public void CollectAttackInput() {
    if        (Input.IsActionJustPressed("action_primary")) {
      Attack = "Primary";
    } else if (Input.IsActionJustPressed("action_primary")) {
      Attack = "Secondary";
    } else {
      Attack = null;
    }

    if (Attack != null) {
      if (Direction.Y != 0) {
        AttackPosition          = AttackPositions.GetValueOrDefault("Vertical")!;
        AttackPosition.Position = new Vector2(0.0f, Mathf.Abs(AttackPosition.Position.Y) * Direction.Y);
      } else {
        AttackPosition          = AttackPositions.GetValueOrDefault("Horizontal")!;
        AttackPosition.Position = new Vector2(Mathf.Abs(AttackPosition.Position.X) * Direction.X, 0.0f);
      }
      StateMachine.ChangeState("Attack");
    }
  }

  public void DisableCollider(bool value) {
    var collider      = (CollisionShape2D)AttackPosition.GetChild(0).GetChild(0);
    collider.Disabled = value;
  }
  
  public void SpawnProjectile() {
    throw new NotImplementedException();
  }

  public void SetJump(bool value) {
    Jump = value;
  }

  private void ApplyGravity(double delta) {
    Velocity = Velocity with { Y = Velocity.Y + (Gravity * (float)delta) }; 
  }

  private void FlipSprite() {
    Sprite2D.FlipH = Direction.X > 0.0f;
  }

  private void RegisterAttackPositions() {
    foreach (Node child in AttacksRoot.GetChildren()) {
      if (child is not Marker2D marker) {
        throw new InvalidCastException();
      }

      var area          = (Area2D)marker.GetChild(0);
      area.AreaEntered += (area) => { GD.Print($"Area entered : {area}"); };
      AttackPositions.Add(marker.Name, marker);
    }
  }
}
