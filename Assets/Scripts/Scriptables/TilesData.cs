using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TileData", menuName = "Scriptable Objects/TileData")]
public class TilesData : ScriptableObject
{
    [Serializable]
    public class TileData
    {
        [HideInInspector] public Tile.Variant variant;
        public Sprite levelEditorRepresentation;
        public GameObject prefab;
    }

    public List<TileData> tilesData;

    public TileData GetTileDataByVariant(Tile.Variant variant)
    {
        return tilesData.Find(td => td.variant == variant);
    }

    private void OnValidate()
    {
        tilesData.RemoveAll(tileData => tileData == null);
        foreach (TileData tileData in tilesData)
        {
            if (tileData == null) continue;
            tileData.variant = tileData.prefab.GetComponent<Tile>().variant;
            if (tilesData.Exists(otherTile => otherTile.variant == tileData.variant && tileData != otherTile))
            {
                Debug.LogError("Tile Datas contain two tiles with same associated variant, one will may be ignored");
                break;
            }
        }
    }
}
