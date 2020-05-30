using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeleteButtonController : MonoBehaviour
{
    Manager manager;

    private void Start()
    {
        manager = GameObject.FindGameObjectWithTag("Manager").GetComponent<Manager>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "DrawnObject")
            manager.DeleteTriggerOn(other.gameObject);
    }

    private void OnTriggerExit(Collider other)
    {
        manager.DeleteTriggerOff();
    }
}
