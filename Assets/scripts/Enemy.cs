using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] GameObject player;
    [SerializeField] float aggroRange;
    [SerializeField] float speed;
    void Update()
    {
        if (Vector2.Distance(player.transform.position, transform.position) < aggroRange)
        {
            if(player.transform.position.x < transform.position.x)
            {
                transform.Translate(Vector2.left * Time.deltaTime * speed);
            }
            else
            {
                transform.Translate(Vector2.right * Time.deltaTime * speed);
            }
        }
    }
}
