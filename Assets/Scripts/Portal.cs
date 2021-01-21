using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EZCameraShake;

public class Portal : MonoBehaviour
{

    //Linked portal
    public Portal targetPortal;

    //Normal for visible face of portal
    public Transform normalVisible;


    //Normal for invisible face of portal
    public Transform normalInvisible;



    //Portal camera
    public Camera portalCamera;


    //Mesh renderer for portal
    public Renderer viewThroughRenderer;


    //Render texture for portal
    private RenderTexture viewThroughRenderTexture;

    //Material with the custom shader
    private Material viewThroughMaterial;


    //Main camera on player
    private Camera mainCamera;


    private CameraController mainCameraController;

    //Vector plane for near-clipping
    private Vector4 vectorPlane;



    private HashSet<PortableObject> objectsInPortal = new HashSet<PortableObject>();

    private HashSet<PortableObject> objectsInPortalToRemove = new HashSet<PortableObject>();

    void Start()
    {
        //Create render texture

        viewThroughRenderTexture = new RenderTexture(Screen.width, Screen.height, 24, RenderTextureFormat.DefaultHDR);
        viewThroughRenderTexture.Create();


        //Assign render texture to portal material

        viewThroughMaterial = viewThroughRenderer.material;

        viewThroughMaterial.mainTexture = viewThroughRenderTexture;

        portalCamera.targetTexture = viewThroughRenderTexture;

        mainCamera = Camera.main;

        mainCameraController = mainCamera.GetComponent<CameraController>();


        //Calculate clipping plane
        var plane = new Plane(normalVisible.forward, transform.position);
        vectorPlane = new Vector4(plane.normal.x, plane.normal.y, plane.normal.z, plane.distance);


        StartCoroutine(WaitForFixedUpdateLoop());
    }


    private IEnumerator WaitForFixedUpdateLoop()
    {
        var waitForFixedUpdate = new WaitForFixedUpdate();
        while (true)
        {
            yield return waitForFixedUpdate;
            try 
            {
                CheckForPortalCrossing();
            }
            catch(Exception e)
            {
                Debug.LogException(e);
            }
        }
    }

    private void CheckForPortalCrossing()
    {
        //Clear removal queue
        //Use clear so we don't allocate garbage every frame
        objectsInPortalToRemove.Clear();


        //Check for every touching object

        foreach (var portableObject in objectsInPortal)
        {

            //Remove if null
            if (portableObject == null)
            {
                objectsInPortalToRemove.Add(portableObject);
                continue;
            }


            //Check if the portable object is behind the portal using the dot product
            var pivot = portableObject.transform;
            var directionToPivotFromTransform = pivot.position - transform.position;
            directionToPivotFromTransform.Normalize();

            //if the dot product is less than 0 then the portable object has crossed the portal

            var pivotToNormalDotProduct = Vector3.Dot(directionToPivotFromTransform, normalVisible.forward);
            if (pivotToNormalDotProduct > 0)
            {
                CameraShaker.Instance.ShakeOnce(2f, 0.1f, 0.1f, 1f);
                continue;
            }

        
            var newPosition = TransformPositionBetweenPortals(this, targetPortal, portableObject.transform.position);
            var newRotation = TransformRotationBetweenPortals(this, targetPortal, portableObject.transform.rotation);

            //Magnitude, Roughness, Fade in, Fade out
            CameraShaker.Instance.ShakeOnce(2f, 2.5f, 0.1f, 1f);

            portableObject.transform.SetPositionAndRotation(newPosition, newRotation);


            portableObject.OnHasTeleported(this, targetPortal, newPosition, newRotation);


            //Remove as it is no longer in this portal


            objectsInPortalToRemove.Add(portableObject);
        }

        foreach (var portableObject in objectsInPortalToRemove)
        {
            objectsInPortal.Remove(portableObject);
        }
    }


    //Late update avoids any single frame lag with Update
    void LateUpdate()
    {

        //Calculate the positional difference between the player and the portal and then the target portal
        var virtualPosition = TransformPositionBetweenPortals(this, targetPortal, mainCamera.transform.position);
        var virtualRotation = TransformRotationBetweenPortals(this, targetPortal, mainCamera.transform.rotation);

        //Move the portal camera to mimic player movement
        portalCamera.transform.SetPositionAndRotation(virtualPosition, virtualRotation);



        //Calculate projection matrix

        var clipThroughSpace = Matrix4x4.Transpose(Matrix4x4.Inverse(portalCamera.worldToCameraMatrix)) * targetPortal.vectorPlane;

        //Set portal camera projection matrix to clip walls between target portal and portal camera

        //Inherits main camera near/far clip plane and FOV settings


        var obliqueProjectionMatrix = mainCamera.CalculateObliqueMatrix(clipThroughSpace);

        portalCamera.projectionMatrix = obliqueProjectionMatrix;

        //Render portal camera
        portalCamera.Render();
    }


    private void OnTriggerEnter(Collider other)
    {
        var portableObject = other.GetComponent<PortableObject>();
        if (portableObject)
        {
            objectsInPortal.Add(portableObject);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        var portableObject = other.GetComponent<PortableObject>();
        if (portableObject)
        {
            objectsInPortal.Remove(portableObject);
        }
    }

    //Avoid memory leakage and clean up cloned assets
    private void OnDestroy()
    {
        viewThroughRenderTexture.Release();

        Destroy(viewThroughMaterial);

        Destroy(viewThroughRenderTexture);
    }



    //Helper functions for calculating the positional and rotational differences for the player and the linked portal


    public static Vector3 TransformPositionBetweenPortals(Portal sender, Portal target, Vector3 position)
    {
        return target.normalInvisible.TransformPoint(sender.normalVisible.InverseTransformPoint(position));
    }


    public static Quaternion TransformRotationBetweenPortals(Portal sender, Portal target, Quaternion rotation)
    {
        return target.normalInvisible.rotation * Quaternion.Inverse(sender.normalVisible.rotation) * rotation;
    }


}
