( ---------------------------------------------------------------------------- )
( **************************************************************************** )
(       Video Hardware Interface                                               )
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
$C00000 constant vdp-data
$C00004 constant vdp-ctrl
$C00008 constant vdp-hv
$C0001C constant vdp-debug

code vdp-status ( -- h ) tos push, tos clear, vdp-ctrl [#] tos h move, next

{:} vdp-stat: ( mask -- ) create h, does> ( -- flag ) h@ vdp-status and flag ;
$200 vdp-stat: vdp-fifo-empty?      $010 vdp-stat: odd-frame?
$100 vdp-stat: vdp-fifo-full?       $008 vdp-stat: vdp-vblank?
$080 vdp-stat: vbi-occurred?        $004 vdp-stat: vdp-hblank?
$040 vdp-stat: sprite-limit?        $002 vdp-stat: vdp-dma?
$020 vdp-stat: sprite-collide?      $001 vdp-stat: pal-console?

: ntsc-console? ( -- flag ) pal-console? not ;

( ---------------------------------------------------------------------------- )
(       VDP Register Buffer                                                    )
( ---------------------------------------------------------------------------- )
20 bytes buffer: vdp-buffer

create (vdp-buffer)  4 c, 4 c, 0 , 0 , 0 , 2 h, 0 h,

code (init-video-config)
    vdp-buffer [#] a1 lea, (vdp-buffer) [#] a2 lea,
    5 d1 do [a2]+ [a1]+ move, loop next

code !video-config
    $8000 # d1 h move, vdp-buffer [#] a1 lea, vdp-ctrl [#] a2 lea,
    19 d7 do [a1]+ d1 c move, d1 [a2] h move, $0100 # d1 h add, loop next

( ---------------------------------------------------------------------------- )
(       "Immediate" Video Registers
( ---------------------------------------------------------------------------- )
code +video ( -- )
    vdp-buffer 1+ [#] a1 lea, $8104 # d1 h move, [a1] d1 c move,
    $40 # d1 c or, d1 [a1] c move, d1 vdp-ctrl [#] h move, next

code -video ( -- )
    vdp-buffer 1+ [#] a1 lea, $8104 # d1 h move, [a1] d1 c move,
    $BF # d1 c and, d1 [a1] c move, d1 vdp-ctrl [#] h move, next

code autoinc ( value -- )
    tos vdp-buffer $F + [#] c move, $8F00 # tos h add,
    tos vdp-ctrl [#] h move, tos pop, next

code 2autoinc ( -- )
    $8F02 # vdp-ctrl [#] h move, 2 # vdp-buffer $F + [#] c move, next

code hbi-counter ( value -- )
    tos vdp-buffer $A + [#] c move, $8A00 # tos h add,
    tos vdp-ctrl [#] h move, tos pop, next

( ---------------------------------------------------------------------------- )
(       Buffered Video Registers                                               )
( ---------------------------------------------------------------------------- )
{:} vdpreg: ( index shift -- )
    create c, c, ;code ( value -- )
        d1 clear, [dfa]+ d1 c move, d1 tos h lsr, [dfa]+ d1 c move,
        vdp-buffer [#] a1 lea, tos [a1 d1+ 0] c move, tos pop, next
{:} vdpreg>: ( index shift -- )
    create c, c, ;code ( -- value )
        tos push, d1 clear, [dfa]+ d1 c move, d2 clear, [dfa]+ d2 c move,
        vdp-buffer [#] a1 lea, [a1 d2+ 0] tos c move, $7F # tos and,
        d1 tos h lsl, next

2 10 vdpreg:  planeA             7  0 vdpreg:  backColorIndex
2 10 vdpreg>: planeA>            7  0 vdpreg>: backColorIndex>
3 10 vdpreg:  windowPlane       13 10 vdpreg:  hScrollTable
3 10 vdpreg>: windowPlane>      13 10 vdpreg>: hScrollTable>
4 13 vdpreg:  planeB            17  1 vdpreg:  windowPlaneX
4 13 vdpreg>: planeB>           17  1 vdpreg>: windowPlaneX>
5  9 vdpreg:  spritePlane       18  0 vdpreg:  windowPlaneY
5  9 vdpreg>: spritePlane>      18  0 vdpreg>: windowPlaneY>

( ---------------------------------------------------------------------------- )
(       Video Mode Registers                                                   )
( ---------------------------------------------------------------------------- )
{:} +vdpreg: ( index value -- )
    create c, c, ;code ( -- )
        [dfa]+ d1 c move, d2 clear, [dfa]+ d2 c move,
         vdp-buffer [#] a1 lea, d1 [a1 d2+ 0] c or, next
{:} -vdpreg: ( index value -- )
    create invert c, c, ;code ( -- )
        [dfa]+ d1 c move, d2 clear, [dfa]+ d2 c move,
         vdp-buffer [#] a1 lea, d1 [a1 d2+ 0] c and, next

 0 $20 +vdpreg: +blankleft      11 $04 +vdpreg: +vstrips
 0 $20 -vdpreg: -blankleft      11 $04 -vdpreg: -vstrips
 0 $10 +vdpreg: +hbi            11 $03 +vdpreg: +hlines
 0 $10 -vdpreg: -hbi            11 $03 -vdpreg: -hlines

 1 $20 +vdpreg: +vbi            12 $81 +vdpreg: +h320
 1 $20 -vdpreg: -vbi            12 $81 -vdpreg: +h256
12 $08 +vdpreg: +shadow         12 $06 +vdpreg: +v448
12 $08 -vdpreg: -shadow         12 $06 -vdpreg: +v224
 1 $08 +vdpreg: +pal             1 $08 -vdpreg: +ntsc            

17 $80 +vdpreg: +windowRight    17 $80 -vdpreg: +windowLeft
18 $80 +vdpreg: +windowBottom   18 $80 -vdpreg: +windowTop

11 $02 +vdpreg: (+hstrips)
: +hstrips ( -- ) -hlines (+hstrips) ;

( ---------------------------------------------------------------------------- )
(       Plane Size Register                                                    )
( ---------------------------------------------------------------------------- )
{:} vdpsize: ( value -- )
    create c, ;code ( -- ) [dfa]+   vdp-buffer 16 + [#]   c move, next

$00 vdpsize: 32x32planes        $01 vdpsize: 64x32planes
$10 vdpsize: 32x64planes        $03 vdpsize: 128x32planes
$30 vdpsize: 32x128planes       $11 vdpsize: 64x64planes

( ---------------------------------------------------------------------------- )
(       Video Memory Access                                                    )
( ---------------------------------------------------------------------------- )
{:} vdp-io: ( u -- )
    create , ;code ( addr -- )
        2 # tos lsl, 2 # tos h lsr, tos swap, [dfa] tos or,
        tos  vdp-ctrl [#] move, tos pop, next

$40000000 vdp-io: write-vram        $00000000 vdp-io: read-vram
$C0000000 vdp-io: write-cram        $00000020 vdp-io: read-cram
$40000010 vdp-io: write-vsram       $00000010 vdp-io: read-vsram

code  >vdp ( n -- ) tos  vdp-data [#]   move, tos pop, next
code h>vdp ( h -- ) tos  vdp-data [#] h move, tos pop, next

code  vdp> ( -- n ) tos push, vdp-data [#] tos move, next
code hvdp> ( -- h ) tos push, tos clear, vdp-data [#] tos h move, next

code write ( addr  #halves -- )
    vdp-data [#] a1 lea, a2 pop,
    0 # tos bittest, z<> if tos dec, [a2]+ [a1] h move, endif
    1 # tos h lsr, tos h dec, tos begin [a2]+ [a1] move, loop tos pop, next
code read ( addr  #halves -- )
    vdp-data [#] a1 lea, a2 pop,
    0 # tos bittest, z<> if tos dec, [a1] [a2]+ h move, endif
    1 # tos h lsr, tos h dec, tos begin [a1] [a2]+ move, loop tos pop, next

: store-video   ( src dest  #halves -- ) 2autoinc swap write-vram  write ;
: fetch-video   ( src dest  #halves -- ) 2autoinc  rot  read-vram   read ;
: store-color   ( src dest  #halves -- ) 2autoinc swap write-cram  write ;
: fetch-color   ( src dest  #halves -- ) 2autoinc  rot  read-cram   read ;
: store-vscroll ( src dest  #halves -- ) 2autoinc swap write-vsram write ;
: fetch-vscroll ( src dest  #halves -- ) 2autoinc  rot  read-vsram  read ;

( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )















