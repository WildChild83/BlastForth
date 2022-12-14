( ---------------------------------------------------------------------------- )
( **************************************************************************** )
(       String Variables using Dynamic Memory                                  )
( **************************************************************************** )
( ---------------------------------------------------------------------------- )

( ---------------------------------------------------------------------------- )
(                                                                              )
(       Dependencies:                                                          )
(           - forth.fs                                                         )
(           - core.fs                                                          )
(           - allocate.fs                                                      )
(                                                                              )
( ---------------------------------------------------------------------------- )
Forth definitions

( ---------------------------------------------------------------------------- )
synonym $init off

code ($free) ( $addr -- addr ) tos a1 move, [a1] tos move, [a1] clear, next
: $free ( $addr -- ) ($free) ?if free throw endif ;

code ($!1) ( addr u $addr -- addr u $addr u' $addr )
    d1 peek, tos push, cell # d1 add, d1 push, next
code ($!2) ( addr u $addr addr' -- addr addr' u )
    a1 pull, tos [a1] move, tos a1 move,
    tos pull, tos [a1]+ move, a1 push, next
: $! ( addr u $addr -- ) ($!1) $free allocate throw ($!2) move ;

code $@ ( $addr -- addr u )
    tos a1 move, [a1] a1 move, [a1]+ tos move, a1 push, next

code $@len ( $addr -- u ) tos a1 move, [a1] a1 move, [a1] tos move, next



: $. ( $addr -- ) $@ type ;

( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
















