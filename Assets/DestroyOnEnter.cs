using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyOnEnter : MonoBehaviour
{
    void OnTriggerEnter() {
        Destroy(gameObject); // Destroy the GameObject this script's attached to
    }
}
