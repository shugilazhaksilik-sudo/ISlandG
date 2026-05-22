using UnityEngine;

public class Item : MonoBehaviour
{
    public ItemData itemData;

    [Header("Состояние предмета")]
    [Tooltip("Оставшаяся прочность. Если -1, то предмет считается абсолютно новым.")]
    public int currentDurability = -1;
}