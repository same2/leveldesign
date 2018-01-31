using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider))]
public class GenericTrigger : MonoBehaviour {

    [SerializeField]
    private string tagFilter;

    [SerializeField]
    private UnityEvent onEnter;
    [SerializeField]
    private UnityEvent onExit;
    [SerializeField]
    private UnityEvent onStay;

    public void Awake() {
        GetComponent<Collider>().isTrigger = true;
    }

    public void OnTriggerEnter(Collider other) {
        if (other.gameObject.tag == tagFilter) {
            onEnter.Invoke();
        }
    }

    public void OnTriggerExit(Collider other) {
        if (other.gameObject.tag == tagFilter) {
            onExit.Invoke();
        }
    }

    public void OnTriggerStay(Collider other) {
        if (other.gameObject.tag == tagFilter) {
            onStay.Invoke();
        }
    }
}
