using UnityEngine;

public class GameManager : MonoBehaviour
{
    void Awake() // Используем Awake, чтобы настройка сработала сразу
    {
        // Ограничиваем частоту кадров до 50
        Application.targetFrameRate = 50;
    }
}