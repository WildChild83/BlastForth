# BlastForth Word Reference

### Data Stack Manipulators

**swap ( a b -- b a )**  
Exchange the top two stack items.

**dup ( a -- a a )**  
*Duplicate* the topmost stack item.

**drop ( a -- )**  
*Drop* the topmost stack item (pop it and discard the value).

**over ( a b -- a b a )**  
Copy the second-to-top item, making the copy the new top-of-stack item.

**tuck ( a b -- b a b )**  
Copy the topmost item, making the copy the *third* stack item.

**nip ( a b -- b )**  
Discard the second-to-top stack item.

**rot ( a b c -- b c a )**          *"rote"*  
*Rotate* the third-to-top item to the top of the stack.

**-rot ( a b c -- c a b )**         *"minus rote"*  
*Reverse rotate* the topmost stack item to the third position.

**2swap ( a1 a2 b1 b2 -- b1 b2 a1 a2 )**  
**2dup  ( a1 a2 -- a1 a2 a1 a2 )**  
**2drop ( a1 a2 -- )**  
**2over ( a1 a2 b1 b2 -- a1 a2 b1 b2 a1 a2 )**  
**2tuck ( a1 a2 b1 b2 -- b1 b2 a1 a2 b1 b2 )**  
**2nip  ( a1 a2 b1 b2 -- b1 b2 )**  
**2rot  ( a1 a2 b1 b2 c1 c2 -- b1 b2 c1 c2 a1 a2 )**  
**-2rot ( a1 a2 b1 b2 c1 c2 -- c1 c2 a1 a2 b1 b2 )**  
**2-rot** synonym of **-2rot**  
Analogous to their single-cell counterparts, these words operate on double cell (64-bit) numbers.

**?dup ( a --  0 | a a )**          *"question dup"*  
Duplicate the topmost stack item if and only if it is non-zero.

**third ( a b c -- a b c a )**  
Copy the third-to-top item to the top of the stack.

**fourth ( a b c d -- a b c d a )**  
Copy the fourth-to-top item to the top of the stack.

**pick ( xn..x0  n -- xn..x0  xn )**  
Copy the *nth* item to the top of the stack.  **1 pick** is equivalent to **over** and **0 pick** is equivalent to **dup**.

**roll ( xn..x0  n -- xn-1..x0  xn )**  
Rotate the *nth* item to the top of the stack.  **2 roll** is equivalent to **rot**, **1 roll** to **swap**, and **0 roll** is a null operation.

### Return Stack Manipulators

**>r ( n -- ) ( R: -- n )**         *"to R"*  
Move the topmost stack item to the top of the Return Stack.

**r> ( -- n ) ( R: n -- )**         *"R from"*  
Move the topmost item on the Return Stack back to the Data Stack.

**r@ ( -- n ) ( R: n -- n )**       *"R fetch"*  
*Copy* the topmost Return Stack item, leaving the Return Stack unmodified.



