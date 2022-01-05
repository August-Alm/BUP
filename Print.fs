namespace BUP

module Print =

  open System.Text

  let inline append (x : string) (sb : StringBuilder) = sb.Append x

  let rec appendNode (nd : Node) (sb : StringBuilder) =
    match getNodeKind nd with
    | NodeKind.LEAF -> appendLeaf (mkLeaf nd) sb
    | NodeKind.SINGLE -> appendSingle (mkSingle nd) sb
    | NodeKind.BRANCH -> appendBranch (mkBranch nd) sb
  
  and appendLeaf (l : Leaf) sb = append (getLeafName l) sb

  and appendSingle (s : Single) sb =
    sb
    |> append "λ"
    |> appendLeaf (getLeaf s)
    |> append "."
    |> appendNode (getChild s)
  
  and appendBranch (b : Branch) sb =
    sb
    |> append "("
    |> appendNode (getLChild b)
    |> append " "
    |> appendNode (getRChild b)
    |> append ")"
  
  let stringOfNode (nd : Node) =
    (appendNode nd (StringBuilder 64)).ToString ()
  
  let printNode (nd : Node) = printfn "%s" (stringOfNode nd)

  let printDLL (lks : UplinkDLL) =
    iterDLL (fun lk -> printNode (getNode lk)) lks