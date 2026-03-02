using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TileData", menuName = "Scriptable Objects/TileData")]
public class TilesData : ScriptableObject
{
    [Serializable]
    public class TileData
    {
        [HideInInspector] public string ID;
        public Sprite levelEditorRepresentation;
        public GameObject prefab;
    }

    public List<TileData> tilesData;

    public TileData GetTileDataByID(string ID)
    {
        return tilesData.Find(td => td.ID == ID);
    }

    private void OnValidate()
    {
        tilesData.RemoveAll(tileData => tileData == null);
        foreach (TileData tileData in tilesData) tileData.ID = tileData.prefab.name;
        foreach (TileData tileData in tilesData)
        {
            if (tilesData.Exists(otherTile => otherTile.ID == tileData.ID && tileData != otherTile))
            {
                Debug.LogError("Tile Datas contain two tiles with same associated ID" + tileData.ID + ",one will may be ignored");
                break;
            }
        }
    }
}
