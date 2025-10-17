using Godot;
using System;

public partial class TitleUI : CanvasLayer {
  private GlobalAudioManager audioManager;
  private Slider             musicSlider;
  private Slider             soundSlider;

  [Export] private PackedScene gameScene;

  public override void _Ready() {
    audioManager = (GlobalAudioManager)GetTree().GetFirstNodeInGroup("AudioManager");
    musicSlider  = GetNode<Slider>("MarginContainer/HBoxContainer/MusicContainer/MusicSlider");
    soundSlider  = GetNode<Slider>("MarginContainer/HBoxContainer/SoundContainer/SoundSlider");

    musicSlider.Value = audioManager.MusicVolume;
    soundSlider.Value = audioManager.SoundVolume;
  }

  private void OnPlayButtonPressed() {
    var instance = gameScene.Instantiate<Node2D>();
    GetParent().CallDeferred("add_child", instance);
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
