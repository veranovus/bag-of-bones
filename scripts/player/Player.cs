using Godot;
using System;
using System.Collections.Generic;
using System.Security;
using System.Security.Principal;

public partial class Player : CharacterBody2D, IDamageable {
  [Signal] public delegate void PlayerDiedEventHandler();

  public AnimatedSprite2D   Sprite2D        { get; private set; }
  public AnimationPlayer    AnimationPlayer { get; private set; }
  public PlayerStateMachine StateMachine    { get; private set; }
  public Timer              UltimateTimer   { get; private set; }
  public Timer              AttackTimer     { get; private set; }
  public Timer              RegenTimer      { get; private set; }
  public Timer              RegenStartTimer { get; private set; }
  public EntityAudioManager AudioManager    { get; private set; }
  public PlayerUI           PlayerUI        { get; private set; }
  public DamageOverTimeArea DamageArea      { get; private set; }
  public CpuParticles2D     ParticleEmitter { get; private set; }
  public Camera             Camera          { get; private set; }

  [Export] public float       Speed         { get; private set; }
  [Export] public float       JumpSpeed     { get; private set; }
  [Export] public int         Health        { get; private set; }
  [Export] public PackedScene Projectile    { get; private set; }
  [Export] public int         Damage        { get; private set; }
  [Export] public int         SpecialCharge { get; private set; }
  // NOTE: Not an `[Export]` because it was resulting in circular dependencies.
           public PackedScene GameOverScene { get; private set; }

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
  public int      Score          { get; private set; }
  public int      CurrentBone    { get; private set; }
  public int      Difficulty     { get; private set; }
  public bool     DisableGravity { get; private set; }
  public bool     Ultimate       { get; private set; }

  private const    float      Gravity              = 980.0f;
  private const    float      UltimateTime         = 10.0f;
  private const    float      RegenStartTime       = 3.5f;
  private const    float      RegenStartTimeReduce = 0.5f;
  private const    float      RegenTime            = 0.2f;
  private const    int        RegenAmount          = 2;
  private const    int        AttackCost           = 5;
  private const    float      AttackTime           = 0.1f;
  private readonly Marker2D[] AttackPositions = new Marker2D[2];

  public override void _Ready() {
    Sprite2D        = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
    AnimationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
    StateMachine    = GetNode<PlayerStateMachine>("StateMachine");
    UltimateTimer   = GetNode<Timer>("UltimateTimer");
    AttackTimer     = GetNode<Timer>("AttackTimer");
    RegenTimer      = GetNode<Timer>("RegenTimer");
    RegenStartTimer = GetNode<Timer>("RegenStartTimer");
    AudioManager    = GetNode<EntityAudioManager>("EntityAudioManager");
    PlayerUI        = GetNode<PlayerUI>("PlayerUI");
    DamageArea      = GetNode<DamageOverTimeArea>("DamageOverTimeArea");
    ParticleEmitter = GetNode<CpuParticles2D>("CPUParticles2D");
    Camera          = (Camera)GetTree().GetFirstNodeInGroup("Camera");
    GameOverScene   = ResourceLoader.Load<PackedScene>("res://scenes/ui/game_over_ui.tscn");

    ConnectOnDifficultyIncreased();
    RegisterAttackPositions();
    SetUltimateTimer();
    SetAttackTimer();
    SetRegenTimer();
    SetRegenStartTimer(true);
    StateMachine.InitialStateOnReady();
    SetDefaultStats();
  }

  public override void _Process(double delta) {
    FlipSprite();
  }

  public override void _PhysicsProcess(double delta) {
    UpdateUltimateVFXPositions();
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
    CurrentBone   = 0;
    Difficulty    = 0;

    DamageArea.SetDamage(int.MaxValue);
    DamageArea.Disable();

    PlayerUI.UpdateHealthbar();
    PlayerUI.UpdateSpecialbar();
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
    } else if (Input.IsActionJustPressed("action_ultimate")) {
      Attack = "Ultimate";
    } else if (Input.IsActionPressed("action_ultimate_controller_left") && 
               Input.IsActionPressed("action_ultimate_controller_right")) {
      Attack = "Ultimate";
    } else {
      Attack = null;
    }

