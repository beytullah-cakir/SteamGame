using System.ComponentModel;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = "Scriptable Object/Item")]
public class Item : ScriptableObject
{
    public TileBase tile;
    public Sprite image;
    public ItemType itemType;
    public ActionType actionType;
    public Vector2Int range=new Vector2Int(5,4);
    public bool stackable = true;

}
public enum ItemType
{
    MONEY,
    GUN
}

public enum ActionType
{
    DÝG,
    MINE
}
