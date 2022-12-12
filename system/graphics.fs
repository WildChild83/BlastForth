( ---------------------------------------------------------------------------- )
( **************************************************************************** )
(       Picture Manager                                                        )
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

code (gnode.next)  ( node -- next ) $FFFF # tos and, next
code (gnode.size)  ( node -- size ) tos h clear, tos swap, next
code (gnode>) ( node -- next size )
    tos h push, half # sp sub, tos h clear, tos swap, next
code (>gnode) ( next size -- node )
    tos swap, half # sp add, tos h pull, next

( ---------------------------------------------------------------------------- )
hvalue (gnode.start)

: (vram-ubound) ( -- vram-limit )
    $10000 foreground-table min   background-table min
               window-table min       sprite-table min   scroll-table min ;

: init-graphics ( -- )
    tile to (gnode.start)
    $10000 foreground-table min background-table min
               window-table min     sprite-table min scroll-table min
    0 swap (gnode.start) - (>gnode) (gnode.start) (gnode!) ;

: allocate-vram ( bytes -- vramaddr )
    (gnode.start) 
;

( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )











