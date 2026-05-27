module App.Router
open App.Utils
open App.Menu   
open App.Types
open App.Game
open System

type RouterState =
| ShowingMenu
| ShowingAlienGame  // El estado del juego
| Terminated

let initialState = ShowingMenu

let dibujarPantallaGameOver () =
    Console.Clear()
    mostrarMensaje 15 4 ConsoleColor.Red "💥💥 ══════════════════════ 💥💥"
    mostrarMensaje 22 5 ConsoleColor.Red "G A M E   O V E R"
    mostrarMensaje 15 6 ConsoleColor.Red "💥💥 ══════════════════════ 💥💥"
    mostrarMensaje 12 8 ConsoleColor.White "Presiona cualquier tecla para continuar..."
    Console.ReadKey(true) |> ignore
let rec mainLoop state =
    match state with 
    | ShowingMenu ->
        let opciones = [|
            (box NewGame, "Iniciar Juego") // box sirve para convertir un valor a tipo obj, necesario para el menú genérico
            (box ContinueGame, "Continuar") 
            (box Exit, "Salir")
        |]
        dibujarTituloMenu()

        match unbox (Menu.mostrar 10 7 opciones) with // aca utilizamos unbox para recuperar el comando original del tipo MenuCommands
        | NewGame -> 
            Game.mainLoop App.Types.estadoInicial |> ignore
            ShowingMenu
            
        | ContinueGame -> 
            // Intentamos buscar un archivo guardado
            match App.Guardar.cargarPartida() with
            | Some estadoCargado ->
                // Si hay partida, corre el juego con esos datos guardados y luego vuelve al menú
                Game.mainLoop estadoCargado |> ignore
                ShowingMenu
            | None ->
                // Si no hay archivo guardado, arranca una limpia desde cero
                Game.mainLoop App.Types.estadoInicial |> ignore
                ShowingMenu
                
        | Exit -> 
            Terminated
        
    | ShowingAlienGame ->  
        let estadoFinal = Game.mainLoop App.Types.estadoInicial
        
        // 2. Revisamos con qué motivo terminó la partida
        match estadoFinal.ProgramState with
        | Finished PerdióVidas -> 
            dibujarPantallaGameOver()
            ShowingMenu

        | Finished SalidaVoluntaria ->
            Console.Clear()
            ShowingMenu
        |_ -> 
            Console.Clear()
            ShowingMenu
        
    | Terminated ->
        Terminated
    |> fun s ->
        if s <> Terminated then
            mainLoop s
    //evalúa el estado de navegación de salida de la pantalla anterior. 
    //Si el estado es diferente de Terminated, la función se invoca recursivamente a sí misma pasándose el nuevo estado.
    //Si el usuario seleccionó 'Salir' (Terminated), la recursividad se detiene, la función finaliza y el programa termina.
let mostrar() =
    initialState
    |> mainLoop