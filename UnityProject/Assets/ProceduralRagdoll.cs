using UnityEngine;

public class ProceduralRagdoll : MonoBehaviour
{
    public void InitializeRagdoll(Vector3 impactVelocity)
    {
        // 1. Disable Animator so it stops forcing poses
        Animator animator = GetComponent<Animator>();
        if (animator != null) animator.enabled = false;

        // 2. Disable root collider and kinematic rigidbody if they exist
        // This ensures the main capsule collider doesn't interfere with the bone physics
        Collider rootCollider = GetComponent<Collider>();
        if (rootCollider != null) rootCollider.enabled = false;

        Rigidbody rootRb = GetComponent<Rigidbody>();
        if (rootRb != null) rootRb.isKinematic = true;

        // 3. Maximize Joint Floppiness
        CharacterJoint[] joints = GetComponentsInChildren<CharacterJoint>();
        foreach (CharacterJoint joint in joints)
        {
            // Set all limits near 180 degrees to remove all bone stiffness
            SoftJointLimit extremeLimit = new SoftJointLimit { limit = 175f };

            joint.lowTwistLimit = extremeLimit;
            joint.highTwistLimit = extremeLimit;
            joint.swing1Limit = extremeLimit;
            joint.swing2Limit = extremeLimit;

            // Enable projection to prevent limbs from tearing off at high speeds
            joint.enableProjection = true;
        }

        // 4. Activate Child Rigidbodies and Apply Impact Physics
        Rigidbody[] bones = GetComponentsInChildren<Rigidbody>();
        foreach (Rigidbody bone in bones)
        {
            if (bone == rootRb) continue; // Skip root

            bone.isKinematic = false;
            bone.useGravity = true;

            // Lower drag makes limbs flop faster through the air
            bone.linearDamping = 0.05f;
            bone.angularDamping = 0.05f;

            // Apply impact velocity
            bone.linearVelocity = impactVelocity;

            // Add chaotic torque (spin) to individual limbs for a more brutal crash look
            Vector3 randomTorque = new Vector3(
                Random.Range(-50f, 50f),
                Random.Range(-50f, 50f),
                Random.Range(-50f, 50f)
            );
            bone.AddTorque(randomTorque, ForceMode.VelocityChange);
        }
    }
}