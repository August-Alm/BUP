namespace BUP

[<AutoOpen>]
module Types =

  open System

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
    LanguagePrimitives.EnumOfValue (int x)
  
  let inline withMeasure<[<Measure>]'m> (x) =
    LanguagePrimitives.IntPtrWithMeasure<'m> x
  
  let inline withoutMeasure<'a> (p : 'a) = (# "" p : IntPtr #)

  let inline isNil x = (withoutMeasure x = IntPtr.Zero)

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
  
  type Node = nativeint<nodePtr>

  let inline mkNode x = withMeasure<nodePtr> (withoutMeasure x)


  (* ***** ***** *)

  type UplinkRel =
    | CHILD = 0
    | LCHILD = 1
    | RCHILD = 2
  
  type Uplink = nativeint<uplinkPtr>

  let inline mkUplink (x) : Uplink = withMeasure<uplinkPtr> x


  (* ***** ***** *)

  type UplinkDLL = nativeint<uplinkPtrPtr>

  let inline mkUplinkDLL (x) = withMeasure<uplinkPtrPtr> x
  

  (* ***** ***** *)
  
  type Leaf = nativeint<leafPtr>

  let inline mkLeaf (x) = withMeasure<leafPtr> (withoutMeasure x)


  (* ***** ***** *)
  
  type Single = nativeint<singlePtr>

  let inline mkSingle x = withMeasure<singlePtr> (withoutMeasure x)


  (* ***** ***** *)

  type Branch = nativeint<branchPtr>

  let inline mkBranch x = withMeasure<branchPtr> (withoutMeasure x)