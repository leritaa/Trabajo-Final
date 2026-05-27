module App.Pause

open System
open App.Types
open App.Menu

// Tipo para controlar lo que decide el usuario en la pausa
type PauseCommand = 
| ResumeGame
| SaveGame
| BackToMainMenu

let mostrar() =
    // Creamos las opciones transformando los comandos a objetos limpios 
    // para que Menu.mostrar los acepte sin quejarse de los tipos
    let opciones = [|
        (box ResumeGame, "Continuar")
        (box SaveGame, "Guardar")
        (box BackToMainMenu, "Menu Principal")
    |]

    // Le aplicamos unbox al resultado para recuperar nuestro PauseCommand original
    match unbox (Menu.mostrar 15 8 opciones) with
    | ResumeGame -> ResumeGame
    | SaveGame -> SaveGame
    | BackToMainMenu -> BackToMainMenu