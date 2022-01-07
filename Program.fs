namespace BUP

module Program =

  open Upcopy
  open Print
  open Parse
  open Parse.Input
  open FsCheck
  open FsCheck.Xunit

  type Tests =
    
    static member ClearEq (b : bool) : bool =
      Memory.clearNames (); Memory.clearHeap (); b

    [<Property>]
    static member ``1. Parsing of λf.λx.(f x)`` () =
      let str = "λf.λx.(f x)"
      let node = Parser(InputOfString str).ReadNode ()
      Tests.ClearEq ("λf.λx.(f x)" = stringOfNode node)

    [<Property>]
    static member ``2. Parsing of @f = λx.x; (f f)`` () =
      let str = "@f = λx.x; (f f))"
      let node = Parser(InputOfString str).ReadNode ()
      Tests.ClearEq ("(λx.x λx.x)" = stringOfNode node)

    [<Property>]
    static member ``3. Weak normalisation of (λx.x λy.y)`` () =
      let str = "(λx.x λy.y)"
      let node = Parser(InputOfString str).ReadNode ()
      Tests.ClearEq ("λy.y" = stringOfNode (normaliseWeakHead node))

    [<Property>]
    static member ``4. Weak normalisation of @f = λx.x; (f f)`` () =
      let str = "@f = λx.x; (f f))"
      let node = Parser(InputOfString str).ReadNode ()
      Tests.ClearEq ("λx.x" = stringOfNode (normaliseWeakHead node))

    [<Property>]
    static member ``5. Normalisation of @mul = λm.λn.λs.λz.((m (n s)) z); @two = λs.λz.(s (s z)); ((mul two) two)`` () =
      let str = "@mul = λm.λn.λs.λz.((m (n s)) z); @two = λs.λz.(s (s z)); ((mul two) two)"
      let node = Parser(InputOfString str).ReadNode ()
      Tests.ClearEq ("λs.λz.(s (s (s (s z))))" = stringOfNode (normalise node))

    //[<Property>]
    //static member ``5. Normalisation of @two = λs.λz.(s (s z)); (two two)`` () =
    //  let str = "@two = λs.λz.(s (s z)); (two two)"
    //  let node = Parser(InputOfString str).ReadNode ()
    //  //Tests.ClearEq ("λs.λz.(s (s (s (s z))))" = stringOfNode (normalise node))
    //  let actual = stringOfNode (normalise node)
    //  printfn "%s" actual
    //  Tests.ClearEq ("λs.λz.(s (s (s (s z))))" = actual)

    [<Property>]
    static member ``6. Normalisation of λu.λt.(λx.@f = λy.(x (u y));((x f) f) t)`` () =
      let str = "λu.λt.(λx.@f = λy.(x (u y));((x f) f) t)"
      let node = Parser(InputOfString str).ReadNode ()
      Tests.ClearEq ("λu.λt.((t λy.(t (u y))) λy.(t (u y)))" = stringOfNode (normalise node))

  [<EntryPoint>]
  let main _ =
    Check.QuickAll<Tests> ()
    1