    if (Attack != null) {
      if (Attack == "Ultimate") {
        if (!PayUltimateCost()) {
          return;
        }
      } else if (!PayAttackCost(AttackCost)) {
        AudioManager.PlayRandomAudio("LowHealth");
        return;
      }

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
    if (Invincible || Ultimate) {
      return true;
    }

    CurrentHealth -= damage;
    if (CurrentHealth <= 0) {
      CurrentHealth = 0;
      SetAlive(false);
    }

    RegenTimer.Stop();
    RegenStartTimer.Start();

    PlayerUI.UpdateHealthbar();
    PlayerUI.PlayAnimation("Hurt");

    HurtDirection = (GlobalPosition - position).Normalized();
    StateMachine.ChangeState("Hurt");

    return Alive;
  }

  public void ChangeToDefaultState() {
    if (!Alive) {
      return;
    }
    StateMachine.ChangeState("Move");
  }

  public void DealDamage(Node2D node) {
    if (node is IDamageable damageable) {
      if (!damageable.TakeDamage(Damage, GlobalPosition)) {
        AddScore();
      }
    }
  }

  public void AddScore(int value = 1) {
    Score += value;
    PlayerUI.UpdateScore();
    PlayerUI.PlayAnimation("ScoreUp");
  }

  public void AddBone(int value) {
    if (CurrentBone == SpecialCharge) {
      return;
    }
    if (Ultimate) {
      return;
    }

    CurrentBone += value;
    if (CurrentBone > SpecialCharge) {
      CurrentBone = SpecialCharge;
    }

    PlayerUI.UpdateSpecialbar();
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

  public void SetUltimate(bool value) {
    if (Ultimate = value) {
      DamageArea.Enable();
      ParticleEmitter.Emitting = true;

      UltimateTimer.Start();
      CreateSpecialbarDrainTween();
    } else {
      DamageArea.Disable();
      ParticleEmitter.Emitting = false;
    }
  }

  public void SetDisableGravity(bool value) {
    if (DisableGravity = value) {
      Velocity = Vector2.Zero;
    }
  }

  private void SetAlive(bool value) {
    if (Alive = value) {
      SetDefaultStats();
    }
  }

  private void Regenerate() {
    if (!Alive || !CanAttack) {
      return;
    }
    CurrentHealth += RegenAmount;
    if (CurrentHealth > Health) {
      CurrentHealth = Health;
    }
    PlayerUI.UpdateHealthbar();
  }

  private bool PayUltimateCost() {
    return CurrentBone == SpecialCharge;
  }

  private bool PayAttackCost(int value) {
    if (Ultimate) {
      return true;
    }
    if (CurrentHealth - value < 1) {
      return false;
    }

    CurrentHealth -= value;
    PlayerUI.UpdateHealthbar();

    RegenTimer.Stop();
    RegenStartTimer.Start();

    return true;
  }

  private void UpdateUltimateVFXPositions() {
    if (Ultimate) {
      DamageArea.GlobalPosition      = Camera.GlobalPosition;
      ParticleEmitter.GlobalPosition = Camera.GlobalPosition - new Vector2(0.0f, 1080.0f / 2.0f);
      PlayerUI.UpdateSpecialbar();
    } else {
      DamageArea.Position = Vector2.Zero;
    }
  }

  private void ApplyGravity(double delta) {
    if (DisableGravity) {
      return;
    }
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

  private void CreateSpecialbarDrainTween() {
      var tween = CreateTween();
      tween.TweenProperty(this, "CurrentBone", 0, UltimateTime);
  }

  private void OnDifficultyIncreased(int difficulty) {
    Difficulty = difficulty;
    SetRegenStartTimer();
  }

  private void ConnectOnDifficultyIncreased() {
    var game = (Game)GetTree().GetFirstNodeInGroup("Game");
    game.DifficultyIncreased += OnDifficultyIncreased;

    Difficulty = game.Difficulty;
  }

  private void SetUltimateTimer() {
    UltimateTimer.WaitTime = UltimateTime;
    UltimateTimer.OneShot  = true;
    UltimateTimer.Timeout += () => { SetUltimate(false); };
  }

  private void SetAttackTimer() {
    AttackTimer.WaitTime = AttackTime;
    AttackTimer.OneShot  = true;
    AttackTimer.Timeout += () => { CanAttack = true; };
  }

  private void SetRegenTimer() {
    RegenTimer.WaitTime = RegenTime;
    RegenTimer.OneShot  = false;
    RegenTimer.Timeout += Regenerate;
  }

  private void SetRegenStartTimer(bool first = false) {
    RegenStartTimer.WaitTime = RegenStartTime - (Difficulty * RegenStartTimeReduce);
    if (!first) {
      return;
    }
    RegenStartTimer.OneShot  = true;
    RegenStartTimer.Timeout += () => { RegenTimer.Start(); };
  }
}
