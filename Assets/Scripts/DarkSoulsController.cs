using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class DarkSoulsController : TP_Player
{

    [System.Serializable]
    public class Attack
    {
        public string animationName;
        public float hitTime;
        public float animationTime;
        public AnimationCurve attackSpeedCurve;
    }

    public enum STATE { FREE, ATTACK, ROLL, BLOCK }
    public STATE _state = STATE.FREE;

    #region MOVEMENT
    public enum MOVE_TYPE { NONE = 0, FRONT, BACK, LEFT, RIGHT };
    private MOVE_TYPE currentMoveType = MOVE_TYPE.NONE;

    private Vector3 move;
    #endregion

    #region Lock Enemy
    private bool lockingEnemy = false;
    private Enemy lockedEnemy;
    private bool strafing = false;
    private new bool backwards = false;
    #endregion

    #region Rolling
    private bool rolling = false;
    public AnimationCurve rollSpeedCurve;
    private float rollTimeSeconds = 1.15f;
    private float rollCurrentTime = 0;
    private Vector3 rollMove;
    #endregion

    #region Attack
    private enum ATK_TYPE { ATK_SOFT = 0, ATK_BACKSTAB }
    private enum ATK_PHASE { ATK_START = 0, ATK_ANIM, ATK_END }
    private ATK_TYPE currentAttack;
    private ATK_PHASE currentAtkPhase;
    private bool attacking = false;
    private float currentAttackTimeSeconds;
    private float attackCurrentTime = 0;
    private AttackHitbox _attackHitbox;
    private Vector3 attackMove;
    public List<Attack> attacks;
    #endregion

    #region Blocking
    private bool blocking = false;
    private float walkBlockingSpeed;
    #endregion

    private new void Awake()
    {
        base.Awake();
        _attackHitbox = tpc.transform.Find("AttackHitbox").GetComponent<AttackHitbox>();
        walkBlockingSpeed = 0.3f * walk_speed;
    }

    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {

        PlayerStateMachine();

        if (Input.GetKeyDown(KeyCode.X)) {
            LockEnemy();
        }

        #region Animator settings
        // Animations
        tpc.GetFullBodyAnimator().SetBool("walking", walking);
        tpc.GetFullBodyAnimator().SetBool("strafe", strafing);
        tpc.GetFullBodyAnimator().SetBool("backwards", backwards);
        tpc.GetFullBodyAnimator().SetBool("jump", jump);
        #endregion
    }

    protected new void FixedUpdate()
    {
        // Jump 
        if (jump)
        {
            _rb.AddForce(Vector3.up * jump_force, ForceMode.Impulse);
        }

        // Update animation flags
        if (_cm.type == CameraMovement.CAMERA_TYPE.FREE_LOOK)
        {
            walking = (horizontalInput != 0 || verticalInput != 0);

        }

        /*else if (_cm.type == CameraMovement.CAMERA_TYPE.LOCKED)
        {
            walking = (v > 0 && h == 0);
            backwards = (v < 0 && h == 0);
            strafeLeft = (h < 0);
            strafeRight = (h > 0);
        }*/
    }

    void LockEnemy() {
        if (!lockingEnemy) // Lock
        {
            lockedEnemy = FindClosestEnemy();
            if (lockedEnemy) {
                lockingEnemy = true;
                UIManager.s.LockEnemy(lockedEnemy);
                Debug.Log("Locking enemy");
            }
        }
        else {  // Unlock 
            lockingEnemy = false;
            backwards = false;
            strafing = false;
            lockedEnemy = null;
            UIManager.s.UnlockEnemy();
        }
    
    }

    private Enemy FindClosestEnemy() {
        Enemy[] enemies = FindObjectsOfType<Enemy>();

        if (enemies.Length > 0)
        {
            float distanceToPlayer = Vector3.Distance(enemies[0].transform.position, transform.position);
            float currentBest = distanceToPlayer;
            int bestIndex = 0;
            for (int i = 1; i < enemies.Length; ++i)
            {
                distanceToPlayer = Vector3.Distance(enemies[i].transform.position, transform.position);
                if (distanceToPlayer < currentBest)
                {
                    bestIndex = i;
                    currentBest = distanceToPlayer;
                }
            }

            return enemies[bestIndex];
        }
        else return null;
    }

    protected new void PlayerStateMachine()
    {
        switch (_state)
        {
            case STATE.FREE:
                {
                    MovePlayer();
                    break;
                }
            case STATE.ATTACK:
                {
                    AttackSubstateMachine();
                    break;
                }
            case STATE.ROLL:
                {
                    ExecuteRoll();
                    break;
                }
            case STATE.BLOCK:
                {
                    ExecuteBlock();
                    break;
                }
            default: break;
        }

        CheckForStateChange();
    }

    void CheckForStateChange()
    {

        switch (_state)
        {
            case STATE.FREE:
                {
                    if (Input.GetKey(KeyCode.Q)) _state = STATE.BLOCK;
                    else if (Input.GetKeyDown(KeyCode.C)) _state = STATE.ROLL;
                    else if (Input.GetMouseButtonDown(0))
                    {
                        _state = STATE.ATTACK;
                        currentAtkPhase = ATK_PHASE.ATK_START;
                    }
                    break;
                }
            case STATE.ATTACK:
                {
                    break;
                }
            case STATE.ROLL:
                {

                    break;
                }
            case STATE.BLOCK:
                {
                    if (Input.GetKeyUp(KeyCode.Q))
                        _state = STATE.FREE;

                    break;
                }
            default: break;
        }
    }


    #region Movement methods

    private void MovePlayer() {

        // Move the player (movement will be slightly different depending on the camera type)
        move = GetPlayerMoveNormalized();
        currentMoveType = GetCurrentMoveType();
        float speed = GetCurrentSpeed();

        move *= speed;

        _cam.transform.position += move * Time.deltaTime;
        transform.position += move * Time.deltaTime;    // Move the actual player global gameobject

        SetPlayerMovementAnimation();
    }

    private void SetPlayerMovementAnimation()
    {
        // Rotate body
        if (!lockingEnemy) tpc.transform.rotation = Quaternion.LookRotation(Vector3.RotateTowards(tpc.transform.forward, move, rotation_speed, 0.0f));
        else
        {
            Vector3 lockedEnemyDirection = lockedEnemy.transform.position - transform.position;
            tpc.transform.rotation = Quaternion.LookRotation(Vector3.RotateTowards(tpc.transform.forward, lockedEnemyDirection, rotation_speed, 0.0f));

            string animToPlay;
            switch (currentMoveType)
            {
                case MOVE_TYPE.RIGHT:
                    backwards = false;
                    strafing = true;
                    animToPlay = (blocking) ? "BlockStrafeRight" : "StrafeRunRight";
                    tpc.GetFullBodyAnimator().Play(animToPlay);
                    break;
                case MOVE_TYPE.LEFT:
                    backwards = false;
                    strafing = true;
                    animToPlay = (blocking) ? "BlockStrafeLeft" : "StrafeRunLeft";
                    tpc.GetFullBodyAnimator().Play(animToPlay);
                    break;
                case MOVE_TYPE.BACK:
                    strafing = false;
                    backwards = true;
                    animToPlay = (blocking) ? "BlockBackwards" : "RunBackwards";
                    tpc.GetFullBodyAnimator().Play(animToPlay);
                    break;
                default:
                    strafing = false;
                    backwards = false;
                    break;
            }
        }
    }

    private MOVE_TYPE GetCurrentMoveType()
    {
        float moveAngle = Vector3.SignedAngle(tpc.transform.forward, move, Vector3.up); // Positive: moving right - Negative: moving left

        MOVE_TYPE result = MOVE_TYPE.NONE;
        if (move != Vector3.zero)
        {
            if (moveAngle > 30 && moveAngle < 150)
            {
                result = MOVE_TYPE.RIGHT;
            }
            else if (moveAngle < -30 && moveAngle > -150)
            {
                result = MOVE_TYPE.LEFT;
            }
            else if (Mathf.Abs(moveAngle) >= 150)
            {
                result = MOVE_TYPE.BACK;
            }
            else
            {
                result = MOVE_TYPE.FRONT;
            }
        }

        return result;
    }

    private float GetCurrentSpeed()
    {
        float result = 0;

        if (currentMoveType == MOVE_TYPE.FRONT)
        {
            result = (blocking) ? walkBlockingSpeed : walk_speed;
        }
        else if (currentMoveType == MOVE_TYPE.BACK)
        {
            result = (blocking) ? walkBlockingSpeed : backwards_walk_speed;
        }
        else if (currentMoveType == MOVE_TYPE.LEFT || currentMoveType == MOVE_TYPE.RIGHT)
        {
            result = (blocking) ? walkBlockingSpeed : strafe_speed;
        }

        return result;
    }

    private Vector3 GetPlayerMoveNormalized() {
        // Gets the input
        horizontalInput = Input.GetAxis("Horizontal");
        verticalInput = Input.GetAxis("Vertical");
        
        // Calculate camera relative directions to move:
        camFwd = Vector3.Scale(_cam.transform.forward, new Vector3(1, 1, 1)).normalized;
        Vector3 camFlatFwd = Vector3.Scale(_cam.transform.forward, new Vector3(1, 0, 1)).normalized;
        Vector3 flatRight = new Vector3(_cam.transform.right.x, 0, _cam.transform.right.z);

        Vector3 m_CharForward = Vector3.Scale(camFlatFwd, new Vector3(1, 0, 1)).normalized;
        Vector3 m_CharRight = Vector3.Scale(flatRight, new Vector3(1, 0, 1)).normalized;


        // Draws a ray to show the direction the player is aiming at
        //Debug.DrawLine(transform.position, transform.position + camFwd * 5f, Color.red);

        // Move the player (movement will be slightly different depending on the camera type)
        float w_speed;
        move = Vector3.zero;

        w_speed = walk_speed;
        move = (verticalInput * m_CharForward + horizontalInput * m_CharRight).normalized;

        return move;
    }

    #endregion

    void ExecuteRoll() {
        if (!rolling)
        {
            rolling = true;

            rollMove = move;
            tpc.transform.rotation = Quaternion.LookRotation(Vector3.RotateTowards(tpc.transform.forward, move, rotation_speed, 0.0f));
            tpc.GetFullBodyAnimator().Play("Roll");
            Invoke("EnableActions", rollTimeSeconds + 0.15f);
        }
        else {
            rollCurrentTime += Time.deltaTime;
            float speed = rollSpeedCurve.Evaluate(rollCurrentTime / rollTimeSeconds) * 5.0f;
            
            tpc.transform.rotation = Quaternion.LookRotation(Vector3.RotateTowards(tpc.transform.forward, move, rotation_speed, 0.0f));
            transform.position += rollMove.normalized * speed * Time.deltaTime;
            _cam.transform.position += rollMove.normalized * speed * Time.deltaTime;

            if (rollCurrentTime >= rollTimeSeconds)
                _state = STATE.FREE;
        }
    }

    #region Attack methods
    void AttackSubstateMachine() {
        switch (currentAtkPhase) {
            case ATK_PHASE.ATK_START: { // Starts the attack
                    attackCurrentTime = 0;

                    // Decides the type of attack
                    Enemy enemyBackstabbed = _attackHitbox.BackstabbedEnemy();
                    int attackToPlay = -1;
                    if (enemyBackstabbed)   // If an enemy is being backstabbed, perform backstab attack
                    {
                        attackToPlay = 0;
                        currentAttack = ATK_TYPE.ATK_BACKSTAB;
                        transform.position = enemyBackstabbed.transform.position - enemyBackstabbed.transform.forward * 0.5f;
                        _attackHitbox.KillEnemy(enemyBackstabbed, attacks[attackToPlay].hitTime);
                    }
                    else // Otherwise perform normal soft attack
                    {
                        attackToPlay = 1;
                        currentAttack = ATK_TYPE.ATK_SOFT;
                        
                        attackMove = move;
                        _attackHitbox.HitAllEnemies(attacks[attackToPlay].hitTime);

                    }
                    currentAttackTimeSeconds = attacks[attackToPlay].animationTime;
                    tpc.GetFullBodyAnimator().Play(attacks[attackToPlay].animationName);
                    Invoke("EnableActions", attacks[attackToPlay].animationTime + 0.5f);
                    currentAtkPhase = ATK_PHASE.ATK_ANIM;
                    break; 
                }
            case ATK_PHASE.ATK_ANIM: {
                    switch (currentAttack) {
                        case ATK_TYPE.ATK_SOFT: SoftAttacking(); break;
                        case ATK_TYPE.ATK_BACKSTAB: BackStabbing(); break;
                        default: break;
                    }


                    // Update time
                    attackCurrentTime += Time.deltaTime;
                    if (attackCurrentTime >= currentAttackTimeSeconds)
                        currentAtkPhase = ATK_PHASE.ATK_END;
                    break; 
                }
            case ATK_PHASE.ATK_END: {
                    attackCurrentTime = 0;
                    _state = STATE.FREE;
                    break; 
                }
            default: break;
        }
    
    }

    private void SoftAttacking()
    {
        float speed = attacks[1].attackSpeedCurve.Evaluate(attackCurrentTime / currentAttackTimeSeconds) * 5.0f;
        transform.position += attackMove.normalized * speed * Time.deltaTime;
        _cam.transform.position += attackMove.normalized * speed * Time.deltaTime;
        
    }

    public void BackStabbing() {
        
    }

    #endregion

    void EnableActions() { 
        rolling = false;
        rollCurrentTime = 0;

        attacking = false;
        attackCurrentTime = 0;
    }


    void ExecuteBlock() {
        if (!blocking)
        {
            blocking = true;
            tpc.GetFullBodyAnimator().SetBool("blocking", true);
            tpc.GetFullBodyAnimator().Play("BlockIdle");
        }
        else {
            MovePlayer();
            tpc.GetFullBodyAnimator().SetBool("walking", (move != Vector3.zero));
            bool blockButtonReleased = !(Input.GetKey(KeyCode.Q));
            if (blockButtonReleased) {
                tpc.GetFullBodyAnimator().SetBool("blocking", false);
                blocking = false;
            }
        }

    }
}
