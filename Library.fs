namespace BUP

[<AutoOpen>]
module internal Library =

  open Memory

  (* ***** ***** *)

  let inline getRelation (lk : Uplink) =
    toEnum<UplinkRel> heap[int lk]
    
  let inline getNext (lk : Uplink) = mkUplink (heap[int lk + 1])
  let inline setNext (lk : Uplink) (nxt : Uplink) =
    heap[int lk + 1] <- int nxt
  
  let inline getPrevious (lk : Uplink) = mkUplink (heap[int lk + 2])
  let inline setPrevious (lk : Uplink) (prv : Uplink) =
    heap[int lk + 2] <- int prv
  
  let inline reinitializeUplink (lk : Uplink) =
    setNext lk (mkUplink -1); setPrevious lk (mkUplink -1)

  let inline link (lk1 : Uplink) (lk2 : Uplink) =
    setNext lk1 lk2; setPrevious lk2 lk1

  let unlink (lk : Uplink) =
    let prv = getPrevious lk
    let nxt = getNext lk
    if isNil prv then
      if isNil nxt then ()
      else
        setPrevious nxt (mkUplink -1)
        setNext lk (mkUplink -1)
    else
      if isNil nxt then
        setNext prv (mkUplink -1)
        setPrevious lk (mkUplink -1)
      else
        setPrevious nxt prv
        setNext prv nxt
        reinitializeUplink lk

  let inline getNode (lk : Uplink) =
    match getRelation lk with
    | UplinkRel.CHILD -> mkNode (int lk - 4)
    | UplinkRel.LCHILD -> mkNode (int lk - 3)
    | UplinkRel.RCHILD -> mkNode (int lk - 6)
    

  (* ***** ***** *)

  let inline getHead (lks : UplinkDLL) = mkUplink heap[int lks]
  let inline setHead (lks : UplinkDLL) (h : Uplink) = heap[int lks] <- int h
  
  let inline initializeDLL (lks : UplinkDLL) = setHead lks (mkUplink -1)

  let inline isEmpty (lks : UplinkDLL) = isNil (getHead lks)
  
  let inline isLengthOne (lks : UplinkDLL) : bool =
    let h = getHead lks
    if isNil h then false
    elif isNil (getNext h) then true
    else false 

  let inline append (lks : UplinkDLL) (lk : Uplink) =
    let h = getHead lks
    if not (isNil h) then link lk h
    setHead lks lk
  
  let inline iterDLL ([<InlineIfLambda>] a : Uplink -> unit) (lks : UplinkDLL) =
    let mutable lk = getHead lks
    if isNil lk then ()
    else
      let mutable nxt = mkUplink -1
      while not (isNil lk) do
        a lk; nxt <- getNext lk; lk <- nxt

  (* ***** ***** *)
  
  let inline getLeafId (l : Leaf) = heap[int l] &&& 3

  let inline getLeafNameId (l : Leaf) = heap[int l] >>> 2
  let inline setLeafNameId l nameId =
    heap[int l] <- ((nameId <<< 2) ||| int NodeKind.LEAF)

  let inline getLeafName (l : Leaf) = getName (getLeafNameId l)
  let inline setLeafName (l : Leaf) (name : string) =
    setLeafNameId l (addName name)

  let inline getLeafParents (l : Leaf) = mkUplinkDLL (int l + 1)
  let inline setLeafParents (l : Leaf) (lks : UplinkDLL) =
    heap[int l + 1] <- int (getHead lks)
  

  (* ***** ***** *)
  
  let inline getSingleId (s : Single) = heap[int s]
    
  let inline getLeaf (s : Single) = mkLeaf (int s + 1) 
  let inline setLeaf (s : Single) (l : Leaf) =
    let l' = mkLeaf (int s + 1) in setLeafParents l' (getLeafParents l)
    
  let inline getChild (s : Single) = mkNode (heap[int s + 3])
  let inline setChild (s : Single) (ch : Node) = heap[int s + 3] <- int ch
    
  let inline getChildUplink (s : Single) = mkUplink (int s + 4)
  
  let inline getSingleParents (s : Single) = mkUplinkDLL (int s + 7)
  let inline setSingleParents (s : Single) (lks : UplinkDLL) =
    heap[int s + 7] <- int (getHead lks)
    
  let inline initializeSingle (s : Single) (nameId : int) =
    setLeafNameId (getLeaf s) nameId
    

  (* ***** ***** *)

  let inline getBranchId (b : Branch) = heap[int b]
    
  let inline getLChild (b : Branch) = mkNode heap[int b + 1]
  let inline setLChild (b : Branch) (lch : Node) = heap[int b + 1] <- int lch
  
  let inline getRChild (b : Branch) = mkNode heap[int b + 2]
  let inline setRChild (b : Branch) (rch : Node) = heap[int b + 2] <- int rch
  
  let inline getLChildUplink (b : Branch) = mkUplink (int b + 3)
  
  let inline getRChildUplink (b : Branch) = mkUplink (int b + 6)
  
  let inline getBranchParents (b : Branch) = mkUplinkDLL (int b + 9)
  let inline setBranchParents (b : Branch) (lks : UplinkDLL) =
    heap[int b + 9] <- int (getHead lks)
  
  let inline getCache (b : Branch) = mkBranch (heap[int b + 10])
  let inline setCache (b : Branch) (cc : Branch) = heap[int b + 10] <- int cc

  let inline clearCache (b : Branch) = setCache b (mkBranch -1)
  

  (* ***** ***** *)

  let inline getNodeKind (nd : Node) = toEnum<NodeKind> (heap[int nd] &&& 3)

  let getParents (nd : Node) : UplinkDLL =
    match getNodeKind nd with
    | NodeKind.LEAF -> getLeafParents (mkLeaf nd)
    | NodeKind.SINGLE -> getSingleParents (mkSingle nd)
    | NodeKind.BRANCH -> getBranchParents (mkBranch nd)
  
  let setParents (nd : Node) (lks : UplinkDLL) =
    match getNodeKind nd with
    | NodeKind.LEAF -> setLeafParents (mkLeaf nd) lks
    | NodeKind.SINGLE -> setSingleParents (mkSingle nd) lks
    | NodeKind.BRANCH -> setBranchParents (mkBranch nd) lks

  let inline initializeParents (nd : Node) = initializeDLL (getParents nd)

  let inline addToParents (lk : Uplink) (nd : Node) =
    append (getParents nd) lk