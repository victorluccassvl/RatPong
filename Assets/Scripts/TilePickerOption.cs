using System;
using UnityEngine;
using UnityEngine.UI;
using KBCore.Refs;

public class TilePickerOption : MonoBehaviour
{
    [SerializeField, Self] private Button button;

    public TilesData.TileData Option { get; private set; }

    public Action<TilePickerOption> OnOptionPicked = delegate { };

    public void OnClick() => OnOptionPicked(this);

    public void Setup(TilesData.TileData tileData)
    {
        Option = tileData;

        gameObject.name = $"PickerOption [{(Option == null ? "Delete" : Option.variant)}]";

        button.image.sprite = Option?.levelEditorRepresentation;
    }
}
