namespace BUP

module Upcopy =

  open Heaps

  let mutable private heap : MemoryHeap = null

  let initializeMemory (hp : MemoryHeap) = heap <- hp

  let rec private cleanUp (lk : Uplink) =
    let nd = getNode lk
    match getRelation lk with
    | UplinkRel.CHILD ->
      let s = mkSingle nd
      let l = getLeaf s
      iterDLL cleanUp (getLeafParents l)
      iterDLL cleanUp (getSingleParents s)
    | _ ->
      let b = mkBranch nd
      let cc = getCache b
      if isNil cc then ()
      else
        clearCache b
        addToParents (getLChildUplink cc) (getLChild cc)
        addToParents (getRChildUplink cc) (getRChild cc)
        iterDLL cleanUp (getBranchParents b)

  let rec private lambdaScan (s : Single) =
    iterDLL cleanUp (getLeafParents (getLeaf s))
    let ch = getChild s
    match getNodeKind ch with
    | NodeKind.SINGLE -> lambdaScan (mkSingle ch)
    | _ -> ()
    
  let private clearCaches (redlam : Single) (topapp : Branch) =
    let topcopy = getCache topapp
    clearCache topapp
    addToParents (getLChildUplink topcopy) (getLChild topcopy)
    addToParents (getRChildUplink topcopy) (getRChild topcopy)
    lambdaScan redlam
  
  let private delPar (nd : Node) (lk : Uplink) =
    let lks = getParents nd
    if getHead lks = lk then
      let nxt = getNext lk
      reinitializeUplink lk
      mkFirst nxt
      setHead lks nxt
    else
      unlink lk

  let private freeNode (nd : Node) =
    match getNodeKind nd with
    | NodeKind.LEAF -> ()
    | NodeKind.SINGLE ->
      let s = mkSingle nd
      delPar (getChild s) (getChildUplink s)
    | NodeKind.BRANCH ->
      let b = mkBranch nd
      delPar (getLChild b) (getLChildUplink b)
      delPar (getRChild b) (getRChildUplink b)

  let private installChild (nd : Node) (lk : Uplink) =
    match getRelation lk with
    | UplinkRel.CHILD -> setChild (mkSingle (getNode lk)) nd
    | UplinkRel.LCHILD -> setLChild (mkBranch (getNode lk)) nd
    | UplinkRel.RCHILD -> setRChild (mkBranch (getNode lk)) nd
  
  let private replaceChild (nd : Node) (lks : UplinkDLL) =
    let mutable lk = getHead lks
    if int lk = -1 then ()
    else
      let mutable lkN = getNext lk
      while int lkN <> -1 do
        installChild nd lk
        lk <- lkN
        lkN <- getNext lk
      installChild nd lk
      let h = getHead (getParents nd)
      link lk h
      setParents nd lks
    
  let rec private newSingle oldvar body =
    let varpars = getLeafParents oldvar
    let s = heap.AllocSingle ()
    setChild s body
    addToParents (getChildUplink s) body
    let var = mkNode (getLeaf s)
    iterDLL (upcopy var) varpars
    mkNode s
  
  and private newBranch func argm =
    let b = heap.AllocBranch ()
    setLChild b func
    setRChild b argm
    mkNode b
  
  and private upcopy newChild parUplk =
    match getRelation parUplk with
    | UplinkRel.CHILD ->
      let s = mkSingle (getNode parUplk)
      let var = getLeaf s
      let nd = newSingle var newChild
      iterDLL (upcopy nd) (getSingleParents s)
    | UplinkRel.LCHILD ->
      let b = mkBranch (getNode parUplk)
      let cc = getCache b
      if isNil cc then
        let nd = newBranch newChild (getRChild b)
        setCache b (mkBranch nd)
        iterDLL (upcopy nd) (getBranchParents b)
      else
        setLChild cc newChild
    | UplinkRel.RCHILD ->
      let b = mkBranch (getNode parUplk)
      let cc = getCache b
      if isNil cc then
        let nd = newBranch (getLChild b) newChild
        setCache b (mkBranch nd)
        iterDLL (upcopy nd) (getBranchParents b)
      else
        setRChild cc newChild

  let rec reduce (redex : Branch) =
    let func = getLChild redex
    let argm = getRChild redex
    if getNodeKind func <> NodeKind.SINGLE then
      failwith "Not a redex!"
    else
      let func = mkSingle func
      let var = getLeaf func
      let body = getChild func
      let lampars = getSingleParents func
      let varpars = getLeafParents var

      let answer =

        if isLengthOne lampars then
          replaceChild argm varpars
          body
        
        elif isNil varpars then
          body

        else

          let rec scandown (nd : Node) =
            match getNodeKind nd with
            | NodeKind.LEAF -> struct (argm, ValueNone)
            | NodeKind.SINGLE ->
              let s = mkSingle nd
              let struct (body', topapp) = scandown (getChild s)
              let func' = newSingle (getLeaf s) body'
              struct (func', topapp)
            | NodeKind.BRANCH ->
              let b = mkBranch nd
              let b' = newBranch (getLChild b) (getRChild b) 
              setCache b (mkBranch b')
              iterDLL (upcopy argm) varpars
              struct (b', ValueSome b)
          
          let struct (ans, topappOpt) = scandown body

          match topappOpt with
          | ValueNone -> ans
          | ValueSome app -> clearCaches func app; ans
      
      replaceChild answer (getBranchParents redex)
      freeNode (mkNode redex)
      answer

  let rec normaliseWeakHead (nd : Node) =
    match getNodeKind nd with
    | NodeKind.BRANCH ->
      let b = mkBranch nd
      let func = getLChild b
      normaliseWeakHead func
      match getNodeKind func with
      | NodeKind.SINGLE -> normaliseWeakHead (reduce b)
      | _ -> ()
    | _ -> ()
  
  let rec normalise (nd : Node) =
    match getNodeKind nd with
    | NodeKind.BRANCH ->
      let b = mkBranch nd
      let func = getLChild b
      normaliseWeakHead func
      match getNodeKind func with
      | NodeKind.SINGLE -> normalise (reduce b)
      | NodeKind.LEAF -> normalise (getRChild b)
      | NodeKind.BRANCH ->
        normalise func; normalise (getRChild b)
    | NodeKind.SINGLE ->
      let s = mkSingle nd
      normalise (getChild s)
    | _ -> ()