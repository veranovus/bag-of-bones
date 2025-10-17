using Godot;
using System;
using System.Collections.Generic;

public partial class Camera : Camera2D {
  [Export] private Texture2D foregroundTexture;

  private Node2D             foregroundRoot;
  private DamageOverTimeArea damageArea;
  private Player             player;

  private int    damage;
  private float  speed;
  private int    generation;
  private int    difficulty;

  private readonly List<Sprite2D> foregrounds = [];
  private const    int            Damage         = int.MaxValue;
  private const    float          Speed          = 128.0f;
  private const    float          SpeedIncrease  = 32.0f;
  private const    float          FollowMargin   = 1080.0f / 4.0f * 0.5f;

  public override void _Ready() {
    foregroundRoot = GetNode<Node2D>("Foregrounds");
    damageArea     = GetNode<DamageOverTimeArea>("DamageOverTimeArea");
    player         = (Player)GetTree().GetFirstNodeInGroup("Player");

    ConnectOnPlayerDied();
    ConnectOnDifficultyIncreased();
    
    SetInitialStats();
    damageArea.SetDamage(damage);
    damageArea.Enable();

    SpawnForeground();
    SpawnForeground();
  }

  public override void _PhysicsProcess(double delta) {
    if (!player.Alive) {
      return;
    }

    var diff   = player.GlobalPosition - GlobalPosition;
    var offset = 0.0f; 
    if (diff.Y > FollowMargin) {
      offset = (diff.Y * 0.9f) * (float)(delta / 0.25);
      Position = Position with { Y = Position.Y + offset };
    } else {
      offset = speed * (float)delta;
      Position = Position with { Y = Position.Y + offset };
    }

    MoveForegrounds(offset * 0.25f);
  }

  private void SetInitialStats() {
    damage     = Damage;
    speed      = Speed;
    generation = 0;
    difficulty = 0;
  }

  private void MoveForegrounds(float offset) {
    for (int i = 0; i < foregrounds.Count; ++i) {
      var sprite      = foregrounds[i];
      sprite.Position = new Vector2(
        sprite.Position.X,
        sprite.Position.Y - offset
      );
    }

    var first = foregrounds[0];
    if (MathF.Abs(first.Position.Y) >= 1080.0f) {
      SpawnForeground();
      foregrounds.Remove(first);
      first.QueueFree();
    }
  }

  private void OnPlayerDied() {
    damageArea.Disable();

    var diff  = player.GlobalPosition - GlobalPosition;
    var tween = CreateTween();

    tween.SetParallel(true);
    tween.TweenProperty(this, "position", Position + new Vector2(0.0f, diff.Y), 0.10f);
    foreach (var fg in foregrounds) {
      tween.TweenProperty(fg, "position", fg.Position + new Vector2(0.0f, diff.Y) * 0.25f, 0.10f);
    }
  }

  private void OnDifficultyIncreased(int value) {
    difficulty = value;
    damage     = Damage; 
    speed      = Speed  + (difficulty * SpeedIncrease);
    damageArea.SetDamage(damage);
  }

  private void ConnectOnDifficultyIncreased() {
    var game = (Game)GetTree().GetFirstNodeInGroup("Game");
    game.DifficultyIncreased += OnDifficultyIncreased;

    difficulty = game.Difficulty;
  }

  private void ConnectOnPlayerDied() {
    player.Connect(
      Player.SignalName.PlayerDied,
      Callable.From(OnPlayerDied),
      (uint)ConnectFlags.OneShot
    );
  }

  private void SpawnForeground() {
    var instance = new Sprite2D {
      Name    = "Foreground",
      Texture = foregroundTexture,
      FlipH   = (generation % 2) == 0,
    };
    if (foregrounds.Count > 0) {
      instance.Position = foregrounds[^1].Position + new Vector2(0.0f, 1080.0f);
    }
    foregrounds.Add(instance);
    foregroundRoot.AddChild(instance);
    generation += 1;
  }
}
