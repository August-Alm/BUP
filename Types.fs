namespace BUP

[<AutoOpen>]
module Types =

  type FixedStack<'a> (capacity : int) =
    let mutable count = 0
    let storage = Array.zeroCreate<'a> capacity

    member _.Count with get () = count

    member _.Push (x : 'a) = storage[count] <- x; count <- count + 1

    member _.Pop () = count <- count - 1; storage[count]

    member _.TryPop (x : 'a byref) =
      if count = 0 then false
      else
        count <- count - 1
        x <- storage[count]
        true
    
    member _.Clear () = count <- 0
  
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


  (* ***** ***** *)

  type NodeKind =
    | LEAF = 0
    | SINGLE = 1
    | BRANCH = 2
  
  type Node = int<nodePtr>

  let inline mkNode x = withMeasure<nodePtr> (int x)


  (* ***** ***** *)

  type UplinkRel =
    | CHILD = 0
    | LCHILD = 1
    | RCHILD = 2
  
  type Uplink = int<uplinkPtr>

  let inline mkUplink x = withMeasure<uplinkPtr> (int x)


  (* ***** ***** *)

  type UplinkDLL = int<uplinkPtrPtr>

  let inline mkUplinkDLL x = withMeasure<uplinkPtrPtr> (int x)
  

  (* ***** ***** *)
  
  type Leaf = int<leafPtr>

  let inline mkLeaf x = withMeasure<leafPtr> (int x)


  (* ***** ***** *)
  
  type Single = int<singlePtr>

  let inline mkSingle x = withMeasure<singlePtr> (int x)


  (* ***** ***** *)

  type Branch = int<branchPtr>

  let inline mkBranch x = withMeasure<branchPtr> (int x)