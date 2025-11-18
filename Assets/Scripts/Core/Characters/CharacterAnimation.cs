using UnityEngine;

[RequireComponent(typeof(Animator))]
public class CharacterAnimation : MonoBehaviour
{
    [SerializeField] private CharacterAnimationEventListenners animationEventListenners;
    [SerializeField] private CharacterController characterController;
    [SerializeField] private Animator _animatorController;
    public Animator AnimatorController => _animatorController;
    

    private void Awake()
    {
        if (_animatorController == null)
        {
            _animatorController = GetComponent<Animator>();
        }

        if (characterController == null)
        {
            characterController = GetComponent<CharacterController>();
        }

        characterController.OnAttack += HandleAttackAnimation;
        animationEventListenners.OnHit += HandleHitAnimation;
    }

    private void Update()
    {
        this._animatorController.SetFloat("Speed", characterController.GetVelocity().z);
    }

    private void HandleAttackAnimation()
    {
        this._animatorController.SetTrigger("Attack");
    }

    private void HandleHitAnimation()
    {
        characterController.HandleAttackTarget();
    }

    private void OnDisable()
    {
        characterController.OnAttack -= HandleAttackAnimation;
        animationEventListenners.OnHit -= HandleHitAnimation;
    }
    

}