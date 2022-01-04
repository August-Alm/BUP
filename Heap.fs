namespace BUP

module Memory =

  open System.Collections.Generic
  
  let private singleBlock = Array.zeroCreate 8
  let private branchBlock = Array.zeroCreate 11

  let private capacity = 65536

  let internal heap = ResizeArray<int> capacity

  let private freedSingles = Stack<Single> (capacity / 16)

  let private freedBranches = Stack<Branch> (capacity / 22)

  let internal clearHeap () =
    heap.Clear ()
    heap.Capacity <- capacity
    freedSingles.Clear ()
    freedBranches.Clear ()

  let internal allocSingle () =
    if freedSingles.Count > 0 then
      freedSingles.Pop ()
    else
      let addr = heap.Count
      heap.AddRange singleBlock
      mkSingle addr
  
  let internal deallocSingle s = freedSingles.Push s

  let internal allocBranch () =
    if freedBranches.Count > 0 then
      freedBranches.Pop ()
    else
      let a = heap.Count
      heap.AddRange branchBlock
      mkBranch a
  
  let deallocBranch b = freedBranches.Push b


  (* ***** ***** *)

  let internal names = ResizeArray<string> (capacity / 16)

  let internal nameIds = Dictionary<string, int> (capacity / 16)

  let inline getNameId x = nameIds[x]

  let inline getName id = names[id]

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