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

: nodesize  ( node -- u )         cell- h@ ;
: nextnode  ( node -- node' )     half- h@ ram ;
: ?lastnode ( node -- node flag ) half- h@ 0= ;

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

( ---------------------------------------------------------------------------- )
code (alloc-find) ( u -- prev this u' )
    2 # tos h add, a1 clear, -1 # d1 move, begin 
        [a1 -2 +] d1 h move,
        z= if tos push, -59 # tos move, throw-primitive, endif
        d1 a2 move, [a2 -4 +] tos h compare, ugt while a2 a1 move, repeat
    a1 push, a2 push, next
    
code (alloc-addr)* ( prev this u -- addr )
    \ TOS=size+2
    a2 pull, \ A2=this
    a1 pull, \ A1=prev
    -1 # d1 move, [a2 -2 +] d1 h move, \ D1=next
    [a2 -4 +] d2 h move, tos d2 h sub, \ D2=this.newsize
    
    
    
    a2 a3 move, tos a3 sub, \ A3=addr
    
    tos d1 h move, 2 # d1 h sub,
    d1 [a2
    next

: (alloc-addr) ( prev this u -- prev this u addr )
    2dup - half- 2dup h! half+ ;

code (alloc-store) ( prev this u -- prev this next )
    
    next

: allocate ( u -- addr )
    (alloc-find) (alloc-addr)*
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















