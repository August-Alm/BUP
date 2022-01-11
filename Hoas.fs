namespace BUP

module Hoas =

  // HOAS
  type Tm = V of int | L of (Tm -> Tm) | A of (Tm * Tm)

  // de Bruijn
  type Term = Var of int | Lam of Term | App of (Term * Term)

  let inline app t u = match t with L f -> f u | _ -> A (t, u)
  
  type Ko =
    | KoId
    | KoLam of Ko
    | KoArg of Ko * int * Tm
    | KoApp of Ko * Term

  let rec quote tm =
    let rec go n tm (ko : Ko) =
      match tm with
      | V x -> ev ko (Var (n - x - 1))
      | L t -> go (n + 1) (t (V n)) (KoLam ko) 
      | A (t, u) -> go n t (KoArg (ko, n, u)) 
    and ev ko trm =
      match ko with
      | KoId -> trm
      | KoLam ko -> ev ko (Lam trm)
      | KoArg (ko, n, u) -> go n u (KoApp (ko, trm)) 
      | KoApp (ko, func) -> ev ko (App (func, trm))
    go 0 tm KoId

  let rec private reduce tm = 
    match tm with A (L f, u) -> reduce (f u) | _ -> tm
  
  let rec normalise tm =
    match tm with
    | V _ -> tm
    | L f -> L(fun x -> normalise (f x))
    | A (t, u) ->
      let t = normalise t
      match t with
      | L f -> reduce (f (normalise u))
      | _ -> A (t, normalise u)

  let private toInt(n : Term) : int =
    let rec loop acc (x : Term) =
      match x with
      | App (Var 1, u) -> loop (acc + 1) u
      | Var 0 -> acc
      | _ -> failwith "Not a Church nat."
    match n with
    | Lam (Lam bod) -> loop 0 bod
    | _ -> failwith "Not a Church nat."

  open FsCheck.Xunit

  type Tests =

    [<Property>]
    static member ``7. Hoas normalisation of (2*5)^2 * (2*5) * 5 = 5k.`` () =
      let n2 = L(fun s -> L(fun z -> app s (app s z)))
      let n5 = L(fun s -> L(fun z -> app s (app s (app s (app s (app s z))))))
      let mul = L(fun m -> L(fun n -> L(fun s -> app m (app n s))))
      let n10 = app (app mul n2) n5
      let n100 = app (app mul n10) n10
      let n1k = app (app mul n100) n10
      let n5k = app (app mul n1k) n5
      let t = System.Diagnostics.Stopwatch ()
      t.Start ()
      let y = quote (normalise n5k)
      t.Stop ()
      printfn "Hoas normalised in %i ms." t.ElapsedMilliseconds
      toInt y = 5000

    [<Property>]
    static member ``8. Hoas normalisation of full 15 deep binary tree.``() =
      let ps = Array.zeroCreate<Tm> 16
      ps[0] <- L(fun x -> x)
      for i = 1 to 15 do
        ps[i] <- A(ps[i - 1], ps[i - 1])
      let p15 = ps[15]
      let t = System.Diagnostics.Stopwatch ()
      t.Start ()
      let y = quote (normalise p15)
      t.Stop ()
      printfn "Hoas normalised in %i ms." t.ElapsedMilliseconds
      y = Lam (Var 0) 

    [<Property>]
    static member ``9. Hoas normalisation of factorial of seven.`` () =
      let one = L(fun s -> L(fun z -> app s z))
      let one_one = L(fun g -> app (app g one) one)
      let snd = L(fun a -> L(fun b -> b))
      let F =
        L(fun p -> app p (
          L(fun a -> L(fun b -> L(fun g ->
            app
              (app g (L(fun s -> L(fun z -> app s (app (app a s) z)))))
              (L(fun s -> app a (app b s))))))))
      let fact = L(fun k -> app (app (app k F) one_one) snd)
      let seven =
        L(fun s -> L(fun z ->
          app s (app s (app s (app s (app s (app s (app s z))))))))
      let x = app fact seven
      let t = System.Diagnostics.Stopwatch ()
      t.Start ()
      let y = quote (normalise x)
      t.Stop ()
      printfn "Hoas normalised in %i ms." t.ElapsedMilliseconds
      toInt y = 5040