using TMPro;
using UnityEngine;

public class GameMathExamples : MonoBehaviour
{
    public TextMeshProUGUI textField01;
    public TextMeshProUGUI textField02;
    public TextMeshProUGUI textField03;
    public TextMeshProUGUI textField04;
    public TextMeshProUGUI textField05;
    public TextMeshProUGUI textField06;
    public TextMeshProUGUI textField07;
    public TextMeshProUGUI textField08;
    public Transform sineCube;
    private Vector3 initialSineCubePos;
    public Transform vectorHeading;
    public Transform vectorTarget;
    public Transform eulerRotationExample;
    private Vector3 initialEulerExampleRot;

    // Start is called before the first frame update
    void Start()
    {
        initialSineCubePos = sineCube.position;
        initialEulerExampleRot = eulerRotationExample.eulerAngles;
    }

    // Update is called once per frame
    void Update()
    {
        // This is the time since the app has started.
        float _timeSinceGameStart = Time.realtimeSinceStartup;
        textField01.text = "Time since startup: " + _timeSinceGameStart.ToString();

        // This is how many frames have rendered since the app has started.
        int _frameCount = Time.renderedFrameCount;
        textField02.text = "Frame count: " + _frameCount.ToString();

        // Here is how you round to an integer
        float _roundedTime = Mathf.Round(_timeSinceGameStart);
        textField03.text = "Rounding to an int: " + _roundedTime.ToString();

        // Here is an example for the modulo operator
        int _mod = _frameCount % 10;
        textField04.text = "Mod 10 of the framecount: " + _mod.ToString();

        // Here is an algebra example, the lesson is to use parentheses liberally
        float _algebra = (3.2f / (6f + 3.3f)) * 55f;
        textField05.text = "Here is some algebra: " + _algebra.ToString();

        // Here is an example of a sine function working
        float _sineOutput = Mathf.Sin(_timeSinceGameStart);
        textField06.text = "Here is a sine function: " + _sineOutput.ToString();
        sineCube.position = new Vector3(
            initialSineCubePos.x + (_sineOutput),
            initialSineCubePos.y,
            initialSineCubePos.z
        );
        // Example of Vector Headings
        Vector3 _newHeadingDirection = (vectorTarget.position - vectorHeading.position).normalized;
        float _distanceToTarget = Vector3.Distance(vectorHeading.position, vectorTarget.position);
        textField07.text =
            "Heading Dir: "
            + _newHeadingDirection.ToString()
            + " Distance to Target: "
            + _distanceToTarget.ToString();
        // Rotate the vectorHeading to face the target
        vectorHeading.rotation = Quaternion.LookRotation(_newHeadingDirection);
        // Example of Euler Rotations
        Vector3 _newEulerRotation = new Vector3(
            initialEulerExampleRot.x,
            initialEulerExampleRot.y * _sineOutput,
            initialEulerExampleRot.z
        );
        eulerRotationExample.rotation = Quaternion.Euler(_newEulerRotation);
    }
}
