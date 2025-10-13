using Godot;
using System;

public partial class Enemy : CharacterBody2D {
  public AnimatedSprite2D  Sprite2D     { get; private set; }
  public CollisionShape2D  Collider     { get; private set; }
  public EnemyStateMachine StateMachine { get; private set; }
  public RayCast2D[]       Raycasts     { get; private set; } = new RayCast2D[2];

  [Export] public int   Health { get; set; }
  [Export] public float Speed  { get; set; }

  private readonly float Gravity = 980.0f;

  public override void _Ready() {
    Sprite2D     = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
    Collider     = GetNode<CollisionShape2D>("CollisionShape2D");
    StateMachine = GetNode<EnemyStateMachine>("StateMachine");

    SpawnRaycasts();
    StateMachine.InitialStateOnReady();
  }

  public override void _Process(double delta) {
    FlipSprite();
  }

  public override void _PhysicsProcess(double delta) {
    ApplyGravity(delta);
    MoveAndSlide();
  }

  private void ApplyGravity(double delta) {
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

    var margin = 5.0f;
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
