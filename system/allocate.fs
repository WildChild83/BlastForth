( ---------------------------------------------------------------------------- )
( **************************************************************************** )
(       Heaps of Memory                                                        )
( **************************************************************************** )
( ---------------------------------------------------------------------------- )

( ---------------------------------------------------------------------------- )
(                                                                              )
(       Dependencies:                                                          )
(           - 68k.fs                                                           )
(           - forth.fs                                                         )
(           - core.fs                                                          )
(                                                                              )
( ---------------------------------------------------------------------------- )
Forth definitions

( ---------------------------------------------------------------------------- )
alignram 0 buffer: pad

( ---------------------------------------------------------------------------- )
code ram ( haddr -- addr ) -1 # d1 move, tos d1 h move, d1 tos move, next

synonym (node) false
8 { make (alloc-minimum) }

: nodesize  ( node -- u )         cell- h@ ;
: nextnode  ( node -- node' )     half- h@ ram ;
: ?lastnode ( node -- node flag ) dup half- h@ 0= ;

: @node ( node -- next size ) dup nextnode swap nodesize ;
: !node ( next size node -- ) tuck cell- h! half- h! ;
: !next ( next node -- ) half- h! ;

: allocation ( addr -- u ) half- h@ ;

: .node ( node -- )
    ." a=" dup $FFFF and hex.
    ." n=" dup half- h@ hex.
    ." s=" nodesize . ;

: init-memory ( -- )
    $FFFE (node) !next
    0 $FFFE pad $FFFF and - (node) nextnode !node ;

: .freelist ( -- )
    (node) begin ?lastnode not while nextnode dup cr .node repeat drop ;

( ---------------------------------------------------------------------------- )
code allocate ( u -- addr )
    (alloc-minimum) # tos h compare, lt if (alloc-minimum) # tos move, endif
    3 # tos h add, -2 # tos b and, a1 clear, -1 # d1 move, begin 
        [a1 -2 +] d1 h move,
        z= if tos push, -59 # tos move, throw-primitive, endif
        d1 a2 move, [a2 -4 +] tos h compare, ugt while a2 a1 move, repeat
    [a2 -2 +] d1 h move, [a2 -4 +] d2 h move, tos d2 h sub,
    (alloc-minimum) # d2 h compare, ult= if
        [a2 -4 +] tos h move, d1 [a1 -2 +] h move, tos a2 sub, else
        tos a2 sub, d1 [a2 -2 +] h move, d2 [a2 -4 +] h move,
        a2 [a1 -2 +] h move, endif
    2 # tos h sub, tos [a2]+ h move, a2 tos move, next

( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
















