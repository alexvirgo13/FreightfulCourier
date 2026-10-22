using System;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.InputSystem;

public class MovementController : MonoBehaviour
{
    [SerializeField] private sbyte currentTrack = 0;
    [SerializeField] private float torqueAmount = 1.0f;
    [SerializeField, Min(0f)] private float boostSpeedModifier = 0.3f;
    [SerializeField] private float baseSpeed = 10.0f;
    [SerializeField] private Slider fuelMeter;
    [SerializeField, Range(0f, 100f)] private float fuelLeft = 100.0f;
    [SerializeField] private float fuelPerSecond = 25.0f;
    [SerializeField, Min(0f)] private float boostFuelModifier = 0.5f;
    [SerializeField] private GameObject[] groundSpriteShapes;
    [SerializeField] private Rigidbody2D rigidBody;
    [SerializeField] private Transform skins;    
    [SerializeField] private int flipPoints = 350;
    
    private (
        SurfaceEffector2D effector, 
        EdgeCollider2D collider, 
        bool contact
    ) [] surfaces;

    private InputAction moveAction;
    private Vector2 moveInput;
    private ScoreManager scoreManager;
    private float previousRotation;
    private float totalRotation;
    private int flipCount;

    private sbyte targettedTrack = -1;
    private bool isGrounded = false, isAlive = true, freeFuel = false;
    private int activePowerUps;

    private void Start ()
    {
        surfaces = new(SurfaceEffector2D, EdgeCollider2D, bool)[groundSpriteShapes.Length];
        for (sbyte i = 0; i < surfaces.Length; i++){
            surfaces[i].effector = groundSpriteShapes[i].GetComponent<SurfaceEffector2D>();
            surfaces[i].collider = groundSpriteShapes[i].GetComponent<EdgeCollider2D>();

            surfaces[i].effector.speed = baseSpeed;
            surfaces[i].effector.enabled = (i == currentTrack);
            surfaces[i].collider.isTrigger = (i != currentTrack);

            surfaces[i].contact = false;
        }
        
        moveAction = InputSystem.actions.FindAction("Move");
        if(rigidBody == null)
            rigidBody = transform.parent.GetComponentInParent<Rigidbody2D>();
        if(skins == null)
            skins = transform.parent.parent.GetChild(1).Find("Skins");
        SetLayerOrder();

        scoreManager = transform.parent
                .parent
                .GetComponent<ScoreManager>();
    }

    private void Update ()
    {
        moveInput = moveAction.ReadValue<Vector2>();
        if (isGrounded) Boost();
        else Spin();
        ScheduleSwitch();
        BurnFuel();
        CalculateFlips();
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider == surfaces[currentTrack].collider){
            surfaces[currentTrack].contact = true;
            isGrounded = true;
            //Debug.Log($"Grounded on track {currentTrack}");
        } 
        else
        {
            isGrounded = false;
            for (sbyte i = 0; i < surfaces.Length; i++)
                if(collision.collider == surfaces[i].collider)
                {
                    //Debug.LogWarning($"On track {i}");
                    surfaces[i].contact = true;
                }
        }

