using Godot;
using System;

public partial class Death : State<Player> {
  public override void OnEnter() {
    DisableParent();
    PlayDeathAnimation();
  }

  private void DisableParent() {
    Parent.SetInvincible(true);
    Parent.SetProcess(false);
    Parent.SetPhysicsProcess(false);
    Parent.EmitSignal(Player.SignalName.PlayerDied);
  }

  private void PlayDeathAnimation() {
    Parent.AnimationPlayer.Connect(
      AnimationPlayer.SignalName.AnimationFinished,
      Callable.From<String>(InstantiateGameOverScene),
      (uint)ConnectFlags.OneShot
    );
    Parent.AnimationPlayer.Queue("Death");
  }

  private void InstantiateGameOverScene(string name) {
    var root     = GetTree().GetFirstNodeInGroup("Game").GetParent();
    var instance = Parent.GameOverScene.Instantiate<CanvasLayer>();
    root.CallDeferred("add_child", instance);
  }
}
