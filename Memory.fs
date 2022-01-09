namespace BUP

module internal Memory =

  open System.Collections.Generic
  
  let private singleBlock = Array.zeroCreate<int> 8
  let private branchBlock = Array.zeroCreate<int> 11

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