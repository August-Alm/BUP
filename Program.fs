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
    static member ``5. Normalisation of @mul = λm.λn.λs.(m (n s)); @two = λs.λz.(s (s z)); ((mul two) two)`` () =
      let str = "@mul = λm.λn.λs.(m (n s)); @two = λs.λz.(s (s z)); ((mul two) two)"
      let node = Parser(InputOfString str).ReadNode ()
      Tests.ClearEq ("λs.λz.(s (s (s (s z))))" = stringOfNode (normalise node))

    [<Property>]
    static member ``6. Normalisation of λu.λt.(λx.@f = λy.(x (u y));((x f) f) t)`` () =
      let str = "λu.λt.(λx.@f = λy.(x (u y));((x f) f) t)"
      let node = Parser(InputOfString str).ReadNode ()
      Tests.ClearEq ("λu.λt.((t λy.(t (u y))) λy.(t (u y)))" = stringOfNode (normalise node))

    //[<Property>]
    //static member ``7. Normalisation of Church ((2*5)^2)^2 * (2*5)^2 * 5 = 5M.`` () =
    //  let str =
    //    "@ n2 = λs.λz.(s (s z));
    //     @ n5 = λs.λz.(s (s (s (s (s z)))));
    //     @ mul = λm.λn.λs.(m (n s));
    //     @ n10 = ((mul n2) n5);
    //     @ n100 = ((mul n10) n10);
    //     @ n10k = ((mul n100) n100);
    //     @ n1M = ((mul n10k) n100); 
    //     @ n5M = ((mul n1M) n5);
    //     n5M"
    //  let mutable node = Parser(InputOfString str).ReadNode ()
    //  let t = System.Diagnostics.Stopwatch ()
    //  t.Start ()
    //  normaliseMut &node
    //  t.Stop ()
    //  printfn "Normalised in %A ms." t.ElapsedMilliseconds
    //  Tests.ClearEq true

    [<Property>]
    static member ``8. Normalisation of chain of 20 pearls.`` () =
      let sb = System.Text.StringBuilder 400
      sb.Append "@ p0 = λx.x;\n" |> ignore
      for i = 0 to 19 do sb.Append $"@ p{i + 1} = (p{i} p{i});\n" |> ignore
      sb.Append "p20" |> ignore

    [<Property>]
    static member ``9. Normalisation of factorial of eight.`` () =
      let str =
       "@ one = λs.λz.(s z);
        @ oneone = λg.((g one) one);
        @ snd = λa.λb.b;
        @ F = λp.(p λa.λb.λg.((g λs.λz.(s ((a s) z))) λs.(a (b s))));
        @ fact = λk.(((k F) oneone) snd);
        @ eight = λs.λz.(s (s (s (s (s (s (s (s z))))))));
        (fact eight)"
      let mutable node = Parser(InputOfString str).ReadNode ()
      let t = System.Diagnostics.Stopwatch ()
      t.Start ()
      normaliseMut &node
      t.Stop ()
      printfn "Normalised in %A ms." t.ElapsedMilliseconds
      Tests.ClearEq true

  [<EntryPoint>]
  let main _ =
    Check.QuickAll<Tests> ()
    1