namespace BUP

module Program =

  open Parse
  open Print

  let inp = Input.InputOfString "λx.x"
  
  let parser = Parser inp

  [<EntryPoint>]
  let main _ =
    let nd = parser.ReadNode ()
    printNode nd
    1