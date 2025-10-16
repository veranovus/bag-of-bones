using Godot;
using System;

public partial class TitleUI : Control {
  private GlobalAudioManager audioManager;

  [Export] private PackedScene gameScene;

  public override void _Ready() {
    audioManager = (GlobalAudioManager)GetTree().GetFirstNodeInGroup("AudioManager");
  }

  private void OnPlayButtonPressed() {
    var instance = gameScene.Instantiate<Node2D>();
    GetParent().GetParent().CallDeferred("add_child", instance);
    CallDeferred("queue_free");
  }

  private void OnQuitButtonPressed() {
    GetTree().Quit();
  }

  private void OnMusicSliderValueChanged(float value) {
    audioManager.SetMusicVolume(value);
  }

  private void OnSoundSliderValueChanged(float value) {
    audioManager.SetSoundVolume(value);
  }
}
