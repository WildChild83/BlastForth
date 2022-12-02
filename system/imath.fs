( ---------------------------------------------------------------------------- )
( **************************************************************************** )
(       Integer Math Routines                                                  )
( **************************************************************************** )
( ---------------------------------------------------------------------------- )

( ---------------------------------------------------------------------------- )
(                                                                              )
(       Dependencies:                                                          )
(           - 68k.fs                                                           )
(           - forth.fs                                                         )
(                                                                              )
( ---------------------------------------------------------------------------- )

( ---------------------------------------------------------------------------- )
(       Number Sizes                                                           )
( ---------------------------------------------------------------------------- )
code ?char ( n -- n flag )
    
    
    next


( ---------------------------------------------------------------------------- )
(       Multiplication                                                         )
( ---------------------------------------------------------------------------- )
{ ' * } anon
    d1 pull, d1 d2 h move, tos d2 mulu, d1 d3 h move, d1 swap,
    tos d1 mulu, d1 swap, d1 h clear, d2 d1 add, tos swap,
    d3 tos mulu, tos swap, tos h clear, d1 tos add, next
host/target: *

( ---------------------------------------------------------------------------- )
code um*        d1 pull, d1 d2 h move, tos d2 mulu, d1 swap, d1 d3 h move,
                tos d3 mulu, tos swap, tos d4 h move, d1 d4 mulu, d1 swap,
                tos d1 mulu, tos clear, d1 swap, d1 tos h move, d1 h clear,
                d2 d1 add, d4 tos addx, d2 clear, d3 swap, d3 d2 h move,
                d3 h clear, d3 d1 add, d2 tos addx, d1 push, next

code (m*1)      tos d1 move, neg if tos neg, endif d1 swap, d2 pull,
                neg if d2 neg, d1 h neg, endif d2 push, d1 h rpush, next
code (m*2)      d1 h rpull, neg if [sp] neg, tos negx, endif next
    : m* (m*1) um* (m*2) ;

( ---------------------------------------------------------------------------- )
(       Division                                                               )
( ---------------------------------------------------------------------------- )
rawcode (32/32) \ TOS=divisor, D1=dividend, quotient=D2, modulus=D3
                 \ clobbers D1-D5   credit: bearcave.com/software/divide.htm
    tos test, z= if 10 # tos move, throw-primitive, endif
    d1 tos compare, ugt if d2 clear, d1 d3 move, return, endif
    z= if 1 # d2 move, d3 clear, return, endif
    d2 clear, d3 clear, 32 # d4 move,
    begin 1 # d1 lsl, 1 # d3 roxl, d4 h dec, d3 tos compare, ult= until
    1 # d3 lsr, 1 # d1 roxr,
    d4 begin 1 # d1 lsl, 1 # d3 roxl, d3 d5 move, tos d5 sub,
             1 # d2 roxl, 0 # d2 bitchg, z= if d5 d3 move, endif loop
    return, end
code u/mod ( u1 u2 -- mod quot )
    d1 pull, $FFFF # tos compare, ult= if
        tos d1 divu, tos clear, d1 tos h move,
        d1 h clear, d1 swap, d1 push, next, endif
    (32/32) subroutine, d3 push, d2 tos move, next
code /modf ( n1 n2 -- mod quot )
    d1 pull, tos d2 move, $8000 # d2 add, $FFFF0000 # d2 and, z= if
        tos d2 h move, tos d1 divs, d1 tos h move, d1 swap, d1 h test,
        z<> if tos h test, neg if tos h dec, d2 d1 h add, endif endif
        tos ext, d1 ext, d1 push, next, endif
    tos d6 move, neg if tos neg, endif d1 d7 move, neg if d1 neg, endif
    (32/32) subroutine, d6 test, neg if d3 neg, endif d6 d7 xor,
    neg if d2 neg, d3 test, z<> if d2 dec, d3 neg, d6 d3 add, endif endif
    d3 push, d2 tos move, next
code /mods ( n1 n2 -- rem quot )
    d1 pull, tos d2 move, $8000 # d2 add, $FFFF0000 # d2 and, z= if
        tos d1 divs, d1 tos h move, tos ext,
        d1 swap, d1 ext, d1 push, next, endif
    tos d6 move, neg if tos neg, endif d1 d7 move, neg if d1 neg, endif
    (32/32) subroutine,
    d7 d6 xor, neg if d2 neg, endif d7 test, neg if d3 neg, endif
    d3 push, d2 tos move, next

: u/    ( u1 u2 -- quot ) u/mod   nip ;
:  /f   ( n1 n2 -- quot )  /modf  nip ;
:  /s   ( n1 n2 -- quot )  /mods  nip ;
: umod  ( u1 u2 -- mod )  u/mod  drop ;
:  modf ( n1 n2 -- mod )   /modf drop ;
:  mods ( n1 n2 -- rem )   /mods drop ;

rawcode (64/32) \ TOS=divisor, D2:D1=dividend, quotient=D3, modulus=D4
                \ clobbers D1-D6
    tos test, z= if 10 # tos move, throw-primitive, endif
     d2 test, z= if
        d1 tos compare, ugt if d3 clear, d1 d4 move, return, endif
        z= if 1 # d3 move, d4 clear, return, endif
    endif
    d3 clear, d4 clear, 64 # d5 move,
    begin 1 # d1 lsl, 1 # d2 roxl, 1 # d4 roxl, d5 h dec, d4 tos compare,
    ult= until 1 # d4 lsr, 1 # d2 roxr, 1 # d1 roxr,
    d5 begin 1 # d1 lsl, 1 # d2 roxl, 1 # d4 roxl, d4 d6 move, tos d6 sub,
             1 # d3 roxl, 0 # d3 bitchg, z= if d6 d4 move, endif loop
    return, end
code um/mod ( udlo udhi u -- mod quot )
    d2 pull, d1 pull, (64/32) subroutine, d4 push, d3 tos move, next

code fm/mod ( dlo dhi n -- mod quot ) next

code sm/rem ( dlo dhi n -- rem quot ) next

: u*/mod  ( u1 u2 u3 -- mod u' ) >r um* r> um/mod ;
:  */modf ( n1 n2 n3 -- mod n' ) >r  m* r> fm/mod ;
:  */mods ( n1 n2 n3 -- rem n' ) >r  m* r> sm/rem ;

: u*/  ( u1 u2 u3 -- u' ) u*/mod  nip ;
:  */f ( n1 n2 n3 -- n' )  */modf nip ;
:  */s ( n1 n2 n3 -- n' )  */mods nip ;

( ---------------------------------------------------------------------------- )
FlooredDivision [IF]
    synonym  /     /f
    synonym   mod   modf
    synonym  /mod  /modf
    synonym */mod */modf
    synonym */    */f
[ELSE]
    synonym  /     /s
    synonym   mod   mods
    synonym  /mod  /mods
    synonym */mod */mods
    synonym */    */s
[THEN]

( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )

















