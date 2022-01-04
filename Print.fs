namespace BUP

module Print =

  let rec nodeToString (nd : Node) =
    match getNodeKind nd with
    | NodeKind.LEAF -> leafToString (mkLeaf nd)
    | NodeKind.SINGLE -> singleToString (mkSingle nd)
    | NodeKind.BRANCH -> branchToString (mkBranch nd)

  and leafToString (l : Leaf) =
    $"x_{getLeafId l}"

  and singleToString (s : Single) =
    let var = getLeaf s
    let body = getChild s
    $"lam {leafToString var}.{nodeToString body}"
  
  and branchToString (b : Branch) =
    let func = getLChild b
    let argm = getRChild b
    $"({nodeToString func} {nodeToString argm}"