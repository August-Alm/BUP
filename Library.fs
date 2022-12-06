namespace BUP

[<AutoOpen>]
module internal Library =

  open Memory

  (* ***** ***** *)

  let inline getRelation (lk : Uplink) =
    toEnum<UplinkRel> (int heap[withoutMeasure lk])
    
  let inline getNext (lk : Uplink) = mkUplink (heap[addOffset lk 1])
  let inline setNext (lk : Uplink) (nxt : Uplink) =
    heap[addOffset lk 1] <- withoutMeasure nxt
  
  let inline getPrevious (lk : Uplink) = mkUplink (heap[addOffset lk 2])
  let inline setPrevious (lk : Uplink) (prv : Uplink) =
    heap[addOffset lk 2] <- withoutMeasure prv
  
  let inline reinitializeUplink (lk : Uplink) =
    setNext lk (mkUplink System.IntPtr.Zero); setPrevious lk (mkUplink System.IntPtr.Zero)

  let inline link (lk1 : Uplink) (lk2 : Uplink) =
    setNext lk1 lk2; setPrevious lk2 lk1

  let unlink (lk : Uplink) =
    let prv = getPrevious lk
    let nxt = getNext lk
    if isNil prv then
      if isNil nxt then ()
      else
        setPrevious nxt (mkUplink System.IntPtr.Zero)
        setNext lk (mkUplink System.IntPtr.Zero)
    else
      if isNil nxt then
        setNext prv (mkUplink System.IntPtr.Zero)
        setPrevious lk (mkUplink System.IntPtr.Zero)
      else
        setPrevious nxt prv
        setNext prv nxt
        reinitializeUplink lk

  let inline getNode (lk : Uplink) =
    match getRelation lk with
    | UplinkRel.CHILD -> mkNode (addOffset lk -4)
    | UplinkRel.LCHILD -> mkNode (addOffset lk -3)
    | UplinkRel.RCHILD -> mkNode (addOffset lk -6)
    

  (* ***** ***** *)

  let inline getHead (lks : UplinkDLL) = mkUplink heap[withoutMeasure lks]
  let inline setHead (lks : UplinkDLL) (h : Uplink) = heap[withoutMeasure lks] <- withoutMeasure h
  
  let inline initializeDLL (lks : UplinkDLL) = setHead lks (mkUplink System.IntPtr.Zero)

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
      let mutable nxt = getNext lk
      while not (isNil lk) do
        a lk; lk <- nxt; nxt <- getNext lk

  (* ***** ***** *)
  
  let inline getLeafId (l : Leaf) = (int heap[withoutMeasure l]) &&& 3

  let inline getLeafNameId (l : Leaf) = (int heap[withoutMeasure l]) >>> 2
  let inline setLeafNameId l nameId =
    heap[withoutMeasure l] <- System.IntPtr ((nameId <<< 2) ||| int NodeKind.LEAF)

  let inline getLeafName (l : Leaf) = getName (getLeafNameId l)
  let inline setLeafName (l : Leaf) (name : string) =
    setLeafNameId l (addName name)

  let inline getLeafParents (l : Leaf) = mkUplinkDLL (addOffset l 1)
  let inline setLeafParents (l : Leaf) (lks : UplinkDLL) =
    heap[addOffset l 1] <- withoutMeasure (getHead lks)
  

  (* ***** ***** *)
  
  let inline getSingleId (s : Single) = int heap[withoutMeasure s]
    
  let inline getLeaf (s : Single) = mkLeaf (addOffset s 1) 
  let inline setLeaf (s : Single) (l : Leaf) =
    let l' = mkLeaf (addOffset s 1) in setLeafParents l' (getLeafParents l)
    
  let inline getChild (s : Single) = mkNode (heap[addOffset s 3])
  let inline setChild (s : Single) (ch : Node) = heap[addOffset s 3] <- withoutMeasure ch
    
  let inline getChildUplink (s : Single) = mkUplink (addOffset s 4)
  
  let inline getSingleParents (s : Single) = mkUplinkDLL (addOffset s 7)
  let inline setSingleParents (s : Single) (lks : UplinkDLL) =
    heap[addOffset s 7] <- withoutMeasure (getHead lks)
    
  let inline initializeSingle (s : Single) (nameId : int) =
    setLeafNameId (getLeaf s) nameId
    

  (* ***** ***** *)

  let inline getBranchId (b : Branch) = heap[withoutMeasure b]
    
  let inline getLChild (b : Branch) = mkNode heap[addOffset b 1]
  let inline setLChild (b : Branch) (lch : Node) = heap[addOffset b 1] <- withoutMeasure lch
  
  let inline getRChild (b : Branch) = mkNode heap[addOffset b 2]
  let inline setRChild (b : Branch) (rch : Node) = heap[addOffset b 2] <- withoutMeasure rch
  
  let inline getLChildUplink (b : Branch) = mkUplink (addOffset b 3)
  
  let inline getRChildUplink (b : Branch) = mkUplink (addOffset b 6)
  
  let inline getBranchParents (b : Branch) = mkUplinkDLL (addOffset b 9)
  let inline setBranchParents (b : Branch) (lks : UplinkDLL) =
    heap[addOffset b 9] <- withoutMeasure (getHead lks)
  
  let inline getCache (b : Branch) = mkBranch (heap[addOffset b 10])
  let inline setCache (b : Branch) (cc : Branch) = heap[addOffset b 10] <- withoutMeasure cc

  let inline clearCache (b : Branch) = setCache b (mkBranch System.IntPtr.Zero)
  

  (* ***** ***** *)

  let inline getNodeKind (nd : Node) = toEnum<NodeKind> ((int heap[withoutMeasure nd]) &&& 3)

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