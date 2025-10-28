using UnityEditor.Animations;
using UnityEngine;

[RequireComponent(typeof(AnimatorController))]
public class CharacterAnimation : MonoBehaviour
{
    [SerializeField] private CharacterController characterController;
    private AnimatorController _animatorController;
    public AnimatorController AnimatorController => _animatorController;

    private void Awake()
    {
        if (_animatorController == null)
        {
            _animatorController = GetComponent<AnimatorController>();
        }
    }
}
