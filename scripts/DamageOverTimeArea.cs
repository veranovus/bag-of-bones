using Godot;
using System;
using System.Collections.Generic;

public partial class DamageOverTimeArea : Area2D {
  private CanvasLayer canvasLayer;
  private Timer       timer;

  [Export] private float damageFrequency = 1.0f;
  [Export] private bool  showOverlay     = false;

  private int      damage;
  private Callable deathCallback;

  private readonly List<Node2D> Bodies = [];

  public override void _Ready() {
    canvasLayer = GetNode<CanvasLayer>("CanvasLayer");
    timer       = GetNode<Timer>("Timer");

    SetTimer();
  }

  public void Enable() {
    Bodies.Clear();
    Monitoring          = true;
    canvasLayer.Visible = showOverlay;
  }

  public void Disable() {
    Monitoring          = false;
    canvasLayer.Visible = false;
  }

  public void SetDamage(int value) {
    damage = value;
  } 

  public void SetDeathCallback(Callable action) {
    deathCallback = action;
  }

  private void DealDamage() {
    if (Bodies.Count == 0) {
      timer.Stop();
    }
    Bodies.RemoveAll(item => item == null);

    var remove = new List<Node2D>();
    foreach (var node in Bodies) {
      var damageable = (IDamageable)node;
      if (damageable.TakeDamage(damage, GlobalPosition)) {
        continue;
      }
      remove.Add(node);
      deathCallback.Call();
    }

    foreach (var node in remove) {
      Bodies.Remove(node);
    }
  }

  private void OnBodyEntered(Node2D node) {
    if (node is IDamageable damageable) {
      Bodies.Add(node);
      if (Monitoring) {
        timer.Start();
      }
    }
  }

  private void OnBodyExited(Node2D node) {
    if (node is IDamageable damageable) {
      Bodies.Remove(node);
    }
  }

  private void SetTimer() {
    timer.WaitTime = damageFrequency;
    timer.OneShot  = false;
    timer.Timeout += DealDamage;
  }
}
