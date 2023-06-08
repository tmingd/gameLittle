using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
    private CharacterController _cc;
    public float MoveSpeed = 5f;
    private Vector3 _movementVelocity;
    private PlayerInput _playerInput;
    private float _verticalVelocity;
    public float Gravity = -9.8f;

    private Animator _animator;

    private float attackStartTime;
    //攻击滑动持续时间
    public float AttackSlideDuration = 0.4f;
    //定义滑动速度
    public float AttackSlideSpeed = 0.06f;

    //角色收到攻击时受到冲击力
    private Vector3 impactOnCharacter;

    //判断角色是玩家还是敌人
    public bool IsPlayer = true;
    //附加到敌人身上的NavMesh代理的私有引用
    private UnityEngine.AI.NavMeshAgent _navMeshAgent;
    //存储玩家的位置，使敌人知道该往哪里移动
    private Transform TargetPlayer;


    //Health
    private Health _health;
    //Damage Caster
    private DamageCaster _damageCaster;


    //State Machine 状态机
    public enum CharacterState
    {
        Normal, Attacking, Dead, BeingHit, Slide, Spawn
    }
    public CharacterState CurrentState;

    //敌人在spawn状态停留时间
    public float SpawnDuiation = 2f;
    //跟踪敌人在Spawn状态停留时间多少
    public float currentSpawnTime;
    //玩家和敌人闪烁
    private MaterialPropertyBlock _materialPropertyBlock;
    private SkinnedMeshRenderer _skinnedMeshRenderer;


    //掉落物品
    public GameObject ItemToDrop;

    //短暂无敌
    public bool IsInvincible;
    public float InvincibleDuration = 2f;

    //添加硬币
    public int Coin;

    //用于连击函数
    private float attackAnimationDuration;

    //用于冲刺速度
    public float SliceSpeed = 9f;
    private void Awake()
    {
        _health = GetComponent<Health>();
        _cc = GetComponent<CharacterController>();
        _animator = GetComponent<Animator>();
        _damageCaster = GetComponentInChildren<DamageCaster>();

        _skinnedMeshRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
        _materialPropertyBlock = new MaterialPropertyBlock();
        _skinnedMeshRenderer.GetPropertyBlock(_materialPropertyBlock);

        if (!IsPlayer)
        {
            _navMeshAgent = GetComponent<UnityEngine.AI.NavMeshAgent>();
            TargetPlayer = GameObject.Find("Player").transform;
            _navMeshAgent.speed = MoveSpeed;

            SwitchStateTo(CharacterState.Spawn);
        }
        else
        {
            _playerInput = GetComponent<PlayerInput>();
        }
    }

    private void CalculateplayerMovement()
    {
        //如果鼠标左键按下玩家处于地面上切换到攻击状态
        if (_playerInput.MouseButtonDown && _cc.isGrounded)
        {
            SwitchStateTo(CharacterState.Attacking);
            return;
        } else if (_playerInput.SpaceKeyDown && _cc.isGrounded)
        {
            SwitchStateTo(CharacterState.Slide);
            return;
        }
        _movementVelocity.Set(_playerInput.HorizontalInput, 0f, _playerInput.VerticalInput);
        _movementVelocity.Normalize();
        _movementVelocity = Quaternion.Euler(0, -45f, 0) * _movementVelocity;

        //设置Animator中的Speed
        _animator.SetFloat("Speed", _movementVelocity.magnitude);

        _movementVelocity *= MoveSpeed * Time.deltaTime;

        //解决角色旋转问题
        if (_movementVelocity != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(_movementVelocity);
        }
        //下落代码
        _animator.SetBool("AirBorne", !_cc.isGrounded);


    }

    //敌人移动函数
    private void CalculateEnemymovement()
    {
        if (Vector3.Distance(TargetPlayer.position, transform.position) >= _navMeshAgent.stoppingDistance)
        {
            _navMeshAgent.SetDestination(TargetPlayer.position);
            _animator.SetFloat("Speed", 0.2f);
        }
        else
        {
            _navMeshAgent.SetDestination(transform.position);
            _animator.SetFloat("Speed", 0f);

            SwitchStateTo(CharacterState.Attacking);
        }
    }

    private void FixedUpdate()
    {
        //声明一个开关以便于我们的角色可以对不同的状态作出反应
        switch (CurrentState)
        {
            case CharacterState.Normal:
                {
                    if (IsPlayer)
                    {
                        CalculateplayerMovement();
                    }
                    else
                    {
                        CalculateEnemymovement();
                    }

                    break;
                }
            case CharacterState.Attacking:
                {


                    if (IsPlayer)
                    {
                        if (Time.time < attackStartTime + AttackSlideDuration)
                        {
                            float timePassed = Time.time - attackStartTime;
                            float lerpTime = timePassed / AttackSlideDuration;
                            _movementVelocity = Vector3.Lerp(transform.forward * AttackSlideSpeed, Vector3.zero, lerpTime);
                        }

                        //连击
                        if (_playerInput.MouseButtonDown && _cc.isGrounded)
                        {
                            //获取当前正在播放的动画的名称
                            string currentClipName = _animator.GetCurrentAnimatorClipInfo(0)[0].clip.name;
                            //动画播放时间
                            attackAnimationDuration = _animator.GetCurrentAnimatorStateInfo(0).normalizedTime;

                            if (currentClipName != "LittleAdventurerAndie_ATTACK_03" && attackAnimationDuration > 0.5f && attackAnimationDuration < 0.7f)
                            {
                                _playerInput.MouseButtonDown = false;
                                SwitchStateTo(CharacterState.Attacking);

                               // CalculateplayerMovement();
                            }
                        }
                    }

                    break;
                }
            case CharacterState.Dead:
                return;
            case CharacterState.BeingHit:


                break;
            case CharacterState.Slide:
                _movementVelocity = transform.forward * SliceSpeed * Time.deltaTime;
                break;
            case CharacterState.Spawn:
                currentSpawnTime -= Time.deltaTime;
                if (currentSpawnTime <= 0)
                {
                    SwitchStateTo(CharacterState.Normal);

                }
                break;
        }

        if (impactOnCharacter.magnitude > 0.2f)
        {
            _movementVelocity = impactOnCharacter * Time.deltaTime;
        }
        impactOnCharacter = Vector3.Lerp(impactOnCharacter, Vector3.zero, Time.deltaTime * 5);


        //解决角色下落问题
        if (IsPlayer)
        {
            if (_cc.isGrounded == false)
            {
                _verticalVelocity = Gravity;
            }
            else
            {
                _verticalVelocity = Gravity * 0.3f;
            }

            _movementVelocity += _verticalVelocity * Vector3.up * Time.deltaTime;
            _cc.Move(_movementVelocity);
            _movementVelocity = Vector3.zero;
        }
        else
        {
            if (CurrentState != CharacterState.Normal)
            {
                _cc.Move(_movementVelocity);
                _movementVelocity = Vector3.zero;
            }
        }


    }

    public void SwitchStateTo(CharacterState newState)
    {
        if (IsPlayer)
        {
            _playerInput.ClearCache();
        }

        //Exiting State
        switch (CurrentState)
        {
            case CharacterState.Normal:
                break;
            case CharacterState.Attacking:
                if (_damageCaster != null)
                {
                    DisableDamageCaster();
                }

                if (IsPlayer)
                {
                    GetComponent<PlayerVFXManager>().StopBlade();
                }
                break;
            case CharacterState.Dead:
                return;
            case CharacterState.BeingHit:
                break;
            case CharacterState.Slide:
                break;
            case CharacterState.Spawn:
                IsInvincible = false;
                break;
        }
        //Entering State
        switch (newState)
        {
            case CharacterState.Normal:
                break;
            case CharacterState.Attacking:
                if (!IsPlayer)
                {
                    Quaternion newRoration = Quaternion.LookRotation(TargetPlayer.position - transform.position);
                    transform.rotation = newRoration;
                }
                _animator.SetTrigger("Attack");

                if (IsPlayer)
                {

                    attackStartTime = Time.time;
                    RotateToCusor();
                }
                break;
            case CharacterState.Dead:
                _cc.enabled = false;
                _animator.SetTrigger("Dead");
                StartCoroutine(MaterialDissolve());

                if (!IsPlayer)
                {
                    SkinnedMeshRenderer mesh = GetComponentInChildren<SkinnedMeshRenderer>();
                    mesh.gameObject.layer = 0;
                }
                break;
            case CharacterState.BeingHit:
                _animator.SetTrigger("BeingHit");
                if (IsPlayer)
                {
                    IsInvincible = true;
                    StartCoroutine(DelayCancelInvincible());
                }

                break;
            case CharacterState.Slide:
                _animator.SetTrigger("Slide");
                break;
            case CharacterState.Spawn:
                IsInvincible = true;
                currentSpawnTime = SpawnDuiation;
                StartCoroutine(MaterialAppear());
                break;

        }
        CurrentState = newState;


        /*        Debug.Log("Switched to"+ CurrentState);*/


    }

    public void AttackAnimation()
    {
        SwitchStateTo(CharacterState.Normal);
    }

    public void SlideAnimationEnds()
    {
        SwitchStateTo(CharacterState.Normal);
    }



    public void AttackAnimationEnds()
    {
        SwitchStateTo(CharacterState.Normal);
    }




    public void BeingHitAnimationEnds()
    {
        SwitchStateTo(CharacterState.Normal);
    }

    public void ApplyDamage(int damage, Vector3 attackerPos = new Vector3())
    {
        if (IsInvincible)
        {
            return;
        }
        if (_health != null)
        {
            _health.ApplyDamage(damage);
        }

        if (!IsPlayer)
        {
            GetComponent<EnemyVFXManager>().PlayBeingHitVFX(attackerPos);
        }

        StartCoroutine(MaterialBlink());

        if (IsPlayer)
        {
            SwitchStateTo(CharacterState.BeingHit);
            AddImpact(attackerPos, 10f);
        }
        else
        {
            AddImpact(attackerPos, 2.5f);
        }


    }

    IEnumerator DelayCancelInvincible()
    {
        yield return new WaitForSeconds(InvincibleDuration);
        IsInvincible = false;
    }

    //计算冲击力
    private void AddImpact(Vector3 attackerPos, float force)
    {
        Vector3 impactDir = transform.position - attackerPos;
        impactDir.Normalize();
        impactDir.y = 0;
        impactOnCharacter = impactDir * force;
    }
    public void EnableDamageCaster()
    {
        _damageCaster.EnableDamageCaster();
    }
    public void DisableDamageCaster()
    {
        _damageCaster.DisableDamageCaster();
    }

    IEnumerator MaterialBlink()
    {
        _materialPropertyBlock.SetFloat("_blink", 0.4f);
        _skinnedMeshRenderer.SetPropertyBlock(_materialPropertyBlock);

        yield return new WaitForSeconds(0.2f);

        _materialPropertyBlock.SetFloat("_blink", 0f);
        _skinnedMeshRenderer.SetPropertyBlock(_materialPropertyBlock);
    }

    IEnumerator MaterialDissolve()
    {
        yield return new WaitForSeconds(2);

        float dissolveTimeDuration = 2f;
        float currentDissolveTime = 0;
        float dissovleHight_start = 20f;
        float disssolveHeight_target = -10f;
        float dissolveHight;

        _materialPropertyBlock.SetFloat("_enableDissolve", 1f);
        _skinnedMeshRenderer.SetPropertyBlock(_materialPropertyBlock);

        while (currentDissolveTime < dissolveTimeDuration)
        {
            currentDissolveTime += Time.deltaTime;
            dissolveHight = Mathf.Lerp(dissovleHight_start, disssolveHeight_target, currentDissolveTime / dissolveTimeDuration);
            _materialPropertyBlock.SetFloat("_dissolve_height", dissolveHight);
            _skinnedMeshRenderer.SetPropertyBlock(_materialPropertyBlock);
            yield return null;
        }
        if (!IsPlayer)
        {
            DropItem();
        }

    }

    //物品掉落
    public void DropItem()
    {
        if (ItemToDrop != null)
        {
            Instantiate(ItemToDrop, transform.position, Quaternion.identity);
        }
    }

    //拾取物品
    public void PickUpItem(PickUp item)
    {
        switch (item.Type)
        {
            case PickUp.PickUpType.Heal:
                AddHealth(item.value);
                break;
            case PickUp.PickUpType.Coin:
                AddCion(item.value);
                break;
        }
    }

    private void AddHealth(int health)
    {
        _health.AddHealth(health);
        GetComponent<PlayerVFXManager>().PlayHealVFX();
    }

    private void AddCion(int coin)
    {
        Coin += coin;
    }
    public void RotateToTarget()
    {
        if (CurrentState != CharacterState.Dead)
        {
            transform.LookAt(TargetPlayer, Vector3.up);
        }
    }
    IEnumerator MaterialAppear()
    {
        float dissolveTimeDuration = SpawnDuiation;
        float currentDissolveTime = 0;
        float dissolveHight_start = -10f;
        float dissolveHight_target = 20f;
        float dissolveHight;

        _materialPropertyBlock.SetFloat("_enableDissolve", 1f);
        _skinnedMeshRenderer.SetPropertyBlock(_materialPropertyBlock);

        while (currentDissolveTime < dissolveTimeDuration)
        {
            currentDissolveTime += Time.deltaTime;
            dissolveHight = Mathf.Lerp(dissolveHight_start, dissolveHight_target, currentDissolveTime / dissolveTimeDuration);
            _materialPropertyBlock.SetFloat("_dissolve_height", dissolveHight);
            _skinnedMeshRenderer.SetPropertyBlock(_materialPropertyBlock);
            yield return null;
        }

        _materialPropertyBlock.SetFloat("_enableDissolve", 0f);
        _skinnedMeshRenderer.SetPropertyBlock(_materialPropertyBlock);
    }

    private void OnDrawGizmos()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hitResult;
        if (Physics.Raycast(ray, out hitResult, 1000, 1 << LayerMask.NameToLayer("CursorTest")))
        {
            Vector3 cursotPos = hitResult.point;
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(cursotPos, 1);
        }
    }

    private void RotateToCusor()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hitResult;
        if (Physics.Raycast(ray, out hitResult, 1000, 1 << LayerMask.NameToLayer("CursorTest")))
        {
            Vector3 cursotPos = hitResult.point;
            transform.rotation = Quaternion.LookRotation(cursotPos - transform.position,Vector3.up);
        }
    }
}
