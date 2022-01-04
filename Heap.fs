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

  [<Struct>]
  type NameData = { Id : int; RefCnt : int }

  let private names = ResizeArray<string> (capacity / 16)

  let private namesData = Dictionary<string, NameData> (capacity / 16)

  let getNameId x = namesData[x].Id

  let getName id = names[id]

  let addName x =
    match namesData.TryGetValue x with
    | false, _ ->
      let id = names.Count
      names.Add x
      namesData.Add (x, {Id = id; RefCnt = 1})
      id
    | true, {Id = id; RefCnt = refCnt} ->
      namesData[x] <- {Id = id; RefCnt = refCnt + 1}
      id
  
  let decrefName x =
    let {Id = id; RefCnt = refCnt} = namesData[x]
    if refCnt = 0 then
      namesData.Remove x |> ignore
      names[id] <- null
    else
      namesData[x] <- {Id = id; RefCnt = refCnt - 1} 

  let clearNames () =
    names.Clear ()
    names.Capacity <- capacity / 16
    namesData.Clear ()