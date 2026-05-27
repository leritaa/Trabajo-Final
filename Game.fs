module App.Game
open System
open System.Threading
open App.Utils
open App.Types
open App.Menu
open App.Guardar


let actualizarTick state =
    {state with Tick = state.Tick + 1}

let actualizarClock state =
    if state.Tick <> 0 && state.Tick % 40 = 0 then 
        // Suma 1 segundo al reloj de la derecha
        {state with Clock = state.Clock + 1; RedibujarPantalla = true}
    else
        state

let dibujarAlien state =
    let sprite =
        if state.AlienState = Alive then 
            "👽"
        else
            "💥"
    mostrarMensaje state.AlienX state.AlienY ConsoleColor.Yellow sprite

let dibujarEnemigo state =
    let sprite =
        if state.EnemigoEstado = Alive then 
            "👾"
        else
            "💥"
    mostrarMensaje state.EnemigoX state.EnemigoY ConsoleColor.Yellow sprite

let dibujarMisiles state =
    state.Misiles
    |> List.iter ( fun misil ->
        mostrarMensaje misil.X misil.Y ConsoleColor.Yellow "=>" )

let dibujarMisilesEnemigos state =
    state.MisilesEnemigos
    |> List.iter ( fun misil ->
        mostrarMensaje misil.X misil.Y ConsoleColor.Red "<=" )

let dibujarScore state =
    let textoScore = "PUNTAJE: " + string state.Score
    mostrarMensaje 2 1 ConsoleColor.Cyan textoScore

let dibujarVidas state =
    let textoVidas = "VIDAS: " + string state.Vidas
    mostrarMensaje 2 2 ConsoleColor.Red textoVidas

let dibujarReloj state =
    // Tu texto rústico normal
    let textoReloj = ""+ string state.Clock + "s"
    
    // Usamos la función de tu profesor: ella sola calcula la columna,
    // así que solo le pasamos la fila (1), el color y el texto.
    displayMessageRight 1 ConsoleColor.Green textoReloj
let redibujarPantalla state =
    if state.RedibujarPantalla then 
        Console.Clear()
        [|
            dibujarAlien
            dibujarMisiles
            dibujarEnemigo
            dibujarMisilesEnemigos
            dibujarScore
            dibujarVidas
            dibujarReloj
        |]
        |> Array.iter (fun f -> f state)
        {state with RedibujarPantalla=false}
    else
        state


let actualizarMisiles state =
    if state.Misiles <> [] then 
        state.Misiles
        |> Seq.map (fun misil -> {misil with X=misil.X+1})
        |> Seq.filter (fun misil -> misil.X < Console.BufferWidth-2)
        |> Seq.toList
        |> fun nuevosMisiles ->
            {state with Misiles = nuevosMisiles;RedibujarPantalla=true} 
    else
        state

let actualizarMisilesEnemigos state =
    if state.MisilesEnemigos <> [] then 
        state.MisilesEnemigos
        |> Seq.map (fun misil -> {misil with X=misil.X-1})
        |> Seq.filter (fun misil -> misil.X >= 0)
        |> Seq.toList
        |> fun nuevosMisiles ->
            {state with MisilesEnemigos = nuevosMisiles;RedibujarPantalla=true} 
    else
        state

let actualizarDisparoEnemigo state =
    if state.EnemigoEstado = Alive && state.Tick % 10 = 0 then 
        let nuevoMisil = {
            X = state.EnemigoX-2
            Y = state.EnemigoY
        }
        {state with MisilesEnemigos= nuevoMisil :: state.MisilesEnemigos; RedibujarPantalla=true}
    else
        state
let actualizarEnemigo state =
    if state.EnemigoEstado= Alive && state.Tick % 4 = 0 then 
        let nuevaY = state.EnemigoY+state.EnemigoDir
        match nuevaY with 
        | y when y > Console.BufferHeight-1 -> Console.BufferHeight-1,-1
        | y when y < 0 -> 0,1
        | y -> y, state.EnemigoDir
        |> fun (y,dir) ->
            {state with EnemigoY=y;EnemigoDir=dir;RedibujarPantalla=true}
    else
        state


