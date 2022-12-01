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
code  h*        2 # sp add, [sp]+ tos h muls, next
code uh*        2 # sp add, [sp]+ tos h mulu, next

( ---------------------------------------------------------------------------- )
(       Division                                                               )
( ---------------------------------------------------------------------------- )
code h/         d1 pull, tos d1 divs, d1 tos h move, tos ext, next
code hmod       d1 pull, tos d1 divs, d1 swap, d1 tos h move, tos ext, next
code h/mod      d1 pull, tos d1 divs, d1 tos h move, tos ext,
                d1 swap, d1 ext, d1 push, next

code uh/        d1 pull, tos d1 divu, tos clear, d1 tos h move, next
code uhmod      d1 pull, tos d1 divs, d1 swap, tos clear, d1 tos h move, next
code uh/mod     d1 pull, tos d1 divs, tos clear, d1 tos h move,
                d1 h clear, d1 swap, d1 push, next

( ---------------------------------------------------------------------------- )
rawcode (64/32) \ TOS=divisor, D2:D1=dividend, result=TOS,
    
    return, end


code (um/mod)   next 


code (sm/rem)   next


( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )

















