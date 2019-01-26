module TestHelpers

open Xunit 
open System

let assertSuccess() = Assert.True(true)
let assertFail() = Assert.True(false)
let assertTrue (x: bool) = Assert.True(x)
let assertFalse (x: bool) = Assert.False(x)
let assertEqual x y = if x = y then assertSuccess() else assertFail()
let assertNotEqual x y = if x <> y then assertSuccess() else assertFail()
let assertIsNone x =
  match x with
  | None -> assertSuccess()
  | _ -> assertFail()
let asserIsSome x =
  match x with
  | Some _ -> assertSuccess()
  | _ -> assertFail()
let private rnd = new System.Random()
let internal randomDigit() = rnd.Next(0,10)
let internal randomDigitSequence count = List.init count (fun _ -> randomDigit())
let internal formatDate (x: DateTimeOffset) =
  sprintf "%02i%02i%02i" x.Day x.Month (x.Year % 100)