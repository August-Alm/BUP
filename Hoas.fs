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

 (*
  let private toInt(n : Term) : int =
    let rec loop acc (x : Term) =
      match x with
      | App (Var 1, u) -> loop (acc + 1) u
      | Var 0 -> acc
      | _ -> failwith "Not a Church nat."
    match n with
    | Lam (Lam bod) -> loop 0 bod
    | _ -> failwith "Not a Church nat."
  *)

  open BenchmarkDotNet.Attributes

  type Benchmarks () =

    [<Benchmark>]
    member _.Hoas50k () =
      let two = L(fun s -> L(fun z -> app s (app s z)))
      let five = L(fun s -> L(fun z -> app s (app s (app s (app s (app s z))))))
      let mul = L(fun m -> L(fun n -> L(fun s -> app m (app n s))))
      let n10 = app (app mul two) five
      let n100 = app (app mul n10) n10
      let n10k = app (app mul n100) n100
      let n50k = app (app mul n10k) five
      quote (normalise n50k)

    [<Benchmark>]
    member _.HoasNoQuote50k () =
      let two = L(fun s -> L(fun z -> app s (app s z)))
      let five = L(fun s -> L(fun z -> app s (app s (app s (app s (app s z))))))
      let mul = L(fun m -> L(fun n -> L(fun s -> app m (app n s))))
      let n10 = app (app mul two) five
      let n100 = app (app mul n10) n10
      let n10k = app (app mul n100) n100
      let n50k = app (app mul n10k) five
      normalise n50k

    [<Benchmark>]
    member _.HoasTree15 () =
      let mutable p15 = L(fun x -> x)
      for _ = 1 to 15 do p15 <- A(p15, p15)
      quote (normalise p15)

    [<Benchmark>]
    member _.HoasFact8 () =
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
      let eight =
        L(fun s -> L(fun z ->
          app s (app s (app s (app s (app s (app s (app s (app s z)))))))))
      quote (normalise (app fact eight))