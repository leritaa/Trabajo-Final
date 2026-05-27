module App.Utils

open System
open System.Threading

let mostrarMensaje x y color (msg:string) =
    Console.SetCursorPosition(x,y) // mueve el cursos invisible a la coordenada x (columna) y e (fila)
    Console.ForegroundColor <- color //cambia el color de la letra
    msg |> Console.Write // toma el mensaje y lo imprime en esa posicion exacta
let displayMessageRight y color (msg:string) =
    let start = Console.BufferWidth - msg.Length // calcula donde empezar a escribir restando el tamaño del texto al ancho total 
    mostrarMensaje  start y color msg // reutiliza la funcion anterior pasandole esa x calculada (queda alineado a la derecha)




let createMainLoop pipeline Running = //recibe funciones de actualizacion 
    let rec mainLoop state =
        let newState =
            pipeline
            |> Array.fold (fun acc f -> f acc) state //aplica cada funcion al estado, cada una toma el estado de la anterior, lo actualiza y se lo pasa a la siguiente, se obtiene un nuevo estado
        if Running newState then
            Thread.Sleep 25 // duerme para establecer fps al juego
            mainLoop newState
        else
            newState
    mainLoop

