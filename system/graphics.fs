( ---------------------------------------------------------------------------- )
( **************************************************************************** )
(       Picture Management                                                     )
( **************************************************************************** )
( ---------------------------------------------------------------------------- )

( ---------------------------------------------------------------------------- )
(                                                                              )
(       Dependencies:                                                          )
(           - 68k.fs                                                           )
(           - forth.fs                                                         )
(           - core.fs                                                          )
(           - allocate.fs                                                      )
(           - video.fs                                                         )
(                                                                              )
( ---------------------------------------------------------------------------- )
Forth definitions

( ---------------------------------------------------------------------------- )
32 constant tile
{ :noname 32 + ; }      anon 32 # tos   add, next       host/target: tile+
{ :noname 32 - ; }      anon 32 # tos   sub, next       host/target: tile-
{ :noname 32 * ; }      anon  5 # tos h asl, next       host/target: tiles
{ :noname 32 / ; }      anon  5 # tos h asr, next       host/target: tile/

code (gnode@) ( vram -- node )
    video-ctrl [#] a1 address, $8F02 # [a1] h move, 2 # tos lsl, 2 # tos h lsr,
    tos swap, tos [a1] move, [a1 -4 +] tos move, next
code (gnode!) ( node vram -- )
    video-ctrl [#] a1 address, $8F02 # [a1] h move, 2 # tos lsl, 2 # tos h lsr,
    $4000 # tos h or, tos swap, tos [a1] move, [sp]+ [a1 -4 +] move,
    tos pull, next

code (gnode.next)  ( node -- next ) tos h clear, tos swap, next
code (gnode.size)  ( node -- size ) $FFFF # tos and, next
code (gnode>) ( node -- size next )
    tos h push, half # sp sub, tos h clear, tos swap, next
code (>gnode) ( size next -- node )
    tos swap, half # sp add, tos h pull, next

( ---------------------------------------------------------------------------- )
hvalue (gnode.start)

: init-graphics ( -- )
    2 kilobytes to (gnode.start)
    $10000 foreground-table min background-table min
               window-table min     sprite-table min scroll-table min
    (gnode.start) - 0 (>gnode) (gnode.start) (gnode!) ;

( ---------------------------------------------------------------------------- )
code (galloc1) ( bytes -- prev this bytes )
    DEBUG [IF]
        tos d1 move, $FFFF000F # d1 and,
        z<> if tos push, -267 # tos move, tp rpull, next, endif
    [THEN]
    d1 clear, d1 push, (gnode.start) [#] d1 h move,
    z= if -268 # tos move, tp rpull, next, endif d1 push, next
code (galloc2) ( prev this bytes -- prev' this' [this>] bytes flag )
    d1 pull, d1 d2 move, 2 # d2 lsl, 2 # d2 h lsr, d2 swap,
    d2 video-ctrl [#] move, video-data [#] d2 move, tos d2 h sub,
    cclr if cell # sp sub, d2 push, tos push, -1 # tos move, next, endif
    d1 poke, d2 h clear, d2 swap, d2 push,
    z= if cell # sp add, -268 # tos move, tp rpull, next, endif
    tos push, tos clear, next
code (galloc3) ( prev this this> bytes -- vramaddr ior )
    video-ctrl [#] a1 address, d3 pull, d2 pull, d1 pull, d3 h test, z<> if
        d2 tos h add, d1 h test, z= if tos (gnode.start) [#] h move,
            else 2 # d1 lsl, 2 # d1 h lsr, $4000 # d1 h or, d1 swap,
                 d1 [a1] move, tos [a1 -4 +] h move, endif
        2 # tos lsl, 2 # tos h lsr, $4000 # tos h or, tos swap,
        tos [a1] move, d3 [a1 -4 +] move, d2 push, tos clear, next, endif           
    d3 swap, d1 h test, z= if d3 (gnode.start) [#] h move,
        else 2 # d1 lsl, 2 # d1 h lsr, $4000 # d1 h or, d1 swap,
            d1 [a1] move, d3 [a1 -4 +] h move, endif
    d2 push, tos clear, next
: allocate-video ( bytes -- vramaddr ior )
    2 >autoinc (galloc1) begin (galloc2) until (galloc3) ;

( ---------------------------------------------------------------------------- )
code (gfree1) ( vramaddr bytes -- vramaddr prev next bytes )
    DEBUG [IF]
        tos d1 move, z= if -267 # tos move, tp rpull, next, endif
        $FFFF000F # d1 and, z<> if -267 # tos move, tp rpull, next, endif
        d1 peek, $FFFF000F # d1 and,
        z<> if -269 # tos move, tp rpull, next, endif
    [THEN] d1 clear, d1 push, (gnode.start) [#] d1 h move, d1 push, next
code (gfree2) ( vramaddr prev next bytes -- vramaddr prev' next' bytes flag )
    d1 pull, d1 h test, z= if d1 push, tos push, -1 # tos move, next, endif
    d2 pull, [sp] d1 h compare,
    ugt if d2 push, d1 push, tos push, -1 # tos move, next, endif
    d1 push, $FFFF # d1 and, 2 # d1 lsl, 2 # d1 h lsr, d1 swap,
    d1 video-ctrl [#] move, video-data [#] d1 move,
    d1 swap, d1 push, tos push, tos clear, next
code (gfree3) ( vramaddr prev next bytes -- ior )
    video-ctrl [#] a1 address, d1 pull, d2 pull, d3 pull, \ D1=next, D2=prev, D3=vramaddr
    d1 h test, z<> if d3 d4 h move, tos d4 h add, d4 d1 h compare, z= if
        d4 clear, d1 d4 h move, 2 # d4 lsl, 2 # d4 h lsr, d4 swap, d4 [a1] move,
        [a1 -4 +] d4 move, d4 tos h add, d4 swap, d4 d1 h move, endif endif
    d2 test, z= if
        d1 swap, tos d1 h move, d3 (gnode.start) [#] h move,
        2 # d3 lsl, 2 # d3 h lsr, $4000 # d3 h or, d3 swap,
        d3 [a1] move, d1 [a1 -4 +] move, tos clear, next, endif
    d2 d4 h move, d2 swap, d2 d4 h add, d4 d3 h compare, z<> if
        d2 h clear, d2 swap, 2 # d2 lsl, 2 # d2 h lsr, $4000 # d2 h or,
        d2 swap, d2 [a1] move, d3 [a1 -4 +] h move,
        else d2 tos h add, d2 swap, d2 d3 h move, endif
    d1 swap, tos d1 h move, 2 # d3 lsl, 2 # d3 h lsr, $4000 # d3 h or,
    d3 swap, d3 [a1] move, d1 [a1 -4 +] move, tos clear, next
: free-video ( vramaddr bytes -- ior )
    2 >autoinc (gfree1) begin (gfree2) until (gfree3) ;

( ---------------------------------------------------------------------------- )
code (gavail1) ( -- bytes vramaddr )
    tos push, tos clear, d1 clear, (gnode.start) [#] d1 h move,
    z= if tp rpull, next, endif tos push, d1 tos move, next
code (gavail2) ( bytes vramaddr -- bytes [vramaddr] )
    2 # tos lsl, 2 # tos h lsr, tos swap, tos video-ctrl [#] move,
    video-data [#] tos move, tos [sp half +] h add, tos h clear, tos swap, 
    tos h test, z= if tos pull, tp rpull, endif next
: available-video ( -- bytes ) 
    2 >autoinc (gavail1) begin (gavail2) again ;

( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )











