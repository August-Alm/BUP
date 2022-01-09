namespace BUP

module internal Memory =

  open System.Collections.Generic
  
  let private singleBlock =
    [|
      int NodeKind.SINGLE;
        
      int NodeKind.LEAF; // leaf
      -1; // leaf parents
        
      -1; // child node
      
      int UplinkRel.CHILD;
      -1; // uplink next
      -1; // uplink previous
      
      -1; // single parents
    |]
  
  let private branchBlock = 
    [|
      int NodeKind.BRANCH;

      -1; // left child
      -1; // right child

      int UplinkRel.LCHILD;
      -1; // next
      -1; // previous

      int UplinkRel.RCHILD;
      -1;
      -1;

      -1; // parents
      -1; // cache 
    |]

  let private capacity = 65536

  let internal heap = Array.zeroCreate<int> capacity
  let mutable address = 0

  let private freedSingles = Stack<Single> (capacity / 16)

  let private freedBranches = Stack<Branch> (capacity / 22)

  let internal clearHeap () =
    Array.fill heap 0 address 0
    address <- 0
    freedSingles.Clear ()
    freedBranches.Clear ()

  let internal allocSingle () =
    if freedSingles.Count > 0 then
      freedSingles.Pop ()
    else
      let addr = address
      singleBlock.CopyTo (heap, addr)
      address <- address + singleBlock.Length
      mkSingle addr
  
  let internal deallocSingle s = freedSingles.Push s

  let internal allocBranch () =
    if freedBranches.Count > 0 then
      freedBranches.Pop ()
    else
      let addr = address
      branchBlock.CopyTo (heap, addr)
      address <- address + branchBlock.Length
      mkBranch addr
  
  let deallocBranch b = freedBranches.Push b


  (* ***** ***** *)

  let internal names = ResizeArray<string> (capacity / 16)

  let internal nameIds = Dictionary<string, int> (capacity / 16)

  let inline getNameId x = nameIds[x]

  let inline getName id = if id >= 0 then names[id] else failwith "How??"

  let addName x =
    match nameIds.TryGetValue x with
    | false, _ ->
      let id = names.Count
      names.Add x
      nameIds.Add (x, id)
      id
    | true, id ->
      nameIds[x] <- id
      id
  
  let clearNames () =
    names.Clear ()
    names.Capacity <- capacity / 16
    nameIds.Clear ()