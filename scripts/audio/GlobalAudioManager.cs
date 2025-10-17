using Godot;
using System;

public partial class GlobalAudioManager : AudioStreamPlayer {
  public float MusicVolume { get; private set; } = 0.5f;
  public float SoundVolume { get; private set; } = 0.5f;

  public override void _Ready() {
    VolumeLinear = MusicVolume;
    Play();
  }

  public void SetMusicVolume(float ratio) {
    MusicVolume  = ratio;
    VolumeLinear = MusicVolume;
  }

  public void SetSoundVolume(float ratio) {
    SoundVolume = ratio;
  }

  public void OnStreamFinished() {
    Play();
  }
}
