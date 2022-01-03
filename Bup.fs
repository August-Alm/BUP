namespace BUP

module Base = 

  open System.Collections.Generic

  (* Core datatype definitions
  ******************************************************************************
  * There are three kinds of nodes: lambdas, var refs and applications.
  * Each kind gets its own ML datatype, instead of having a single,
  * three-constructor datatype. Why? It allows us to encode more structure
  * in the ML type system. E.g., the *parent* of a node can only be a lambda
  * or an app; not a var-ref. So we can define a two-constructor node-parent
  * type, ruling out the var-ref possibility. And so forth.
  *
  * Note, also, that some of these "foo option ref" record fields are because we
  * are constructing circular structure. Backpointers are initialised to
  * "ref NONE," then we slam in "SOME <node>" after we have later created <node>.
  *)

  (* bodyRef is the parent record belonging to our child node (our body) that
  * points back to us. I.e., suppose our body node N has three parents, of
  * which we are one. Then N has a three-element doubly-linked list (DLL)
  * of parent records, one for each parent. The one that points back to us
  * is the record sitting in *our* "bodyRef" field. This allows us to delink
  * ourselves from the child’s parent list & detach the child in constant time
  * when copying up through the lambda node.
  *)
  type LambdaType =
    { Var : VarType;
      Body : Term option ref;
      BodyRef : LinkedListNode<ChildCell> option ref;
      Parents : LinkedList<ChildCell> ref;
      Uniq : int }
  (* funcRef and argRef are similar to the bodyRef field
  * of the LambdaType record above. *)
  and AppType =
    { Func : Term option ref;
      Arg : Term option ref;
      FuncRef : LinkedListNode<ChildCell> option ref;
      ArgRef : LinkedListNode<ChildCell> option ref;
      Copy : AppType option ref;
      Parents : LinkedList<ChildCell> ref;
      Uniq : int }
  and VarType =
    { Name : string;
      Parents : LinkedList<ChildCell> ref;
      Uniq : int}
  and Term =
    | LambdaT of LambdaType
    | AppT of AppType
    | VarT of VarType
  (* Type of a general LC node. *)
  (* This tells us what our relationship to our parents is. *)
  and ChildCell =
    | AppFunc of AppType
    | AppArg of AppType
    | LambdaBody of LambdaType

  (* Get the parents of a Term. *)
  let termParRef = function
    | LambdaT l -> l.Parents
    | AppT a -> a.Parents
    | VarT v -> v.Parents

  (* A rather subtle point:
  *******************************************************************************
  * When we do upsearch/copying, we chase uplinks/backpointers, copying old tree
  * structure, creating new tree structure as we go. But we don’t want to search
  * up through *new* structure by accident -- that might induce an infinite
  * search/copy. Now, the the only way we can have a link from an old node up to
  * a new parent is by cloning an app node -- when we create a new app, it has
  * one new child NC and one old child OC. So our new app node will be added to
  * the parent list of the old child -- and if we should later copy up through
  * the old child, OC, we’d copy up through the new app node -- that is, we’d
  * copy the copy. This could get us into an infinite loop. (Consider reducing
  *
  (\x. x x) y
  * for example. Infinite-loop city.)
  *
  * We eliminate this problem in the following way: we don’t install *up* links
  * to app nodes when we copy. We just make the downlinks from the new app node
  * to its two children. So the upcopy search won’t ever chase links from old
  * structure up to new structure; it will only see old structure.
  *
  * We *do* install uplinks from a lambda’s body to a newly created lambda node,
  * but this link always goes from new structure up to new structure, so it will
  * never affect the our search through old structure. The only way we can have a
  * new parent with an old child is when the parent is an app node.
  *
  * When we are done, we then make a pass over the new structure, installing the
  * func->app-node or arg->app-node uplinks. We do this in the copy-clearing
  * pass -- as we wander the old app nodes, clearing their cache slots, we take
  * the corresponding new app node and install backpointers from its children
  * up to it.
  *
  * In other words, for app nodes, we only create downlinks, and later bring the
  * backpointer uplinks into sync with them.
  *)
  (* Given a term and a ChildCell, add the childcell to term’s parents. *)
  let addToParents(node, cclink : LinkedListNode<ChildCell>) =
    let p = termParRef node in
    (!p).AddFirst cclink |> ignore //DL.add_before(!p, cclink)

  (* Is dll exactly one elt in length? *)
  let len1 (p : LinkedList<ChildCell>) = p.Count = 1

  (* clearCopies(redlam, topapp)
  ******************************************************************************
  * When we’re finished constructing the contractum, we must clean out the
  * app nodes’ copy slots (reset them to NONE) to reset everything for the next
  * reduction.
  * - REDLAM is the lambda we reduced.
  *
  * - TOPAPP is the highest app node under the reduced lambda -- it holds
  *
  the highest copy slot we have to clear out. If we clear it first, then
  *
  we are guaranteed that any upwards copy-clearing search started below it
  *
  will terminate upon finding an app w/an empty copy slot.
  *
  * Every lambda from REDLAM down to TOPAPP had its var as the origin of an
  * upcopy:
  * - For REDLAM, the upcopy mapped its var to the redex’s argument term.
  * - The other, intermediate lambdas *between* REDLAM & TOPAPP (might be zero
  *
  of these) were copied to fresh lambdas, so their vars were mapped to
  *
  fresh vars, too.
  * So, now, for each lambda, we must search upwards from the lambda’s var,
  * clearing cached copies at app nodes, stopping when we run into an
  * already-cleared app node.
  *
  * This cache-clearing upsearch is performed by the internal proc cleanUp.
  * (Get it?)
  *
  * When we created fresh app nodes during the upcopy phase, we *didn’t*
  * install uplinks from their children up to the app nodes -- this ensures
  * the upcopy doesn’t copy copies. So we do it now.
  *)
  let clearCopies(redlam, topapp) =
    let topcopy = topapp.Copy
    (* Clear out top*)
    match !topcopy with
    | Some {Arg = arg; ArgRef = argRef; Func = func; FuncRef = funcRef} ->
      topcopy := None (* app & install*)
      addToParents((!arg).Value, (!argRef).Value) (* uplinks to *)
      addToParents((!func).Value, (!funcRef).Value) (* its copy. *)
    
    let rec cleanUp = function
    | AppFunc {Copy = copy; Parents = parents} ->
      match !copy with
      | None -> ()
      | Some {Arg = arg; ArgRef =argRef; Func = func; FuncRef = funcRef} ->
        copy := None
        addToParents((!arg).Value, (!argRef).Value) (* Add uplinks *)
        addToParents((!func).Value, (!funcRef).Value) (* to copy. *)
        for p in !parents do cleanUp p
    | AppArg {Copy = copy; Parents = parents} ->
      match !copy with
      | None -> ()
      | Some {Arg = arg; ArgRef = argRef; Func = func; FuncRef = funcRef} ->
        copy := None
        addToParents((!arg).Value, (!argRef).Value); (* Add uplinks *)
        addToParents((!func).Value, (!funcRef).Value); (* to copy. *)
        for p in !parents do cleanUp p
    | LambdaBody {Parents = parents; Var = var} ->
      for p in !var.Parents do cleanUp p
      for p in !parents do cleanUp p

    let rec lambdascan {Var = var; Body = bodref} =
      for p in !var.Parents do cleanUp p
      match (!bodref).Value with
      | LambdaT l -> lambdascan l
      | _ -> ()

    lambdascan redlam

  (* freeDeadNode term -> unit
  ***************************************************************************
  * Precondition: (termParents term) is empty -- term has no parents.
  *
  * A node with no parents can be freed. Furthermore, freeing a node
  * means we can remove it from the parent list of its children... and
  * should such a child thus become parentless, it, too can be freed.
  * So we have a recursive/DAG-walking/ref-counting sort of GC algo here.
  *
  * IMPORTANT: In this SML implementation, we don’t actually *do* anything
  * with the freed nodes -- we don’t, for instance, put them onto a free
  * list for later re-allocation. We just drop them on the floor and let
  * SML’s GC collect them. But it doesn’t matter -- this GC algo is *not
  * optional*. We *must* (recursively) delink dead nodes. Why? Because
  * we don’t want subsequent up-copies to spend time copying up into dead
  * node subtrees. So we remove them as soon as a beta-reduction makes
  * them dead.
  *
  * So this procedure keeps the upwards back-pointer picture consistent with
  * the "ground truth" down-pointer picture.
  *)
  let freeDeadNode node =
    let rec free = function
    | AppT {Func = func; FuncRef = funcRef; Arg = arg; ArgRef = argRef; Parents = parents} ->
      delPar ((!func).Value, (!funcRef).Value) (* Node no longer parent *)
      delPar ((!arg).Value, (!argRef).Value) (* of func or arg children. *)
    | LambdaT {Body = body; BodyRef = bodyRef; Parents = parents} ->
      delPar((!body).Value, (!bodyRef).Value)
      (* We wouldn’t actually want to dealloc a parentless var node, because
      * its binding lambda still retains a ref to it. Responsibility for
      * freeing a var node should be given to the code (just above) that
      * freed its lambda.  *)
    | VarT _ -> ()

    (* Remove CCLINK from TERM’s parent’s dll.
    * If TERM’s parent list becomes empty, it’s dead, too, so free it. *)
    and delPar (term, cclink : LinkedListNode<ChildCell>) =
      let links = cclink.List
      if links.First = cclink then
        links.RemoveFirst ()
        let parref = termParRef term
        parref := links
        if links.Count = 0 then free term

    free node

  (* Replace one child w/another in the tree.
  * - OLDPREF is the parent dll for some term -- the old term.
  * - NEW is the replacement term.
  * Add each element of the dll !OLDPREF to NEW’s parent list. Each such
  * element indicates some parental downlink; install NEW in the right slot
  * of the indicated parent. When done, set OLDPREF := NIL.
  *
  * Actually, we don’t move the dll elements over to NEW’s parent list one at
  * a time -- that involves redundant writes. E.g., if !OLDPREF is 23 elements
  * long, don’t move the elements over one at a time -- they are already nicely
  * linked up. Just connect the last elt of !OLDPREF & the first element of
  * NEW’s existing parent list, saving 22*2=44 writes. Because it physically
  * hurts to waste cycles.
  *)
  let replaceChild (oldpref : LinkedList<ChildCell> ref, newterm) =
    let cclinks = !oldpref
    let newparref = termParRef newterm
   
    let installChild = function
    | LambdaBody {Body = body} -> body := Some newterm
    | AppFunc {Func = func} -> func := Some newterm
    | AppArg {Arg = arg} -> arg := Some newterm

    let mutable cclink = cclinks.First
    while not (isNull cclink) do
      let next = cclink.Next
      installChild cclink.Value
      cclinks.Remove cclink
      (!newparref).AddLast cclink
      cclink <- next

    oldpref := null

  (* Allocate a fresh lambda L and a fresh var V. Install BODY as the body of
  * the lambda -- L points down to BODY, and L is added to BODY’s parent list.
  * The fresh var’s name (semantically irrelevant, but handy for humans) is
  * copied from oldvar’s name.
  *
  * Once this is done, kick off an OLDVAR->V upcopy to fix up BODY should it
  * contain any OLDVAR refs.
  *)
  fun newLambda(oldvar, body) =
  let val Var{name, parents = varparents, ...} = oldvar
  val var = Var{name = name,
  uniq = newUniq(),
  parents = ref DL.NIL}
  val bodyRefCell = ref NONE
  val ans = Lambda{var
  = var,
  body
  = ref(SOME body),
  bodyRef = bodyRefCell,
  uniq
  = newUniq(),
  parents = ref DL.NIL}
  val cclink = DL.new(LambdaBody ans)
  in bodyRefCell := SOME cclink;
  addToParents(body, cclink);
  (* Propagate the new var up through the lambda’s body. *)
  DL.app (upcopy (VarT var)) (!varparents);
  LambdaT ans
  end
  (* Allocate a fresh app node, with the two given params as its children.
  * DON’T install this node on the children’s parent lists -- see "a subtle
  * point" above for the reason this would get us into trouble.
  *)
  and newApp(func, arg) =
  let val funcRef = ref NONE
  val argRef = ref NONE
  val app
  = App{func
  = ref(SOME func),
  arg
  = ref(SOME arg),
  funcRef = funcRef,
  argRef = argRef,
  copy
  = ref NONE,
  parents = ref DL.NIL,
  uniq
  = newUniq()}
  in funcRef := SOME( DL.new(AppFunc app) );
  argRef := SOME( DL.new(AppArg app) );
  app
  end
  (* upcopy newChild parRef -> unit
  ******************************************************************************
  * The core up-copy function.
  * parRef represents a downlink dangling from some parent node.
  * - If the parent node is a previously-copied app node, mutate the
  *
  copy to connect it to newChild via the indicated downlink, and quit
  * - If the parent is an app node that hasn’t been copied yet, then
  *
  make a copy of it, identical to parent except that the indicated downlink
  *
  points to newChild. Stash the new copy away inside the parent. Then take
  *
  the new copy and recursively upcopy it to all the parents of the parent.
  * - If the parent is a lambda node L (and, hence, the downlink is the
  *
  "body-of-a-lambda" connection), make a new lambda with newChild as
  *
  its body and a fresh var for its var. Then kick off an upcopy from
  *
  L’s var’s parents upwards, replacing L’s var with the fresh var.
  *
  (These upcopies will guaranteed terminate on a previously-replicated
  *
  app node somewhere below L.) Then continue upwards, upcopying the fresh
  *
  lambda to all the parents of L.
  *)
  and upcopy newChild (LambdaBody(Lambda{var, parents,...})) =
  DL.app (upcopy (newLambda(var, newChild))) (!parents)
  (* Cloning an app from the func side *)
  | upcopy new_child (AppFunc(App{copy as ref NONE, arg, parents, ...})) =
  let val new_app = newApp(new_child, valOf(!arg))
  in copy := SOME new_app;
  DL.app (upcopy (AppT new_app)) (!parents)
  end
  (* Copied up into an already-copied app node. Mutate the existing copy & quit. *)
  | upcopy newChild (AppFunc(App{copy = ref(SOME(App{func,...})), ...})) =
  func := SOME newChild
  (* Cloning an app from the arg side *)
  | upcopy new_child (AppArg(App{copy as ref NONE, func, parents, ...})) =
  let val new_app = newApp(valOf(!func), new_child)
  in copy := SOME new_app;
  DL.app (upcopy (AppT new_app)) (!parents)
  end
  (* Copied up into an already-copied app node. Mutate the existing copy & quit. *)
  | upcopy newChild (AppArg(App{copy = ref(SOME(App{arg,...})),...})) =
  arg := SOME newChild
  (* Contract a redex; raise an exception if the term isn’t a redex. *)
  fun reduce(a as App{funcRef, func = ref(SOME(LambdaT l)),
  argRef, arg = ref(SOME argterm),
  parents, ...}) =
  let val Lambda {var, body, bodyRef, parents = lampars, ...} = l
  val Var{parents = vpars as ref varpars, ...} = var
  val ans = if len1(!lampars)
  (* The lambda has only one parent -- the app node we’re
  * reducing, which is about to die. So we can mutate the
  * lambda. Just alter all parents of the lambda’s vars to
  * point to ARGTERM instead of the var, and we’re done!
  *)
  then (replaceChild(vpars, argterm);
  valOf(!body))
  (* Fast path: If lambda’s var has no refs,
  * the answer is just the lambda’s body, as-is.
  *)
  else if varpars = DL.NIL then valOf(!body)
  (*
    The standard case. We know two things:
    1. The lambda has multiple pars, so it will survive the
    reduction, and so its body be copied, not altered.
    2. The var has refs, so we’ll have to do some substitution.
    First, start at BODY, and recursively search down
    through as many lambdas as possible.
    - If we terminate on a var, the var is our lambda’s var,
    for sure. (OTW, #2 wouldn’t be true.) So just return
    BODY back up through all these down-search lambda-
    skipping calls, copying the initial lambdas as we go.
    - If we terminate on an app, clone the app & stick the
    clone in the app’s copy slot. Now we can do our VAR->ARG
    up-copy stuff knowing that all upcopying will guaranteed
    terminate on a cached app node.
    When we return up through the initial-lambda-skipping
    recursion, we add on copies of the lambdas through
    which we are returning, *and* we also pass up that top
    app node we discovered. We will need it in the
    subsequent copy-clearing phase.
  *)
  else let fun scandown(v as VarT _) = (argterm,NONE) (* No app! *)
  | scandown(l as LambdaT(Lambda{body,var,...})) =
  let val (body’,topapp) = scandown(valOf(!body))
  val l’ = newLambda(var, body’)
  in (l’, topapp)
  end
  | scandown(AppT(a as App{arg,func,copy,...})) =
  (* Found it -- the top app.
  *)
  (* Clone & cache it, then kick off a *)
  (* var->arg upcopy.
  *)
  let val a’ = newApp(valOf(!func), valOf(!arg))
  in copy := SOME a’;
  DL.app (upcopy argterm) varpars;
  (AppT a’, SOME a)
  end
  val (ans, maybeTopApp) = scandown (valOf(!body))
  (* Clear out the copy slots of the app nodes. *)
  in case maybeTopApp of
  NONE => ()
  | SOME app => clearCopies(l,app);
  ans
  end
  (* We’ve constructed the contractum & reset all the copy slots. *)
  in replaceChild(parents, ans);
  freeDeadNode (AppT a);
  ans
  end
  (* Replace redex w/the contractrum. *)
  (* Dealloc the redex.
  *)
  (* Done.
  *)
  (* Call-by-name reduction to weak head-normal form. *)
  fun normaliseWeakHead(AppT(app as App{func, arg, ...})) =
  (normaliseWeakHead(valOf(!func));
  case valOf(!func) of LambdaT _ => normaliseWeakHead(reduce app)
  | _
  => ())
  | normaliseWeakHead _ = ()
  (* Normal-order reduction to normal form. *)
  fun normalise(AppT(app as App{func, arg, uniq,...})) =
  (normaliseWeakHead(valOf(!func));
  case valOf(!func) of LambdaT _ => normalise(reduce app)
  | VarT _
  => normalise(valOf(!arg))
  | app’
  => (normalise app’;
  normalise(valOf(!arg))))
  | normalise(LambdaT(Lambda{body,...})) = normalise(valOf(!body))
  | normalise _ = ()