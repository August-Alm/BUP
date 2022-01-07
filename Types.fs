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
    | RCHILD = 1
    | LCHILD = 2
  
  type Uplink = int<uplinkPtr>

  let mkUplink x =
    if isNil x then failwith "Uplinks can never be nil."
    else withMeasure<uplinkPtr> (int x)


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