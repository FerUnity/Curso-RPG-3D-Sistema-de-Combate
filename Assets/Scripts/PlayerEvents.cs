using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerEvents : MonoBehaviour
{
    // Script que estara en el GO que posee al animator,
    // o sea en el GO RPG-Character hijo y que controlara los eventos del animator:
    //Doble click en la animacion/Events

    //Referenciamos a la clase PlayerMotion.cs que esta en el GO Padre de donde esta este Script:
    PlayerMotion playerMotion;//Lo asignamos en met Awake()

    private void Awake()
    {
        //Asigmamos la var creada a la clase PlayerMotion.cs :
        playerMotion = GetComponentInParent<PlayerMotion>();
    }

    //Creamos met que se llamara en el evento de animacion de FIN DE CAIDA,
    //creado en la animacion Unarmed-Land.
    //Este met llamara al met FallEnd() de la clase PlayerMotion.cs:
    public void Land()
    {
        playerMotion.FallEnd();
    }

}
