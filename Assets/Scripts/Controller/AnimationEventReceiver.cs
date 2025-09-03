using UnityEngine;
using UnityEngine.Events;

/// Receives animation events (e.g., from Starter Assets clips) so you don't get
/// "AnimationEvent 'OnLand' has no receiver" warnings. Put this on the SAME
/// GameObject that has the Animator (e.g., under Graphics/PlayerArmature).
public class AnimationEventReceiver : MonoBehaviour
{
    [Header("Optional UnityEvents")]
    public UnityEvent onLand;
    public UnityEvent onFootstep;

    // Called via AnimationEvent
    public void OnLand() { onLand?.Invoke(); }

    // Optionally pass event for footstep sounds
    public void OnFootstep(AnimationEvent evt)
    {
        onFootstep?.Invoke();
        // evt.stringParameter / evt.intParameter are available if needed.
    }
}