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
    video-data [#] a1 address, $8F02 # [a1 4 +] h move,
    $40000000 # [a1 4 +] move, d1 clear,
    64 kilobytes cell/ d2 do d1 [a1] move, loop next

( ---------------------------------------------------------------------------- )
(       VDP Register Buffer                                                    )
( ---------------------------------------------------------------------------- )
value  video-mode-registers
hvalue foreground-table     hvalue window-table     hvalue background-table
hvalue sprite-table         cvalue backcolor-index  cvalue hint-counter>
hvalue scroll-table         cvalue autoinc>         cvalue (plane-size)
cvalue windowX              cvalue windowY

( ---------------------------------------------------------------------------- )
rawcode (config-)  a3 inc, end
rawcode (config0)  d1 [a1] h move, d2 d1 h add, return, end
rawcode (config1) [a2]+ d1 c move, (config0) primitive, end
rawcode (config2) [a3]+ d1 c move, (config0) primitive, end
rawcode (config3) [a3]+ d1 c move, 2 # d1 c lsr, (config-) primitive, end

code set-video-config ( -- )
    video-ctrl [#] a1 address, video-mode-registers [#] a2 address,
    [a2 4 +] a3 address, $8000 # d1 h move, $100 # d2 h move,
    (config1) subroutine, (config1) subroutine,
    (config3) subroutine, (config3) subroutine,
    [a3]+ d1 c move, 3 # d1 c rol, (config-) subroutine,
    [a3]+ d1 c move, 1 # d1 c lsr, (config-) subroutine, d2 d1 h add,
    (config2) subroutine,  $200 # d1 h add,  (config2) subroutine,
    (config1) subroutine, (config1) subroutine,
    (config3) subroutine,  d2 d1 h add,
    (config2) subroutine, (config2) subroutine,
    (config2) subroutine, (config2) subroutine, next

( ---------------------------------------------------------------------------- )
(       "Immediate" Video Settings                                             )
( ---------------------------------------------------------------------------- )
{:} video-reg2: ( c -- ) create c,  ;code ( -- )
    video-mode-registers 1+ [#] a1 address, $8100 # d1 h move,
    [dfa] d1 c move, neg if [a1] d1 c and, else [a1] d1 c or, endif
    d1 [a1] c move, d1 video-ctrl [#] h move, next
$44 video-reg2: +video          $24 video-reg2: +vbi
$BF video-reg2: -video          $DF video-reg2: -vbi
code vbi-enabled? ( -- flag )
    tos push, $60 # tos c move, video-mode-registers 1+ [#] tos c and,
    $60 # tos c compare, tos z= set?, tos c extend, next

code +hbi ( -- )
    video-mode-registers [#] a1 address, $8014 # d1 h move,
    [a1] d1 c or, d1 [a1] c move, d1 video-ctrl [#] h move, next
code -hbi ( -- )
    video-mode-registers [#] a1 address, $80EF # d1 h move,
    [a1] d1 c and, d1 [a1] c move, d1 video-ctrl [#] h move, next
code hbi-enabled? ( -- flag )
    tos push, $10 # tos c move, video-mode-registers [#] tos c and,
    tos z<> set?, tos c extend, next

code >autoinc ( value -- )
    tos autoinc> [#] c move, $8F00 # tos h add,
    tos video-ctrl [#] h move, tos pull, next
code >hint-counter ( value -- )
    tos hint-counter> [#] c move, $8A00 # tos h add,
    tos video-ctrl [#] h move, tos pull, next

( ---------------------------------------------------------------------------- )
(       "Buffered" Video Settings                                              )
( ---------------------------------------------------------------------------- )
{:} +vdpreg: ( index value -- )
    create c, c, ;code ( -- )
        [dfa]+ d1 c move, d2 clear, [dfa]+ d2 c move,
        video-mode-registers [#] a1 address, d1 [a1 d2+ 0] c or, next
{:} -vdpreg: ( index value -- )
    create invert c, c, ;code ( -- )
        [dfa]+ d1 c move, d2 clear, [dfa]+ d2 c move,
        video-mode-registers [#] a1 address, d1 [a1 d2+ 0] c and, next
 0 $20 +vdpreg: +blankleft      2 $03 +vdpreg: +hlines
 0 $20 -vdpreg: -blankleft      2 $03 -vdpreg: -hlines
 1 $08 +vdpreg: +pal            2 $04 +vdpreg: +vstrips
 1 $08 -vdpreg: +ntsc           2 $04 -vdpreg: -vstrips
18 $80 +vdpreg: +windowRight    3 $08 +vdpreg: +shadow
18 $80 -vdpreg: +windowLeft     3 $08 -vdpreg: -shadow
19 $80 +vdpreg: +windowBottom   3 $06 +vdpreg: +interlace
19 $80 -vdpreg: +windowTop      3 $06 -vdpreg: -interlace
 3 $81 +vdpreg: +h320           3 $81 -vdpreg: +h256

2 $02 +vdpreg: (+hstrips)
: +hstrips ( -- ) -hlines (+hstrips) ;

: +v224 ( -- ) +ntsc -interlace ;
: +v240 ( -- ) +pal  -interlace ;
: +v448 ( -- ) +ntsc +interlace ;
: +v480 ( -- ) +pal  +interlace ;

: +vnative ( -- ) pal-console? if +pal exit endif +ntsc ;

code pixel-width ( -- u )
    tos push, 256 # tos move, video-mode-registers 3 + [#] d1 c move,
    $81 # d1 c and, z<> if 64 # tos h add, endif next
code pixel-height ( -- u )
    tos push, 224 # tos move, video-mode-registers [#] a1 address,
    [a1 1 +] d1 c move, $08 # d1 c and, d1 d1 c add, d1 tos c add,
    [a1 3 +] d1 c move, 2 # d1 test-bit, z<> if tos tos h add, endif next
code scanline-height ( -- u )
    tos push, 224 # tos move, video-mode-registers 1+ [#] d1 c move,
    $08 # d1 c and, d1 d1 c add, d1 tos c add, next
code pixel-size ( -- width height )
    tos push, 256 # d1 move, video-mode-registers [#] a1 address,
    [a1 3 +] tos c move, $81 # tos c and, z<> if 64 # d1 h add, endif
    d1 push, 224 # tos move, [a1 1 +] d1 c move, $08 # d1 c and,
    d1 d1 c add, d1 tos c add, [a1 3 +] d1 c move, 2 # d1 test-bit,
    z<> if tos tos h add, endif next

( ---------------------------------------------------------------------------- )
(       Plane Size Register                                                    )
( ---------------------------------------------------------------------------- )
{:} vdpsize: ( value -- )
    create c, ;code ( -- ) [dfa]+  (plane-size) [#]  c move,  next

$00 vdpsize: 32x32planes        $01 vdpsize: 64x32planes
$10 vdpsize: 32x64planes        $03 vdpsize: 128x32planes
$30 vdpsize: 32x128planes       $11 vdpsize: 64x64planes

code plane-width ( -- u )
    tos push, tos clear, (plane-size) [#] tos c move,
    tos c inc, 5 # tos c lsl, next
code plane-height ( -- u )
    tos push, tos clear, (plane-size) [#] tos c move,
    $10 # tos c add, tos tos c add, $F0 # tos c and, next
code plane-size ( -- width height )
    tos push, tos clear, (plane-size) [#] tos c move,
    $11 # tos c add, tos tos c add, tos d1 move, $F0 # tos c and,
    4 # d1 c lsl, d1 push, next

( ---------------------------------------------------------------------------- )
(       Initialize Video Register Driver                                       )
( ---------------------------------------------------------------------------- )
: (init-video) ( -- ) $04040000 to video-mode-registers  2 >autoinc ;

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
    video-data [#] a1 address, a2 pull,
    1 # tos h lsr, cset if [a2]+ [a1] h move, endif
    tos h dec, pos if tos begin [a2]+ [a1] move, loop endif tos pull, next
code video>buffer ( addr  #halves -- )
    video-data [#] a1 address, a2 pull,
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
    backcolor-index 2*  -interrupts[  write-cram  h!video  ]-interrupts  ;

( ---------------------------------------------------------------------------- )
(       Blast Processing                                                       )
( ---------------------------------------------------------------------------- )
DMAQueueItems 16 * make DMAQueueSize
alignram DMAQueueSize buffer: dma-queue      hvalue dma-ptr

code init-dma ( -- )
    dma-queue [#] a1 address, [a1 DMAQueueSize +] a2 address, a1 [a2]+ h move,
    DMAQueueItems d1 do
        $8F009400 # [a1]+ move, $93009700 # [a1]+ move,
        $96009500 # [a1]+ move, cell # a1 add,
    loop next

( ---------------------------------------------------------------------------- )
{:} dma: ( u -- ) create ,  ;code ( src dest #halves -- )
    tos h test, z= if 2 cells # sp add, tos pull, next, endif
    dma-ptr [#] a2 address, -1 # d1 move, [a2] d1 h move, a2 d1 h compare,
    ugt= if tos push, -266 # tos move, throw-primitive, endif
    d1 a1 move, autoinc> [#] [a1 1 +] c move, d1 pull, d2 pull,
    1 # d2 lsr, $7FFFFF # d2 and, d2 [a1 5 +] movep, tos [a1 3 +] h movep,
    12 # a1 add, 2 # d1 lsl, 2 # d1 h lsr, d1 swap, [dfa] d1 or, d1 [a1]+ move,
    a1 [a2] h move, tos pull, next
$40000080 dma: dma>vram    $C0000080 dma: dma>cram    $40000090 dma: dma>vsram

{:} video-move: ( dma-xt  cpu-xt -- ) create , ,
        does> ( src dest #halves -- ) vbi-enabled? if cell+ endif @ execute ;
' dma>vram   ' >vram   video-move:  move>vram       aka move>video
' dma>cram   ' >cram   video-move:  move>cram       aka move>color
' dma>vsram  ' >vsram  video-move:  move>vsram      aka move>vscroll

( ---------------------------------------------------------------------------- )
code flush-dma-queue ( -- )
    dma-queue [#] a2 address, [a2 DMAQueueSize +] a1 address,
    [a1] d2 h move, a2 [a1] h move, a2 d2 h compare, ugt if
        video-ctrl [#] a1 address, begin 1 # [a1 1 +] test-bit, z= until
        $A11100 [#] a3 address, $8114 # d3 h move,
        video-mode-registers 1+ [#] d3 c or, d3 [a1] h move,
        begin [a2]+ [a1] move, [a2]+ [a1] move, [a2]+ [a1] move,
              $100 # [a3] h move, [a2]+ [a1] move, 0 # [a3] h move,
              a2 d2 h compare, z= until
        $8104 # d3 h move, video-mode-registers 1+ [#] d3 c or, d3 [a1] h move,
    endif next

( ---------------------------------------------------------------------------- )
code video-copy ( src dest #halves -- ) next

code video-fill ( addr #halves c -- ) next

( ---------------------------------------------------------------------------- )
(       Plane Scrolling                                                        )
( ---------------------------------------------------------------------------- )
variable (screenX)          defer (scrollX)
variable (screenY)          defer (scrollY)

: <screenX> ( -- ) scroll-table write-vram  (screenX) @ !video ;
: <screenY> ( -- )            0 write-vsram (screenY) @ !video ;

: scroll-planes ( -- ) (scrollX) (scrollY) ;
: planeX  ( ax bx -- ) (screenX) tuck half+ h! h!  ['] <screenX> is (scrollX) ;
: planeY  ( ay by -- ) (screenY) tuck half+ h! h!  ['] <screenY> is (scrollY) ;

: move>planeX ( src -- )
    scroll-table scanline-height 2* move>video  ['] noop is (scrollX) ;
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
    video-sprite-ptr [#] a2 address,
    video-sprite-buffer $FFFF and # [a2]+ h move, 0 # [a2]+ c move, next

code video-sprite ( attrib  layout  x y -- )
    video-sprite-ptr [#] a2 address, -1 # d1 move, [a2]+ d1 h move, d1 a1 move,
    d2 clear, [a2] d2 c move, d2 c inc, d2 [a2] c move, 128 # tos h add,
    tos [a1]+ h move, tos pull, half # sp add, [sp]+ d2 h or, d2 [a1]+ h move,
    half # sp add, [sp]+ [a1]+ h move, 128 # tos h add, tos [a1]+ h move,
    a1 -[a2] h move, tos pull, next

code (]video-sprites) ( -- #halves )
    tos push, -1 # tos move, video-sprite-ptr [#] tos h move, tos a1 move,
    0 # [a1 -5 +] c move, video-sprite-buffer $FFFF and # tos h sub,
    1 # tos h lsr, $FFFF # tos and, next

: ]video-sprites ( -- ) 
    video-sprite-buffer  sprite-table  (]video-sprites)  move>video ;

( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )















