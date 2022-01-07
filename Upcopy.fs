namespace BUP

module Upcopy =

  open Memory
  open Library

  let rec private cleanUp (lk : Uplink) =
    let nd = getNode lk
    match getRelation lk with
    | UplinkRel.CHILD ->
      let s = mkSingle nd
      let l = getLeaf s
      iterDLL (fun lk -> cleanUp lk) (getLeafParents l)
      iterDLL (fun lk -> cleanUp lk) (getSingleParents s)
    | UplinkRel.LCHILD | UplinkRel.RCHILD ->
      let b = mkBranch nd
      let cc = getCache b
      if isNil cc then ()
      else
        clearCache b
        addToParents (getLChildUplink cc) (getLChild cc)
        addToParents (getRChildUplink cc) (getRChild cc)
        iterDLL (fun lk -> cleanUp lk) (getBranchParents b)

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
      delPar ch (getChildUplink s)
      if isEmpty (getParents ch) then freeNode ch
      deallocSingle s
    | NodeKind.BRANCH ->
      let b = mkBranch nd
      let lch = getLChild b
      let rch = getRChild b
      delPar lch (getLChildUplink b)
      delPar rch (getRChildUplink b)
      if isEmpty (getParents lch) then freeNode lch
      if isEmpty (getParents rch) then freeNode rch
      deallocBranch b

  let private installChild (nd : Node) (lk : Uplink) =
    match getRelation lk with
    | UplinkRel.CHILD -> setChild (mkSingle (getNode lk)) nd
    | UplinkRel.LCHILD -> setLChild (mkBranch (getNode lk)) nd
    | UplinkRel.RCHILD -> setRChild (mkBranch (getNode lk)) nd
  
  let private replaceChild (newch : Node) (oldch : Node) =
    let oldpars = getParents oldch
    if not (isEmpty oldpars) then
      let mutable lk = getHead oldpars 
      let mutable nxt = getNext lk
      installChild newch lk
      while not (isNil nxt) do
        lk <- nxt
        nxt <- getNext lk
        installChild newch lk
      let newpars = getParents newch
      if not (isEmpty newpars) then
        link lk (getHead newpars)
        setParents newch oldpars
      initializeParents oldch
  
  let rec private newSingle oldvar body =
    let varpars = getLeafParents oldvar
    let s = allocSingle ()
    initializeSingle s (getLeafNameId oldvar)
    setChild s body
    addToParents (getChildUplink s) body
    let var = mkNode (getLeaf s)
    iterDLL (fun lk -> upcopy var lk) varpars
    mkNode s
  
  and private newBranch func argm =
    let b = allocBranch ()
    initializeBranch b
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
        iterDLL (fun lk -> upcopy nd lk) (getBranchParents b)
      else
        setLChild cc newChild
    | UplinkRel.RCHILD ->
      let b = mkBranch (getNode parUplk)
      let cc = getCache b
      if isNil cc then
        let nd = newBranch (getLChild b) newChild
        setCache b (mkBranch nd)
        iterDLL (fun lk -> upcopy nd lk) (getBranchParents b)
      else
        setRChild cc newChild

  let rec reduce (redex : Branch) =
    let func = getLChild redex
    let argm = getRChild redex
    let func = mkSingle func
    let var = getLeaf func
    let body = getChild func
    let lampars = getSingleParents func
    let varpars = getLeafParents var

    if false then //isLengthOne lampars then
      replaceChild argm (mkNode var)
      let answer = getChild func
      replaceChild answer (mkNode redex)
      freeNode (mkNode redex)
      answer
      
    (* elif isNil varpars then body *)

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
          iterDLL (upcopy argm) (getLeafParents var)
          struct (b', ValueSome b)

      let struct (ans, topappOpt) = scandown body

      let answer =
        match topappOpt with
        | ValueNone -> ans
        | ValueSome app -> clearCaches func app; ans

      replaceChild answer (mkNode redex)
      freeNode (mkNode redex)
      answer
  
  let inline private isRedex (nd : Node) : Branch =
    if isNil nd || getNodeKind nd <> NodeKind.BRANCH then
      mkBranch -1
    else
      let b = mkBranch nd
      if getNodeKind (getLChild b) = NodeKind.SINGLE then b
      else mkBranch -1

  let rec normaliseWeakHead (nd : Node) : Node =
    let mutable ans = nd
    let mutable b = isRedex ans
    while not (isNil b) do
      ans <- reduce b
      b <- isRedex ans
    ans
  
  let rec normalise (nd : Node) : Node =
    let ans = normaliseWeakHead nd

//    let rec loop x =
//      match getNodeKind x with
//      | NodeKind.LEAF -> ()
//      | NodeKind.SINGLE ->
//        let ch = getChild (mkSingle x)
//        loop (normaliseWeakHead ch)
//      | NodeKind.BRANCH ->
//        let lch = getLChild (mkBranch x)
//        let rch = getRChild (mkBranch x)
//        loop (normaliseWeakHead lch)
//        loop (normaliseWeakHead rch)

    let rec loop x =
      match getNodeKind x with
      | NodeKind.LEAF -> ()
      | NodeKind.SINGLE ->
        loop (getChild (mkSingle x))
      | NodeKind.BRANCH ->
        let b = mkBranch x
        let func = getLChild b
        match getNodeKind func with
        | NodeKind.LEAF -> loop (getRChild b)
        | NodeKind.SINGLE -> loop (reduce b)
        | NodeKind.BRANCH -> loop func; loop (getRChild b)

    loop ans
    ans