using UnityEngine;
using UnityEngine.InputSystem.Interactions;

public class NpcBlink : MonoBehaviour
{
    // eyes of the NPC to blink (turn black then white again)
    public MeshRenderer eyeLeft;
    public MeshRenderer eyeRight;
    public Material eyeOpenColor;
    public Material eyeClosedColor;
    private float blinkInterval = 11.0f; // seconds between blinks
    private float blinkIntervelVariance = 6.0f; // seconds of random variance
    public float blinkDuration = 0.1f; // seconds eyes stay closed
    private float timeSinceLastBlink = 0.0f;
    private float eyesClosedTime = 0.0f;
    private float nextBlinkTime = 0.0f;
    private bool eyesAreClosed = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // set initial next blink time
        nextBlinkTime = blinkInterval + Random.Range(-blinkIntervelVariance, blinkIntervelVariance);
    }

    // Update is called once per frame
    void Update()
    {
        timeSinceLastBlink += Time.deltaTime;

        if (!eyesAreClosed && timeSinceLastBlink >= nextBlinkTime)
        {
            // close eyes
            CloseEyes();
            eyesAreClosed = true;
            eyesClosedTime = 0.0f;
        }

        if (eyesAreClosed)
        {
            eyesClosedTime += Time.deltaTime;
            if (eyesClosedTime >= blinkDuration)
            {
                // open eyes
                OpenEyes();
                eyesAreClosed = false;
                timeSinceLastBlink = 0.0f;
                // set next blink time
                nextBlinkTime =
                    blinkInterval + Random.Range(-blinkIntervelVariance, blinkIntervelVariance);
            }
        }
    }

    void SetEyeMaterial(MeshRenderer eye, Material mat)
    {
        var mats = eye.materials;
        mats[0] = mat; // change sclera slot
        eye.materials = mats;
    }

    void OpenEyes()
    {
        SetEyeMaterial(eyeLeft, eyeOpenColor);
        SetEyeMaterial(eyeRight, eyeOpenColor);
    }

    void CloseEyes()
    {
        SetEyeMaterial(eyeLeft, eyeClosedColor);
        SetEyeMaterial(eyeRight, eyeClosedColor);
    }
}
