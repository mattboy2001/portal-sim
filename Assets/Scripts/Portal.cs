using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    //Vector plane for near-clipping
    private Vector4 vectorPlane;

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


        //Calculate clipping plane
        var plane = new Plane(normalVisible.forward, transform.position);
        vectorPlane = new Vector4(plane.normal.x, plane.normal.y, plane.normal.z, plane.distance);

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
