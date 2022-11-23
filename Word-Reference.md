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

`/` *( n h -- h' )*  "slash"  
Divide a signed single-cell number *by a half-cell number,* producing a signed *half-cell* quotient.

`u/` *( u uh -- uh' )*  "U slash"  
Divide an unsigned single-cell number *by a half-cell number,* producing an unsigned *half-cell* quotient.

`mod` *( n h -- uh )*  "mod"  
Perform *signed* division and return the *modulus* (remainder).

`umod` *( u uh -- uh' )*  "U mod"  
Perform *unsigned* division and return the *modulus* (remainder).

`/mod` *( n h -- mod quot )*  "slash mod"  
Perform *signed* division and return the modulus and quotient.

`u/mod` *( u uh -- umod uquot )*  "U slash mod"  
Perform *unsigned* division and return the modulus and quotient.

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



