using Godot;
using Godot.Collections;
using System;

[GlobalClass]
public partial class AudioPlaylistResource : Resource {
  [Export] public string             Name;
  [Export] public Array<AudioStream> Playlist;
}
