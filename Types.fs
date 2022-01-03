namespace BUP

[<AutoOpen>]
module Types =

  let inline toEnum<'a when 'a : enum<int>> (x) : 'a =
    LanguagePrimitives.EnumOfValue x
  
  let inline withMeasure<[<Measure>]'m> (x) : int<'m> =
    LanguagePrimitives.Int32WithMeasure x

  let inline isNil x = (int x = -1)

  [<Measure>] type nodePtr
  [<Measure>] type uplinkPtr
  [<Measure>] type uplinkPtrPtr
  [<Measure>] type leafPtr
  [<Measure>] type singlePtr
  [<Measure>] type branchPtr


  (* ***** ***** ***** *)

  [<AbstractClass; AllowNullLiteral>]
  type Heap () =
    abstract Item : int -> int with get, set
  
  let mutable private heap : Heap = null
  
  let initializeHeap (hp : Heap) = heap <- hp
  

  (* ***** ***** ***** *)

  type NodeKind =
    | LEAF = 0
    | SINGLE = 1
    | BRANCH = 2
  
  
  type Node = int<nodePtr>

  let inline mkNode x = withMeasure<nodePtr> (int x)

  let getNodeKind (nd : Node) = toEnum<NodeKind> (heap[int nd] &&& 3)

  let initializeNode (nd : Node) = heap[int nd] <- -1


  (* ***** ***** ***** *)

  type UplinkRel =
    | CHILD = 0
    | RCHILD = 1
    | LCHILD = 2
  
  
  type Uplink = int<uplinkPtr>

  let inline mkUplink x = withMeasure<uplinkPtr> (int x)

  let getNext (lk : Uplink) = mkUplink (heap[int lk])
  let setNext (lk : Uplink) (nxt : Uplink) = heap[int lk] <- int nxt
  
  let getPrevious (lk : Uplink) = mkUplink (heap[int lk + 1])
  let setPrevious (lk : Uplink) (prv : Uplink) = heap[int lk + 1] <- int prv
  
  let getRelation (lk : Uplink) = toEnum<UplinkRel> heap[int lk + 2]
  let setRelation (lk : Uplink) (rel : UplinkRel) = heap[int lk + 2] <- int rel
    
  let setUplink (lk : Uplink) (nxt, prv, rel) =
    setNext lk nxt; setPrevious lk prv; setRelation lk rel
    
  let initializeUplink (lk : Uplink) (rel) =
    heap[int lk] <- -1; heap[int lk + 1] <- -1; heap[int lk + 2] <- int rel

  let reinitializeUplink (lk : Uplink) = initializeUplink lk (getRelation lk)

  let mkFirst (lk : Uplink) = heap[int lk + 1] <- -1

  let mkLast (lk : Uplink) = heap[int lk] <- -1

  let link (lk1 : Uplink) (lk2 : Uplink) =
    setNext lk1 lk2; setPrevious lk2 lk1

  let unlink (lk : Uplink) =
    let prv = getPrevious lk
    let nxt = getNext lk
    link prv nxt
    reinitializeUplink lk
  
  let inline getNode (lk : Uplink) =
      match getRelation lk with
      | UplinkRel.CHILD -> mkNode (int lk - 3)
      | UplinkRel.LCHILD -> mkNode (int lk - 3)
      | UplinkRel.RCHILD -> mkNode (int lk - 6)
    

  (* ***** ***** ***** *)

  type UplinkDLL = int<uplinkPtrPtr>

  let inline mkUplinkDLL x = withMeasure<uplinkPtrPtr> (int x)
  
  let getHead (lks : UplinkDLL) = mkUplink heap[int lks]
  let setHead (lks : UplinkDLL) (h : Uplink) = heap[int lks] <- int h
  
  let initializeDLL (lks : UplinkDLL) = heap[int lks] <- -1

  let isEmpty (lks : UplinkDLL) = (heap[int lks] = -1)
  
  let isLengthOne (lks : UplinkDLL) : bool =
    let h = getHead lks
    if isNil h then false
    elif isNil (getNext h) then true
    else false

  let prepend (lk : Uplink) (lks : UplinkDLL) =
    if not (isEmpty lks) then link lk (getHead lks)
    setHead lks lk
  
  let iterDLL (a : Uplink -> unit) (lks : UplinkDLL) =
    let rec loop (u : Uplink) =
      let nxt = getNext u
      if isNil nxt then a u else a u; loop nxt
    if isEmpty lks then () else loop (getHead lks)


  (* ***** ***** ***** *)
  
  type Leaf = int<leafPtr>

  let inline mkLeaf x = withMeasure<leafPtr> (int x)

  let getLeafId (l : Leaf) = heap[int l]
  let setLeafId (l : Leaf) id = heap[int l] <- id
  
  let getLeafParents (l : Leaf) = mkUplinkDLL (int l + 1)
  let setLeafParents (l : Leaf) (lks : UplinkDLL) = heap[int l + 1] <- int (getHead lks)
  
  let initializeLeaf (l : Leaf) =
    setLeafId l ((int l <<< 2) ||| int NodeKind.LEAF)
    initializeDLL (getLeafParents l)
  

  (* ***** ***** ***** *)
  
  type Single = int<singlePtr>

  let inline mkSingle x = withMeasure<singlePtr> (int x)

  let getSingleId (s : Single) = heap[int s]
  let setSingleId (s : Single) id = heap[int s] <- id
    
  let getLeaf (s : Single) = mkLeaf (int s + 1) 
  let setLeaf (s : Single) (l : Leaf) =
    let l' = mkLeaf (int s + 1) in setLeafParents l' (getLeafParents l)
    
  let getChild (s : Single) = mkNode (heap[int s + 3])
  let setChild (s : Single) (ch : Node) = heap[int s + 3] <- int ch
    
  let getChildUplink (s : Single) = mkUplink (int s + 4)
  
  let getSingleParents (s : Single) = mkUplinkDLL (int s + 7)
  let setSingleParents (s : Single) (lks : UplinkDLL) = heap[int s + 7] <- int (getHead lks)
    
  let initializeSingle (s : Single) =
    setSingleId s ((int s <<< 2) ||| int NodeKind.SINGLE)
    initializeLeaf (getLeaf s)
    initializeNode (getChild s)
    initializeDLL (getSingleParents s)
    initializeUplink (getChildUplink s) UplinkRel.CHILD
    

  (* ***** ***** ***** *)

  type Branch = int<branchPtr>

  let inline mkBranch x = withMeasure<branchPtr> (int x)
  
  let getBranchId (b : Branch) = heap[int b]
  let setBranchId (b : Branch) id = heap[int b] <- id
    
  let getLChild (b : Branch) = mkNode heap[int b + 1]
  let setLChild (b : Branch) (lch : Node) = heap[int b + 1] <- int lch
  
  let getRChild (b : Branch) = mkNode heap[int b + 2]
  let setRChild (b : Branch) (rch : Node) = heap[int b + 2] <- int rch

  
  let getLChildUplink (b : Branch) = mkUplink (int b + 3)
  
  let getRChildUplink (b : Branch) = mkUplink (int b + 6)
  
  let getBranchParents (b : Branch) = mkUplinkDLL (int b + 7)
  let setBranchParents (b : Branch) (lks : UplinkDLL) = heap[int b + 7] <- int (getHead lks)
  
  let getCache (b : Branch) = mkBranch (int b + 10)
  let setCache (b : Branch) (cc : Branch) = heap[int b + 10] <- int cc
  let clearCache (b : Branch) = heap[int b + 10] <- -1
  
  let initializeBranch (b : Branch) =
    setBranchId b ((int b <<< 2) ||| int NodeKind.BRANCH)
    initializeNode (getLChild b)
    initializeNode (getRChild b)
    initializeUplink (getLChildUplink b) UplinkRel.LCHILD
    initializeUplink (getRChildUplink b) UplinkRel.RCHILD
    initializeDLL (getBranchParents b)
    clearCache b

  (* ***** ***** ***** *)

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

  let addToParents (lk : Uplink) (nd : Node) =
    prepend lk (getParents nd)