using UnityEngine;
using UnityEngine.VFX;

public class TrailManTest : MonoBehaviour
{

    [SerializeField] private Texture2D trailTexture;
    [SerializeField] private Vector2 speedOfTrail;
    [SerializeField] private Color newColourOne;
    [SerializeField] private Color newColourTwo;
    [SerializeField] public float TimerAmount;
    [SerializeField] public float TimeLimit;

    [SerializeField] private Material trailMaterial;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

        //Material currentTrail = GetComponent<Material>();
        //currentTrail.SetVector("_SpeedTrail", new Vector2(speedOfTrail.x, speedOfTrail.y));

        trailMaterial = GetComponent<TrailRenderer>().material;


        
    }

    // Update is called once per frame
    void Update()
    {
        TimerAmount += Time.deltaTime;

        if (TimerAmount > TimeLimit)
        {
            Debug.Log("Sumit <3 \nHA GAAAAAYYYYYY!!!");
            trailMaterial.SetVector("_SpeedTrail", new Vector2(speedOfTrail.x, speedOfTrail.y));
            trailMaterial.SetColor("_ColourOne", newColourOne);
            trailMaterial.SetColor("_ColourTwo", newColourTwo);
            trailMaterial.SetTexture("_TrailTexture", trailTexture);




        }

        else if (TimerAmount > 10f)
        {
            Debug.Log("Further Speed");
            TimerAmount = 0;
        }

    }
}
