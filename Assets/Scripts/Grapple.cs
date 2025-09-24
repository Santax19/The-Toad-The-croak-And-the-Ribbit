using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grapple : MonoBehaviour
{
    [Header("Referencias")]
    public CharacterController controller;
    public Camera cam;

    [Header("Ajustes del Grapple")]
    public float grappleRange = 30f;      // distancia máxima del grappling
    public float grappleSpeed = 20f;      // velocidad de arrastre
    public float stopDistance = 2f;       // distancia mínima para cortar el grapple
    public LayerMask grappleMask;         // en qué capas se puede enganchar

    private Vector3 grapplePoint;
    private bool isGrappling = false;

    void Update()
    {
        // inicio del grapple (click derecho)
        if (Input.GetMouseButtonDown(1))
        {
            TryStartGrapple();
        }

        // Cancelar grapple manualmente (al soltar click)
        if (Input.GetMouseButtonUp(1) && isGrappling)
        {
            StopGrapple();
        }

        // Movimiento mientras estas enganchado
        if (isGrappling)
        {
            DoGrapple();
        }
    }

    void TryStartGrapple()
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, grappleRange, grappleMask))
        {
            grapplePoint = hit.point;
            isGrappling = true;
        }
    }

    void DoGrapple()
    {
        Vector3 direction = (grapplePoint - transform.position).normalized;
        float distance = Vector3.Distance(transform.position, grapplePoint);

        // mover al jugador hacia el punto
        controller.Move(direction * grappleSpeed * Time.deltaTime);

        // cortar grapple cuando estas cerca
        if (distance < stopDistance)
        {
            StopGrapple();
        }
    }

    void StopGrapple()
    {
        isGrappling = false;
    }
}
