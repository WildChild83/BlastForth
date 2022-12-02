# BlastForth Word Reference

Forth environments use a *dual stack* machine architecture.  The *Data Stack* holds the numbers that get passed as arguments to the various words, and it also holds the return values produced by those words.  The *Return Stack* holds the return addresses of nested subroutine calls.  The use of a dual-stack architecture means there is no need to allocate a "stack frame," nor push registers, when a word invokes another word.  This makes subroutine calls very fast.

Since the Data Stack is the primary structure the programmer works with, it is usually just called "the stack." 

This reference uses a *stack effect notation* of the form *( operands -- results )*, where "operands" represents the items on the stack before the word is invoked, and "results" shows the state of the stack after the word returns.  The operands and results portions both have the top-of-stack item on the right and deeper items toward the left.

## Data Stack Manipulators

`swap` *( a b -- b a )*  
Exchange the top two stack items.

`dup` *( a -- a a )*  
*Duplicate* the topmost stack item, leaving the copy on top of the item copied.

`drop` *( a -- )*  
*Drop* the topmost stack item (pop it and discard the value).

`over` *( a b -- a b a )*  
Copy the second-to-top item, making the copy the new top-of-stack item.

`nip` *( a b -- b )*  
Discard the second-to-top stack item.  Equivalent to `swap drop`.

`tuck` *( a b -- b a b )*  
Copy the topmost item, making the copy the *third* stack item.  Equivalent to `swap over`.

`rot` *( a b c -- b c a )*  "rote"  
*Rotate* the third-to-top item to the top of the stack.

`-rot` *( a b c -- c a b )*  "minus rote"  
*Reverse rotate* the topmost stack item to the third position.

`2swap` *( a1 a2 b1 b2 -- b1 b2 a1 a2 )*  "two swap"  
`2dup` *( a1 a2 -- a1 a2 a1 a2 )*  "two doop"  
`2drop` *( a1 a2 -- )*  "two drop"  
`2over` *( a1 a2 b1 b2 -- a1 a2 b1 b2 a1 a2 )*  "two over"  
`2nip` *( a1 a2 b1 b2 -- b1 b2 )*  "two nip"  
`2tuck` *( a1 a2 b1 b2 -- b1 b2 a1 a2 b1 b2 )*  "two tuck"  
`2rot` *( a1 a2 b1 b2 c1 c2 -- b1 b2 c1 c2 a1 a2 )*  "two rote"  
`-2rot` *( a1 a2 b1 b2 c1 c2 -- c1 c2 a1 a2 b1 b2 )*  "minus two rote"  
`2-rot` is a synonym of `-2rot`  
Analogous to their single-cell counterparts, these words operate on double-cell (64-bit) numbers.

`?dup` *( a --  0 | a a )*  "question dup"  
Duplicate the topmost stack item if and only if it is non-zero.

`third` *( a b c -- a b c a )*  
Copy the thirdmost item to the top of the stack.

`fourth` *( a b c d -- a b c d a )*  
Copy the fourthmost item to the top of the stack.

`pick` *( xn..x0  n -- xn..x0  xn )*  
Copy the *nth* item to the top of the stack.  `1 pick` is equivalent to `over` and `0 pick` is equivalent to `dup`.

`roll` *( xn..x0  n -- xn-1..x0  xn )*  
Rotate the *nth* item to the top of the stack.  `2 roll` is equivalent to `rot`, `1 roll` to `swap`, and `0 roll` is a null operation.

## Return Stack Manipulators

`>r` *( n -- ) ( R: -- n )*  "to R"  
Move the topmost item on the Data Stack to the top of the Return Stack.

`r>` *( -- n ) ( R: n -- )*  "R from"  
Move the topmost item on the Return Stack back to the Data Stack.

`r@` *( -- n ) ( R: n -- n )*  "R fetch"  
*Copy* the topmost Return Stack item, leaving the Return Stack unmodified.

