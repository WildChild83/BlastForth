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
hvariable (node)

code ram ( haddr -- addr ) -1 # d1 move, tos d1 h move, d1 tos move, next

: nodesize  ( node -- u )         half+ h@ ;
: nextnode  ( node -- node' )     h@ ram ;
: ?lastnode ( node -- node flag ) h@ 0= ;

: .node ( node -- )
    ." a=" dup $FFFF and hex.
    ." n=" dup h@ hex.
    ." s=" nodesize . ;

: init-memory ( -- )
    $FFFC dup (node) h!
    $10000 pad $FFFF and -   (node) nextnode ! ;

( ---------------------------------------------------------------------------- )
: (alloc-find) ( u -- prev this u' )
    half+ >r (node) dup nextnode
    begin dup nodesize r@ < while
        nip dup nextnode ?lastnode -59 and throw
    repeat r> ;

: allocate ( u -- addr )
    
;

( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )















