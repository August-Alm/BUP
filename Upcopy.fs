namespace BUP

module Upcopy =

  open Memory
  open Library
  open System.Collections.Generic

  let cleanupStack = FixedStack<UplinkDLL> (pown 2 15)

  let private cleanup () =
    while cleanupStack.Count > 0 do
      iterDLL
        (fun lk ->
          let nd = getNode lk
          match getRelation lk with
          | UplinkRel.CHILD ->
            let s = mkSingle nd
            let l = getLeaf s
            cleanupStack.Push (getSingleParents s)
            cleanupStack.Push (getLeafParents l)
          | _ ->
            let b = mkBranch nd
            let cc = getCache b
            if not (isNil cc) then
              clearCache b
              addToParents (getLChildUplink cc) (getLChild cc)
              addToParents (getRChildUplink cc) (getRChild cc)
              cleanupStack.Push (getBranchParents b))
        (cleanupStack.Pop ())

  let private lambdaScan (s : Single) =
    cleanupStack.Push (getLeafParents (getLeaf s))
    let mutable s = s
    let mutable ch = getChild s
    while getNodeKind ch = NodeKind.SINGLE do
      s <- mkSingle ch
      ch <- getChild s
      cleanupStack.Push (getLeafParents (getLeaf s))
    cleanup ()
    
  let private clearCaches (redlam : Single) (topapp : Branch) =
    let topcopy = getCache topapp
    clearCache topapp
    addToParents (getLChildUplink topcopy) (getLChild topcopy)
    addToParents (getRChildUplink topcopy) (getRChild topcopy)
    lambdaScan redlam
  
  let private delPar (nd : Node) (lk : Uplink) =
    let lks = getParents nd
    if isLengthOne lks then
      initializeParents nd
    else
      let h = getHead lks
      if h = lk then setHead lks (getNext h)
      unlink lk

  let freeStack = FixedStack<Node> (pown 2 15)
  
  let private freeNode (nd : Node) =
    freeStack.Push nd
    while freeStack.Count > 0 do
      let nd = freeStack.Pop ()
      match getNodeKind nd with
      | NodeKind.LEAF -> ()
      | NodeKind.SINGLE ->
        let s = mkSingle nd
        let ch = getChild s
        delPar ch (getChildUplink s)
        if isEmpty (getParents ch) then freeStack.Push ch
        deallocSingle s
      | NodeKind.BRANCH ->
        let b = mkBranch nd
        let lch = getLChild b
        let rch = getRChild b
        delPar lch (getLChildUplink b)
        delPar rch (getRChildUplink b)
        if isEmpty (getParents lch) then freeStack.Push lch
        if isEmpty (getParents rch) then freeStack.Push rch
        deallocBranch b

  let private installChild (nd : Node) (lk : Uplink) =
    match getRelation lk with
    | UplinkRel.CHILD -> setChild (mkSingle (getNode lk)) nd
    | UplinkRel.LCHILD -> setLChild (mkBranch (getNode lk)) nd
    | UplinkRel.RCHILD -> setRChild (mkBranch (getNode lk)) nd
  
  let private replaceChild (newch : Node) (oldpars : UplinkDLL) =
    if not (isEmpty oldpars) then
      let mutable lk = getHead oldpars 
      let mutable nxt = getNext lk
      installChild newch lk
      while not (isNil nxt) do
        lk <- nxt
        nxt <- getNext lk
        installChild newch lk
      let newpars = getParents newch
      if not (isEmpty newpars) then link lk (getHead newpars)
      setHead newpars (getHead oldpars)
      initializeDLL oldpars
  
  let private upcopyArgs = FixedStack<struct (Node * UplinkDLL)> (pown 2 15)

  let inline private pushArg x = fun lks -> upcopyArgs.Push (struct (x, lks))

  let inline private newSingle oldvar body =
    let varpars = getLeafParents oldvar
    let s = allocSingle ()
    initializeSingle s (getLeafNameId oldvar)
    setChild s body
    addToParents (getChildUplink s) body
    let var = mkNode (getLeaf s)
    pushArg var varpars
    mkNode s

  let inline private newBranch func argm =
    let b = allocBranch ()
    setLChild b func
    setRChild b argm
    mkNode b
  
  let private upcopy () =
    while upcopyArgs.Count > 0 do
      let struct (newChild, parUplks) = upcopyArgs.Pop ()
      iterDLL
        (fun parUplk ->
          match getRelation parUplk with
          | UplinkRel.CHILD ->
            let s = mkSingle (getNode parUplk)
            let var = getLeaf s
            let nd = newSingle var newChild
            pushArg nd (getSingleParents s)
          | UplinkRel.LCHILD ->
            let b = mkBranch (getNode parUplk)
            let cc = getCache b
            if isNil cc then
              let nd = newBranch newChild (getRChild b)
              setCache b (mkBranch nd)
              pushArg nd (getBranchParents b)
            else
              setLChild cc newChild
          | UplinkRel.RCHILD ->
            let b = mkBranch (getNode parUplk)
            let cc = getCache b
            if isNil cc then
              let nd = newBranch (getLChild b) newChild
              setCache b (mkBranch nd)
              pushArg nd (getBranchParents b)
            else
              setRChild cc newChild)
        parUplks

  let private singleStack = FixedStack<Single> (pown 2 15)

  let private reduce (redex : Branch) =
    let func = mkSingle (getLChild redex)
    let argm = getRChild redex
    let var = getLeaf func
    let body = getChild func
    let lampars = getSingleParents func
    let varpars = getLeafParents var

    if isEmpty varpars then
      replaceChild body (getBranchParents redex)
      freeNode (mkNode redex)
      body

    elif isLengthOne lampars then
      replaceChild argm varpars
      let answer = getChild func
      replaceChild answer (getBranchParents redex)
      delPar argm (getRChildUplink redex)
      delPar (mkNode func) (getLChildUplink redex)
      deallocBranch redex
      delPar (getChild func) (getChildUplink func)
      deallocSingle func
      answer

    else

      let inline scandown (nd : Node) =
        let inline helper (nd : Node) (knd : NodeKind) =
          if knd = NodeKind.LEAF then
            struct (argm, mkBranch -1)
          else // NodeKind.BRANCH
            let b = mkBranch nd
            let b' = newBranch (getLChild b) (getRChild b) 
            setCache b (mkBranch b')
            pushArg argm varpars
            struct (b', b)
        let mutable struct (deepChild, topapp) =
          match getNodeKind nd with
          | NodeKind.SINGLE ->
            let mutable s = mkSingle nd
            let mutable ch = getChild s
            let mutable k = getNodeKind ch
            singleStack.Push s
            while k = NodeKind.SINGLE do
              s <- mkSingle ch
              ch <- getChild s
              k <- getNodeKind ch
              singleStack.Push s
            helper ch k
          | knd -> helper nd knd
        let mutable g = mkSingle -1
        while singleStack.TryPop &g do
          deepChild <- newSingle (getLeaf g) deepChild
        struct (deepChild, topapp)
        
      let struct (answer, topapp) = scandown body

      upcopy () // Where the action is.

      if not (isNil topapp) then clearCaches func topapp

      replaceChild answer (getBranchParents redex)
      freeNode (mkNode redex)
      answer
  
  let inline private isRedex (nd : Node) : Branch =
    if getNodeKind nd <> NodeKind.BRANCH then
      mkBranch -1
    else
      let b = mkBranch nd
      if getNodeKind (getLChild b) = NodeKind.SINGLE then b
      else mkBranch -1

  let normaliseWeakHead (nd : Node byref) =
    let mutable b = isRedex nd
    while not (isNil b) do
      nd <- reduce b
      b <- isRedex nd

  type private NormState = (struct (bool * Node * Node))

  let private normStates = FixedStack<NormState> (pown 2 26)

  let normaliseIter (node : Node byref) =
    normStates.Push (struct (false, node, node))
    while normStates.Count > 0 do
      let struct (isLCh, root, nd) = normStates.Pop ()
      if not isLCh then
        match getNodeKind nd with
        | NodeKind.LEAF -> node <- root
        | NodeKind.SINGLE ->
          let ch = getChild (mkSingle nd)
          normStates.Push (struct (false, root, ch))
        | NodeKind.BRANCH ->
          let lch = getLChild (mkBranch nd)
          normStates.Push (struct (true, root, nd))
          normStates.Push (struct (false, lch, lch))
      else
        match getNodeKind node with
        | NodeKind.SINGLE ->
          let red = reduce (mkBranch nd)
          if nd = root then
            normStates.Push (struct (false, red, red))
          else
            normStates.Push (struct (false, root, red))
        | _ ->
          let rch = getRChild (mkBranch nd)
          normStates.Push (struct (false, root, rch))

  // This is faster! But can cause stack overflow.
  let rec normaliseRec (nd : Node byref) =
    match getNodeKind nd with
    | NodeKind.LEAF -> ()
    | NodeKind.SINGLE ->
      let mutable ch = getChild (mkSingle nd)
      normaliseRec &ch 
    | NodeKind.BRANCH ->
      let b = mkBranch nd
      let mutable lch = getLChild b
      normaliseRec &lch
      match getNodeKind lch with
      | NodeKind.SINGLE ->
        nd <- reduce b
        normaliseRec &nd
      | _ ->
        let mutable rch = getRChild b
        normaliseRec &rch
  
  let inline normalise (nd : Node byref) = normaliseIter &nd