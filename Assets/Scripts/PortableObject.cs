using System.Collections;
using System.Collections.Generic;
using UnityEngine;



//Used to identify an object as portable and manage the event firing after moving through the portal
public class PortableObject : MonoBehaviour
{


    //Delegates to any method with the same signature
    public delegate void HasTeleportedHandler(Portal sender, Portal destination, Vector3 newPosition, Quaternion newRotation);


    public event HasTeleportedHandler HasTeleported;

    //Invokes the event attached to the event handler
    public void OnHasTeleported(Portal sender, Portal destination, Vector3 newPosition, Quaternion newRotation)
    {
        HasTeleported?.Invoke(sender, destination, newPosition, newRotation);
    }
}
