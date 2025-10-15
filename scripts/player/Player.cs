using Godot;
using System;
using System.Collections.Generic;
using System.Security;

public partial class Player : CharacterBody2D, IDamageable {
  public AnimatedSprite2D   Sprite2D        { get; private set; }
  public AnimationPlayer    AnimationPlayer { get; private set; }
  public PlayerStateMachine StateMachine    { get; private set; }
  public Timer              AttackTimer     { get; private set; }

  [Export] public float       Speed      { get; private set; }
  [Export] public float       JumpSpeed  { get; private set; }
  [Export] public int         Health     { get; private set; }
  [Export] public PackedScene Projectile { get; private set; }
  [Export] public int         Damage     { get; private set; }

  public bool     Jump           { get; private set; }
  public Vector2  Direction      { get; private set; }
  public Vector2  HurtDirection  { get; private set; }
  public Marker2D AttackPosition { get; private set; }
  #pragma warning disable
  public string?  Attack         { get; private set; }
  #pragma warning restore
  public int      CurrentHealth  { get; private set; }
  public bool     Alive          { get; private set; }
  public bool     Invincible     { get; private set; }
  public bool     CanAttack      { get; private set; }

  private readonly float      Gravity         = 980.0f;
  private readonly Marker2D[] AttackPositions = new Marker2D[2];
  private readonly float      AttackTime      = 0.2f;

  public override void _Ready() {
    Sprite2D        = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
    AnimationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
    StateMachine    = GetNode<PlayerStateMachine>("StateMachine");
    AttackTimer     = GetNode<Timer>("AttackTimer");

    RegisterAttackPositions();
    SetAttackTimer();
    StateMachine.InitialStateOnReady();
    SetDefaultStats();
  }

  public override void _Process(double delta) {
    FlipSprite();
  }

  public override void _PhysicsProcess(double delta) {
    ApplyGravity(delta);
    MoveAndSlide();
  }

  public void SetDefaultStats() {
    CurrentHealth = Health;
    Alive         = true;
    Invincible    = false;
    Jump          = true;
    Direction     = Vector2.Right;
    CanAttack     = true;
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
    if (!CanAttack) {
      return;
    }

    if        (Input.IsActionJustPressed("action_primary")) {
      Attack = "Primary";
    } else if (Input.IsActionJustPressed("action_secondary")) {
      Attack = "Secondary";
    } else {
      Attack = null;
    }

    if (Attack != null) {
      if (MathF.Abs(Direction.Y) >= MathF.Abs(Direction.X)) {
        AttackPosition          = AttackPositions[1];
        AttackPosition.Position = new Vector2(0.0f, MathF.Abs(AttackPosition.Position.Y) * MathF.Sign(Direction.Y));
      } else {
        AttackPosition          = AttackPositions[0];
        AttackPosition.Position = new Vector2(MathF.Abs(AttackPosition.Position.X) * MathF.Sign(Direction.X), 0.0f);
      }
      CanAttack = false;
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

  public bool TakeDamage(int damage, Vector2 position) {
    if (!Alive) {
      return false;
    }
    if (Invincible) {
      return true;
    }

    CurrentHealth -= damage;
    if (CurrentHealth <= 0) {
      CurrentHealth = 0;
      SetAlive(false);
    }
    HurtDirection = (GlobalPosition - position).Normalized();
    StateMachine.ChangeState("Hurt");

    return Alive;
  }

  public void DealDamage(Node2D node) {
    if (node is IDamageable damageable) {
      damageable.TakeDamage(Damage, GlobalPosition);
    }
  }

  public void SetShaderActive(bool value) {
    var material = (ShaderMaterial)Sprite2D.Material;
    material.SetShaderParameter("active", value);
  }

  public void SetInvincible(bool value) {
    Invincible = value;
  }

  public void SetJump(bool value) {
    Jump = value;
  }

  private void SetAlive(bool value) {
    if (Alive = value) {
      SetDefaultStats();
    }
  }

  private void ApplyGravity(double delta) {
    Velocity = Velocity with { Y = Velocity.Y + (Gravity * (float)delta) }; 
  }

  private void FlipSprite() {
    Sprite2D.FlipH = Direction.X > 0.0f;
  }

  private void RegisterAttackPositions() {
    var children = GetNode("Actions").GetChildren();
    for (int i = 0; i < AttackPositions.Length; ++i) {
      var child = children[i];
      if (child is not Marker2D marker) {
        throw new InvalidCastException();
      }

      var area           = (Area2D)marker.GetChild(0);
      area.BodyEntered  += DealDamage;
      AttackPositions[i] = marker;
    }
  }

  private void SetAttackTimer() {
    AttackTimer.WaitTime = AttackTime;
    AttackTimer.OneShot  = true;
    AttackTimer.Timeout += () => { CanAttack = true; };
  }
}
