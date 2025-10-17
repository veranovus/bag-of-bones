using Godot;
using System;
using System.Collections.Generic;

public partial class DamageOverTimeArea : Area2D {
  private CanvasLayer canvasLayer;
  private Timer       timer;

  [Export] private float damageFrequency = 1.0f;
  [Export] private bool  showOverlay     = false;

  private int          damage;
  private List<Node2D> bodies = [];

  public override void _Ready() {
    canvasLayer = GetNode<CanvasLayer>("CanvasLayer");
    timer       = GetNode<Timer>("Timer");

    SetTimer();
  }

  public void Enable() {
    bodies.Clear();
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

  private void DealDamage() {
    if (bodies.Count == 0) {
      timer.Stop();
    }
    bodies.RemoveAll(item => item == null);

    var remove = new List<Node2D>();
    foreach (var node in bodies) {
      var damageable = (IDamageable)node;
      if (damageable.TakeDamage(damage, GlobalPosition)) {
        continue;
      }
      remove.Add(node);
    }

    foreach (var node in remove) {
      bodies.Remove(node);
    }
  }

  private void OnBodyEntered(Node2D node) {
    if (node is IDamageable damageable) {
      bodies.Add(node);
      if (Monitoring) {
        timer.Start();
      }
    }
  }

  private void OnBodyExited(Node2D node) {
    if (node is IDamageable damageable) {
      bodies.Remove(node);
    }
  }

  private void SetTimer() {
    timer.WaitTime = damageFrequency;
    timer.OneShot  = false;
    timer.Timeout += DealDamage;
  }
}
