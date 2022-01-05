namespace BUP

module Program =

  open Upcopy
  open Print
  open Parse
  open Parse.Input
  open FsCheck
  open FsCheck.Xunit

  type Tests =

    [<Property>]
    static member ``1. Test that λf.λx.(f x) parses correctly`` () =
      let str = "λf.λx.(f x)"
      let node = Parser(InputOfString str).ReadNode ()
      str = stringOfNode node 

    [<Property>]
    static member ``2. Test that @f = λx.x; (f f) parses correctly`` () =
      let str = "@f = λx.x; (f f))"
      let node = Parser(InputOfString str).ReadNode ()
      "(λx.x λx.x)" = stringOfNode node

    [<Property>]
    static member ``3. Test that (λx.x λy.y) weak-normalises correctly`` () =
      let str = "(λx.x λy.y)"
      let node = Parser(InputOfString str).ReadNode ()
      "λy.y" = stringOfNode (normaliseWeakHead node)

    [<Property>]
    static member ``4. Test that @f = λx.x; (f f) weak-normalises correctly`` () =
      let str = "@f = λx.x; (f f))"
      let node = Parser(InputOfString str).ReadNode ()
      "λx.x" = stringOfNode (normaliseWeakHead node)

    [<Property>]
    static member ``5. Test that @two = λs.λz.(s (s z)); (two two) normalises correctly`` () =
      let str = "@two = λs.λz.(s (s z)); (two two)"
      let node = Parser(InputOfString str).ReadNode ()
      "λs.λz.(s (s (s (s z))))" = stringOfNode (normalise node)

    [<Property>]
    static member ``6. Test that λu.λt.(λx.@f = λy.(x (u y));((x f) f) t) normalises correctly`` () =
      let str = "λu.λt.(λx.@f = λy.(x (u y));((x f) f) t)"
      let node = Parser(InputOfString str).ReadNode ()
      "λu.λt.((t λy.(t (u y))) λy.(t (u y)))" = stringOfNode (normalise node)

  [<EntryPoint>]
  let main _ =
    Check.QuickAll<Tests> ()
    1