module App.Guardar
open System
open System.IO
open App.Types
open App.Utils

let rutaArchivo = "partida.txt" // Ahora es un archivo de texto normal

let misilesATexto misiles =
    misiles 
    |> List.map (fun m -> sprintf "%d,%d" m.X m.Y) // convierte cada misil a texto "["x,y"]"
    |> String.concat ";" // une intercalando puntos yc omas "x,y;x,y"

let textoAMisiles (texto: string) =
    if System.String.IsNullOrWhiteSpace(texto) then [] // si el texto está devuelve una lista vacia
    else
        texto.Split([|';'|], System.StringSplitOptions.RemoveEmptyEntries) //  sino, lo convierte a una lista de misiles, separando por ";" 
        |> Array.map (fun par -> // mapea cada par para sacar X y Y 
            let partes = par.Split(',')
            { X = int partes.[0]; Y = int partes.[1] } //convierte cada pedazo a enteros y arma un registrao
        )
        |> Array.toList

// FUNCIÓN PARA GUARDAR LA PARTIDA
let guardarPartida (state: State) =
    try
        // Desarmamos el estado línea por línea
        let lineas = [|
            string state.Tick
            string state.Clock
            string state.Score
            string state.Vidas
            string state.AlienX
            string state.AlienY
            (if state.AlienState = Alive then "Alive" else "Hit")
            string state.EnemigoX
            string state.EnemigoY
            (if state.EnemigoEstado = Alive then "Alive" else "Hit")
            misilesATexto state.Misiles
            misilesATexto state.MisilesEnemigos
        |] // crea un array de strings con cada propiedad del estado, convirtiendo a texto
        
        // Guardamos las líneas directamente en el archivo de texto
        File.WriteAllLines(rutaArchivo, lineas)
        
        // Letrero verde de éxito garantizado
        Console.Clear()
        mostrarMensaje 5 5 System.ConsoleColor.Green "¡PARTIDA GUARDADA CON ÉXITO!"
        System.Threading.Thread.Sleep(1200)
    with
    | _ -> () // Si algo falla lo ignora limpiamente

// 📂 FUNCIÓN PARA CARGAR LA PARTIDA
let cargarPartida () : State option =
    if File.Exists(rutaArchivo) then
        try
            let lineas = File.ReadAllLines(rutaArchivo)
            
            // Reconstruimos el registro State leyendo el archivo línea por línea en el mismo orden
            let estadoCargado = {
                Tick = int lineas.[0]
                Clock = int lineas.[1]
                Score = int lineas.[2]
                Vidas = int lineas.[3]
                AlienX = int lineas.[4]
                AlienY = int lineas.[5]
                AlienState = if lineas.[6] = "Alive" then Alive else Hit
                EnemigoX = int lineas.[7]
                EnemigoY = int lineas.[8]
                EnemigoEstado = if lineas.[9] = "Alive" then Alive else Hit
                Misiles = textoAMisiles lineas.[10]
                MisilesEnemigos = textoAMisiles lineas.[11]
                
                // Forzamos a que al cargar empiece jugando y redibuje la pantalla
                ProgramState = Running
                RedibujarPantalla = true
                ColisionAlien = 0
                ColisionEnemigo = 0
                EnemigoDir = 1 // Dirección inicial rústica por defecto
            }
            Some estadoCargado
        with
        | _ -> None
    else
        None
// esta es una estructura de proteccion, si hay un error el juego atrapa el error y no se cierra de golpe