        //DumpContactPoints();
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.collider == surfaces[currentTrack].collider)
        {
            surfaces[currentTrack].contact = false;
            isGrounded = false;
            //Debug.Log("Now airborne");
        } 
        else 
            for (sbyte i = 0; i < surfaces.Length; i++)
                if(collision.collider == surfaces[i].collider)
                {
                    //Debug.LogWarning($"Off track {i}");
                    surfaces[i].contact = false;
                    break;
                }

        //DumpContactPoints();
    }

    void OnTriggerEnter2D(Collider2D collider)
    {
        CapsuleCollider2D capsuleCollider = GetComponent<CapsuleCollider2D>();

        if (!capsuleCollider.IsTouching(collider))
        {
            //Debug.Log("Box Trigger");
        }
        else if (collider == surfaces[currentTrack].collider)
        {
            surfaces[currentTrack].contact = true;
        }
        else if (targettedTrack >= 0 && targettedTrack < surfaces.Length
                && collider == surfaces[targettedTrack].collider
                && rigidBody.linearVelocityY < 0)
        {
            surfaces[targettedTrack].contact = true;
            AttemptSwitch(targettedTrack);
        }
        else 
            for (sbyte i = 0; i < surfaces.Length; i++)
                if(collider == surfaces[i].collider)
                {
                    //Debug.Log($"Entered track {i}");
                    surfaces[i].contact = true;
                    break;
                }

        //DumpContactPoints();
    }

    void OnTriggerExit2D(Collider2D collider)
    {
        CapsuleCollider2D capsuleCollider = GetComponent<CapsuleCollider2D>();
        
        if (!capsuleCollider.IsTouching(collider))
        {
            //Debug.Log("Box Trigger");
        }
        else if (collider == surfaces[currentTrack].collider)
        {
            surfaces[currentTrack].contact = false;
            return;
        }

        for (sbyte i = 0; i < surfaces.Length; i++)
            if(collider == surfaces[i].collider)
            {
                if (i == targettedTrack && rigidBody.linearVelocityY > 0)
                {
                    //Debug.Log($"Attempting exit switch...");
                    isGrounded = false;
                    AttemptSwitch(i);
                    targettedTrack = -1;
                }
                else
                {
                    //Debug.Log($"Exited track {i}");
                }
                surfaces[i].contact = false;
                break;
            }

        //DumpContactPoints();
    }

    /// <summary>
    /// Checks if the player can change speed, if so it modifies it
    /// </summary>
    void Boost()
    {
        if (moveInput.x > 0)
                surfaces[currentTrack].effector.speed = baseSpeed * (1.0f + boostSpeedModifier);
        else if (moveInput.x < 0)
                surfaces[currentTrack].effector.speed = baseSpeed * (1.0f - boostSpeedModifier);
        else
            surfaces[currentTrack].effector.speed = baseSpeed;
    }

    /// <summary>
    /// Checks if the player can spin, if so it executes the maneuver
    /// </summary>
    void Spin()
    {
        if (!isAlive) return;
        if(moveInput.x > 0)
            rigidBody.AddTorque(-torqueAmount);
        else if(moveInput.x < 0)
            rigidBody.AddTorque(torqueAmount);
    }

    /// <summary>
    /// Checks if the player wants to switch track and then calls the function that checks if it can
    /// </summary>
    void ScheduleSwitch()
    {
        if (moveInput.y == 0) return;

        sbyte targetTrack;

        if(moveInput.y > 0)
            targetTrack = (sbyte) (currentTrack - 1);  //Climb track
        else
            targetTrack = (sbyte) (currentTrack + 1);  //Drop track

        if (targetTrack == targettedTrack) return;

        if (targetTrack >= 0 && targetTrack < surfaces.Length)
        {
            //Debug.Log("BEGIN - User switch");
            targettedTrack = targetTrack;
            //Debug.Log("END - User switch");
        }
    }

    /// <summary>
    /// Checks if the player can switch to the specified track and
    /// either targets it or calls the function that switches them
    /// </summary>
    /// <param name="target">Track to be switched onto</param>
    void AttemptSwitch (sbyte target)
    {
        if (surfaces[target].contact)
        {
            SwitchTrack(target);
        }
        else if (targettedTrack != target)
        {
            //Debug.Log($"Targetting track {target}");
            targettedTrack = target;
        } else
        {
            //Debug.Log("Switch attempt timed out");
        }

        //DumpContactPoints();
    }

    /// <summary>
    /// Switches the player's active train track
    /// </summary>
    /// <param name="target">The index of the targeted track</param>
    void SwitchTrack(sbyte target)
    {
        //Debug.Log($"Switching to track {target}");

        targettedTrack = -1;

        surfaces[currentTrack].effector.speed = baseSpeed;
        surfaces[currentTrack].effector.enabled = false;
        surfaces[currentTrack].collider.isTrigger = true;

        surfaces[target].collider.isTrigger = false;
        surfaces[target].effector.enabled = true;

        currentTrack = target;

        SetLayerOrder();
    }

    /// <summary>
    /// Adjusts the player's sprite sorting order to match its active track
    /// </summary>
    void SetLayerOrder ()
    {
        if (skins != null) 
            foreach(Transform skin in skins)
                skin.GetComponent<SpriteRenderer>()
                    .sortingOrder = (currentTrack + 1) * 10;
    }

    /// <summary>
    /// Prints through console the platforms in contact with the player
    /// </summary>
    private void DumpContactPoints()
    {
        string surfaceContacts = "Contacts:\n\t> ";
        int i = 0;

        foreach (var surface in surfaces)
        {
            surfaceContacts += "index: " + i + ", contact?: " + (surface.contact?"true/":"false/");
        }
            
        
        //Debug.Log(surfaceContacts);
    }

    private void BurnFuel ()
    {
        if (freeFuel) return;

        float fuelUsage = fuelPerSecond * Time.deltaTime;
        if(moveInput.x > 0)
            fuelUsage *= 1 + boostFuelModifier;
        else if (moveInput.x < 0)
            fuelUsage *= 1 - (boostFuelModifier * 0.75f);   //fuel savings on slowdown are smaller than the waste on boost
        
        fuelLeft = Mathf.Clamp(fuelLeft - fuelUsage, 0.0f, 100.0f);

        if (fuelLeft == 0.0f) {
                Limp();
                Invoke(nameof(Reload), 2);
        }

        fuelMeter.value = fuelLeft / 100.0f;
        
    }

    private void Reload()
    {
        MenuController.Reload();
    }

    public void AddFuel (float amount)
    {
        fuelLeft = Mathf.Clamp(fuelLeft + amount, 0.0f, 100.0f);
    }

    /// <summary>
    /// Stops smoke particles, train animations and surface effectors.
    /// </summary>
    public void Limp()
    {
        foreach (var surface in surfaces) surface.effector.enabled = false;

        isAlive = false;

        transform.parent
            .parent
            .Find("ParticleSystems")
            .GetComponentInChildren<ParticleSystem>()
            .Stop();

        transform.parent
            .parent
            .Find("Skins")
            .GetComponentInChildren<Animator>()
            .enabled = false;
    }

    /// <summary>
    /// Measures the number of flips the player performs.
    /// </summary>
    void CalculateFlips()
    {
        float currentRotation = transform.rotation.eulerAngles.z;
        totalRotation += Mathf.DeltaAngle(previousRotation, currentRotation);
        if (Mathf.Abs(totalRotation) > 340f)
        {
            flipCount++;
            scoreManager.BonusPoints(flipCount * flipPoints); // Update the score in the ScoreManager
            totalRotation = 0f; // Reset the total rotation after a flip is detected
        }
        previousRotation = currentRotation;
    }

     public void ApplyPowerUp(PowerUpScriptableObject powerUpData)
    {
        activePowerUps++;
        switch (powerUpData.PowerUpType)
        {
            case "Boost":
                boostSpeedModifier += powerUpData.PowerUpValue;
                boostFuelModifier += powerUpData.PowerUpValue;
                break;
            case "MoonG":
                rigidBody.gravityScale -= powerUpData.PowerUpValue;
                torqueAmount += powerUpData.PowerUpValue;
                break;
            case "Freeze":
                scoreManager.FreezeCounter();
                freeFuel = true;
                break;
            default:
                Debug.LogWarning($"Unknown power-up type: {powerUpData.PowerUpType}");
                break;
        }
    }

    public void DeactivatePowerUp(PowerUpScriptableObject powerUpData)
    {
        activePowerUps--;
        if (activePowerUps == 0) switch (powerUpData.PowerUpType)
        {
            case "Boost":
                boostSpeedModifier -= powerUpData.PowerUpValue;
                boostFuelModifier -= powerUpData.PowerUpValue;
                break;
            case "MoonG":
                rigidBody.gravityScale += powerUpData.PowerUpValue;
                torqueAmount -= powerUpData.PowerUpValue;
                break;
            case "Freeze":
                scoreManager.UnfreezeCounter();
                freeFuel = false;
                break;
            default:
                Debug.LogWarning($"Unknown power-up type: {powerUpData.PowerUpType}");
                break;
        }
    }

}
