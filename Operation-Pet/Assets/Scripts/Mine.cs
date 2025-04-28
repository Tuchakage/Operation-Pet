using UnityEngine;
public class Mine : MonoBehaviour
    {
        public float explosionForce = 10.0f; // Force of the mine explosion
        public float explosionRadius = 5.0f; // Radius within which the mine affects objects
        public float upwardModifier = 2.0f; // Adds upward force to the explosion
        public float detonationDelay = 2.0f; // Time before the mine detonates

        private void Start()
        {
            // Automatically detonate the mine after the delay
            Invoke(nameof(Detonate), detonationDelay);
        }

        private void Detonate()
        {
            // Find all colliders within the explosion radius
            Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);

            // Apply force to each collider with a Rigidbody
            foreach (Collider collider in colliders)
            {
                Rigidbody rb = collider.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.AddExplosionForce(explosionForce, transform.position, explosionRadius, upwardModifier, ForceMode.Impulse);
                }
            }

            // Destroy the mine object
            Destroy(gameObject);
        }

        private void OnDrawGizmos()
        {
            // Visualize the explosion radius in the editor
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, explosionRadius);
        }
    }