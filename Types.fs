module App.Types

open System

type MotivoFinJuego = 
| PerdióVidas      // Cuando las vidas llegan a 0 
| SalidaVoluntaria // Cuando el jugador decide salir desde el menú o durante el juego

type ProgramState =
| Running
| Paused
| Finished of MotivoFinJuego

type SpriteState = //condiciones del alien y el enemigo
| Alive
| Hit

type Misil = {
    X: int
    Y: int
}
type MenuCommands =
| NewGame
| ContinueGame
| Exit





type State = { // todo lo que hay en el juego
    ProgramState: ProgramState
    AlienX: int
    AlienY: int
    AlienState: SpriteState
    RedibujarPantalla: bool
    Tick: int
    Misiles: Misil list
    EnemigoX: int
    EnemigoY: int
    EnemigoDir: int
    EnemigoEstado: SpriteState
    MisilesEnemigos: Misil list
    ColisionAlien: int
    ColisionEnemigo: int
    Score:int
    Vidas:int
    Clock:int
}

let estadoInicial = { // todo lo que hay en el inicio del juego
    ProgramState = Running
    AlienX = Console.BufferWidth/2
    AlienY = Console.BufferHeight/2
    AlienState = Alive
    RedibujarPantalla = true
    Tick = -1
    Misiles = []
    EnemigoX = Console.BufferWidth-2
    EnemigoY = 0
    EnemigoDir = 1
    EnemigoEstado = Alive
    MisilesEnemigos = []
    ColisionAlien = 0
    ColisionEnemigo = 0
    Score = 0
    Vidas = 3
    Clock = 0
}