`rdrop` *( R: n -- )*  "R drop"  
Discard the topmost Return Stack item.

`2>r` *( a1 a2 -- ) ( R: -- a1 a2 )*  "two to R"  
`2r>` *( -- a1 a2 ) ( R: a1 a2 -- )*  "two R from"  
`2r@` *( -- a1 a2 ) ( R: a1 a2 -- a1 a2 )*  "two R fetch"  
`2rdrop` *( R: a1 a2 -- )*  "two R drop"  
Move double-cell numbers to and from the Return Stack.

`rsecond` *( -- n1 ) ( R: n1 n2 -- n1 n2 )*  "R second"  
Copy the second-to-top Return Stack item to the Data Stack.

`rthird` *( -- n1 ) ( R: n1 n2 n3 -- n1 n2 n3)*  "R third"  
Copy the thirdmost Return Stack item to the Data Stack.

## Comments

`(` *( -- )*  "paren"  
Begin a *block comment* that is terminated with a `)`.  Everything between `(` and `)` is ignored by compiler.  The `(` *must* have one or more whitespace characters immediatly following it.  If it is not followed by whitespace, as in `(emit)` for instance, then the `(` is understood to be part of a name and not a comment.

`\` *( -- )*  "backslash"  
Begin a *line comment*.  The compiler discards everything between `\` and the end of the line.  Like `(`, the `\` must be immediately followed by whitespace in order to be a comment.

## Arithmetic Operators

`+` *( n1 n2 -- n' )*  "plus"  
Add the top two stack items together, leaving their sum on the stack.

`-` *( n1 n2 -- n' )*  "minus"  
Subtract the top item from the second-to-top item (*n'*=*n1*-*n2*).

`negate` *( n -- n' )*  "negate"  
Perform arithmetic (two's complement) inversion on the top stack item.

`d+` *( d1lo d1hi  d2lo  d2hi -- d'lo d'hi )*  "D plus"  
*Double-cell* addition: add two double-cell (64-bit) numbers and return the double-cell result.

`d-` *( d1lo d1hi  d2lo  d2hi -- d'lo d'hi )*  "D minus"  
*Double-cell* subtraction: subtract *d2* from *d1* and return the double-cell result.

`dnegate` *( dlo dhi -- d'lo d'hi )*  "D negate"  
*Double-cell* negation: *d'* is the arithmetic inverse of *d*.

`m+` *( dlo dhi  n -- d'lo d'hi )*  "M plus"  
*Mixed* addition: add a *signed* number to a double-cell (64-bit) number.

`um+` *( dlo dhi  u -- d'lo 'dhi )*  "U M plus"  
*Unsigned mixed* addition: add an unsigned number to a double-cell number.

`*` *( n1 n2 -- n' )*  "star"  
Multiply two single-cell (32-bit) numbers, producing a single-cell result.  Works on both signed and unsigned numbers.

`m*` *( n1 n2 -- dlo dhi )*  "M star"  
*Mixed* multiplication: multiply two signed single-cell numbers, producing a signed double-cell result.

`um*` *( u1 u2 -- udlo udhi )*  "U M star"  
*Unsigned mixed* muliplication: multiply two unsigned single-cell numbers, producing an unsigned double-cell result.

`h*` *( h1 h2 -- n )*  "H star"  
Multiply two signed half-cell (16-bit) numbers, producing a signed single-cell result.

`uh*` *( uh1 uh2 -- u )*  "U H star"  
Multiply two unsigned half-cell numbers, producing an unsigned single-cell result.

`um/mod` *( udlo udhi u -- umod uquot )*  "U M slash mod"  
Perform *unsigned* division.  Divide double-cell number *ud* by *u*, returning the quotient and modulus (remainder).

`fm/mod` *( dlo dhi n -- mod quot )*  "F M slash mod"  
Perform *floored signed* division.  The quotient is always rounded toward negative infinity and the modulus is always positive.

`sm/rem` *( dlo dhi n -- rem quot )*  "S M slash rem"  
Perform *symmetric signed* division.  The quotient is rounded toward zero and the remainder can be positive or negative.

*NOTE: The following operators use symmetric division by default.  This can be changed in the Project Attributes portion of the "system.fs" file if you want floored division instead.*

`/` *( n1 n2 -- n' )*  "slash"  
Divide *n1* by *n2* using signed division.

`u/` *( u1 u2 -- u' )*  "U slash"  
Divide *u1* by *u2* using unsigned division.

`mod` *( n1 n2 -- n')*  "mod"  
Perform *signed* division and return the *modulus* (remainder).

`umod` *( u1 u2 -- u' )*  "U mod"  
Perform *unsigned* division and return the *modulus* (remainder).

`/mod` *( n1 n2 -- mod quot )*  "slash mod"  
Perform *signed* division and return the modulus and quotient.

`u/mod` *( u1 u2 -- umod uquot )*  "U slash mod"  
Perform *unsigned* division and return the modulus and quotient.

`*/` *( n1 n2 n3 -- n' )*  "star slash"  
Multiply *n1* by *n2* producing a double-cell intermediate result, then divide it by *n3* producing a single-cell final result.  This word allows fractions to be implemented with integers only, without risk of overflow.

`*/mod` *( n1 n2 n3 -- mod n' )*  "star slash mod"  
Like `*/` but also returns the modulus (remainder) of the division operation.

`u*/` *( u1 u2 u3 -- u' )*  "U star slash"  
`u*/mod` *( u1 u2 u3 -- mod u' )*  "U star slash mod"  
Like `*/` and `*/mod` but these work on unsigned numbers.

`/f` *( n1 n2 -- quot )*  "slash F"  
`modf` *( n1 n2 -- mod )*  "mod F"  
`/modf` *( n1 n2 -- mod quot )*  "slash mod F"  
`*/f` *( n1 n2 n3 -- n' )*  "star slash F"  
`*/modf` *( n1 n2 -- mod n' )*  "star slash mod F"  
Force the use of floored division.

`/s` *( n1 n2 -- quot )*  "slash S"  
`mods` *( n1 n2 -- rem )*  "mod S"  
`/mods` *( n1 n2 -- rem quot )*  "slash mod S"  
`*/s` *( n1 n2 n3 -- n' )*  "star slash S"  
`*/mods` *( n1 n2 -- rem n' )*  "star slash mod S"  
Force the use of symmetric division.

`/h` *( n h -- quot )*  "slash H"  
`modh` *( n h -- rem )*  "mod H"  
`/modh` *( n h -- rem quot )*  "slash mod H"  
Divide *n* by *half-cell* number *h*.  The upper 16 bits of *h* are ignored.  These operators perform only a single hardware division, making them faster than the others.  Hardware division on the 68k CPU is always symmetric.

`s>d` *( n -- dlo dhi )*  "S to D"  
Convert a signed *single*-cell number to a *double*-cell number.

`d>s` *( dlo dhi -- n )*  "D to S"  
Convert a *double*-cell number to a *single*-cell number.  Equivalent to `drop`.

`1+` *( n -- n' )*  "one plus"  
`2+` *( n -- n' )*  "two plus"  
`4+` *( n -- n' )*  "four plus"  
`8+` *( n -- n' )*  "eight plus"  
`1-` *( n -- n' )*  "one minus"  
`2-` *( n -- n' )*  "two minus"  
`4-` *( n -- n' )*  "four minus"  
`8-` *( n -- n' )*  "eight minus"  
Increment or decrement by a small power of 2.  These are faster than `+` and `-`.

`2*` *( n -- n' )*  "two star"  
`4*` *( n -- n' )*  "four star"  
`8*` *( n -- n' )*  "eight star"  
`2/` *( n -- n' )*  "two slash"  
`4/` *( n -- n' )*  "four slash"  
`8/` *( n -- n' )*  "eight slash"  
Multiply or divide by a small power of 2.  These are faster than `lshift` and `rshift`.

`d1+` *( dlo dhi -- d'lo d'hi )*  "D one plus"  
`d1-` *( dlo dhi -- d'lo d'hi )*  "D one minus"  
`d2*` *( dlo dhi -- d'lo d'hi )*  "D two star"  
`d2/` *( dlo dhi -- d'lo d'hi )*  "D two slash"  

## Bitwise Operators

`and` *( n1 n2 -- n' )*  "and"  
Logically "and" the arguments together and return the result.

`or` *( n1 n2 -- n' )*  "or"  
Perform an *inclusive* "or" operation.

`xor` *( n1 n2 -- n' )*  "xor"  
Perform an *exclusive* "or" operation.

`invert` *( n -- n' )*  "invert"  
Perform logical (one's complement) inversion.

`lshift` *( n u -- n' )*  "L shift"  
Logically shift N to the left by U bit positions.

`rshift` *( n u -- n' )*  "R shift"  
Logically shift N to the right by U bit positions.

`arshift` *( n u -- n' )*  "A R shift"  
*Arithmetically* shift N to the right, preserving the sign of N.

`dlshift` *( dlo dhi  u -- d'lo d'hi )*  "D L shift"  
Left shift the double-cell number D by U bit positions.

`drshift` *( dlo dhi  u -- d'lo d'hi )*  "D R shift"  
Right shift the double-cell number D by U bit positions.

`lrotate` *( n u -- n' )*  "L rotate"  
Perform a 32-bit leftward rotation of N by U bit positions.

`rrotate` *( n u -- n' )*  "R rotate"  
Perform a 32-bit rightward rotation of N by U bit positions.

`hlrotate` *( h u -- h' )*  "H L rotate"  
16-bit (*half*-cell) leftward rotation.

`hrrotate` *( h u -- h' )*  "H R rotate"  
16-bit (*half*-cell) rightward rotation.

`clrotate` *( c u -- c' )*  "C L rotate"  
8-bit (*char*) leftward rotation.

`crrotate` *( c u -- c' )*  "C R rotate"  
8-bit (*char*) rightward rotation.

`mux` *( n1 n2 mask -- n' )*  "mux"  
*Multiplex* two numbers.  Logically "and" N2 with the mask, and N1 with the mask's inverse, and "or" the results together.

`demux` *( n mask -- n1 n2 )*  "demux"  
*De-multiplex* a number.  N2 is the logical "and" of N with the mask, and N1 is the "and" of N with the mask's inverse.

`true` *( -- flag )*  "true"  
Push a *true flag* onto the stack.  A true flag is a cell with all bits set.

`false` *( -- flag )*  "false"  
Push a *false flag* onto the stack.  A false flag has all bits cleared.

## Flow Control

`if` *( n -- )*  
If *n* is non-zero, continue normal execution.  If *n* is zero, branch forward to a corresponding `else`, `then`, or `endif`.

`else` *( -- )*  
Branch forward to a corresponding `then` or `endif`.

`then` *( -- )*  
Resolve a forward branch and mark the end of a control structure.

`endif` is a synonym for `then`  
Some programmers find `if..else..endif` to be more natural than `if..else..then`.

`exit` *( -- )*  
Terminate execution of the current word and return to the calling word.  The address to return to is at the top of the Return Stack.  Usually placed inside an `if..then` structure to perform a conditional exit.

`ahead` *( -- )*  
Branch forward to a corresponding `then`.  This word exists because it is compiled by `else`, but programmers rarely ever use it directly.

`begin` *( -- )*  
Mark the origin of a `begin..again`, `begin..until`, or `begin..while..repeat` structure.

`again` *( -- )*  
Branch backward to the corresponding `begin`.  A `begin..again` structure is an infinite loop.

`until` *( n -- )*  
If *n* is non-zero, leave the control structure and continue normal execution.  If *n* is zero, branch backward to the corresponding `begin`.

`while` *( n -- )*  
If *n* is non-zero, continue normal execution.  If *n* is zero, branch forward to a corresponding `repeat` and leave the control structure.

`repeat` *( -- )*  
Branch backward to the corresponding `begin`, skipping past the intermediate `while`.

`do` *( limit index -- ) ( R: -- limit index )*  
Move the loop parameters *limit* and *index* to the Return Stack, and mark the origin of a `do..loop` structure.

`loop` *( R: limit index -- limit index' |  )*  
Increment *index* by 1.  If *index* equals *limit*, drop the loop parameters from the Return Stack and continue normal execution.  If *index* hasn't reached *limit* yet, branch backward to the corresponding `do` or `?do`.

`?do` *( limit index -- ) ( R: --  | limit index )*  "question do"  
Check *index* against *limit*.  If they are equal, drop them both and branch forward to a corresponding `loop`, `+loop`, or `-loop`, skipping past the loop entirely.  Otherwise perform the function of `do`.  

`+loop` *( n -- ) ( R: limit index -- limit index' |  )*  "plus loop"  
`-loop` *( n -- ) ( R: limit index -- limit index' |  )*  "minus loop"  
Similar to `loop` except the step value *n* is added to or subtracted from *index* instead of incrementing *index* by 1.  `+loop` adds *n* to *index* and `-loop` subtracts *n* from *index*.  `+loop` terminates the loop when *index* crosses the boundary between *limit minus 1* and *limit*, and `-loop` terminates when *index* crosses the boundary between *limit plus 1* and *limit*.  The use of negative step values is permitted but not recommended because it tends to make the program difficult to follow.

`unloop` *( R: limit index -- )*  
Discard the loop parameters from the Return Stack.  You must always remember to `unloop` before `exit`ing a word from inside a `do..loop`.  If you forget then the machine will interpret the *index* parameter as a return address and behave badly!

`leave` *( -- ) ( R: limit index -- )*  
Discard the loop parameters and branch forward to a corresponding `loop`, `+loop`, or `-loop`, terminating the loop prematurely.  This is equivalent to factoring the `do..loop` into its own word definition and performing `unloop exit`.

 `culminate` *( R: limit index -- limit index' )*  "culminate"  
`+culminate` *( R: limit index -- limit index' )*  "plus culminate"  
`-culminate` *( R: limit index -- limit index' )*  "minus culminate"  
`/culminate` *( R: limit index -- limit index' )*  "slash culminate"  
Similar to `leave` except these merely adjust the loop parameters so that the loop terminates at the end of the current iteration.  The current loop iteration continues and finishes normally.  Which version of `culminate` you should use depends on which version of `loop` you're in, and whether the step value is positive or negative.  Here are the guidelines:

Use `culminate` with `loop`.  
If the step value is *positive*, use `+culminate` with `+loop` and `-culminate` with `-loop`.  
If the step value is *negative*, use `/culminate`.  

## Memory Access

`@` *( addr -- n )*  "fetch"  
`h@` *( addr -- h )*  "H fetch"  
`c@` *( addr -- c )*  "C fetch"  
`2@` *( addr -- dlo dhi )*  "two fetch"  
*Fetch* an item from memory address *addr*.  The fetched item becomes the new top-of-stack item, replacing the address.  Different versions exist for fetching 32-bit *cells* (`@`), 16-bit *half cells* (`h@`), 8-bit *chars* (`c@`), and 64-bit *double cells* (`2@`).

`!` *( n addr -- )*  "store"  
`h!` *( h addr -- )*  "H store"  
`c!` *( c addr -- )*  "C store"  
`2!` *( dlo dhi addr -- )*  "two store"  
*Store* a stack item into memory at the given address.  The number of bytes written to memory depends on which version of store is used.

`sh@` *( addr -- n )*  "S H fetch"  
`sc@` *( addr -- n )*  "S C fetch"  
Fetch a half cell or char and *sign extend* it to a full 32-bit cell.

`+!` *( n addr -- )*  "plus store"  
`h+!` *( h addr -- )*  "H plus store"  
`c+!` *( c addr -- )*  "C plus store"  
`2+!` *( dlo dhi addr -- )*  "two plus store"  
*Add* a stack item to the number at the given memory address, and *store* the result at the address in memory.

## Defining Words

Aka "words that create new words."  Certain words in this section parse the next token from the input buffer and create a new word with that token as its name.  This is annotated with *"name"* in the Defining Word's stack comment.

`create` *( "name" -- )*  "create"  
*Create* a new word named *"name"* and place its code field in ROM at the current position of the ROMspace pointer.  When invoked, the new word pushes its ROM address onto the Data Stack, allowing software to access whatever data is at that position in ROM.  This region of data is called the *data field* of the Created Word.  The "type" of the data depends on how the Created Word is used by other parts of the program.  All other Defining Words implicitly invoke `create`.

*Example usage:*  
`create base-stats  25 ,  50 ,  40 ,  10 ,`

`,` *( n -- )*  "comma"  
`h,` *( h -- )*  "H comma"  
`c,` *( c -- )*  "C comma"  
`2,` *( dlo dhi -- )*  "two comma"  
*Compile* the top-of-stack item into the *data field* in ROM of the most recent Created Word.  Different versions exist for compiling 32-bit *cells* (`,`), 16-bit *half cells* (`h,`), 8-bit *chars* (`c,`), and 64-bit *double cells* (`2,`).

`:` *( "name" -- )*  "colon"  
Create a new *colon definition* and switch the environment to *compilation state*.  Compile each subsequent word's address into the *data field* of the new colon definition (using `,`), until a `;` (semicolon) is encountered.  Once compiled, the new word can be executed analogously to a "subroutine" call.

*Example usage:*  
`: checkpoint ( n -- ) start-location !   rings @ 50 >= if bonus-stage endif ;`

`;` *( -- )*  "semicolon"  
Finalize the currently-compiling colon definition, and switch the environment back to *interpretation state*.

`constant` *( n "name" -- )*  "constant"  
Create a new *constant* and place *n* in the constant's data field.  When invoked, the new word pushes *n* onto the Data Stack.

*Example usage:*  
`7 constant max-speed`  
`... speed @ max-speed > if max-speed  speed ! endif ...`  

`buffer:` *( u "name" -- )*  "buffer colon"  
Create a new *RAM buffer*.  Place the current value of the RAMspace pointer into the buffer's *data field* and advance the pointer by *n* bytes.  When invoked, the buffer will push it's RAM address onto the stack for software to access, i.e. with `@` (fetch) or `!` (store).  All Defining Words that allot space in RAM implicitly invoke `buffer:`.

`variable` *( "name" -- )*  "variable"  
Create a new user variable.  A `variable` is a `buffer:` with one cell (4 bytes) allotted to it.

*Example usage:*  
`variable hitpoints`  
`: heal ( n -- ) hitpoints +! ;`  
`: damage ( n -- ) hitpoints @ swap - dup hitpoints ! 0< if die endif ;`  

`value` *( "name" -- )*  "value"  
Create a new user value.  A `value` is the same as a `variable`, except whereas a `variable` pushes its address when invoked, a `value` automatically fetches the number at its address and pushes that.  To assign a number to a `value`, use `to` as explained below.  Under certain circumstances, a `value` might be more convenient than a `variable`.

*Example usage:*  
`value alive`  
`: die ( -- ) false to alive`  
`: mainloop ( -- ) begin alive while ... repeat ;`  

`hvariable` *( "name" -- )*  "H variable"  
`cvariable` *( "name" -- )*  "C variable"  
`2variable` *( "name" -- )*  "two variable"  
`hvalue` *( "name" -- )*  "H value"  
`cvalue` *( "name" -- )*  "C value"  
`2value` *( "name" -- )*  "two value"  
Allot RAM buffers of various widths.



