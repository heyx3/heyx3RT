using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
 
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
 

//Taken from:
//https://forum.unity3d.com/threads/image-effect-in-scene-editor-views.102515/#post-2946859


/// <summary>
/// Any image effect scripts attached to this object
///     are automatically attached to the editor Scene view camera as well.
/// </summary>
[ExecuteInEditMode]
public class SceneViewCameraProxy : MonoBehaviour
{
    #if UNITY_EDITOR
    private SceneView SceneView;
    public Camera SceneCamera;
 
    public bool ReflectionCheckForIE = true;
    public bool CheckForStandardIE = true;
 
    public bool UpdateOnChange = true;
    public bool ResetIEOnDisable = true;
    public bool DebugImageEffects = false;
 
    // Used only for Update
    private int lastComponentCount;
    private Component[] cachedComponents;
 
    public void OnEnable()
    {
        UpdateImageEffects();
    }
 
    public void OnValidate()
    { // Update when a variable on this script was changed
        if (!UpdateOnChange)
            OnEnable ();
    }
 
    public void OnDisable ()
    { // Reset image effects on disabling this component if desired
        if (ResetIEOnDisable)
            ResetImageEffects ();
    }
 
    public void Update ()
    {
        if (UpdateOnChange)
        { // Update scene camera with changed image effects using cached components, as long as none are added or removed
            if (DebugImageEffects)
                Debug.Log("Updating reference camera due to changed components!");
            int componentCount = GetComponents<Component>().Length;
            if (lastComponentCount != componentCount)
            { // Image Effects might have been added or removed, so refetch them
                lastComponentCount = componentCount;
                cachedComponents = GetImageEffectComponents(gameObject);
            }
            UpdateSceneCamera ();
            if (SceneCamera != null)
                InternalCopyComponents (cachedComponents, SceneCamera.gameObject);
        }
    }
 
    private void UpdateSceneCamera()
    {
        if (UnityEditor.SceneView.lastActiveSceneView != null)
            SceneView = UnityEditor.SceneView.lastActiveSceneView;
        SceneCamera = SceneView == null? null : SceneView.camera;
    }
 
    /// <summary>
    /// Returns all components filtered for image effects
    /// </summary>
    private Component[] GetImageEffectComponents(GameObject GO)
    {
        Component[] components = GO.GetComponents<Component>();
        if (components != null && components.Length > 0)
        { // Exclude Transform and Camera components
            if (ReflectionCheckForIE)
            { // Check if component implements OnRenderImage used for image postprocessing -> Perfect check!
                components = components.Where((Component c) => c.GetType ().GetMethod ("OnRenderImage", BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public) != null).ToArray();
            }
            else if (CheckForStandardIE)
            { // Check if it is an standard image effects; unfortunately does not always work on 3rd party components!
                components = components.Where ((Component c) => c.GetType ().IsSubclassOf (typeof(UnityStandardAssets.ImageEffects.PostEffectsBase))).ToArray ();
            }
            else
            { // Check for all Components possibly being image effects, but may include normal scripts!
                components = components.Where((Component c) => {
                    Type cT = c.GetType ();
                    return c != this && cT != typeof(Transform) && cT != typeof(GUILayer) && cT != typeof(Camera);
                }).ToArray();
            }
        }
        return components;
    }
 
    /// <summary>
    /// Updates the image effects found on the proxy object to the scene camera
    /// </summary>
    private void UpdateImageEffects()
    {
        UpdateSceneCamera ();
        if (SceneCamera == null)
            return;
 
        if (DebugImageEffects)
            Debug.Log ("Applying image effects to '" + SceneCamera.gameObject + "':");
 
        lastComponentCount = GetComponents<Component>().Length;
        cachedComponents = GetImageEffectComponents(gameObject);
        InternalCopyComponents (cachedComponents, SceneCamera.gameObject);
    }
 
    /// <summary>
    /// Resets all image effects found on the scene camera
    /// </summary>
    private void ResetImageEffects()
    {
        UpdateSceneCamera ();
        if (SceneCamera == null)
            return;
 
        if (DebugImageEffects)
            Debug.Log ("Resetting image effects of '" + SceneCamera.gameObject + "':");
 
        Component[] components = GetImageEffectComponents (SceneCamera.gameObject);
        for (int i = 0; i < components.Length; i++)
        {
            Component comp = components[i];
            if (DebugImageEffects)
                Debug.Log(comp.GetType().Name);
            DestroyImmediate (comp);
        }
    }
 
    private void InternalCopyComponents (Component[] components, GameObject target)
    {
        if (components != null && components.Length > 0)
        {
            for (int i = 0; i < components.Length; i++)
            {
                Component comp = components[i];
                Type cType = comp.GetType();
                if (DebugImageEffects)
                    Debug.Log(cType.Name);
                // Copy component values
                Component existingComp = target.GetComponent(cType) ?? target.AddComponent(cType);
                EditorUtility.CopySerialized(comp, existingComp);
            }
        }
    }
 
    #endif
}