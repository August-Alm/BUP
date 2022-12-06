namespace BUP

#nowarn "9"

module internal Memory =

  open System.Collections.Generic
  open System.Runtime.InteropServices
  open System
  open System.Buffers
  open Microsoft.FSharp.NativeInterop
  open System.Collections.Generic
  
  let private singleBlock =
    [|
      IntPtr (int NodeKind.SINGLE);
        
      IntPtr (int NodeKind.LEAF); // leaf
      IntPtr.Zero; // leaf parents
        
      IntPtr.Zero; // child node
      
      int UplinkRel.CHILD;
      IntPtr.Zero; // uplink next
      IntPtr.Zero; // uplink previous
      
      IntPtr.Zero; // single parents
    |]
  
  let private branchBlock = 
    [|
      IntPtr (int NodeKind.BRANCH);

      IntPtr.Zero; // left child
      IntPtr.Zero; // right child

      int UplinkRel.LCHILD;
      IntPtr.Zero; // next
      IntPtr.Zero; // previous

      int UplinkRel.RCHILD;
      IntPtr.Zero;
      IntPtr.Zero;

      IntPtr.Zero; // parents
      IntPtr.Zero; // cache 
    |]


  let inline addOffset a (i : int) =
      NativePtr.add (NativePtr.ofNativeInt<IntPtr> (withoutMeasure a)) i
      |> NativePtr.toNativeInt


  type Heap (pool : ArrayPool<IntPtr>) =
    let gcHandles = Dictionary<IntPtr, GCHandle> ()
    let mutable disposed = false

    new () = new Heap(ArrayPool.Shared)

  with
    member inline private _.UnsafeFree (handle : GCHandle) =
      pool.Return (handle.Target :?> IntPtr[])
      handle.Free ()
    
    member private this.DisposeWhen disposing =
      if not disposed then
        disposed <- true
        Seq.iter this.UnsafeFree gcHandles.Values
        if disposing then gcHandles.Clear ()

    override this.Finalize () = this.DisposeWhen false

    interface IDisposable with
      member this.Dispose () =
        this.DisposeWhen true; GC.SuppressFinalize this

    member _.Alloc size =
      let arr = pool.Rent size
      let handle = GCHandle.Alloc (arr, GCHandleType.Pinned)
      let addr = handle.AddrOfPinnedObject ()
      gcHandles.Add (addr, handle)
      addr
    
    member _.AllocSingle () =
      let arr = pool.Rent 8
      singleBlock.CopyTo (arr, 0)
      let handle = GCHandle.Alloc (arr, GCHandleType.Pinned)
      let addr = handle.AddrOfPinnedObject ()
      gcHandles.Add (addr, handle)
      mkSingle addr

    member _.AllocBranch () =
      let arr = pool.Rent 11
      branchBlock.CopyTo (arr, 0)
      let handle = GCHandle.Alloc (arr, GCHandleType.Pinned)
      let addr = handle.AddrOfPinnedObject ()
      gcHandles.Add (addr, handle)
      mkBranch addr

    member this.Clear () =
      Seq.iter this.UnsafeFree gcHandles.Values
      gcHandles.Clear ()

    member this.Free addr =
      let mutable h = GCHandle ()
      if gcHandles.Remove (addr, &h) then this.UnsafeFree h
      else failwith "address not owned by this heap!"

    /// Highly unsafe!
    member _.Item
      //with get addr = Marshal.ReadIntPtr addr
      //and set addr p = Marshal.WriteIntPtr (addr, p)
      with inline get addr =
        NativePtr.read<IntPtr> (NativePtr.ofNativeInt addr)
      and inline set addr p =
        NativePtr.write<IntPtr> (NativePtr.ofNativeInt addr) p


  let internal heap = new Heap ()

  let internal clearHeap () = heap.Clear ()

  let internal allocSingle () = heap.AllocSingle ()
  
  let internal deallocSingle (s : Types.Single) = heap.Free (withoutMeasure s)

  let internal allocBranch () = heap.AllocBranch ()
  
  let internal deallocBranch (b : Types.Branch) = heap.Free (withoutMeasure b)


  (* ***** ***** *)

  let private namesCap = pown 2 12

  let internal names = ResizeArray<string> namesCap
  let internal nameIds = Dictionary<string, int> namesCap

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
    names.Capacity <- namesCap
    nameIds.Clear ()