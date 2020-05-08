﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WeaponController : MonoBehaviour {
    public Image ReloadBar;

    public float range = 100f;
    public float damage;
    public float fireRate;
    public float impactForce = 100f;
    public bool fullAuto = false;

    public float reserveAmmo;
    public float magSize;
    public float currAmmo;
    public float reloadTime;
    private bool isReloading = false;

    public ParticleSystem muzzleFlash;
    public GameObject defaultImpact;
    public GameObject enemyImpact;
    public GameObject dirtImpact;

    public int equipSlot = -1;

    private InventoryController inventory;
    private Animator animator;
    private float nextTimeToFire = 0f;

    void Start() {
        ReloadBar = ReloadBar.GetComponent<Image>();
        animator = GetComponent<Animator>();
        inventory = transform.parent.transform.parent.GetComponent<InventoryController>();

        if (equipSlot != -1) SendWeaponState();

        ReloadBar.fillAmount = 0;
    }

    void SendWeaponState() {
        inventory.UpdateWeaponInfo(equipSlot == 0, transform.name, currAmmo, reserveAmmo);
    }

    void Update() {
        if (!isReloading) {
            if (Time.time >= nextTimeToFire && (Input.GetButton("Fire1") && fullAuto || Input.GetButtonDown("Fire1") && !fullAuto)) {
                // set the next firing time to a point in the future (relative to current)
                nextTimeToFire = Time.time + 1f / fireRate;
                Shoot();
            }

            if (Input.GetKeyDown("r") && reserveAmmo > 0 && currAmmo < magSize && !isReloading) {
                StartCoroutine(Reload());
            }
        } else {
            ReloadBar.fillAmount += 1.0f / reloadTime * Time.deltaTime;
        }
    }

    void Shoot() {
        if (currAmmo > 0) {
            muzzleFlash.Play();
            animator.SetTrigger("fire");

            currAmmo -= 1;

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
            
            SendWeaponState();
        }

        if (currAmmo <= 0 && reserveAmmo > 0) StartCoroutine(Reload());
    }

    IEnumerator Reload() {
        isReloading = true;
        Debug.Log("RELOADING");
        animator.SetTrigger("reload");
        yield return new WaitForSeconds(reloadTime);
        isReloading = false;
        ReloadBar.fillAmount = 0;

        if (reserveAmmo > 0) {
            // if we have bullets in mag, return to pool
            reserveAmmo += currAmmo;
            currAmmo = 0;

            if (reserveAmmo < magSize) {
                currAmmo = reserveAmmo;
            } else {
                currAmmo = magSize;
            }

            reserveAmmo -= currAmmo;
        }

        SendWeaponState();
    }
}
