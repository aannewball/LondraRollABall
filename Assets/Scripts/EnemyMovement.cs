/*using UnityEngine;
using UnityEngine.AI;

public class EnemyMovement : MonoBehaviour
{
    // Reference to the player's transform.
    public Transform player;
    private Animator anim;

    // Reference to the NavMeshAgent component for pathfinding.
    private NavMeshAgent navMeshAgent;

    // Start is called before the first frame update.
    void Start()
    {
        // Get and store the NavMeshAgent component attached to this object.
        navMeshAgent = GetComponent<NavMeshAgent>();

        //Get the animator component
        anim = GetComponentInChildren<Animator>();
        //Set the value of speed_f
        if (anim)
        {
            anim.SetFloat("speed_f", navMeshAgent.speed);
        }
    }

    // Update is called once per frame.
    void Update()
    {
        // If there's a reference to the player...
        if (player != null)
        {
            // Set the enemy's destination to the player's current position.
            navMeshAgent.SetDestination(player.position);
        }


    }
}*/

using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Sistema de máquina de estados finitos (FSM) para controlar el comportamiento del enemigo.
/// El enemigo puede estar en cuatro estados: Inactivo, Persiguiendo, Buscando y Atacando.
/// </summary>
public class EnemyFSM : MonoBehaviour
{
    /// <summary>
    /// Enumeración que define los posibles estados del enemigo.
    /// </summary>
    public enum EnemyState { Idle, Chase, Search, Attack }

    /// <summary>
    /// Estado actual del enemigo.
    /// </summary>
    public EnemyState currentState = EnemyState.Idle;

    /// <summary>
    /// Componente NavMeshAgent para la navegación y movimiento del enemigo.
    /// </summary>
    public NavMeshAgent agent;

    /// <summary>
    /// Referencia al Transform del jugador.
    /// </summary>
    public Transform player;

    [Header("Detección")]
    /// <summary>
    /// Radio de detección para identificar al jugador (en unidades de Unity).
    /// Si el jugador está dentro de este radio, el enemigo entra en el estado Chase.
    /// </summary>
    public float detectionRadius = 8f;

    /// <summary>
    /// Radio de ataque. Cuando el jugador está dentro de este radio, el enemigo ataca.
    /// </summary>
    public float attackRadius = 1.5f;

    /// <summary>
    /// Duración de la búsqueda en segundos. Si no encuentra al jugador en este tiempo,
    /// el enemigo vuelve al estado Idle.
    /// </summary>
    public float searchDuration = 3f;

    /// <summary>
    /// Temporizador para controlar la duración de la búsqueda.
    /// </summary>
    private float searchTimer = 0f;

    /// <summary>
    /// Última posición conocida del jugador antes de perderlo de vista.
    /// Se usa para navegar hacia esa posición durante la búsqueda.
    /// </summary>
    private Vector3 lastKnownPosition;


    /// <summary>
    /// Inicialización del componente. Se ejecuta una sola vez al inicio.
    /// Obtiene el componente NavMeshAgent si no está asignado.
    /// </summary>
    void Start()
    {
        if (agent == null) agent = GetComponent<NavMeshAgent>();
    }

    /// <summary>
    /// Actualización del comportamiento del enemigo cada frame.
    /// Ejecuta el estado actual según la máquina de estados.
    /// </summary>
    void Update()
    {
        switch (currentState)
        {
            case EnemyState.Idle:
                IdleState();
                break;

            case EnemyState.Chase:
                ChaseState();
                break;

            case EnemyState.Search:
                SearchState();
                break;

            case EnemyState.Attack:
                AttackState();
                break;
        }
    }

    // ==========================================
    //          MÉTODOS DE ESTADOS
    // ==========================================

    /// <summary>
    /// Estado Inactivo: El enemigo permanece en su posición.
    /// Transita a Chase si detecta al jugador dentro del rango de detección.
    /// </summary>
    void IdleState()
    {
        // El enemigo se mantiene en su posición actual
        agent.SetDestination(transform.position);

        // Verifica si el jugador está dentro del rango de detección
        if (PlayerInRange(detectionRadius))
        {
            currentState = EnemyState.Chase;
        }
    }

    /// <summary>
    /// Estado Perseguir: El enemigo sigue al jugador.
    /// Transita a Search si pierde de vista al jugador.
    /// Transita a Attack si el jugador entra en el rango de ataque.
    /// </summary>
    void ChaseState()
    {
        // Protección: si el jugador fue destruido, vuelve a Idle
        if (player == null)
        {
            currentState = EnemyState.Idle;
            return;
        }

        // Establece el destino hacia la posición actual del jugador
        agent.SetDestination(player.position);
        // Guarda la última posición conocida para la búsqueda
        lastKnownPosition = player.position;

        // Si el jugador sale del rango de detección, comienza la búsqueda
        if (!PlayerInRange(detectionRadius))
        {
            currentState = EnemyState.Search;
            searchTimer = 0f;
        }

        // Si el jugador entra en el rango de ataque, ataca
        if (PlayerInRange(attackRadius))
        {
            currentState = EnemyState.Attack;
        }
    }

    /// <summary>
    /// Estado Buscar: El enemigo busca al jugador en la última posición conocida.
    /// Transita a Chase si detecta al jugador nuevamente.
    /// Transita a Idle si agota el tiempo de búsqueda sin encontrar al jugador.
    /// </summary>
    void SearchState()
    {
        // Navega hacia la última posición conocida del jugador
        agent.SetDestination(lastKnownPosition);
        // Incrementa el temporizador de búsqueda
        searchTimer += Time.deltaTime;

        // Si encuentra al jugador durante la búsqueda, reanuda la persecución
        if (PlayerInRange(detectionRadius))
        {
            currentState = EnemyState.Chase;
        }

        // Si la búsqueda dura demasiado tiempo, abandona la búsqueda
        if (searchTimer >= searchDuration)
        {
            currentState = EnemyState.Idle;
        }
    }

    /// <summary>
    /// Estado Ataque: El enemigo ataca al jugador cuando está dentro del rango de ataque.
    /// Se detiene en su posición actual para atacar.
    /// Transita a Chase si el jugador se aleja del rango de ataque.
    /// </summary>
    void AttackState()
    {
        // El enemigo se detiene en su posición actual
        agent.SetDestination(transform.position);

        // Aquí se podría aplicar daño real al jugador
        Debug.Log("¡El enemigo ataca!");

        // Si el jugador sale del rango de ataque, reanuda la persecución
        if (!PlayerInRange(attackRadius))
        {
            currentState = EnemyState.Chase;
        }
    }

    // ==========================================
    //          MÉTODOS AUXILIARES
    // ==========================================

    /// <summary>
    /// Comprueba si el jugador está dentro de un radio especificado.
    /// </summary>
    /// <param name="radius">Radio de detección en unidades de Unity.</param>
    /// <returns>Verdadero si el jugador está dentro del radio, falso si no o si fue destruido.</returns>
    bool PlayerInRange(float radius)
    {
        // Si el jugador es nulo (fue destruido), retorna falso
        if (player == null)
            return false;

        // Calcula la distancia entre el enemigo y el jugador
        return Vector3.Distance(transform.position, player.position) <= radius;
    }
}
