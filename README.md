# SharpAlliance

## What's going on?

Good question. This is a port to C# of the original C code for Jagged Alliance 2: Gold
which is hosted at: https://github.com/historicalsource/jaggedalliance2

## Is it playable?

Not yet, not for awhile.
If you're wanting to play a ported JA2, check out: https://ja2-stracciatella.github.io/

## Okay, but this isn't very good C# code!

It is not!
The original C code is well written C code.
Since this port is almost a line by line conversion of C to C#,
at the beginning, some nasty patterns are being used to speed
up the port (mainly to help with Find/Replace).

For example, C has a ton of globals, C# rarely has a lot of 'globals' (static).
To avoid rewriting as lines are converted, I just throw globals into
the Globals.cs file and move on.

Once the porting is done, then an effort to convert to idiomatic C# will
start.

### What other short cuts are you taking?

1. When JA2 was written, memory was critical to manage. Now? Doesn't matter as much for the level we're talking about. So, when possible, turning all numbers into `int`.

1. Naming conventions:

* C enums are often named `LIKE_THIS_VALUE`, to help with find/replace, I create a C# enum called `LIKE_THIS` and then find/replace `LIKE_THIS_` to `LIKE_THIS.`

* All standard C# naming conventions are ignored for the time being.

1. Almost all pointers are just treated as the original type and nullable. Sometimes they get an `out` or a `ref`.

1. Linked lists and arrays in the C code are usually converted to `List<T>` cuts out a lot of scaffolding code.

## What's the catch?

Nothing, if you don't own the game, you won't be able to use this to get around purchasing it, this still
needs all the assets from the original game. It's often only $3 bucks or on GoG or Steam, so pay for it.