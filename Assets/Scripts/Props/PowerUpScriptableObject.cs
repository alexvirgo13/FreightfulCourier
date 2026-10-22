using UnityEngine;

[CreateAssetMenu(fileName = "PowerUpScriptableObject", menuName = "PowerUps/PowerUp Data")]
public class PowerUpScriptableObject : ScriptableObject
{
    [SerializeField] private string powerUpType; //The kind of effect it gives on pick up
    [SerializeField] private float timeLimit;    //The duration of the effect given
    [SerializeField] private float powerUpValue; //The amount increase

    public float PowerUpValue { get => powerUpValue; set => powerUpValue = value; }
    public float TimeLimit { get => timeLimit; set => timeLimit = value; }
    public string PowerUpType { get => powerUpType; set => powerUpType = value; }
}
