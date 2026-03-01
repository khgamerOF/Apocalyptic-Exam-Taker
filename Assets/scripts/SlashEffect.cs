using UnityEngine;

public class SlashEffect : MonoBehaviour
{
    [SerializeField] float lifetime = 0.2f;

    void Start()
    {
        Destroy(gameObject, lifetime);
    }
}
