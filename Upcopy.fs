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
  
  let rec private freeNode (nd : Node) =
    match getNodeKind nd with
    | NodeKind.LEAF -> ()
    | NodeKind.SINGLE ->
      let s = mkSingle nd
      delPar (getChild s) (getChildUplink s)
      deallocSingle s
    | NodeKind.BRANCH ->
      let b = mkBranch nd
      delPar (getLChild b) (getLChildUplink b)
      delPar (getRChild b) (getRChildUplink b)
      deallocBranch b

  and private delPar (nd : Node) (lk : Uplink) =
    let lks = getParents nd
    if getHead lks = lk then
      let nxt = getNext lk
      reinitializeUplink lk
      mkFirst nxt
      setHead lks nxt
    else
      unlink lk
    if isEmpty lks then freeNode nd


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
            iterDLL (fun lk -> upcopy argm lk) varpars
            struct (b', ValueSome b)
        
        let struct (ans, topappOpt) = scandown body

        match topappOpt with
        | ValueNone -> ans
        | ValueSome app -> clearCaches func app; ans
      
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

  let rec normaliseWeakHead (nd : Node) : Node =
    let mutable ans = nd
    let mutable b = isRedex ans
    while not (isNil b) do
      ans <- reduce b
      b <- isRedex ans
    ans
  
  let rec normalise (nd : Node) : Node =
    let ans = normaliseWeakHead nd

    let rec loop x =
      match getNodeKind x with
      | NodeKind.LEAF -> ()
      | NodeKind.SINGLE -> loop (getChild (mkSingle nd))
      | NodeKind.BRANCH ->
        let b = mkBranch nd
        let func = getLChild b
        match getNodeKind func with
        | NodeKind.LEAF -> loop (getRChild b)
        | NodeKind.SINGLE -> loop (reduce b)
        | NodeKind.BRANCH -> loop func; loop (getRChild b)

    loop ans
    ans