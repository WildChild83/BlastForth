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
$C00000 constant video-data
$C00004 constant video-ctrl
$C00008 constant video-hv
$C0001C constant video-debug

code video-status ( -- h ) tos push, tos clear, video-ctrl [#] tos h move, next

{:} vdp-stat: ( mask -- ) create h, does> ( -- flag ) h@ video-status and flag ;
$200 vdp-stat: video-fifo-empty?    $010 vdp-stat: odd-frame?
$100 vdp-stat: video-fifo-full?     $008 vdp-stat: vblank?
$080 vdp-stat: vbi-occurred?        $004 vdp-stat: hblank?
$040 vdp-stat: sprite-limit?        $002 vdp-stat: dma?
$020 vdp-stat: sprite-collide?      $001 vdp-stat: pal-console?

: ntsc-console? ( -- flag ) pal-console? not ;

( ---------------------------------------------------------------------------- )
(       VDP Register Buffer                                                    )
( ---------------------------------------------------------------------------- )
20 bytes buffer: video-config-buffer

create (video-config-buffer)  4 c, 4 c, 0 , 0 , 0 , 2 h, 0 h,

code (init-video-config)
    video-config-buffer [#] a1 lea, (video-config-buffer) [#] a2 lea,
    5 d1 do [a2]+ [a1]+ move, loop next

code set-video ( -- )
    $8000 # d1 h move, video-config-buffer [#] a1 lea, video-ctrl [#] a2 lea,
    19 d7 do [a1]+ d1 c move, d1 [a2] h move, $0100 # d1 h add, loop next

( ---------------------------------------------------------------------------- )
(       "Immediate" Video Registers
( ---------------------------------------------------------------------------- )
code +video ( -- )
    video-config-buffer 1+ [#] a1 lea, $8104 # d1 h move, [a1] d1 c move,
    $40 # d1 c or, d1 [a1] c move, d1 video-ctrl [#] h move, next

code -video ( -- )
    video-config-buffer 1+ [#] a1 lea, $8104 # d1 h move, [a1] d1 c move,
    $BF # d1 c and, d1 [a1] c move, d1 video-ctrl [#] h move, next

code autoinc ( value -- )
    tos video-config-buffer $F + [#] c move, $8F00 # tos h add,
    tos video-ctrl [#] h move, tos pull, next

code hbi-counter ( value -- )
    tos video-config-buffer $A + [#] c move, $8A00 # tos h add,
    tos video-ctrl [#] h move, tos pull, next

( ---------------------------------------------------------------------------- )
(       Buffered Video Registers                                               )
( ---------------------------------------------------------------------------- )
{:} vdpreg: ( index shift -- )
    create c, c, ;code ( value -- )
        d1 clear, [dfa]+ d1 c move, d1 tos h lsr, [dfa]+ d1 c move,
        video-config-buffer [#] a1 lea, tos [a1 d1+ 0] c move, tos pull, next
{:} vdpreg>: ( index shift -- )
    create c, c, ;code ( -- value )
        tos push, d1 clear, [dfa]+ d1 c move, d2 clear, [dfa]+ d2 c move,
        video-config-buffer [#] a1 lea, [a1 d2+ 0] tos c move, $7F # tos and,
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
         video-config-buffer [#] a1 lea, d1 [a1 d2+ 0] c or, next
{:} -vdpreg: ( index value -- )
    create invert c, c, ;code ( -- )
        [dfa]+ d1 c move, d2 clear, [dfa]+ d2 c move,
         video-config-buffer [#] a1 lea, d1 [a1 d2+ 0] c and, next

 0 $20 +vdpreg: +blankleft      11 $04 +vdpreg: +vstrips
 0 $20 -vdpreg: -blankleft      11 $04 -vdpreg: -vstrips
 0 $10 +vdpreg: +hbi            11 $03 +vdpreg: +hlines
 0 $10 -vdpreg: -hbi            11 $03 -vdpreg: -hlines

 1 $20 +vdpreg: +vbi            12 $81 +vdpreg: +h320
 1 $20 -vdpreg: -vbi            12 $81 -vdpreg: +h256
12 $08 +vdpreg: +shadow         12 $06 +vdpreg: +interlace
12 $08 -vdpreg: -shadow         12 $06 -vdpreg: -interlace
 1 $08 +vdpreg: +pal             1 $08 -vdpreg: +ntsc            

17 $80 +vdpreg: +windowRight    17 $80 -vdpreg: +windowLeft
18 $80 +vdpreg: +windowBottom   18 $80 -vdpreg: +windowTop

11 $02 +vdpreg: (+hstrips)
: +hstrips ( -- ) -hlines (+hstrips) ;

: +v224 ( -- ) +ntsc -interlace ;
: +v240 ( -- ) +pal  -interlace ;
: +v448 ( -- ) +ntsc +interlace ;
: +v480 ( -- ) +pal  +interlace ;

: +vnative ( -- ) pal-console? if +pal exit endif +ntsc ;

code pixel-width ( -- u )
    tos push, 256 # tos move, video-config-buffer 12 + [#] d1 c move,
    $81 # d1 c and, z<> if 64 # tos h add, endif next
code pixel-height ( -- u )
    tos push, 224 # tos move, video-config-buffer [#] a1 lea,
    [a1  1 +] d1 c move, $08 # d1 c and, d1 d1 c add, d1 tos c add,
    [a1 12 +] d1 c move, 2 # d1 bittest, z<> if tos tos h add, endif next
code pixel-size ( -- width height )
    tos push, 256 # d1 move, video-config-buffer [#] a1 lea,
    [a1 12 +] tos c move, $81 # tos c and, z<> if 64 # d1 h add, endif
    d1 push, 224 # tos move, [a1 1 +] d1 c move, $08 # d1 c and,
    d1 d1 c add, d1 tos c add, [a1 12 +] d1 c move, 2 # d1 bittest,
    z<> if tos tos h add, endif next

( ---------------------------------------------------------------------------- )
(       Plane Size Register                                                    )
( ---------------------------------------------------------------------------- )
{:} vdpsize: ( value -- )
    create c, ;code ( -- ) [dfa]+  video-config-buffer 16 + [#]  c move,  next

$00 vdpsize: 32x32planes        $01 vdpsize: 64x32planes
$10 vdpsize: 32x64planes        $03 vdpsize: 128x32planes
$30 vdpsize: 32x128planes       $11 vdpsize: 64x64planes

code plane-width ( -- u )
    tos push, tos clear, video-config-buffer 16 + [#] tos c move,
    tos c inc, 5 # tos c lsl, next
code plane-height ( -- u )
    tos push, tos clear, video-config-buffer 16 + [#] tos c move,
    $10 # tos c add, tos tos c add, $F0 # tos c and, next
code plane-size ( -- width height )
    tos push, tos clear, video-config-buffer 16 + [#] tos c move,
    $11 # tos c add, tos tos c add, tos d1 move, $F0 # tos c and,
    4 # d1 c lsl, d1 push, next

( ---------------------------------------------------------------------------- )
(       Video Memory Access                                                    )
( ---------------------------------------------------------------------------- )
{:} video-io: ( u -- )
    create , ;code ( addr -- )
        2 # tos lsl, 2 # tos h lsr, tos swap, [dfa] tos or,
        tos  video-ctrl [#] move, tos pull, next

$40000000 video-io: write-vram      $00000000 video-io: read-vram
$C0000000 video-io: write-cram      $00000020 video-io: read-cram
$40000010 video-io: write-vsram     $00000010 video-io: read-vsram

code  !video ( u -- ) tos  video-data [#]   move, tos pull, next
code h!video ( h -- ) tos  video-data [#] h move, tos pull, next

code  @video ( -- u ) tos push, video-data [#] tos move, next
code h@video ( -- h ) tos push, tos clear, video-data [#] tos h move, next

code buffer>video ( addr  #halves -- )
    video-data [#] a1 lea, a2 pull,
    1 # tos h lsr, cset if [a2]+ [a1] h move, endif
    tos h dec, pos if tos begin [a2]+ [a1] move, loop endif tos pull, next
code video>buffer ( addr  #halves -- )
    video-data [#] a1 lea, a2 pull,
    1 # tos h lsr, cset if [a1] [a2]+ h move, endif
    tos h dec, pos if tos begin [a1] [a2]+ move, loop endif tos pull, next

: >vram   ( src dest  #halves -- )
    swap  -interrupts[  write-vram  buffer>video  ]-interrupts  ;
: >cram   ( src dest  #halves -- )
    swap  -interrupts[  write-cram  buffer>video  ]-interrupts  ;
: >vsram  ( src dest  #halves -- )
    swap  -interrupts[  write-vsram buffer>video  ]-interrupts  ;
:  vram>  ( src dest  #halves -- )
    rot   -interrupts[   read-vram  video>buffer  ]-interrupts  ;
:  cram>  ( src dest  #halves -- )
    rot   -interrupts[   read-cram  video>buffer  ]-interrupts  ;
:  vsram> ( src dest  #halves -- )
    rot   -interrupts[   read-vsram video>buffer  ]-interrupts  ;

: backcolor ( color -- )
    backColorIndex> 2*  -interrupts[  write-cram  h!video  ]-interrupts  ;

( ---------------------------------------------------------------------------- )
(       Blast Processing                                                       )
( ---------------------------------------------------------------------------- )
DMAQueueSize buffer: dma-queue




( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )















