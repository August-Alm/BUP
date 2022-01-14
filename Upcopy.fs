namespace BUP

module Upcopy =

  open Memory
  open Library

  let cleanupStack = System.Collections.Generic.Stack<Uplink> (pown 2 12)

  let private cleanup () =
    while cleanupStack.Count > 0 do
      let lk = cleanupStack.Pop ()
      let nd = getNode lk
      match getRelation lk with
      | UplinkRel.CHILD ->
        let s = mkSingle nd
        let l = getLeaf s
        iterDLL cleanupStack.Push (getSingleParents s)
        iterDLL cleanupStack.Push (getLeafParents l)
      | _ ->
        let b = mkBranch nd
        let cc = getCache b
        if not (isNil cc) then
          clearCache b
          addToParents (getLChildUplink cc) (getLChild cc)
          addToParents (getRChildUplink cc) (getRChild cc)
          iterDLL cleanupStack.Push (getBranchParents b)

  let private lambdascan (s : Single) =
    iterDLL cleanupStack.Push (getLeafParents (getLeaf s))
    let mutable s = s
    let mutable ch = getChild s
    while getNodeKind ch = NodeKind.SINGLE do
      s <- mkSingle ch
      ch <- getChild s
      iterDLL cleanupStack.Push (getLeafParents (getLeaf s))
    cleanup ()
    
  let private clearCaches (redlam : Single) (topapp : Branch) =
    let topcopy = getCache topapp
    clearCache topapp
    addToParents (getLChildUplink topcopy) (getLChild topcopy)
    addToParents (getRChildUplink topcopy) (getRChild topcopy)
    lambdascan redlam
  
  let private delpar (nd : Node) (lk : Uplink) =
    let lks = getParents nd
    if isLengthOne lks then
      initializeParents nd
    else
      let h = getHead lks
      if h = lk then setHead lks (getNext h)
      unlink lk

  let rec private freeNode (nd : Node) =
    match getNodeKind nd with
    | NodeKind.LEAF -> ()
    | NodeKind.SINGLE ->
      let s = mkSingle nd
      let ch = getChild s
      delpar ch (getChildUplink s)
      if isEmpty (getParents ch) then freeNode ch
      deallocSingle s
    | NodeKind.BRANCH ->
      let b = mkBranch nd
      let lch = getLChild b
      let rch = getRChild b
      delpar lch (getLChildUplink b)
      delpar rch (getRChildUplink b)
      if isEmpty (getParents lch) then freeNode lch
      if isEmpty (getParents rch) then freeNode rch
      deallocBranch b

  let private installChild (nd : Node) (lk : Uplink) =
    match getRelation lk with
    | UplinkRel.CHILD -> setChild (mkSingle (getNode lk)) nd
    | UplinkRel.LCHILD -> setLChild (mkBranch (getNode lk)) nd
    | UplinkRel.RCHILD -> setRChild (mkBranch (getNode lk)) nd
  
  let private installAndLinkChild (nd : Node) (lk : Uplink) =
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
  
  type private UpcopyArg = (struct (Node * Uplink))

  let private upcopyArgs = System.Collections.Generic.Stack<UpcopyArg> (pown 2 12)

  let inline private pusharg x = fun lk -> upcopyArgs.Push (struct (x, lk))

  let inline private newBranch func argm =
    let b = allocBranch ()
    setLChild b func
    setRChild b argm
    mkNode b

  let rec private newSingle oldvar body =
    let varpars = getLeafParents oldvar
    let s = allocSingle ()
    initializeSingle s (getLeafNameId oldvar)
    setChild s body
    addToParents (getChildUplink s) body
    let var = mkNode (getLeaf s)
    iterDLL (pusharg var) varpars
    upcopy ()
    mkNode s
  
  and private upcopy () =
    while upcopyArgs.Count > 0 do
      let struct (newChild, parUplk) = upcopyArgs.Pop ()
      match getRelation parUplk with
      | UplinkRel.CHILD ->
        let s = mkSingle (getNode parUplk)
        let var = getLeaf s
        let nd = newSingle var newChild
        iterDLL (pusharg nd) (getSingleParents s)
      | UplinkRel.LCHILD ->
        let b = mkBranch (getNode parUplk)
        let cc = getCache b
        if isNil cc then
          let nd = newBranch newChild (getRChild b)
          setCache b (mkBranch nd)
          iterDLL (pusharg nd) (getBranchParents b)
        else
          setLChild cc newChild
      | UplinkRel.RCHILD ->
        let b = mkBranch (getNode parUplk)
        let cc = getCache b
        if isNil cc then
          let nd = newBranch (getLChild b) newChild
          setCache b (mkBranch nd)
          iterDLL (pusharg nd) (getBranchParents b)
        else
          setRChild cc newChild

  let private reduce (redex : Branch) =
    let func = mkSingle (getLChild redex)
    let argm = getRChild redex
    let var = getLeaf func
    let body = getChild func
    let lampars = getSingleParents func
    let varpars = getLeafParents var

    if isNil varpars then
      replaceChild body (getBranchParents redex)
      freeNode (mkNode redex)
      body

    elif isLengthOne lampars then
      replaceChild argm varpars
      let answer = getChild func
      replaceChild answer (getBranchParents redex)
      delpar argm (getRChildUplink redex)
      delpar (mkNode func) (getLChildUplink redex)
      deallocBranch redex
      delpar (getChild func) (getChildUplink func)
      deallocSingle func
      answer

    else


      let scandown (nd : Node) =
        let rec go (nd : Node) (g : Single) =
          match getNodeKind nd with
          | NodeKind.LEAF ->
            ev g (argm, mkBranch -1)
          | NodeKind.SINGLE ->
            let s = mkSingle nd
            ev g (go (getChild s) s)
          | NodeKind.BRANCH ->
            let b = mkBranch nd
            let b' = newBranch (getLChild b) (getRChild b) 
            setCache b (mkBranch b')
            iterDLL (pusharg argm) varpars
            upcopy ()
            ev g (b', b)
        and ev g (u, bopt) =
          if isNil g then (u, bopt)
          else (newSingle (getLeaf g) u, bopt)
        go nd (mkSingle -1)

      let (ans, topappOpt) = scandown body

      let answer =
        if isNil topappOpt then ans
        else clearCaches func topappOpt; ans

      replaceChild answer (getBranchParents redex)
      freeNode (mkNode redex)
      answer
  
  let inline private isRedex (nd : Node) : Branch =
    if isNil nd || getNodeKind nd <> NodeKind.BRANCH then
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

  let rec normalise (nd : Node byref) =
    match getNodeKind nd with
    | NodeKind.LEAF -> ()
    | NodeKind.SINGLE ->
      let s = mkSingle nd
      let mutable ch = getChild s
      normalise &ch
    | NodeKind.BRANCH ->
      let b = mkBranch nd
      let mutable lch = getLChild b
      let mutable rch = getRChild b
      normalise &lch
      match getNodeKind lch with
      | NodeKind.SINGLE -> nd <- reduce b; normalise &nd
      | _ -> normalise &rch