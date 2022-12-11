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
(       TODO:                                                                  )
(           - DMA fill & copy                                                  )
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
code clear-vram ( -- )
    video-data [#] a1 lea, $8F02 # [a1 4 +] h move, $40000000 # [a1 4 +] move,
    d1 clear, 64 kilobytes cell/ d2 do d1 [a1] move, loop next

( ---------------------------------------------------------------------------- )
(       VDP Register Buffer                                                    )
( ---------------------------------------------------------------------------- )
19 bytes buffer: video-config-buffer

create (video-config-buffer) chars[ 4 4 0 0  0 0 0 0  0 0 0 0  0 0 0 2  0 0 0 ]

: (init-video-config)  (video-config-buffer) video-config-buffer  19 cmove ;

code set-video ( -- )
    $8000 # d1 h move, video-config-buffer [#] a1 lea, video-ctrl [#] a2 lea,
    19 d6 do [a1]+ d1 c move, d1 [a2] h move, $0100 # d1 h add, loop next

( ---------------------------------------------------------------------------- )
(       "Immediate" Video Registers
( ---------------------------------------------------------------------------- )
code +video ( -- )
    video-config-buffer 1+ [#] a1 lea, $8144 # d1 h move,
    [a1] d1 c or, d1 [a1] c move, d1 video-ctrl [#] h move, next
code -video ( -- )
    video-config-buffer 1+ [#] a1 lea, $81BF # d1 h move,
    [a1] d1 c and, d1 [a1] c move, d1 video-ctrl [#] h move, next

code +dma ( -- )
    video-config-buffer 1+ [#] a1 lea, $8114 # d1 h move,
    [a1] d1 c or, d1 [a1] c move, d1 video-ctrl [#] h move, next
code -dma ( -- )
    video-config-buffer 1+ [#] a1 lea, $81EF # d1 h move,
    [a1] d1 c and, d1 [a1] c move, d1 video-ctrl [#] h move, next

code vbi-enabled? ( -- flag )
    tos push, $60 # tos c move, video-config-buffer 1+ [#] tos c and,
    $60 # tos c compare, tos z= set?, tos extb, next

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
code screen-lines ( -- u )
    tos push, 224 # tos move, video-config-buffer [#] a1 lea,
    [a1  1 +] d1 c move, $08 # d1 c and, d1 d1 c add, d1 tos c add, next
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
DMAQueueItems 14 * make DMAQueueSize

alignram DMAQueueSize buffer: dma-queue      0 buffer: dma-queue-limit

hvalue dma-ptr        hvalue (dma-mark)

code init-dma ( -- )
    dma-queue [#] a1 lea, [a1 DMAQueueSize +] a2 lea,
    a1 [a2]+ h move, a1 [a2]+ h move,
    DMAQueueItems d1 do
        $94009300 # [a1]+ move, $97009600 # [a1]+ move,
        $9500 # [a1]+ h move, cell # a1 add,
    loop next

: dma-mark ( -- ) dma-ptr to (dma-mark) ;

( ---------------------------------------------------------------------------- )
{:} dma: ( u -- ) create ,  ;code ( src dest #halves -- )
    tos h test, z= if 2 cells # sp add, tos pull, next, endif
    dma-ptr [#] a2 lea, -1 # d1 move, [a2] d1 h move,
    dma-queue-limit $FFFF and # d1 h compare,
    ugt= if tos push, -266 # tos move, throw-primitive, endif d1 a1 move,
    d1 pull, d2 pull, 1 # d2 lsr, $7FFFFF # d2 and, d2 d3 move, tos d3 h add,
    cset if
        d2 [a1 3 +] movep, d4 clear, d2 d4 h move, d4 h neg,
        d4 [a1 1 +] h movep, 10 # a1 add, d1 d5 move, 2 # d5 lsl,
        2 # d5 h lsr, d5 swap, [dfa] d5 or, d5 [a1]+ move,
        a1 d6 h move, dma-queue-limit $FFFF and # d6 h compare,
        ugt= if tos push, -266 # tos move, throw-primitive, endif
        d4 d2 add, d4 d1 add, d3 tos h move, endif
    d2 [a1 3 +] movep, tos [a1 1 +] h movep,
    10 # a1 add, 2 # d1 lsl, 2 # d1 h lsr, d1 swap, [dfa] d1 or, d1 [a1]+ move,
    a1 [a2] h move, tos pull, next
$40000080 dma: dma>vram    $C0000080 dma: dma>cram    $40000090 dma: dma>vsram

{:} video-move: ( dma-xt  cpu-xt -- ) create , ,
        does> ( src dest #halves -- ) vbi-enabled? if cell+ endif @ execute ;
' dma>vram   ' >vram   video-move:  move>vram       aka move>video
' dma>cram   ' >cram   video-move:  move>cram       aka move>color
' dma>vsram  ' >vsram  video-move:  move>vsram      aka move>vscroll

( ---------------------------------------------------------------------------- )
code flush-dma-queue ( -- )
    dma-queue [#] a2 lea, dma-ptr [#] d2 h move, a2 d2 h compare, ugt if
        video-ctrl [#] a1 lea, $8F02 # [a1] h move, $A11100 [#] a3 lea,
        $8114 # d3 h move, video-config-buffer 1+ [#] d3 c or, d3 [a1] h move,
        begin [a2]+ [a1] move, [a2]+ [a1] move, [a2]+ [a1] h move,
              $100 # [a3] h move, [a2]+ [a1] move, 0 # [a3] h move,
              a2 d2 h compare, z= until
        $8104 # d3 h move, video-config-buffer 1+ [#] d3 c or, d3 [a1] h move,
    endif (dma-mark) [#] dma-ptr [#] h move, next

( ---------------------------------------------------------------------------- )
code video-copy ( src dest #halves -- ) next

code video-fill ( addr #halves c -- ) next

( ---------------------------------------------------------------------------- )
(       Plane Scrolling                                                        )
( ---------------------------------------------------------------------------- )
variable (screenX)          defer (scrollX)
variable (screenY)          defer (scrollY)

: <screenX> ( -- ) hScrollTable> write-vram  (screenX) @ !video ;
: <screenY> ( -- )             0 write-vsram (screenY) @ !video ;

: scroll-planes ( -- ) (scrollX) (scrollY) ;
: planeX  ( ax bx -- ) (screenX) tuck half+ h! h!  ['] <screenX> is (scrollX) ;
: planeY  ( ay by -- ) (screenY) tuck half+ h! h!  ['] <screenY> is (scrollY) ;

: move>planeX ( src -- )
    hScrollTable> screen-lines 2* move>video  ['] noop is (scrollX) ;
: move>planeY ( src -- ) 0 pixel-width 4/ move>vscroll  ['] noop is (scrollY) ;

( ---------------------------------------------------------------------------- )
(       Sprite Table                                                           )
( ---------------------------------------------------------------------------- )
synonym video-sprite+ 8+        synonym video-sprites 8*
synonym video-sprite- 8-        synonym video-sprite/ 8/

80 video-sprites buffer: video-sprite-buffer    0 buffer: video-sprite-limit

$0800                           $000 constant 1x1sprite $800 constant 3x1sprite
    $0800 +field +flip          $100 constant 1x2sprite $900 constant 3x2sprite
    $1000 +field +mirror        $200 constant 1x3sprite $A00 constant 3x3sprite
         synonym +palette0 noop $300 constant 1x4sprite $B00 constant 3x4sprite
    $2000 +field +palette1      $400 constant 2x1sprite $C00 constant 4x1sprite
    $2000 +field +palette2      $500 constant 2x2sprite $D00 constant 4x2sprite
    $2000 +field +palette3      $600 constant 2x3sprite $E00 constant 4x3sprite
    $8000 +field +priority      $700 constant 2x4sprite $F00 constant 4x4sprite
drop

hvalue video-sprite-ptr         cvalue video-sprite-link

( ---------------------------------------------------------------------------- )
code video-sprites[ ( -- )
    video-sprite-ptr [#] a2 lea,
    video-sprite-buffer $FFFF and # [a2]+ h move, 0 # [a2]+ c move, next

code video-sprite ( attrib  layout  x y -- )
    video-sprite-ptr [#] a2 lea, -1 # d1 move, [a2]+ d1 h move, d1 a1 move,
    d2 clear, [a2] d2 c move, d2 c inc, d2 [a2] c move, 128 # tos h add,
    tos [a1]+ h move, tos pull, half # sp add, [sp]+ d2 h or, d2 [a1]+ h move,
    half # sp add, [sp]+ [a1]+ h move, 128 # tos h add, tos [a1]+ h move,
    a1 -[a2] h move, tos pull, next

code (]video-sprites) ( -- #halves )
    tos push, -1 # tos move, video-sprite-ptr [#] tos h move, tos a1 move,
    0 # [a1 -5 +] c move, video-sprite-buffer $FFFF and # tos h sub,
    1 # tos h lsr, $FFFF # tos and, next

: ]video-sprites ( -- ) 
    video-sprite-buffer  spritePlane>  (]video-sprites)  move>video ;

( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )















