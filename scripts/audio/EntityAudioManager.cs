using Godot;
using Godot.Collections;
using System;

public partial class EntityAudioManager : AudioStreamPlayer {
  private GlobalAudioManager globalAudioManager;

  [Export] private Array<AudioPlaylistResource> playlists;

  public override void _EnterTree() {
    globalAudioManager = (GlobalAudioManager)GetTree().GetFirstNodeInGroup("AudioManager");
    VolumeLinear       = globalAudioManager.SoundVolume;
  }

  public void PlayRandomAudio(string name) {
    foreach (var playlist in playlists) {
      if (playlist.Name != name) {
        continue;
      }
      Stream = playlist.Playlist[GD.RandRange(0, playlist.Playlist.Count - 1)];
      Play();
      return;
    }
  }
}
