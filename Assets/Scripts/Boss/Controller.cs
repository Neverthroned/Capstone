using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class Controller : MonoBehaviour
{
    public FlamethrowerCollision collisionScript;
    public VisualEffect flamethrowerVFX;

    private bool _isActive = false;

    void Start()
    {
        collisionScript.DisableDamage();
        flamethrowerVFX.Stop();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            ToggleFlamethrower(!_isActive);
        }
    }

    public void ToggleFlamethrower(bool enable)
    {
        _isActive = enable;

        if (enable)
        {
            collisionScript.ResetActivation();
            flamethrowerVFX.Play();
        }
        else
        {
            collisionScript.DisableDamage();
            flamethrowerVFX.Stop();
        }
    }
}