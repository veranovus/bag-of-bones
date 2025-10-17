using Godot;
using System;

public partial class Enemy : CharacterBody2D, IDamageable {
  public AnimatedSprite2D  Sprite2D         { get; private set; }
  public CollisionShape2D  Collider         { get; private set; }
  public EnemyStateMachine StateMachine     { get; private set; }
  public AnimationPlayer   AnimationPlayer  { get; private set; }
  public Player            Player           { get; private set; }
  public RayCast2D[]       Raycasts         { get; private set; } = new RayCast2D[4];
  public Marker2D          ProjectileMarker { get; private set; }
  public Timer             AttackTimer      { get; private set; }
  public Timer             QueueFreeTimer   { get; private set; }
  public CpuParticles2D    ParticleEmitter  { get; private set; }

  [Export] public int         Health     { get; private set; }
  [Export] public float       Speed      { get; private set; }
  [Export] public bool        Fly        { get; private set; }
  [Export] public int         Damage     { get; private set; }
  [Export] public PackedScene Projectile { get; private set; }

  public int     CurrentHealth { get; private set; }
  public bool    Alive         { get; private set; }
  public Vector2 Direction     { get; private set; }
  public Vector2 HurtDirection { get; private set; }
  public bool    Invincible    { get; private set; }
  public bool    CanAttack     { get; private set; }
  public int     CurrentDamage { get; private set; }
  public int     Difficulty    { get; private set; }
  public int     CurrentBone   { get; private set; }
  public bool    InScreen      { get; private set; }
  public bool    FirstTime     { get; private set; } = true;

  private const float Gravity         = 980.0f;
  private const float AttackTime      = 3.0f;
  private const int   DamageIncrease  = 2;
  private const int   Bone            = 5;
  private const int   BoneIncrease    = 1;

  public override void _Ready() {
    Sprite2D         = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
    Collider         = GetNode<CollisionShape2D>("CollisionShape2D");
    StateMachine     = GetNode<EnemyStateMachine>("StateMachine");
    AnimationPlayer  = GetNode<AnimationPlayer>("AnimationPlayer");
    ParticleEmitter  = GetNode<CpuParticles2D>("CPUParticles2D");
    Player           = (Player)GetTree().GetFirstNodeInGroup("Player");
    if (Projectile != null) {
      ProjectileMarker = GetNode<Marker2D>("Projectile");
      SpawnAttackTimer();
    }
    Sprite2D.Material = (Material)Sprite2D.Material.Duplicate();

    ConnectOnDifficultyIncreased();
    ConnectOnPlayerDied();

    SpawnRaycasts();
    SpawnQueueFreeTimer();
    SpawnAttackTimer();
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
    CanAttack     = false;
    CurrentDamage = Damage + (Difficulty * DamageIncrease);
    CurrentBone   = Bone   + (Difficulty * BoneIncrease);
    Difficulty    = 0;
  }

  public void StartAttackTimer() {
    AttackTimer.Start();
  }

  public void SpawnProjectile() {
    var instance      = Projectile.Instantiate<Projectile>();
    var position      = ProjectileMarker.Position * new Vector2(MathF.Sign(Direction.X), 1.0f);
    instance.Position = GlobalPosition + position;
    instance.SetDirection(instance.Position + Direction);
    instance.SetDamage(Difficulty, DamageIncrease);
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

  public void EmitPartciles() {
    ParticleEmitter.Direction = HurtDirection;
    ParticleEmitter.Amount    = CurrentBone;
    ParticleEmitter.Emitting  = true;

    Player.AddBone(CurrentBone);
  }

  public void DealDamage(Node2D node) {
    if (Invincible) {
      return;
    }
    if (node is IDamageable damageable) {
      damageable.TakeDamage(CurrentDamage, GlobalPosition);
    }
  }

  public void SetShaderActive(bool value) {
    var material = (ShaderMaterial)Sprite2D.Material;
    material.SetShaderParameter("active", value);
  }

  public void SetCanAttack(bool value) {
    CanAttack = value;
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
    } else if (Fly) {
      Direction      = (Player.GlobalPosition - GlobalPosition).Normalized();
      Sprite2D.FlipH = Direction.X < 0.0f;
    }
  }

  private void OnScreenEntered() {
    if (FirstTime) {
      StartAttackTimer();
      StateMachine.InitialStateOnReady();
      SetDefaultStats();
      FirstTime = false;
    }
    InScreen = true;
  }

  private void OnScreenExited() {
    InScreen = false;
    QueueFreeTimer.Start(); 
  }

  private void OnPlayerDied() {
    ProcessMode = ProcessModeEnum.Disabled;
  }

  private void OnDifficultyIncreased(int difficulty) {
    Difficulty    = difficulty;
    CurrentDamage = Damage + (Difficulty * ((Fly) ? 1 : DamageIncrease));
    CurrentBone   = Bone   + (Difficulty * BoneIncrease);
  }

  private void ConnectOnDifficultyIncreased() {
    var game = (Game)GetTree().GetFirstNodeInGroup("Game");
    game.DifficultyIncreased += OnDifficultyIncreased;

    Difficulty = game.Difficulty;
  }

  private void ConnectOnPlayerDied() {
    Player.Connect(
      Player.SignalName.PlayerDied,
      Callable.From(OnPlayerDied),
      (uint)ConnectFlags.OneShot
    );
  }

  private void SpawnQueueFreeTimer() {
    QueueFreeTimer = new Timer {
      Name     = "QueueFreeTimer",
      WaitTime = 5.0f,
      OneShot  = true,
    };
    QueueFreeTimer.Timeout += () => { if (!InScreen) QueueFree(); };
    AddChild(QueueFreeTimer);
  }

  private void SpawnRaycasts() {
    var root = new Node2D { Name = "Raycasts" };
    AddChild(root);

    var margin = new Vector2(10.0f, 50.0f);
    var offset = Collider.Shape switch {
      RectangleShape2D rect    => new Vector2(rect.Size.X / 2.0f, rect.Size.Y / 2.0f),
      CircleShape2D    circle  => new Vector2(circle.Radius, circle.Radius),
      CapsuleShape2D   capsule => new Vector2(capsule.Height / 2.0f, capsule.Radius),
      _                        => new Vector2(0.0f, 0.0f), 
    };

    RayCast2D spawnRaycast(string name, float x, float y, Vector2 target) {
      var raycast = new RayCast2D {
        Name           = name,
        Position       = new Vector2(x, y),
        TargetPosition = target,
        CollisionMask  = 0b0011,
      };
      root.AddChild(raycast);
      return raycast;
    }

    Raycasts[0] = spawnRaycast("DownLeft", -offset.X, 0.0f, new Vector2(0.0f, offset.X + margin.Y));
    Raycasts[1] = spawnRaycast("DownRight", offset.X, 0.0f, new Vector2(0.0f, offset.X + margin.Y));
    Raycasts[2] = spawnRaycast("Left", -offset.X, 0.0f, new Vector2(-margin.X, 0.0f));
    Raycasts[3] = spawnRaycast("Right", offset.X, 0.0f, new Vector2( margin.X, 0.0f));
  }

  private void SpawnAttackTimer() {
    AttackTimer = new Timer() {
      Name     = "AttackTimer",
      WaitTime = AttackTime,
      OneShot  = true,
    };
    AttackTimer.Timeout += () => { CanAttack = true; };
    AddChild(AttackTimer);
  }
}
