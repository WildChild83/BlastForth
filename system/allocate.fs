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
(       TODO:                                                                  )
(           - make resize not dumb                                             )
(                                                                              )
( ---------------------------------------------------------------------------- )
Forth definitions

( ---------------------------------------------------------------------------- )
alignram 0 buffer: pad

16 cells constant pad-size          8 { make (alloc-minimum) }

( ---------------------------------------------------------------------------- )
(                                                                              )
(       Free Node is 4 bytes, pointer points at "5th" byte                     )
(           - pointer minus 2 = pointer to next node                           )
(           - pointer minus 4 = size of this node                              )
(       First Node, at address $FFFFFE, is only 2 bytes                        )
(           - is only a "next" pointer                                         )
(                                                                              )
( ---------------------------------------------------------------------------- )
: init-memory ( -- ) -2 dup dup h! pad pad-size + - 16lshift -6 ! ;

code allocate ( u -- addr ior )
    (alloc-minimum) # tos h compare, ult if (alloc-minimum) # tos move, endif
    3 # tos h add, -2 # tos b and, a1 clear, -1 # d1 move, begin 
        [a1 -2 +] d1 h move, z= if tos push, -59 # tos move, next, endif
        d1 a2 move, [a2 -4 +] tos h compare, ugt while a2 a1 move, repeat
    [a2 -2 +] d1 h move, [a2 -4 +] d2 h move, tos d2 h sub,
    (alloc-minimum) # d2 h compare, ult= if
        [a2 -4 +] tos h move, d1 [a1 -2 +] h move, tos a2 sub, else
        tos a2 sub, d1 [a2 -2 +] h move, d2 [a2 -4 +] h move,
        a2 [a1 -2 +] h move, endif
    2 # tos h sub, tos [a2]+ h move, a2 push, tos clear, next

code free ( addr -- ior )
    a1 clear, -1 # d3 move, begin
        [a1 -2 +] d3 h move, z= if tos push, -60 # tos move, next, endif
        d3 tos h compare, ult while d3 a1 move, repeat
    tos a2 move, d2 clear, -[a2] d2 h move, 2 # d2 h add, d2 a2 add,
    0 # a1 h compare, z<> if
        d1 clear, [a1 -4 +] d1 h move, [a2 d1+ 0] a3 address, a3 a1 compare,
        z= if a1 a2 move, d1 d2 h add, else { 2swap }
    endif a2 [a1 -2 +] h move, endif
    d3 a3 move, [a3 d2+ 0] a1 address, a1 a2 compare,
    z= if [a3 -2 +] [a2 -2 +] h move, [a3 -4 +] d2 h add,
    else a3 [a2 -2 +] h move, endif d2 [a2 -4 +] h move, tos clear, next

code allocation ( addr -- u )
    tos a1 move, tos clear, [a1 -2 +] tos h move, next

code available ( -- u )
    tos push, tos clear, a1 clear, -1 # d1 move, begin
    [a1 -2 +] d1 h move, z<> while d1 a1 move, [a1 -4 +] tos h add, repeat next

: resize ( addr1 u -- addr2 ior )
    -interrupts[
        swap dup allocation over 2>r
        free if 2rdrop ]-interrupts -16 exit endif
        dup allocate if 2rdrop ]-interrupts nip -16 exit endif
        r> rot third r> rot min move
    ]-interrupts 0 ;

( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
















