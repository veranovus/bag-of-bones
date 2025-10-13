using Godot;
using System;
using System.Collections.Generic;

public partial class Player : CharacterBody2D, IDamageable {
  public AnimatedSprite2D   Sprite2D        { get; private set; }
  public AnimationPlayer    AnimationPlayer { get; private set; }
  public PlayerStateMachine StateMachine    { get; private set; }
  public Node2D             AttacksRoot     { get; private set; }

  [Export] public float       Speed      { get; private set; }
  [Export] public float       JumpSpeed  { get; private set; }
  [Export] public float       Health     { get; private set; }
  [Export] public PackedScene Projectile { get; private set; }

  public bool     Jump           { get; private set; } = true;
  public Vector2  Direction      { get; private set; } = Vector2.Right;
  public Marker2D AttackPosition { get; private set; }
  #pragma warning disable
  public string?  Attack         { get; private set; }
  #pragma warning restore
  public int      CurrentHealth  { get; private set; }
  public bool     Alive          { get; private set; }

  private readonly float                        Gravity         = 980.0f;
  private readonly Dictionary<String, Marker2D> AttackPositions = [];

  public override void _Ready() {
    Sprite2D        = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
    AnimationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
    StateMachine    = GetNode<PlayerStateMachine>("StateMachine");
    AttacksRoot     = GetNode<Node2D>("Actions");

    RegisterAttackPositions();
    StateMachine.InitialStateOnReady();
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

    Direction = new Vector2(
      Mathf.IsZeroApprox(input.X) ? 0.5f * MathF.Sign(Direction.X) : input.X,
      input.Y
    );

    input.X = MathF.Sign(input.X);
    input.Y = MathF.Sign(input.Y);

    return input;
  }

  public void CollectAttackInput() {
    if        (Input.IsActionJustPressed("action_primary")) {
      Attack = "Primary";
    } else if (Input.IsActionJustPressed("action_secondary")) {
      Attack = "Secondary";
    } else {
      Attack = null;
    }

    if (Attack != null) {
      if (MathF.Abs(Direction.Y) >= MathF.Abs(Direction.X)) {
        AttackPosition          = AttackPositions.GetValueOrDefault("Vertical");
        AttackPosition.Position = new Vector2(0.0f, MathF.Abs(AttackPosition.Position.Y) * MathF.Sign(Direction.Y));
      } else {
        AttackPosition          = AttackPositions.GetValueOrDefault("Horizontal");
        AttackPosition.Position = new Vector2(MathF.Abs(AttackPosition.Position.X) * MathF.Sign(Direction.X), 0.0f);
      }
      StateMachine.ChangeState("Attack");
    }
  }

  public void PlayAttackAnimation() {
    var animation = Attack;
    if (MathF.Abs(Direction.Y) >= MathF.Abs(Direction.X)) {
      animation += (Direction.Y > 0.0f) ? "Down" : "Up";
    }
    Sprite2D.Play(animation);
  }

  public void DisableCollider(bool value) {
    var collider      = (CollisionShape2D)AttackPosition.GetChild(0).GetChild(0);
    collider.Disabled = value;
  }
  
  public void SpawnProjectile() {
    var instance      = Projectile.Instantiate<Projectile>();
    instance.Position = AttackPosition.GlobalPosition;
    instance.SetDirection(instance.Position + AttackPosition.Position.Normalized());
    GetParent().AddChild(instance);
  }

  public bool TakeDamage(int damage, Vector2 direction) {
    if (!Alive) {
      return false;
    }

    CurrentHealth -= damage;
    if (CurrentHealth <= 0) {
      CurrentHealth = 0;
      Alive         = false;
    }
    return Alive;
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
