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
Copy the third-to-top item to the top of the stack.

`fourth` *( a b c d -- a b c d a )*  
Copy the fourth-to-top item to the top of the stack.

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

`rsecond` *( -- n1 ) ( R: n1 n2 -- n1 n2 )*  "R second"  
Copy the second-to-top Return Stack item to the Data Stack.

`rdrop` *( R: n -- )*  "R drop"  
Discard the topmost Return Stack item.

`2>r` *( a1 a2 -- ) ( R: -- a1 a2 )*  "two to R"  
`2r>` *( -- a1 a2 ) ( R: a1 a2 -- )*  "two R from"  
`2r@` *( -- a1 a2 ) ( R: a1 a2 -- a1 a2 )*  "two R fetch"  
`2rdrop` *( R: a1 a2 -- )*  "two R drop"  
Move double-cell numbers to and from the Return Stack.

## Arithmetic Operators

`+` *( n1 n2 -- n' )*  "plus"  
Add the top two stack items together, leaving their sum on the stack.

`-` *( n1 n2 -- n' )*  "minus"  
Subtract the top item from the second-to-top item (*n'*=*n1*-*n2*).

`negate` *( n -- n' )*  
Perform arithmetic (two's complement) negation on the top stack item.

`1+` *( n -- n' )*  "one plus"  
`2+` *( n -- n' )*  "two plus"  
`4+` *( n -- n' )*  "four plus"  
`8+` *( n -- n' )*  "eight plus"  
`1-` *( n -- n' )*  "one minus"  
`2-` *( n -- n' )*  "two minus"  
`4-` *( n -- n' )*  "four minus"  
`8-` *( n -- n' )*  "eight minus"  
Increment or decrement by the specified amount.  These are faster than `+` and `-`.

`2*` *( n -- n' )*  "two star"  
`4*` *( n -- n' )*  "four star"  
`8*` *( n -- n' )*  "eight star"  
`2/` *( n -- n' )*  "two slash"  
`4/` *( n -- n' )*  "four slash"  
`8/` *( n -- n' )*  "eight slash"  
Multiply or divide by a small power of 2.  These are faster than `lshift` and `rshift`.