let detectarColisionConAlien state =
    if state.AlienState = Hit || state.Vidas <= 0 then
        state
    else
        state.MisilesEnemigos
        |> List.filter (fun misil -> not (misil.X = state.AlienX+1 && misil.Y = state.AlienY))
        |> fun nuevosMisiles ->
            if nuevosMisiles.Length <> state.MisilesEnemigos.Length then 
                let nuevasVidas = state.Vidas - 1
                // Si le quedan vidas, el juego sigue en 'Running', si no, 'GameOver'
                let proximoEstado = if nuevasVidas <= 0 then Finished PerdióVidas else state.ProgramState
                {state with 
                    AlienState=Hit
                    MisilesEnemigos=nuevosMisiles
                    RedibujarPantalla=true
                    ColisionAlien=state.Tick
                    Vidas = nuevasVidas
                    ProgramState = proximoEstado
                }
            else
                state
let detectarColisionConEnemigo state =
    if state.EnemigoEstado = Alive then
        state.Misiles
        |> List.filter (fun misil -> not (misil.X = state.EnemigoX-1 && misil.Y = state.EnemigoY))
        |> fun nuevosMisiles ->
            if nuevosMisiles.Length <> state.Misiles.Length then 
                {state with 
                    EnemigoEstado=Hit
                    Misiles= nuevosMisiles
                    RedibujarPantalla=true
                    ColisionEnemigo= state.Tick
                    Score = state.Score + 100
                }
            else
                state
    else 
        state

let resetAlien state =
    if state.AlienState = Hit then 
        let tiempo = state.Tick-state.ColisionAlien
        if tiempo >= 160 then 
            {state with AlienState=Alive;RedibujarPantalla=true}
        else
            state
    else
        state

let resetEnemigo state =
    if state.EnemigoEstado = Hit then 
        let tiempo = state.Tick-state.ColisionEnemigo
        if tiempo >= 160 then 
            {state with 
                EnemigoEstado=Alive
                EnemigoX = Console.BufferWidth-2
                EnemigoY = 0
                RedibujarPantalla=true}
        else
            state
    else
        state
let procesarTecladoApp key state =
    match key with 
    | ConsoleKey.Escape ->
        match App.Pause.mostrar() with
        | App.Pause.ResumeGame -> 
            { state with RedibujarPantalla = true }
            
        | App.Pause.SaveGame -> 
            App.Guardar.guardarPartida state
            { state with RedibujarPantalla = true }
            
        | App.Pause.BackToMainMenu -> 
            { state with ProgramState = Finished SalidaVoluntaria }
    | _ -> state
let procesarTecladoAlien key state =
    if state.AlienState = Alive then 
        match key with 
        | ConsoleKey.Spacebar ->
            let nuevoMisil = {
                X = state.AlienX+2
                Y = state.AlienY
            }
            {state with Misiles = nuevoMisil :: state.Misiles}
        | ConsoleKey.UpArrow ->
            {state with AlienY = max 0 (state.AlienY-1)}
        | ConsoleKey.DownArrow ->
            {state with AlienY = min (Console.BufferHeight-1) (state.AlienY+1)}
        | ConsoleKey.LeftArrow ->
            {state with AlienX = max 0 (state.AlienX-1)}
        | ConsoleKey.RightArrow ->
            {state with AlienX = min (Console.BufferWidth-2) (state.AlienX+1)}
        | _ -> state
        |> fun nuevoEstado ->
            if nuevoEstado <> state then 
                {nuevoEstado with RedibujarPantalla=true}
            else
                state
    else
        state

let procesarTeclado state =
    if Console.KeyAvailable then 
        let k = Console.ReadKey true
        state
        |> procesarTecladoApp k.Key
        |> procesarTecladoAlien k.Key
    else
        state


let rec mainLoop state =
    state
    |> actualizarTick
    |> actualizarClock
    |> actualizarMisiles
    |> actualizarEnemigo
    |> actualizarDisparoEnemigo
    |> actualizarMisilesEnemigos
    |> detectarColisionConAlien
    |> detectarColisionConEnemigo
    |> resetAlien
    |> resetEnemigo
    |> procesarTeclado
    |> redibujarPantalla
    |> fun nuevoEstado ->
        if nuevoEstado.ProgramState = Running then
            Thread.Sleep 25
            nuevoEstado |> mainLoop
        else
            nuevoEstado
// la funcion toma el estado actual del juego y lo pasa por cada funcion
// cada funcion toma el estado anterior, lo actualiza y se lo pasa a la siguiente con nueva info
// al final se obtiene un nuevo estado actualizado, si el juego sigue en 'Running', vuelve a llamarse y si cambia el estado el bucle se rompe