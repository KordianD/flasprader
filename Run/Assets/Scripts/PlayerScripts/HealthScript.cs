﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public class HealthScript : MonoBehaviour
{
    // Start is called before the first frame update
    void Awake()
    {
        if(isBoar || isCannibal)
        {
            _enemyAnim = GetComponent<EnemyAnimator>();
            _enemyController = GetComponent<EnemyController>();
            _navAgent = GetComponent<NavMeshAgent>();

            _enemyAudio = GetComponentInChildren<EnemyAudio>();
        }

        if(isPlayer)
        {
            _playerStats = GetComponent<PlayerStats>();
            _playerMovement = GetComponent<PlayerMovement>();
        }
    }

    // Update is called once per frame
    public void ApplyDamage(float damage)
    {
        if (_isDead)
            return;

        health -= damage;

        if(isPlayer)
        {
            _playerStats.DisplayHealthStats(health);
        }

        if(isBoar || isCannibal)
        {
            if(_enemyController.EnemyState == EnemyState.PATROL)
            {
                _enemyController.chaseDistance = 50f;
            }
        }

        if(health <= 0f)
        {
            PlayerDied();
            _isDead = true;
           
        }
    }

    public void ApplyHealth(bool isFromPotion)
    {
        if (!isPlayer) return;

        if (isFromPotion)
        {
            health += 20f;
        }
        else
        {
            health += 40f;
        }

        health = Mathf.Clamp(health, 0f, 100f);
        print("Health increased: " + health);
        _playerStats.DisplayHealthStats(health);
    }

    public void PlayerDied()
    {
        var playerStats = GameObject.FindWithTag(TagsExtensions.PLAYER_TAG).GetComponent<PlayerStats>();
        if (isCannibal)
        {
            GetComponent<Animator>().enabled = false;
            GetComponent<BoxCollider>().isTrigger = false;
            GetComponent<Rigidbody>().AddTorque(-transform.forward * 10f);
            _enemyController.enabled = false;
            _navAgent.enabled = false;
            _enemyAnim.enabled = false;
            StartCoroutine(DeadSound());

            EnemyManager.instance.EnemyDied(true);
            playerStats.IdleTime = 0f;
            playerStats.Score += KillingEnemiesScore["Cannibal"] * playerStats.Level;
        }
        if(isBoar)
        {
            _navAgent.velocity = Vector3.zero;
            _navAgent.isStopped = true;
            _enemyController.enabled = false;
            _enemyAnim.Dead();

            StartCoroutine(DeadSound());
            EnemyManager.instance.EnemyDied(false);
            playerStats.IdleTime = 2f;
            playerStats.Score += KillingEnemiesScore["Boar"] * playerStats.Level;
        }
        if(isPlayer)
        {
            _playerStats.SaveStatisticsFromGameToFile();
            _playerMovement.Die();
            GameObject[] enemies = GameObject.FindGameObjectsWithTag(TagsExtensions.ENEMY_TAG);
            for(int i =0; i<enemies.Length; i++)
            {
                enemies[i].GetComponent<EnemyController>().enabled = false;
            }

            GetComponent<PlayerMovement>().enabled = false;
            GetComponent<PlayerAttack>().enabled = false;
            GetComponent<WeaponManager>().GetCurrentSelectedWeapon().gameObject.SetActive(false);
            EnemyManager.instance.StopSpawning();
            //SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1);
        }

      if(tag == TagsExtensions.PLAYER_TAG)
       {
         Invoke("RestartGame", 3f);
      }
      else
       {
          Invoke("TurnOffGameObject", 3f);
        }
    }

    void RestartGame()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("SampleScene");
    }

    void TurnOffGameObject()
    {
        gameObject.SetActive(false);
    }

    IEnumerator DeadSound()
    {
        yield return new WaitForSeconds(0.3f);
        _enemyAudio.PlayDeadSound();
    }

    private EnemyAnimator _enemyAnim;
    private NavMeshAgent _navAgent;
    private EnemyController _enemyController;
    private PlayerMovement _playerMovement;
    public float health = 100f;
    public bool isPlayer, isBoar, isCannibal;

    private bool _isDead;

    private EnemyAudio _enemyAudio;
    private PlayerStats _playerStats;
    private static readonly Dictionary<string, int> KillingEnemiesScore = new Dictionary<string, int>
    {
        { "Cannibal", 10 },
        { "Boar", 5 },
    };
}