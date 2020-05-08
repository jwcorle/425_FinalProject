﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponController : MonoBehaviour {
    public GameObject pistol;

    public float damage = 10f;
    public float range = 100f;
    public float fireRate = 10f;
    public float impactForce = 100f;

    public ParticleSystem muzzleFlash;
    public GameObject defaultImpact;
    public GameObject enemyImpact;
    public GameObject dirtImpact;

    private Animator animator;
    private float nextTimeToFire = 0f;

    void Start() {
        animator = pistol.GetComponent<Animator>();
    }

    void Update() {
        // Full-auto
        // if (Input.GetButton("Fire1") && Time.time >= nextTimeToFire) 

        // Semi-auto firing
        if (Input.GetButtonDown("Fire1") && Time.time >= nextTimeToFire) {
            // set the next firing time to a point in the future (relative to current)
            nextTimeToFire = Time.time + 1f / fireRate;
            Shoot();
        }
    }

    void Shoot() {
        muzzleFlash.Play();
        animator.SetTrigger("fire");

        RaycastHit hit;
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, range)) {
            EnemyController enemy = hit.transform.GetComponent<EnemyController>();
            if (enemy != null) {
                enemy.TakeDamage(damage);
            }

            if (hit.rigidbody != null) {
                hit.rigidbody.AddForce(-hit.normal * impactForce);
            }

            if (hit.transform.tag == "Dirt") {
                Instantiate(dirtImpact, hit.point, Quaternion.LookRotation(hit.normal));
            } else if (hit.transform.tag == "Enemy") {
                Instantiate(enemyImpact, hit.point, Quaternion.LookRotation(hit.normal));
            } else {
                Instantiate(defaultImpact, hit.point, Quaternion.LookRotation(hit.normal));
            }
        }
    }
}
