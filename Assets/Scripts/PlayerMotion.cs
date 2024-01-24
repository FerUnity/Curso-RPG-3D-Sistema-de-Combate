using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
//Libreria para el nuevo Input System de Unity,
//como componenete del GO personaje o RPG Character:
using UnityEngine.InputSystem;

public class PlayerMotion : MonoBehaviour
{
    //Este script esta en el GO Player o RPG Character.

    //Referencia a la camara que sigue al Player,
    //el mov del player la dara la perspect de esa camara:
    public Transform cam;

    //Ref a camara CineMachine:
    public CinemachineFreeLook cinemachineFreeLook;
    //Y el target de la camara cinemachineFreeLook:
    public GameObject targetCam;
    //Y veloc de rotacion en X e Y, de esta camara:
    public float rotationSpeedCamX, rotationSpeedCamY;
    //Mov del Mouse que determina el mov de esta camara:
    Vector2 m_look;


    public float jumpPower = 35f;//Fuerza DE SALTO
    //Gravedad: ver FixedUpdate():
    public float gravity = 9.8f;//Aceleracion de gravedad
    public float gravityMultiplayer = 1;//Acc adicional de ser necesario

    //Veloc de mov del personaje y de rotacion tambien:
    public float speed;
    public float speedRotation = 10f;
    //Ref privadas al rb y otras:
    Rigidbody rb;
    Animator anim;
    Vector2 _move;//Dir de mov del Player
    Vector3 move;//Direccion de mov de la camara que sigue al player

    //Para saltar necesito 2 var boo, y 1 LayerMask:
    public bool onGround, isJump;
    public LayerMask groundLayer;//En inspector sera la capa Ground.
                                 //En este proyecto, el GO World estara en capa Ground.
    //2 var float para verificar si estamos o no tocando el piso.
    //A que altura se produce la colision y el tamaño de la collision como esfera:
    public float groundDistanceUp, groundDistance;

    bool stop = false;

    // Awake se ejecuta anytes de start cuando esta cargando el juego, antes de comenzar:
    void Awake()
    {
        //isJump = false;
        //Asignamoa los componenetes del GO a la var creadas arriba:
        rb = GetComponent<Rigidbody>();
        //El Animator esta en un GO hijo del GO donde esta este script:
        anim = GetComponentInChildren<Animator>();

    }

    //met para detectar la colision para generar el salto:
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;//Pinatmos los gizmos amarillos

