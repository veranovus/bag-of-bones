using Godot;
using System;
using System.Collections.Generic;
using System.IO;

public partial class Chunk : Node2D {
  private TileMapLayer tileMap;
  private Vector2      tileSize;

  private static readonly List<PackedScene> Chunks    = [];
  private static readonly Vector2I          ChunkSize = new Vector2I(9, 10);

  public override void _Ready() {
    tileMap  = GetNode<TileMapLayer>("TileMap");
    tileSize = tileMap.TileSet.TileSize * tileMap.Scale;

    if (Chunks.Count == 0) {
      LoadChunkScenes();
    }
  }

  private void OnHalfwayAreaBodyEntered(Node2D node) {
    var scene    = Chunks[GD.RandRange(1, Chunks.Count - 1)];
    var instance = scene.Instantiate<Chunk>();
    var offset   = ChunkSize * tileSize; 

    instance.Position = GlobalPosition + new Vector2(0.0f, offset.Y);
    GetParent().AddChild(instance);
  }

  private void LoadChunkScenes() {
    var files = Directory.GetFiles("scenes/game/level/");
    foreach (var path in files) {
      Chunks.Add(ResourceLoader.Load<PackedScene>(path));
    }
  }
}
