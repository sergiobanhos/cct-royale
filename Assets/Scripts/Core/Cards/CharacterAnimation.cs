using UnityEngine;

[RequireComponent(typeof(Animator))]
public class CharacterAnimation : MonoBehaviour
{
    [SerializeField] private CharacterAnimationEventListenners animationEventListenners;
    [SerializeField] private TroopController troopController;
    [SerializeField] private Animator _animatorController;
    public Animator AnimatorController => _animatorController;
    

    private void Awake()
    {
        if (_animatorController == null)
        {
            _animatorController = GetComponent<Animator>();
        }

        if (troopController == null)
        {
            troopController = GetComponent<TroopController>();
        }

        troopController.OnAttack += HandleAttackAnimation;
        animationEventListenners.OnHit += HandleHitAnimation;
    }

    private void Update()
    {
        this._animatorController.SetFloat("Speed", troopController.GetSpeed());
    }

    private void HandleAttackAnimation()
    {
        this._animatorController.SetTrigger("Attack");
    }

    private void HandleHitAnimation()
    {
        troopController.HandleAttackTarget();
    }

    private void OnDisable()
    {
        troopController.OnAttack -= HandleAttackAnimation;
        animationEventListenners.OnHit -= HandleHitAnimation;
    }
    

}