        //Creamos la esfera que define la colision del Player con el suelo.
        //En la posicion hacia arriba: groundDistanceUp. Con un radio de la esfera de groundDistance:
        Gizmos.DrawSphere(transform.position + (Vector3.up * groundDistanceUp), groundDistance);
    }


    // Usaremos FixedUpdate is called once per frame,
    // porque queremos llamar al met de mov fisico del player
    // y no queremos que dependa del frame del dispositivo donde corre el juego
    void FixedUpdate()
    {

        //Definimos aca si el Player esta o no tocando el suelo,
        //con una esfera igual a la definida en met OnDrawGizmosSelected():
        onGround = Physics.CheckSphere(transform.position + (Vector3.up * groundDistanceUp), groundDistance, groundLayer);

        
        //Efecto gravedad:
        //Si no toca el suelo:
        if (!onGround)
        {
            //Generamos una fuerza de tipo aceleracion hacia abajo de mag = gravity:
            rb.AddForce(-gravity * gravityMultiplayer * Vector3.up, ForceMode.Acceleration);
        }

        //Ahora cuando caiga al suelo luego del salto:
        if (isJump && onGround)
        {
            isJump = false;
            //Y desact las animaciones que tengan que ver con estar en el aire y salto:
            anim.SetBool("OnAir", false);
            rb.velocity = Vector3.zero;

        }
        //PEro si esta cayendo:
        else if (!isJump && !onGround)
        {
            
            anim.SetBool("OnAir", true);
            isJump = true;
            Stopping();
            anim.SetTrigger("Fall");//Animacion de caida
        }

        //Si el Player esta detenido que no se ejecute el mov que sigue en este met FixedUpdate():
        if (stop)
        {
            return;
        }

        //Si es que el player se mueve:
        //Queremos que el player se mueva segun la perspect de la camara, que lo va a estar siguiendo:
        if (_move.x != 0 || _move.y != 0)
        {
            //Obtenemos la var move de la dir de mov del player usando los gizmos de la camara, asi:
            move = cam.forward * _move.y;
            move += cam.right * _move.x;
            move.Normalize();//Para obtener solo la direccion de mov y no la magn
            //Ademas queremos que el player solo se mueve en X y Z, no en Y para que no se eleve:
            move.y = 0;
            //Finalmente Movemos al personaje con su rb,
            //segun la direccion obtenida en move y con una var de velioc speed:
            rb.velocity = move * speed;

            Vector3 dir = cam.forward * _move.y;
            dir += cam.right * _move.x;
            dir.Normalize();
            dir.y = 0;
            //Para la rotacion del player usamos quaterniones, primero del objetivo:
            Quaternion targetR = Quaternion.LookRotation(dir);
            //Luego rotacion del player que sea una fraccion de la rotacion total al targetR,
            //met Slerp(Pos de rot actual, rotacion objetivo, veloc de rotacion):
            Quaternion playerR = Quaternion.Slerp(transform.rotation, targetR, speedRotation * Time.fixedDeltaTime);
            //Y actualizamos la rotacion actual, para que gire el Player:
            transform.rotation = playerR;
        }

    }

    //Creamos met publico para el mov del pàyer con parametro InputValue(lib UnityEngine.InputSystem)
    //que recibira la informacion del InputSystem en cjto con el componente Player Input del GO:

    public void OnMove(InputValue value)
    {
        //Le damos a la var _move el valor que estamos recibiendo
        //del dispositivo externo: teclado, mouse o control que el usuario tenga:

        _move = value.Get<Vector2>();
        //Cambiamos el valor de la var bool Move del animator:anim.SetBool("Move"):

        //Pero si stop es true no queremos que cambien las var de animacion de mas abajo:
        if (stop)
        {
            return;
        }
        //Si los valores de X e Y de _move son 0, el valor de Move = false,
        //si no son 0, la var Move = true:
        /*
        if (_move.x == 0 && _move.y == 0)
        {
            anim.SetBool("Move",false);
        }
        else
        {
            anim.SetBool("Move", true);
        }
        */
        //Otra forma simplificada de decir lo mismo que el if(), es:
        anim.SetBool("Move", (_move.x == 0 && _move.y == 0) ? false : true);

        //Lo mismo para la var float Moving del animator:
        //Si los valores de X e Y de -move son 0, el valor de Moving = 0,
        //si no son 0, la var Moving = 1f:
        anim.SetFloat("Moving", (_move.x == 0 && _move.y == 0) ? 0 : 1f);

        //ademas si los valores de X e Y de -move son 0, que se detenga todo mov del personaje:
        if (_move.x == 0 && _move.y == 0)
        {
            //Que el vector 3 de veloc sea 0 en sus 3 ejes:
            rb.velocity = Vector3.zero;
        }

        //Ademas que los valores float de _move en X e Y,
        //definan el valor de direccion de mov del Player, a las var MoveX y MoveY del animator:
        anim.SetFloat("MoveX", _move.x);
        anim.SetFloat("MoveY", _move.y);



    }

    //MEt que detendra todos los movimientos de animaciones del Player, antes de saltar u otras acciones:
    void Stopping()
    {
        //Antes de saltar, si esta tocando el suelo, que se detengan sus mov y animaciones:
        if (onGround)
        {
            rb.velocity = Vector3.zero;
            stop = true;
            anim.SetFloat("MoveX", 0);
            anim.SetFloat("MoveY", 0);
            anim.SetFloat("Moving", 0);
            anim.SetBool("Move",false);
        }
    }


    //met de SALTO:
    public void OnJump()
    {
        //Al saltar hay que detener los movs:
        Stopping();
        isJump = true;
        Vector2 moveDir = _move;//Hacemos esto por precaucion de no cambiar la var global: _move
        anim.SetTrigger("Jumping");

        //Si es que SI se esta moviendo el Player:
        if (moveDir != Vector2.zero)
        {
            //Obtenemos la var dir usando los gizmos de la camara, asi:
            Vector3 dir = cam.forward * moveDir.y;
            dir += cam.right * moveDir.x;
            dir.Normalize();//Para obtener solo la direccion de mov y no la magn
            //Ademas queremos que el player solo se mueve en X y Z, no en Y para que no se eleve:
            dir.y = 0;

            Quaternion targetR = Quaternion.LookRotation(dir);
            //Y actualizamos la rotacion actual, para que gire el Player:
            transform.rotation = targetR;

            
            //Generamos el salto como impulso, no como cambio de velocidad.
            //En este caso estaremos agregando la fuerza con un Vector3(0,35,0),
            //con met AddForce(Direccion adelante y arriba * Magnitud, Tipo de fuerza=Impulso):
            rb.AddForce((transform.forward + Vector3.up) * jumpPower, ForceMode.Impulse);

        }
        else
        {
            //Si el Player no se esta moviendo, que solo salte hacia arriba, NO hacia adelante:
            rb.AddForce(Vector3.up * jumpPower, ForceMode.Impulse);
        }

        //Y activamos la anim del player en el aire:
        anim.SetBool("OnAir", true);
    }

    //Met que permita que el personaje se pueda mover de nuevo:
    public void StopEnd()
    {
        anim.SetBool("Move", (_move.x == 0 && _move.y == 0) ? false : true);

        //Lo mismo para la var float Moving del animator:
        //Si los valores de X e Y de -move son 0, el valor de Moving = 0,
        //si no son 0, la var Moving = 1f:
        anim.SetFloat("Moving", (_move.x == 0 && _move.y == 0) ? 0 : 1f);
        //Ademas que los valores float de _move en X e Y,
        //definan el valor de direccion de mov del Player, a las var MoveX y MoveY del animator:
        anim.SetFloat("MoveX", _move.x);
        anim.SetFloat("MoveY", _move.y);
        rb.velocity = Vector3.zero;
        stop = false;
    }

    //Met que rep el fin de la caida y que sera llamado por el met Land(), de la clase PlayerEvents.cs
    public void FallEnd()
    {
        //Que el Player se pueda mover otra vez:
        StopEnd();

    }

    //MEt cam CineMachine:
    public void OnCam(InputValue value)
    {
        //Debemos tener la dir de mov de la camara CineMachine que la determina el Mouse,
        //llamando a una var Vector2:
        m_look = value.Get<Vector2>();
        //Accedemos a la rot en X e Y, de la camCineMach:
        cinemachineFreeLook.m_XAxis.Value += m_look.x * rotationSpeedCamX;
        cinemachineFreeLook.m_YAxis.Value += m_look.y * rotationSpeedCamY * Time.fixedDeltaTime;
        //Para que el mov de la cam en Y sea controlado.
    }
}
