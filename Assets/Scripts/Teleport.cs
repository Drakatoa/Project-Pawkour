using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Teleport : MonoBehaviour
{
    public Transform player, Destination;
    public GameObject playerg;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerg.SetActive(false);
            player.position = Destination.position;
            player.rotation = Destination.rotation;
            playerg.GetComponent<PlayerController>().velocity = Vector3.zero;
            playerg.SetActive(true);
        }
    }
}
