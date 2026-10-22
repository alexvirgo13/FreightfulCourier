using UnityEngine;

public class DamageController : MonoBehaviour
{
    [SerializeField] Transform movementCollider;

    void OnTriggerEnter2D(Collider2D collider)
    {
        //TODO: Check for death pits
        if(collider.CompareTag("Killer"))
        {
            Debug.Log("Player has fallen into a pit!");
            
            movementCollider.GetComponent<MovementController>().Limp();
            Invoke(nameof(Reload), 2);
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log("Player has flipped!");
        movementCollider.GetComponent<MovementController>().Limp();
        Invoke(nameof(Reload), 2);
    }

    private void Reload()
    {
        MenuController.Reload();
    }

}
