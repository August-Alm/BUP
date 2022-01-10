namespace BUP

module Hoas =

  // HOAS
  type Tm = V of int | L of (Tm -> Tm) | A of (Tm * Tm)

  // de Bruijn
  type Term = Var of int | Lam of Term | App of (Term * Term)

  let inline app t u = match t with L f -> f u | _ -> A (t, u)
  
  let rec quoteN n tm =
    match tm with
    | V x -> Var (n - x - 1)
    | L t -> Lam (quoteN (n + 1) (t (V n)))
    | A (t, u) -> App (quoteN n t, quoteN n u)
  
  type QState = QVar | QLam | QFunc | QArgm of Term

  // Need quote that doesn't overflow so easily.

  let quote = quoteN 0

  let rec private reduce tm = 
    match tm with A (L f, u) -> reduce (f u) | _ -> tm
  
  let rec normalise tm =
    let red = reduce tm
    match red with
    | V _ -> red
    | L f -> L(fun x -> normalise (f x))
    | A (t, u) -> A (normalise t, normalise u)

  let private toInt(n : Term) : int =
    let rec loop acc (x : Term) =
      match x with
      | App (_, u) -> loop (acc + 1) u
      | _ (* Var *) -> acc
    match n with Lam (Lam bod) -> loop 0 bod | _ -> failwith "Not a Church nat."

  open FsCheck.Xunit

  type Tests =

    [<Property>]
    static member ``1. Hoas normalisation of factorial of six.`` () =
      let n1 = L(fun s -> L(fun z -> app s z))
      let n1_n1 = L(fun g -> app (app g n1) n1)
      let snd = L(fun a -> L(fun b -> b))
      let F =
        L(fun p -> app p (
          L(fun a -> L(fun b -> L(fun g ->
            app
              (app g (L(fun s -> L(fun z -> app s (app (app a s) z)))))
              (L(fun s -> app a (app b s))))))))
      let fact = L(fun k -> app (app (app k F) n1_n1) snd)
      let seven =
        L(fun s -> L(fun z ->
          app s (app s (app s (app s (app s (app s (app s z))))))))
      let x = app fact seven
      let t = System.Diagnostics.Stopwatch ()
      t.Start ()
      let y = normalise x
      t.Stop ()
      printfn "Hoas normalised in %A ms." t.ElapsedMilliseconds
      toInt (quote y) = 5040 //40320
