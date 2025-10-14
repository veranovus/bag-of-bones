using Godot;
using System;

public partial class Enemy : CharacterBody2D, IDamageable {
  public AnimatedSprite2D  Sprite2D        { get; private set; }
  public CollisionShape2D  Collider        { get; private set; }
  public EnemyStateMachine StateMachine    { get; private set; }
  public AnimationPlayer   AnimationPlayer { get; private set; }
  public RayCast2D[]       Raycasts        { get; private set; } = new RayCast2D[2];

  [Export] public int   Health { get; private set; }
  [Export] public float Speed  { get; private set; }
  [Export] public bool  Fly    { get; private set; }
  [Export] public int   Damage { get; private set; }

  public int     CurrentHealth { get; private set; }
  public bool    Alive         { get; private set; }
  public Vector2 Direction     { get; private set; }
  public Vector2 HurtDirection { get; private set; }
  public bool    Invincible    { get; private set; }

  private readonly float Gravity = 980.0f;

  public override void _Ready() {
    Sprite2D        = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
    Collider        = GetNode<CollisionShape2D>("CollisionShape2D");
    StateMachine    = GetNode<EnemyStateMachine>("StateMachine");
    AnimationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");

    SpawnRaycasts();
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
    Direction     = Vector2.Right;
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

  public void SetDirection(Vector2 direction) {
    Direction = direction;
  }

  private void SetAlive(bool value) {
    if (Alive = value) {
      SetDefaultStats();
    }
  }

  private void ApplyGravity(double delta) {
    if (Fly) {
      return;
    }
    Velocity = Velocity with { Y = Velocity.Y + (Gravity * (float)delta) }; 
  }

  private void FlipSprite() {
    if (!Mathf.IsZeroApprox(MathF.Abs(Velocity.X))) {
      Sprite2D.FlipH = Velocity.X < 0.0f;
    }
  }

  private void SpawnRaycasts() {
    var root = new Node2D { Name = "Raycasts" };
    AddChild(root);

    var margin = 50.0f;
    var offset = Collider.Shape switch {
      RectangleShape2D rect    => new Vector2(rect.Size.X / 2.0f, rect.Size.Y / 2.0f),
      CircleShape2D    circle  => new Vector2(circle.Radius, circle.Radius),
      CapsuleShape2D   capsule => new Vector2(capsule.Height / 2.0f, capsule.Radius),
      _                        => new Vector2(0.0f, 0.0f), 
    };

    RayCast2D spawnRaycastDown(string name, float x, float y) {
      var raycast = new RayCast2D {
        Name           = name,
        Position       = new Vector2(x, y),
        TargetPosition = new Vector2(0.0f, offset.Y + margin),
        CollisionMask  = 0b0011,
      };
      root.AddChild(raycast);
      return raycast;
    }

    Raycasts[0] = spawnRaycastDown("Left", -offset.X, 0.0f);
    Raycasts[1] = spawnRaycastDown("Right", offset.X, 0.0f);
  }
}
