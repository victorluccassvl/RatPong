using System;
using UnityEngine;

public class PhysicsEventForward : MonoBehaviour
{
    public Action<Collider2D> OnTriggerEnter2DEvent = delegate { };
    public Action<Collider2D> OnTriggerStay2DEvent = delegate { };
    public Action<Collider2D> OnTriggerExit2DEvent = delegate { };

    private void OnTriggerEnter2D(Collider2D other) => OnTriggerEnter2DEvent(other);
    private void OnTriggerStay2D(Collider2D other) => OnTriggerStay2DEvent(other);
    private void OnTriggerExit2D(Collider2D other) => OnTriggerExit2DEvent(other);
}
