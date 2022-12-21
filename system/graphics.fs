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
(       Tilesize Operators                                                     )
( ---------------------------------------------------------------------------- )
32 constant tile
{ :noname 32 + ; }      anon 32 # tos   add, next       host/target: tile+
{ :noname 32 - ; }      anon 32 # tos   sub, next       host/target: tile-
{ :noname 32 * ; }      anon  5 # tos h asl, next       host/target: tiles
{ :noname 32 / ; }      anon  5 # tos h asr, next       host/target: tile/

( ---------------------------------------------------------------------------- )
( **************************************************************************** )
(       Video Memory Manager                                                   )
( **************************************************************************** )
( ---------------------------------------------------------------------------- )
(       Graphics Free Node is 4 bytes, located in VRAM                         )
(           - first 2 bytes = size of this free region                         )
(           -  next 2 bytes = address of next node                             )
(       Address of First Node is in system RAM   ["gnode"]                     )
(                                                                              )
( ---------------------------------------------------------------------------- )
hvalue (gnode)

( ---------------------------------------------------------------------------- )
code (galloc1) ( bytes -- prev this bytes )
    DEBUG [IF]
        tos d1 move, $FFFF000F # d1 and,
        z<> if tos push, -267 # tos move, tp rpull, next, endif
    [THEN]
    d1 clear, d1 push, (gnode) [#] d1 h move,
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
        d2 tos h add, d1 h test, z= if tos (gnode) [#] h move,
            else 2 # d1 lsl, 2 # d1 h lsr, $4000 # d1 h or, d1 swap,
                 d1 [a1] move, tos [a1 -4 +] h move, endif
        2 # tos lsl, 2 # tos h lsr, $4000 # tos h or, tos swap,
        tos [a1] move, d3 [a1 -4 +] move, d2 push, tos clear, next, endif           
    d3 swap, d1 h test, z= if d3 (gnode) [#] h move,
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
    [THEN] d1 clear, d1 push, (gnode) [#] d1 h move, d1 push, next
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
        d1 swap, tos d1 h move, d3 (gnode) [#] h move,
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
    tos push, tos clear, d1 clear, (gnode) [#] d1 h move,
    z= if tp rpull, next, endif tos push, d1 tos move, next
code (gavail2) ( bytes vramaddr -- bytes [vramaddr] )
    2 # tos lsl, 2 # tos h lsr, tos swap, tos video-ctrl [#] move,
    video-data [#] tos move, tos [sp half +] h add, tos h clear, tos swap, 
    tos h test, z= if tos pull, tp rpull, endif next

: available-video ( -- bytes ) 
    2 >autoinc (gavail1) begin (gavail2) again ;

( ---------------------------------------------------------------------------- )
: nametables ( -- vram-addr ) $10000 foreground-table  min
                                     background-table  min
                                         window-table  min
                                         sprite-table  min
                                         scroll-table  min ;
: init-graphics ( -- )
    0 to (gnode)   2 kilobytes   nametables over -   free-video throw ;

( ---------------------------------------------------------------------------- )
( **************************************************************************** )
(       Sprite Pictures                                                        )
( **************************************************************************** )
( ---------------------------------------------------------------------------- )
(       address of index calculator         4 bytes                            )
(       RAM address of tile index           2 bytes                            )
(       sprite dimensions                   2 bytes                            )
(       size of uncompressed data           2 bytes                            )
(       xt of decompressor                  4 bytes                            )
(       ROM picture data                    x bytes                            )
( ---------------------------------------------------------------------------- )
code pic.index   tos a1 move, -1 # tos move, [a1 4 +] tos h move, next
code pic.size@   tos a1 move, tos clear, [a1 8 +] tos h move, next
code pic.dims@   tos a1 move, tos clear, [a1 6 +] tos h move, next
code pic.data    7 halves # tos add, next

rawcode (x1)  return, end
rawcode (x2)  d1 d1 h add,  return, end
rawcode (x3)  d1 d2 h move, d2 d2 h add, d2 d1 h add, return, end
rawcode (x4)  2 # d1 h lsl, return, end
rawcode (x6)  d1  d1 h add, d1 d2 move, d2 d2 h add, d2 d1 h add, return, end
rawcode (x8)  3 # d1 h lsl, return, end
rawcode (x9)  d1 d2 h move, 3 # d1 h lsl, d2 d1 h add, return, end
rawcode (x12) 2 # d1 h lsl, d1 d2 move, d2 d2 h add, d2 d1 h add, return, end
rawcode (x16) 4 # d1 h lsl, return, end

{ : (index-addr) ( dimensions -- romaddr )
    case
      1x1 of (x1) endof 1x2 of (x2) endof 1x3 of (x3)  endof 1x4 of (x4)  endof
      2x1 of (x2) endof 2x2 of (x4) endof 2x3 of (x6)  endof 2x4 of (x8)  endof
      3x1 of (x3) endof 3x2 of (x6) endof 3x3 of (x9)  endof 3x4 of (x12) endof
      4x1 of (x4) endof 4x2 of (x8) endof 4x3 of (x12) endof 4x4 of (x16) endof
      -1 abort" Invalid sprite dimensions"
    endcase ; }

( ---------------------------------------------------------------------------- )
{:} sprite-picture ( dimensions  data-size  "name" -- )
    create over (index-addr) , here h, half allot swap h, h, 0 , {;}

( ---------------------------------------------------------------------------- )
( **************************************************************************** )
(       Tile Pictures                                                          )
( **************************************************************************** )
( ---------------------------------------------------------------------------- )
(       unused                              4 bytes                            )
(       RAM address of tile index           2 bytes                            )
(       unused                              2 bytes                            )
(       size of uncompressed data           2 bytes                            )
(       xt of decompressor                  4 bytes                            )
(       ROM picture data                    x bytes                            )
( ---------------------------------------------------------------------------- )
{:} tile-picture ( data-size  "name" -- )
    create 0 , here h, half allot 0 h, h, 0 , {;}

( ---------------------------------------------------------------------------- )
( **************************************************************************** )
(       Decompressors                                                          )
( **************************************************************************** )
( ---------------------------------------------------------------------------- )

( ---------------------------------------------------------------------------- )
(       Ultra Fast Tile Compression                                            )
( ---------------------------------------------------------------------------- )
(       Original Author: SikTheHedgehog   github.com/sikthehedgehog/mdtools    )
(       Ported with Modifications by WildChild83                               )
(       see file "system/importing/uftc.fs" for license details                )
( ---------------------------------------------------------------------------- )
value (decomp-ram)      value (decomp-vram)

( ---------------------------------------------------------------------------- )
code (uftc1) ( src dest #tiles -- src' dest a1 #tiles )
    [sp cell +] a2 move, [a2]+ d2 h move, a2 [sp cell +] move,
    [a2 d2+ ih 0] a1 address, a1 push, next
code (uftc2) ( src dest a1 #tiles -- )
    a1 pull, a3 pull, a2 pull,
    tos w dec, neg if tos pull, tp rpull, next, endif
    sp d6 move, tp d5 move,
    [a1]+ d1 h move, [a2 d1+ ih 0] sp address, [a1]+ d1 h move,
    [a2 d1+ ih 0] tp address, [sp]+ [a3]+ h move, [tp]+ [a3]+ h move,
    [sp]+ [a3]+ h move, [tp]+ [a3]+ h move, [sp]+ [a3]+ h move,
    [tp]+ [a3]+ h move, [sp]+ [a3]+ h move, [tp]+ [a3]+ h move,
    [a1]+ d1 h move, [a2 d1+ ih 0] sp address, [a1]+ d1 h move,
    [a2 d1+ ih 0] tp address, [sp]+ [a3]+ h move, [tp]+ [a3]+ h move,
    [sp]+ [a3]+ h move, [tp]+ [a3]+ h move, [sp]+ [a3]+ h move,
    [tp]+ [a3]+ h move, [sp]+ [a3]+ h move, [tp]+ [a3]+ h move,
    d5 tp move, d6 sp move, a2 push, a3 push, a1 push, next
: uftc ( picture -- )
    dup pic.size@ dup allocate throw to (decomp-ram) ( picture size )
    dup allocate-video throw dup to (decomp-vram) ( picture size vram )
    tile/ third pic.index h! swap pic.data (decomp-ram) third tile/ ( size src dest #tiles )
    (uftc1) begin (uftc2) again half/ (decomp-ram) (decomp-vram) rot
    move>video (decomp-ram) free throw ;

( ---------------------------------------------------------------------------- )
( **************************************************************************** )
(       Other Stuff...                                                         )
( **************************************************************************** )
( ---------------------------------------------------------------------------- )
: init-picture ( picture -- ) pic.index 0 swap h! ;

: free-picture ( picture -- )
    dup pic.index h@ tiles swap pic.size@ free-video throw ;

: load-picture ( picture -- )
    dup pic.size@ allocate-video throw dup >r tile/ over pic.index h!
    dup pic.data swap pic.size@ r> swap move>video ;

( ---------------------------------------------------------------------------- )
code stamp ( x y attrib index sprite -- )
    tos a1 move, d1 pull, [a1]+ a2 move, [a2] jump-sub, -1 # d2 move,
    [a1]+ d2 h move, d2 a2 move, [a2] d1 h add, half # sp add, [sp]+ d1 h add,
    d1 swap, [a1]+ d1 h move, video-sprite-ptr [#] a2 address, -1 # d2 move,
    [a2]+ d2 h move, d2 a1 move, [a2] d1 c move, d1 c inc, d1 [a2] c move,
    tos pull, 128 # tos h add, tos [a1]+ h move, d1 swap, d1 [a1]+ move,
    tos pull, 128 # tos h add, tos [a1]+ h move, a1 -[a2] h move, tos pull,
    next

( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )











