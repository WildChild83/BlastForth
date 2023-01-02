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
(       TODO:                                                                  )
(           gmplib.org/~tege/divcnst-pldi94.pdf                                )
(                                                                              )
( ---------------------------------------------------------------------------- )

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

: m*   (m*1) um* (m*2) ;

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
    d4 do 1 # d1 lsl, 1 # d3 roxl, d3 d5 move, tos d5 sub,
          1 # d2 roxl, 0 # d2 change-bit, z= if d5 d3 move, endif loop
    return, end
code u/mod ( u1 u2 -- mod quot )
    d1 pull, $10000 # tos compare, ult if
        tos d1 divu, tos clear, d1 tos h move,
        d1 h clear, d1 swap, d1 push, next, endif
    (32/32) subroutine, d3 push, d2 tos move, next
code /mod ( n1 n2 -- mod quot )
    d1 pull, tos d2 move, $8000 # d2 add, $FFFF0000 # d2 and, z= if
        tos d2 h move, tos d1 divs, d1 tos h move, d1 swap, d1 h test,
        z<> if tos h test, neg if tos h dec, d2 d1 h add, endif endif
        tos extend, d1 extend, d1 push, next, endif
    tos d6 move, neg if tos neg, endif d1 d7 move, neg if d1 neg, endif
    (32/32) subroutine, d6 test, neg if d3 neg, endif d6 d7 xor,
    neg if d2 neg, d3 test, z<> if d2 dec, d3 neg, d6 d3 add, endif endif
    d3 push, d2 tos move, next
code /rem ( n1 n2 -- rem quot )
    d1 pull, tos d2 move, $8000 # d2 add, $FFFF0000 # d2 and, z= if
        tos d1 divs, d1 tos h move, tos extend,
        d1 swap, d1 extend, d1 push, next, endif
    tos d6 move, neg if tos neg, endif d1 d7 move, neg if d1 neg, endif
    (32/32) subroutine, d7 d6 xor, neg if d2 neg, endif
    d7 test, neg if d3 neg, endif d3 push, d2 tos move, next

: u/   ( u1 u2 -- quot ) u/mod  nip ;
:  /f  ( n1 n2 -- quot )  /mod  nip ;
:  /s  ( n1 n2 -- quot )  /rem  nip ;
: umod ( u1 u2 -- mod )  u/mod drop ;
:  mod ( n1 n2 -- mod )   /mod drop ;
:  rem ( n1 n2 -- rem )   /rem drop ;

rawcode (64/32) \ TOS=divisor, D2:D1=dividend, quotient=D3, modulus=D4
                \ clobbers D1-D6
    $10000 # tos compare, ult if
        tos d2 divu, d1 swap, d1 d2 h move, tos d2 divu, d2 d3 h move,
        d1 swap, d1 d2 h move, tos d2 divu, d3 swap, d2 d3 h move,
        d2 swap, d4 clear, d2 d4 h move, return, endif
    tos test, z= if 10 # tos move, throw-primitive, endif
     d2 test, z= if
        d1 tos compare, ugt if d3 clear, d1 d4 move, return, endif
        z= if 1 # d3 move, d4 clear, return, endif endif
    d3 clear, d4 clear, 64 # d5 move,
    begin 1 # d1 lsl, 1 # d2 roxl, 1 # d4 roxl, d5 h dec, d4 tos compare,
        ult= until
    1 # d4 lsr, 1 # d2 roxr, 1 # d1 roxr,
    d5 do 1 # d1 lsl, 1 # d2 roxl, 1 # d4 roxl, d4 d6 move, tos d6 sub,
          1 # d3 roxl, 0 # d3 change-bit, z= if d6 d4 move, endif loop
    return, end
code um/mod ( udlo udhi u -- mod quot )
    d2 pull, d1 pull, (64/32) subroutine, d4 push, d3 tos move, next
code fm/mod ( dlo dhi n -- mod quot )
    d2 pull, d1 pull, tos d6 move, neg if tos neg, endif
    d2 d7 move, neg if d1 neg, d2 negx, endif d6 a1 move,
    (64/32) subroutine, a1 d6 move, neg if d4 neg, endif d6 d7 xor,
    neg if d3 neg, d4 test, z<> if d3 dec, d4 neg, d6 d4 add, endif endif
    d4 push, d3 tos move, next
