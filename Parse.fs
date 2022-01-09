namespace BUP

module Parse =

  module Input =

    [<AbstractClass>]
    type Input () =
      abstract member Pop : unit -> int32
      abstract member Peek : unit -> int32

    type InputOfString (str : string) =
      inherit Input ()
      let s = str
      let len = s.Length
      let mutable i = -1
      override _.Pop () =
        i <- i + 1; if i >= len then -1 else int s[i]
      override _.Peek () =
        let j = i + 1 in if j >= len then -1 else int s[j]

    type InputOfStream (strm : System.IO.StreamReader) =
      inherit Input ()
      override _.Pop () = strm.Read ()
      override _.Peek () = strm.Peek () 

  
  module Tokenizer =

    open Input
    open System.Text

    type TokenRaw =
      | T_Name of string 
      | T_Lam
      | T_Dot
      | T_LPar
      | T_RPar
      | T_Let of string
      | T_Eq
      | T_Seq
      | T_Eof
    
    let inline isAlpha c = (c > 64 && c < 91) || (c > 96 && c < 123)
    let inline isName c = isAlpha c || (c > 46 && c < 58) || c = int '_'
    let inline isCR c = (c = int '\r')
    let inline isLF c = (c = int '\n')
    let inline isSpace c = (c = int ' ' || c = int '\t' || isCR c || isLF c)

    let inline append (c : int) (sb : StringBuilder) =
      sb.Append (char c) |> ignore

    [<Struct>]
    type Position = { Line : int; Column : int }

    let inline error (pos : Position) msg =
      failwith (sprintf "At (%i, %i):\t%s" pos.Line pos.Column msg)

    type Token = Position * TokenRaw
    
    type Tokenizer (inp : Input) =
      let sb = StringBuilder 16
      let mutable line = 1
      let mutable column = 0

      let rec readChar () =
        let mutable c = inp.Pop ()
        if isCR c then
          c <- inp.Pop ()
          if not (isLF c) then
            failwith "'\r' must be followed by '\n'."
          else
            line <- line + 1
            column <- 0
            readChar ()
        elif isLF c then
          line <- line + 1
          column <- 0
          readChar ()
        else
          column <- column + 1
          c
      
      let readNonSpace () =
        let mutable c = readChar ()
        while isSpace c do c <- readChar ()
        c

      let readName (c : int) =
        let mutable c = c
        let mutable cnxt = inp.Peek ()
        while isName cnxt do
          append c sb
          c <- readChar ()
          cnxt <- inp.Peek ()
        append c sb
        let name = sb.ToString ()
        sb.Clear () |> ignore
        name
      
      member _.ReadToken () =
        let c = readNonSpace ()
        let pos = {Line = line; Column = column}
        if c = -1 then
          pos, T_Eof
        else
          match char c with
          | 'λ' -> pos, T_Lam
          | '.' -> pos, T_Dot 
          | '(' -> pos, T_LPar
          | ')' -> pos, T_RPar
          | '@' -> pos, T_Let (readName (readNonSpace ()))
          | ';' -> pos, T_Seq
          | '=' -> pos, T_Eq
          | _ ->
            if isName c then pos, T_Name (readName c)
            else error pos "Invalid token."


  (* ***** ***** *)

  [<Struct>]
  type private Binding = { NameId : int; Bound : Node }

  type private Environment = { mutable Bindings : Binding list }
  with static member Empty = { Bindings = [] }

  let inline private addBound (env : Environment) bnd =
    env.Bindings <- bnd :: env.Bindings

  let private getBound (env : Environment) nameId =
    List.tryFind (fun bnd -> bnd.NameId = nameId) env.Bindings

  let private remBound (env : Environment) (bnd : Binding) =
    let rec remFirst bs =
      match bs with
      | b :: bs' when b = bnd -> bs'
      | b :: bs' -> b :: remFirst bs'
      | _ -> failwith "Binding does not exist in environment."
    env.Bindings <- remFirst env.Bindings
  
  (* ***** ***** *)  

  open Memory
  open Input
  open Tokenizer

  let inline private connectChild (ch : Node) (s : Single) =
    setChild s ch; addToParents (getChildUplink s) ch

  let inline private connectLChild (lch : Node) (b : Branch) =
    setLChild b lch; addToParents (getLChildUplink b) lch

  let inline private connectRChild (rch : Node) (b : Branch) =
    setRChild b rch; addToParents (getRChildUplink b) rch
  

  type Parser (inp : Input) as parser =
    inherit Tokenizer (inp)

    let consumeToken tok =
      match parser.ReadToken () with
      | _, t when t = tok -> ()
      | pos, t ->
        let msg = sprintf "Expected %A but got %A." tok t
        error pos msg

    let rec parseEnv (env : Environment) =
      match parser.ReadToken () with
      | _, T_Lam -> 
        match parser.ReadToken () with
        | _, T_Name x ->
          let xId = addName x
          if getName xId <> x then failwith "No round trip"
          let s = allocSingle ()
          initializeSingle s xId
          let xVar = mkNode (getLeaf s)
          let bnd = {NameId = xId; Bound = xVar}
          addBound env bnd
          consumeToken T_Dot
          let body = parseEnv env
          remBound env bnd
          connectChild body s
          mkNode s
        | pos, _ -> error pos "Expected name."
      | _, T_LPar ->
        let func = parseEnv env
        let argm = parseEnv env
        consumeToken T_RPar
        let b = allocBranch ()
        connectLChild func b
        connectRChild argm b
        mkNode b
      | pos, T_Name x ->
        match getBound env (addName x) with
        | None -> error pos "Free variable."
        | Some bnd -> bnd.Bound
      | _, T_Let x ->
        let xId = addName x
        consumeToken T_Eq
        let bnd = parseEnv env
        consumeToken T_Seq
        let binding = {NameId = xId; Bound = bnd}
        addBound env binding 
        let res = parseEnv env
        remBound env binding
        res
      | pos, _ -> error pos "Syntax error."

    member _.ReadNode () = parseEnv Environment.Empty