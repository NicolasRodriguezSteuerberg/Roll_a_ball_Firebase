using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour {
    // Rigidbody lo que actua con el plano
    private Rigidbody _rb;
    // creamos las variables de movimiento
    private float _movementX;
    private float _movementY;
    public float movementSpeed = 0;

    private int scoreCount;
    private int record;

    private Realtime realtime;

    // Start is called before the first frame update
    void Start() {
        _rb = GetComponent<Rigidbody>();
        scoreCount = 0;
        realtime = GameObject.Find("Realtime").GetComponent<Realtime>();
        StartCoroutine(realtime.GetRecord(recordCallback));
        Debug.Log("RECORD" + record);
    }

    void recordCallback(int record){
        this.record = record;
    }

    // Update is called once per frame
    void FixedUpdate() {
        Vector3 movement = new Vector3(_movementX, 0.0f, _movementY);
        _rb.AddForce(movement * movementSpeed);
    }

    void OnMove(InputValue movementValue){
        Vector2 movementVector = movementValue.Get<Vector2>();
        _movementX = movementVector.x;
        _movementY = movementVector.y;
    }

    void OnTriggerEnter(Collider other) {
        if (other.gameObject.CompareTag("PickUp")){
            other.gameObject.SetActive(false);
            scoreCount++;
            realtime.UpdateScore(scoreCount);
            if(scoreCount > record){
                realtime.UpdateRecord(scoreCount);
            }
        }
    }
}
