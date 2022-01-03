namespace BUP

module Heaps =

  open System.Collections.Generic
  
  let private singleBlock = Array.zeroCreate 8
  let private branchBlock = Array.zeroCreate 11


  [<AllowNullLiteral>]
  type MemoryHeap (cap: int) =
    inherit Heap ()

    let memory = ResizeArray<int> cap
    let freedSingles = Stack<Single> (cap / 16) 
    let freedBranches = Stack<Branch> (cap / 22)

    override _.Item
      with get addr = memory[addr]
      and set addr x = memory[addr] <- x
    
    member _.AllocSingle () =
      let s =
        if freedSingles.Count > 0 then
          freedSingles.Pop ()
        else
          let addr = memory.Count
          memory.AddRange singleBlock
          mkSingle addr
      initializeSingle s; s
    
    member _.DeallocSingle (s : Single) =
      freedSingles.Push s
    
    member _.AllocBranch () =
      let b =
        if freedBranches.Count > 0 then
          freedBranches.Pop ()
        else
          let a = memory.Count
          memory.AddRange branchBlock
          mkBranch a
      initializeBranch b; b
    
    member _.DeallocBranch (b : Branch) =
      freedBranches.Push b
    
    member _.Clear () =
      memory.Clear ()
      memory.Capacity <- cap
      freedSingles.Clear ()
      freedBranches.Clear ()