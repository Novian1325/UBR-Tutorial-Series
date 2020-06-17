using UnityEngine;

public abstract class RichMonoBehaviour : MonoBehaviour
{
#if UNITY_EDITOR
    [SerializeField]
    [TextArea]
    protected string developerDescription = "Enter a description.";
#endif

    /// <summary>
    /// Cached Transform.
    /// </summary>
    protected Transform myTransform;

    /// <summary>
    /// Cached Transform.
    /// </summary>
    public new Transform transform { get => myTransform; }

    protected virtual void Awake()
    {
        GatherReferences();
    }

    /// <summary>
    /// Cache needed component references.
    /// </summary>
    protected virtual void GatherReferences()
    {
        myTransform = this.GetComponent<Transform>();
    }
}
