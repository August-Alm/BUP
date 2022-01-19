namespace BUP

module Program =

  open Upcopy
  open Print
  open Parse
  open Parse.Input
  open FsCheck
  open FsCheck.Xunit
  open BenchmarkDotNet.Attributes
  open BenchmarkDotNet.Running

  let private churchToInt (nd : Node) : int =
    let rec loop sId zId acc (nd : Node) =
      match getNodeKind nd with
      | NodeKind.LEAF ->
        if getLeafId (mkLeaf nd) = zId then acc
        else failwith "Not a Church nat."
      | NodeKind.BRANCH ->
        let lch = getLChild (mkBranch nd)
        match getNodeKind lch with
        | NodeKind.LEAF ->
          if getLeafId (mkLeaf lch) = sId then
            loop sId zId (acc + 1) (getRChild (mkBranch nd))
          else failwith "Not a Church nat."
        | _ -> failwith "Not a Church nat."
      | _ -> failwith "Not a Church nat."

    match getNodeKind nd with
    | NodeKind.SINGLE ->
      let sId = getLeafId (getLeaf (mkSingle nd))
      let ch = getChild (mkSingle nd)
      match getNodeKind ch with
      | NodeKind.SINGLE ->
        let zId = getLeafId (getLeaf (mkSingle ch))
        loop sId zId 0 (getChild (mkSingle ch))
      | _ -> failwith "Not a Church nat."
    | _ -> failwith "Not a Church nat."


  type Tests =
  
    static member private ClearEq (b : bool) : bool =
      Memory.clearHeap (); b

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
      let mutable node = Parser(InputOfString str).ReadNode ()
      normaliseWeakHead &node
      Tests.ClearEq ("λy.y" = stringOfNode node)

    [<Property>]
    static member ``4. Weak normalisation of @f = λx.x; (f f)`` () =
      let str = "@f = λx.x; (f f))"
      let mutable node = Parser(InputOfString str).ReadNode ()
      normaliseWeakHead &node
      Tests.ClearEq ("λx.x" = stringOfNode node)

    [<Property>]
    static member ``5. Normalisation of @two = λs.λz.(s (s z)); (two two).`` () =
      let str = "@two = λs.λz.(s (s z)); (two two)"
      let mutable node = Parser(InputOfString str).ReadNode ()
      normalise &node
      Tests.ClearEq (churchToInt node = 4)

    [<Property>]
    static member ``6. Normalisation of λu.λt.(λx.@f = λy.(x (u y));((x f) f) t)`` () =
      let str = "λu.λt.(λx.@f = λy.(x (u y));((x f) f) t)"
      let mutable node = Parser(InputOfString str).ReadNode ()
      normalise &node
      Tests.ClearEq ("λu.λt.((t λy.(t (u y))) λy.(t (u y)))" = stringOfNode node)

    [<Property>]
    static member ``7. Normalisation of Church ((2*5)^2)^2 * (2*5)^2 = 1M.`` () =
      let str =
        "@ n2 = λs.λz.(s (s z));
         @ n5 = λs.λz.(s (s (s (s (s z)))));
         @ mul = λm.λn.λs.(m (n s));
         @ n10 = ((mul n2) n5);
         @ n100 = ((mul n10) n10);
         @ n10k = ((mul n100) n100);
         @ n1M = ((mul n10k) n100); 
         n1M"
      let mutable node = Parser(InputOfString str).ReadNode ()
      normalise &node
      Tests.ClearEq (churchToInt node = 1000000)

    [<Property>]
    static member ``8. Normalisation of chain of 100 pearls.`` () =
      let sb = System.Text.StringBuilder 2000
      sb.Append "@ p0 = λx.x;\n" |> ignore
      for i = 0 to 99 do sb.Append $"@ p{i + 1} = (p{i} p{i});\n" |> ignore
      sb.Append "p100" |> ignore
      let str = sb.ToString ()
      let mutable node = Parser(InputOfString str).ReadNode ()
      normalise &node
      Tests.ClearEq ("λx.x" = stringOfNode node)

    [<Property>]
    static member ``9. Normalisation of factorial of eight.`` () =
      let str =
       "@ one = λs.λz.(s z);
        @ one_one = λg.((g one) one);
        @ snd = λa.λb.b;
        @ F = λp.(p λa.λb.λg.((g λs.λz.(s ((a s) z))) λs.(a (b s))));
        @ fact = λk.(((k F) one_one) snd);
        @ eight = λs.λz.(s (s (s (s (s (s (s (s z))))))));
        (fact eight)"
      let mutable node = Parser(InputOfString str).ReadNode ()
      normalise &node
      Tests.ClearEq (churchToInt node = 40320)
    

  type Benchmarks () =

    let str50k =
      "@ n2 = λs.λz.(s (s z));
       @ n5 = λs.λz.(s (s (s (s (s z)))));
       @ mul = λm.λn.λs.(m (n s));
       @ n10 = ((mul n2) n5);
       @ n100 = ((mul n10) n10);
       @ n10k = ((mul n100) n100);
       @ n50k = ((mul n10k) n5); 
       n50k"
    
    let mutable nod50k = Parser(InputOfString str50k).ReadNode ()

    let strTree15 =
     let rec loop seed n =
      if n = 0 then seed
      else let s = loop seed (n - 1) in $"({s} {s})" 
     loop "λx.x" 15

    let strFact8 =
     "@ one = λs.λz.(s z);
      @ one_one = λg.((g one) one);
      @ snd = λa.λb.b;
      @ F = λp.(p λa.λb.λg.((g λs.λz.(s ((a s) z))) λs.(a (b s))));
      @ fact = λk.(((k F) one_one) snd);
      @ eight = λs.λz.(s (s (s (s (s (s (s (s z))))))));
      (fact eight)"

    let str1M =
      "@ n2 = λs.λz.(s (s z));
       @ n5 = λs.λz.(s (s (s (s (s z)))));
       @ mul = λm.λn.λs.(m (n s));
       @ n10 = ((mul n2) n5);
       @ n100 = ((mul n10) n10);
       @ n10k = ((mul n100) n100);
       @ n1M = ((mul n10k) n100); 
       n1M"
    
    // 3^(2^4) - 3^(2^4)
    let strArithm =
     "@ n2 = λs.λz.(s (s z));
      @ n3 = λs.λz.(s (s (s z)));
      @ n4 = λs.λz.(s (s (s (s z))));
      @ big = ((n2 n2) n2);
      @ pred = λn.λs.λz.(((n λg.λh.(h (g s))) λu.z) λu.u);
      @ sub = λm.λn.((n pred) m);
      ((sub big) big)"
     
    let strPearls100 =
      let sb = System.Text.StringBuilder 2000
      sb.Append "@ p0 = λx.x;\n" |> ignore
      for i = 0 to 99 do sb.Append $"@ p{i + 1} = (p{i} p{i});\n" |> ignore
      sb.Append "p100" |> ignore
      sb.ToString ()

    [<Benchmark>]
    member _.ParseAndNormalise50k () =
      let mutable node = Parser(InputOfString str50k).ReadNode ()
      normalise &node

    [<Benchmark>]
    member _.Normalise50k () =  normalise &nod50k

    [<Benchmark>]
    member _.ParseAndNormaliseTree15 () =
      let mutable nodeTree15 = Parser(InputOfString strTree15).ReadNode ()
      normalise &nodeTree15

    [<Benchmark>]
    member _.ParseAndNormaliseFact8 () =
      let mutable nodeFact8 = Parser(InputOfString strFact8).ReadNode ()
      normalise &nodeFact8
    
    [<Benchmark>]
    member _.ParseAndNormalise1M () =
      let mutable node1M = Parser(InputOfString str1M).ReadNode ()
      normalise &node1M

    [<Benchmark>]
    member _.ParseAndNormalisePearls100 () =
      let mutable nodePearls100 = Parser(InputOfString strPearls100).ReadNode ()
      normalise &nodePearls100

    //[<Benchmark>]
    //member _.ParseAndNormaliseArithmetic () =
    //  let mutable nodeArithm = Parser(InputOfString strArithm).ReadNode ()
    //  normalise &nodeArithm

    [<IterationCleanup>]
    member _.Cleanup () = Memory.clearHeap ()

  [<EntryPoint>]
  let main _ =
    let str = "@ two = λf.λx.(f (f x)); (two two)"
    let mutable nd = Parser(InputOfString str).ReadNode ()
    Memory.mytest ()
    Memory.clearHeap ()
    Check.All<Tests> (Config.Quick.WithMaxTest 1)
    BenchmarkRunner.Run<Benchmarks> () |> ignore
    //BenchmarkRunner.Run<Hoas.Benchmarks> () |> ignore
    1