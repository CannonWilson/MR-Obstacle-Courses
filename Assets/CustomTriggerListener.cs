using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class CustomTriggerListener : MonoBehaviour
{
    List<InputDevice> leftDevices = new List<InputDevice>();
    List<InputDevice> rightDevices = new List<InputDevice>();
    private InputDevice leftTarget;
    private InputDevice rightTarget;
    public GameObject leftHandAnchor;
    public GameObject rightHandAnchor;
    // The below are only needed if doing actions with holding
    // private GameObject leftHeldProjectile;
    // private GameObject rightHeldProjectile;
    // bool alreadyClickedLeft = false;
    // bool alreadyClickedRight = false;

    private bool leftCanFire = true;
    private bool rightCanFire = true;
    private GameObject lastPlaced;
    private bool objectWasPlaced = false;
    private bool canPlaceObstacle = true;
    private bool canPlaceEnemy = true;
    private int enemiesToSpawn = 10;

    public GameObject projectilePrefab;
    public GameObject obstaclePrefab;
    public GameObject enemyPrefab;
    public GameObject button;

    private int projectileSpeed = 1000;

    // Start is called before the first frame update
    void Start()
    {
        InputDeviceCharacteristics leftControllerCharacteristics = InputDeviceCharacteristics.Left | InputDeviceCharacteristics.Controller;
        InputDevices.GetDevicesWithCharacteristics(leftControllerCharacteristics, leftDevices);

        InputDeviceCharacteristics rightControllerCharacteristics = InputDeviceCharacteristics.Right | InputDeviceCharacteristics.Controller;
        InputDevices.GetDevicesWithCharacteristics(rightControllerCharacteristics, rightDevices);    

        if (leftDevices.Count > 0) {
            leftTarget = leftDevices[0];
        }
        if (rightDevices.Count > 0) {
            rightTarget = rightDevices[0];
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (leftTarget.TryGetFeatureValue(CommonUsages.trigger, out float leftTriggerValue) && leftTriggerValue > 0.1f) {
            // if (!alreadyClickedLeft) {
            //     leftHeldProjectile = Instantiate(projectilePrefab, leftHandAnchor.transform);
            //     leftHeldProjectile.transform.parent = leftHandAnchor.transform;
            //     leftHeldProjectile.GetComponent<Rigidbody>().useGravity = false;
            //     leftHeldProjectile.GetComponent<Rigidbody>().detectCollisions = false;
            //     alreadyClickedLeft = true;
            // }
            if (leftCanFire) {
                GameObject leftProjectile = Instantiate(projectilePrefab, leftHandAnchor.transform);
                leftProjectile.GetComponent<Rigidbody>().AddRelativeForce(leftHandAnchor.transform.forward * projectileSpeed);
                leftProjectile.transform.parent = null;
                leftCanFire = false;
            }
        }
        if (rightTarget.TryGetFeatureValue(CommonUsages.trigger, out float rightTriggerValue) && rightTriggerValue > 0.1f) {
            // if (!alreadyClickedRight) {
            //     rightHeldProjectile = Instantiate(projectilePrefab, rightHandAnchor.transform);
            //     rightHeldProjectile.transform.parent = rightHandAnchor.transform;
            //     rightHeldProjectile.GetComponent<Rigidbody>().useGravity = false;
            //     rightHeldProjectile.GetComponent<Rigidbody>().detectCollisions = false;
            //     alreadyClickedRight = true;
            // }
            if (rightCanFire) {
                GameObject rightProjectile = Instantiate(projectilePrefab, rightHandAnchor.transform);
                rightProjectile.GetComponent<Rigidbody>().AddRelativeForce(rightHandAnchor.transform.forward * projectileSpeed);
                rightProjectile.transform.parent = null;
                rightCanFire = false;
            }
        }


        // Code to control actions when letting go of the triggers
        if (leftTarget.TryGetFeatureValue(CommonUsages.trigger, out float leftTriggerVal) && leftTriggerVal < 0.05f) {
        //     leftHeldProjectile.transform.parent = null;
        //     leftHeldProjectile.GetComponent<Rigidbody>().useGravity = true;
        //     leftHeldProjectile.GetComponent<Rigidbody>().detectCollisions = true;
        //     leftHeldProjectile.GetComponent<Rigidbody>().velocity = leftHandAnchor.GetComponent<Rigidbody>().velocity;
        //     alreadyClickedLeft = false;
            leftCanFire = true;
        }

        if (rightTarget.TryGetFeatureValue(CommonUsages.trigger, out float rightTriggerVal) && rightTriggerVal < 0.05f) {
        //     rightHeldProjectile.transform.parent = null;
        //     rightHeldProjectile.GetComponent<Rigidbody>().useGravity = true;
        //     rightHeldProjectile.GetComponent<Rigidbody>().detectCollisions = true;
        //     rightHeldProjectile.GetComponent<Rigidbody>().velocity = leftHandAnchor.GetComponent<Rigidbody>().velocity;
        //     alreadyClickedRight = false;
            rightCanFire = true;
        }

        // Left grip destroys the last item
        // Right grip places a new item, letting you drag it around until you're satisfied
        if (leftTarget.TryGetFeatureValue(CommonUsages.grip, out float leftGripValue) && leftGripValue > 0.1f) {
            if (objectWasPlaced) {
                Destroy(lastPlaced);
                objectWasPlaced = false;
            }
        }

        if (rightTarget.TryGetFeatureValue(CommonUsages.grip, out float rightGripValue) && rightGripValue > 0.1f) {
            if (canPlaceObstacle) {
                lastPlaced = Instantiate(obstaclePrefab, rightHandAnchor.transform);
                lastPlaced.transform.parent = null;
                objectWasPlaced = true;
                canPlaceObstacle = false;
            }
        }

        if (rightTarget.TryGetFeatureValue(CommonUsages.grip, out float rightGripVal) && rightGripVal < 0.05f) {
            canPlaceObstacle = true;
        }

        if (rightTarget.TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 rightStickVector)) {
            if (rightStickVector.magnitude > 0.3f) {
                if (canPlaceEnemy) {
                    lastPlaced = Instantiate(enemyPrefab, rightHandAnchor.transform);
                    lastPlaced.transform.parent = null; 
                    objectWasPlaced = true;
                    canPlaceEnemy = false;
                }
            }
        }

        if (rightTarget.TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 rightStickVec)) {
            if (rightStickVec.magnitude < 0.1f) {
                canPlaceEnemy = true;
            }
        }

       if (leftTarget.TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 leftStickVector)) {
            if (leftStickVector.magnitude > 0.3f) {
                if (canPlaceEnemy) {
                    canPlaceEnemy = false;
                    StartCoroutine(SpawnEnemies());
                    objectWasPlaced = false;
                }
            }
        }  
    }

    IEnumerator SpawnEnemies() {
        for (int i =0; i<enemiesToSpawn; i++) {
            Vector3 randomPosition = new Vector3(Random.Range(-7f, 7f), Random.Range(-0.5f, 0.5f), Random.Range(-5f,5f)); // X, Y, Z
            Instantiate(enemyPrefab, randomPosition, Quaternion.identity);
            yield return new WaitForSeconds(2f);

            if (i == enemiesToSpawn - 1) {
                canPlaceEnemy = true;
            }
        }
    }
}
