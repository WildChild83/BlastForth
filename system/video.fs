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

{:} vdp-stat: ( mask -- ) create h, ;code ( -- flag )
    tos push, video-ctrl [#] tos h move, [dfa] tos h and,
    tos z<> set?, tos c extend, next
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
    d2 64 kilobytes cell/ for d1 [a1] move, loop next

( ---------------------------------------------------------------------------- )
(       VDP Register Buffer                                                    )
( ---------------------------------------------------------------------------- )
value  video-mode-registers
hvalue foreground-address   hvalue window-address   hvalue background-address
hvalue sprite-address       cvalue backcolor-index  cvalue hint-counter>
hvalue scroll-address       cvalue autoinc>         cvalue (plane-size)
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
{:} vdpreg?: ( index value -- )
    create c, c, ;code ( -- flag )
        tos push, [dfa]+ tos c move, d2 clear, [dfa]+ d2 c move,
        video-mode-registers [#] a1 address, [a1 d2+ 0] tos c and,
        tos z<> set?, tos c extend, next        
0 $20 +vdpreg: +blankleft  0 $20 -vdpreg: -blankleft  0 $20 vdpreg?: blankleft?
1 $08 +vdpreg: +pal        1 $08 -vdpreg: +ntsc       1 $08 vdpreg?: pal?
2 $03 +vdpreg: +hlines     2 $03 -vdpreg: -hlines     2 $03 vdpreg?: hlines?
2 $04 +vdpreg: +vstrips    2 $04 -vdpreg: -vstrips    2 $04 vdpreg?: vstrips?
3 $06 +vdpreg: +interlace  3 $06 -vdpreg: -interlace  3 $06 vdpreg?: interlace?
3 $08 +vdpreg: +shade/tint 3 $08 -vdpreg: -shade/tint 3 $08 vdpreg?: shade/tint?
3 $81 +vdpreg: +h320       3 $81 -vdpreg: +h256       3 $81 vdpreg?: h320?
18 $80 +vdpreg: +windowRight    19 $80 +vdpreg: +windowBottom
18 $80 -vdpreg: +windowLeft     19 $80 -vdpreg: +windowTop
18 $80  vdpreg?: windowRight?   19 $80  vdpreg?: windowBottom?
 2 $02 +vdpreg: (+hstrips)       2 $02  vdpreg?: hstrips?
: +hstrips ( -- ) -hlines (+hstrips) ;

: ntsc?       ( -- flag )  pal? not ;
: h256?       ( -- flag ) h320? not ;
: windowLeft? ( -- flag ) windowRight?  not ;
: windowTop?  ( -- flag ) windowBottom? not ;

: +v224 ( -- ) +ntsc -interlace ;   : v224? ( -- f ) ntsc? interlace? not and ;
: +v240 ( -- ) +pal  -interlace ;   : v240? ( -- f )  pal? interlace? not and ;
: +v448 ( -- ) +ntsc +interlace ;   : v448? ( -- f ) ntsc? interlace?     and ;
: +v480 ( -- ) +pal  +interlace ;   : v480? ( -- f )  pal? interlace?     and ;

: +vnative  ( -- )      pal-console? if +v240  exit endif +v224  ;
:  vnative? ( -- flag ) pal-console? if  v240? exit endif  v224? ;

: #scanlines   ( -- u )  pal? if 240 exit endif 224 ;
: pixel-width  ( -- u ) h320? if 320 exit endif 256 ;
: pixel-height ( -- u ) #scanlines interlace? if 2* endif ;
:  tile-width  ( -- u ) h320? if 40 exit endif 32 ;
:  tile-height ( -- u ) 28 pal? if 2+ endif interlace? if 2* endif ;

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
: plane-pitch ( -- u ) plane-width 2* ;

( ---------------------------------------------------------------------------- )
(       Window Plane                                                           )
( ---------------------------------------------------------------------------- )
:    +top-window ( h -- )                              to windowY 0 to windowX ;
: +bottom-window ( h -- ) tile-height swap -    $80 or to windowY 0 to windowX ;
:   +left-window ( w -- ) tile-width  swap - 2/        to windowX 0 to windowY ;
:  +right-window ( w -- ) tile-width  swap - 2/ $80 or to windowX 0 to windowY ;

: window-rows ( u -- u' ) 6 lshift h320? if 2* endif ;
: window-pitch  ( -- u ) 1 window-rows ;
: window-height ( -- u ) windowY $80 demux if tile-height swap - endif ;
: window-start  ( -- vram )
    window-address windowY $80 demux if window-rows + else drop endif
                   windowX $80 demux if 4* + exit endif drop ;
: window-end ( -- vram )
    window-address windowX if tile-height window-rows + exit endif
                   windowY $80 demux if drop tile-height endif window-rows + ;

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
    tos do [a2]+ [a1] move, loop tos pull, next
code video>buffer ( addr  #halves -- )
    video-data [#] a1 address, a2 pull,
    1 # tos h lsr, cset if [a1] [a2]+ h move, endif
    tos do [a1] [a2]+ move, loop tos pull, next

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
    d1 DMAQueueItems for
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
        $8F00 # d3 h move, autoinc> [#] d3 c move, d3 [a1] h move, 
    endif next

( ---------------------------------------------------------------------------- )
code video-copy ( src dest #halves -- ) next

code video-fill ( addr #halves c -- ) next

( ---------------------------------------------------------------------------- )
(       Plane Scrolling                                                        )
( ---------------------------------------------------------------------------- )
variable (screenX)          defer (scrollX)
variable (screenY)          defer (scrollY)

: <screenX> ( -- ) scroll-address write-vram  (screenX) @ !video ;
: <screenY> ( -- )              0 write-vsram (screenY) @ !video ;

: scroll-planes ( -- ) (scrollX) (scrollY) ;
: planeX  ( ax bx -- ) (screenX) tuck half+ h! h!  ['] <screenX> is (scrollX) ;
: planeY  ( ay by -- ) (screenY) tuck half+ h! h!  ['] <screenY> is (scrollY) ;

: move>planeX ( src -- )
    scroll-address #scanlines 2* move>video  ['] noop is (scrollX) ;
: move>planeY ( src -- ) 0 pixel-width 4/ move>vscroll  ['] noop is (scrollY) ;

( ---------------------------------------------------------------------------- )
(       Sprite Table                                                           )
( ---------------------------------------------------------------------------- )
synonym video-sprite+ 8+        synonym video-sprites 8*
synonym video-sprite- 8-        synonym video-sprite/ 8/

80 video-sprites buffer: video-sprite-buffer    0 buffer: video-sprite-limit

$000 constant 1x1       $800 constant 3x1       $0800 constant mirror
$100 constant 1x2       $900 constant 3x2       $1000 constant flip
$200 constant 1x3       $A00 constant 3x3       $0000 constant palette0
$300 constant 1x4       $B00 constant 3x4       $2000 constant palette1
$400 constant 2x1       $C00 constant 4x1       $4000 constant palette2
$500 constant 2x2       $D00 constant 4x2       $6000 constant palette3
$600 constant 2x3       $E00 constant 4x3       $8000 constant priority
$700 constant 2x4       $F00 constant 4x4
hvalue video-sprite-ptr         cvalue video-sprite-link

( ---------------------------------------------------------------------------- )
code sprites[ ( -- )
    video-sprite-ptr [#] a2 address,
    video-sprite-buffer $FFFF and # [a2]+ h move, 0 # [a2]+ c move, next
code (]sprites) ( -- #halves )
    tos push, -1 # tos move, video-sprite-ptr [#] tos h move, tos a1 move,
    0 # [a1 -5 +] c move, video-sprite-buffer $FFFF and # tos h sub,
    1 # tos h lsr, $FFFF # tos and, next
: ]sprites ( -- ) video-sprite-buffer  sprite-address  (]sprites)  move>video ;

( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )















