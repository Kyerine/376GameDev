﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class CatController : NetworkBehaviour
{

    //for internal referencing
    private Transform Target;
    private Transform loc;
    private LayerMask caster;
    private Animator anim;
    private SpriteRenderer rendy;
    public Transform moveSpot;

    //variables
    private readonly float FollowRange = 10;
    private readonly float PatrolRange = 3;
    private float counter = 5.0f;
    private float counter2 = 0;

    private Vector2 InitialPosition;
    private Vector2 direction;

    private float PatrolSpeed, FollowSpeed;

    private Rigidbody2D rb;

    void Start()
    {
        loc = transform;
        rendy = GetComponent<SpriteRenderer>();
        caster = 1 << LayerMask.NameToLayer("Player");
        anim = GetComponent<Animator>();

        InitialPosition.x = transform.position.x;
        InitialPosition.y = transform.position.y;

        PatrolSpeed = 0.5f;
        FollowSpeed = 1;

        moveSpot.position = new Vector2(Random.Range(InitialPosition.x - PatrolRange, InitialPosition.x + PatrolRange), Random.Range(InitialPosition.y - PatrolRange, InitialPosition.y + PatrolRange));

        rb = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        SearchForTarget();
        if (Target == null)
        {
            Patrol();
        }
        else
        {
            float distance = Vector3.Distance(gameObject.transform.position, Target.transform.position);
            if (distance > 1.2) {
                counter2 = 0;
                Follow();
            }
            else {
                Attack();
            }

        }
        Orientation();
    }

    /***********************************
     *
     * Functions
     *
     ***********************************/

    /* SearchForTarget: looks for a player in the FollowRange
     *******************************************************/

    void SearchForTarget()
    {
        if (!isServer)
            return;

        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(loc.position, FollowRange, caster);

        if (hitColliders.Length > 0)
        {
            int randomint = Random.Range(0, hitColliders.Length);

            if (hitColliders[randomint].GetComponent<PlayerController>().getHealth() <= 0)
            {
                Target = null;
            }
            else
            {
                Target = hitColliders[randomint].transform;
            }
        }
        
    }

    public void PushedBack()
    {
        // pushed back
        Vector2 pushbackdirection = Target.transform.position - gameObject.transform.position;
        pushbackdirection.Normalize();
        rb.AddForce(-pushbackdirection * 5, ForceMode2D.Impulse);
    }


    //Colliding with the player will cause damage to the player
    void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.layer.Equals(8))
        {

        }
    }

    /* Orientation: determines which sprite to use
     ********************************************/

    void Orientation()
    {
        anim.SetBool("Move", true);

        Transform goTo;
        if (Target == null)
            goTo = moveSpot;
        else
            goTo = Target;

        if (Mathf.Abs(loc.position.x - goTo.position.x) > Mathf.Abs(loc.position.y - goTo.position.y))
        {
            anim.SetBool("Side", true);
            anim.SetBool("Up", false);
            anim.SetBool("Down", false);

            if ((loc.position.x - goTo.position.x) < 0)
            {
                rendy.flipX = false; // invoke the change on the Server as you already named the function
                //CmdProvideFlipStateToServer(rendy.flipX);
            }
            else if ((loc.position.x - goTo.position.x) > 0)
            {
                rendy.flipX = true; // invoke the change on the Server as you already named the function
                //CmdProvideFlipStateToServer(rendy.flipX);
            }
        }
        else if (Mathf.Abs((loc.position.x - goTo.position.x)) < Mathf.Abs((loc.position.y - goTo.position.y)))
        {
            anim.SetBool("Side", false);
            if ((loc.position.y - goTo.position.y) < 0)
            {
                anim.SetBool("Up", true);
                anim.SetBool("Down", false);
            }
            else
            {
                anim.SetBool("Up", false);
                anim.SetBool("Down", true);
            }
        }
    }

    /* Patrol: enemy walks towards random move spots
     **********************************************/

    void Patrol()
    {
        // direction of the patrol: towards the 'move spot'
        direction = Vector2.MoveTowards(transform.position, moveSpot.position, PatrolSpeed * Time.deltaTime);
        transform.position = direction;

        // if the enemy reaches the 'move spot'
        if (Vector2.Distance(transform.position, moveSpot.position) < 0.2f)
        {
            // create a new random 'move spot'
            moveSpot.position = new Vector2(Random.Range(InitialPosition.x - PatrolRange, InitialPosition.x + PatrolRange), Random.Range(InitialPosition.y - PatrolRange, InitialPosition.y + PatrolRange));
        }
    }

    /* Follow: enemy follows a player
     ********************************/

    void Follow()
    {
        if (Target != null && isServer)
        {
            direction = Vector2.MoveTowards(transform.position, Target.position, FollowSpeed * Time.deltaTime);
            transform.position = direction;

            counter -= Time.deltaTime;
        }
    }

    /* Attack: enemy cause damage every second
     ****************************************/

    void Attack()
    {
        if (counter2 > 0)
        {
            counter2 -= Time.deltaTime;
        }
        else
        {
            Target.gameObject.GetComponent<PlayerController>().TakeDamage(GetComponent<Health>().getAttackDamage());
            counter2 = 1.0f;
        }
        Target = null;
    }

    /***********************************
     *
     * Network
     *
     ***********************************/

    [Command]
    void CmdProvideFlipStateToServer(bool state)
    {
        rendy.flipX = state; // make the change local on the server
        RpcSendFlipState(state); // forward the change also to all clients
    }

    [ClientRpc]
    void RpcSendFlipState(bool state)
    {
        if (isLocalPlayer) return; // skip this function on the LocalPlayer because he is the one who originally invoked this
        rendy.flipX = state; // make the change local on all clients
    }
}