code sm/rem ( dlo dhi n -- rem quot )
    d2 pull, d1 pull, tos d7 move, neg if tos neg, endif
    d2 d6 move, neg if d1 neg, d2 negx, endif d6 a1 move,
    (64/32) subroutine, a1 d6 move, a1 d6 move, neg if d4 neg, endif
    d6 d7 xor, neg if d3 neg, endif d4 push, d3 tos move, next

: u*/mod ( u1 u2 u3 -- mod u' ) >r um* r> um/mod ;
:  */mod ( n1 n2 n3 -- mod n' ) >r  m* r> fm/mod ;
:  */rem ( n1 n2 n3 -- rem n' ) >r  m* r> sm/rem ;

: u*/  ( u1 u2 u3 -- u' ) u*/mod nip ;
:  */f ( n1 n2 n3 -- n' )  */mod nip ;
:  */s ( n1 n2 n3 -- n' )  */rem nip ;

( ---------------------------------------------------------------------------- )
FlooredDivision [IF] synonym / /f  synonym */ */f
              [ELSE] synonym / /s  synonym */ */s  [THEN]

( ---------------------------------------------------------------------------- )
(       Roots and Powers                                                       )
( ---------------------------------------------------------------------------- )
create (log2-table) bytes[
   -1 0 1 1 2 2 2 2 3 3 3 3 3 3 3 3 4 4 4 4 4 4 4 4 4 4 4 4 4 4 4 4
    5 5 5 5 5 5 5 5 5 5 5 5 5 5 5 5 5 5 5 5 5 5 5 5 5 5 5 5 5 5 5 5
    6 6 6 6 6 6 6 6 6 6 6 6 6 6 6 6 6 6 6 6 6 6 6 6 6 6 6 6 6 6 6 6
    6 6 6 6 6 6 6 6 6 6 6 6 6 6 6 6 6 6 6 6 6 6 6 6 6 6 6 6 6 6 6 6
    7 7 7 7 7 7 7 7 7 7 7 7 7 7 7 7 7 7 7 7 7 7 7 7 7 7 7 7 7 7 7 7
    7 7 7 7 7 7 7 7 7 7 7 7 7 7 7 7 7 7 7 7 7 7 7 7 7 7 7 7 7 7 7 7
    7 7 7 7 7 7 7 7 7 7 7 7 7 7 7 7 7 7 7 7 7 7 7 7 7 7 7 7 7 7 7 7
    7 7 7 7 7 7 7 7 7 7 7 7 7 7 7 7 7 7 7 7 7 7 7 7 7 7 7 7 7 7 7 7 ]

code log2 ( u -- u' )
    (log2-table) [#] a1 address, tos d1 move, d1 swap, d1 h test,
    z<> if d1 d2 h move, 8 # d2 h lsr, d2 h test, z<> if
            d2 a1 h add, 24 # tos move, else d1 a1 w add, 16 # tos move, endif
     else tos d1 h move, 8 # d1 h lsr, d1 h test, z<> if
            d1 a1 h add, 8 # tos move, else tos a1 h add, tos clear, endif
    endif [a1] tos b add, tos c extend, next

create (log10-table) cells[
    1 10 100 1000 10000 100000 1000000 10000000 100000000 1000000000 ]

code (log10) ( u log2 -- u' )
    tos h inc, 1233 # tos mulu, 4 # tos lsl, tos h clear,
    tos swap, tos d1 move, d1 d1 h add, d1 d1 h add, d1 a1 h move,
    [a1 (log10-table) +] d1 move, [sp]+ d1 compare,
    gt if tos h dec, endif next
: log10 ( u -- u' ) dup log2 (log10) ;

code root2 ( u -- u' )
    d1 clear, 1 # d2 move, begin 2 # d2 ror, d2 tos compare, ugt= until
    begin d2 test, z<> while
        d2 d3 move, d1 d3 add, 1 # d1 lsr, d3 tos compare,
        ugt= if d3 tos sub, d2 d1 add, endif 2 # d2 lsr, repeat
    d1 tos move, next       aka sqrt

code power2 ( u -- u' )
    tos dec, tos d1 move,
    1 # d1 lsr, d1 tos or, tos d1 move, 1 # d1 lsr, d1 tos or, tos d1 move,
    2 # d1 lsr, d1 tos or, tos d1 move, 4 # d1 lsr, d1 tos or, tos d1 move,
    8 # d1 lsr, d1 tos or, tos inc, next

( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )

















