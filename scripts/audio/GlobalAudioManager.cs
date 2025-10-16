using Godot;
using System;

public partial class GlobalAudioManager : AudioStreamPlayer {
  [Signal] public delegate void SoundVolumeChangedEventHandler(float volume);

  private float musicVolume = 0.5f;
  private float soundVolume = 0.5f;

  public override void _Ready() {
    VolumeLinear = musicVolume;
    Play();
  }

  public void SetMusicVolume(float ratio) {
    musicVolume  = ratio;
    VolumeLinear = musicVolume;
  }

  public void SetSoundVolume(float ratio) {
    soundVolume = ratio;
    EmitSignal(SignalName.SoundVolumeChanged, musicVolume);
  }

  public void OnStreamFinished() {
    Play();
  }
}
