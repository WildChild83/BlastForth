( ---------------------------------------------------------------------------- )
( **************************************************************************** )
(       Asset Inclusion                                                        )
( **************************************************************************** )
( ---------------------------------------------------------------------------- )

( ---------------------------------------------------------------------------- )
(                                                                              )
(       Dependencies:                                                          )
(           - glossary.fs                                                      )
(           - graphics.fs                                                      )
(                                                                              )
( ---------------------------------------------------------------------------- )
Forth definitions       Glossary Importing definitions {

( ---------------------------------------------------------------------------- )
(       Utility Words                                                          )
( ---------------------------------------------------------------------------- )
create threshholds     25 hostc,  69 hostc, 101 hostc, 130 hostc,
                      158 hostc, 189 hostc, 230 hostc, 255 hostc,
: component ( c -- c' )
    0 begin 2dup threshholds + c@ > while 1+ repeat nip 2* ;
: rgb ( red green blue -- rgb )
    component 4 lshift swap component + 4 lshift swap component + ;

: rgb@   ( addr -- rgb ) dup c@ swap 1+ dup c@ swap 1+ c@ rgb ;
: bgr@   ( addr -- rgb ) dup 2 + c@ swap dup 1+ c@ swap c@ rgb ;
: bgra@  ( addr -- rgb ) dup 3 + c@ 0= if drop -1 exit endif bgr@ ;
: rgb@+  ( addr -- addr' rgb ) dup 3 + swap rgb@  ;
: bgr@+  ( addr -- addr' rgb ) dup 3 + swap bgr@  ;
: bgra@+ ( addr -- addr' rgb ) dup 4 + swap bgra@ ;

:   word! ( u addr -- ) over 8 rshift over 1+ c! c! ;
:   word@ ( addr -- u ) dup c@ swap 1+ c@ 8 lshift + ;
:  dword@ ( addr -- u ) dup word@ swap 2 + word@ 16 lshift + ;
:  signed    ( u -- n ) dup $80000000 and if $7FFFFFFF invert or endif ;
: sdword@ ( addr -- n ) dword@ signed ;

:   u.n ( u n -- ) >r s>d <# r> 0 ?do # loop #> type space ;
: hex.n ( u n -- ) ." $" base @ >r hex u.n r> base ! ;
: hex.3 ( u -- )   3 hex.n ;
: hex.8 ( u -- )   8 hex.n ;

: bounds ( addr u -- limit index ) over + swap ;

( ---------------------------------------------------------------------------- )
(       State Variables                                                        )
( ---------------------------------------------------------------------------- )
0 value width       0 value pitch           0 value image
0 value height      0 value area            defer load-image
0 value bpp         

create palette 60 cells allot           here constant palette]
palette value palette*
: addr-palette ( rgb -- rgb addr )
    palette* palette ?do dup i @ = if i unloop exit endif cell +loop
    palette* dup palette] u>= abort" Too many colors!"  dup cell+ to palette* ;
: index-palette ( rgb -- c ) addr-palette   tuck !   palette - cell / ;

( ---------------------------------------------------------------------------- )
( **************************************************************************** )
(       File Loader                                                            )
( **************************************************************************** )
( ---------------------------------------------------------------------------- )
include importing/png.fs
include importing/bmp.fs

Importing definitions {

( ---------------------------------------------------------------------------- )
: load-file ( addr1 u1 -- addr2 u2 )
    r/o bin open-file throw dup >r file-size throw abort" File too large."
    dup allocate throw swap 2dup r@ read-file throw
    over <> abort" File import error."  r> close-file throw ;
: init-image ( addr u -- addr u )
    ?bmp if init-bmp exit endif
    \ ?png if init-png exit endif
    true abort" Unrecognized image file format." ;
: allocate-image ( -- )
    width 3 and height 3 and or
    abort" Image width and height must both be multiples of 8." 
    bpp 32 > abort" Color depth >32 bits per pixel is not supported." 
    width cells dup to pitch   height * dup to area   allocate throw to image ;
: palletize ( -- )
    palette to palette*   image dup area bounds
    ?do i @ index-palette over c! 1+ cell +loop drop
    pitch cell / to pitch   area cell / to area ;

\ ADDR,U is the image filename.  IMAGE holds address of Loaded Image.
\ Loaded Image is 1 byte per pixel, each byte is a palette index.
: load-image-file ( addr u -- )
    load-file  init-image  allocate-image  load-image  palletize ;

\ After image is loaded this will display its contents
: .image ( -- )
    cr ." Loaded Image: "   image area bounds
    ?do cr 4 spaces i pitch bounds ?do i dword@ >4< hex.8 4 +loop pitch +loop    
    cr 2 spaces ." Palette: " palette* palette ?do i @ hex.3 cell +loop ;

( ---------------------------------------------------------------------------- )
( **************************************************************************** )
(       Palette Management                                                     )
( **************************************************************************** )
( ---------------------------------------------------------------------------- )
0 value rompalette              0 value rompal*
create pal-name 40 allot        0 value rompal]

: !rompalette ( "name" -- )
    parse-name pal-name 2dup c! 1+ swap cmove
    romspace dup to rompalette dup 2 + to rompal* 32 + to rompal] ;

Forth definitions Importing
create dopalette& asm
    dfa push, 5 # tos h lsl, tos push, 16 # tos move, ' move>cram execute, end
create do2palette& asm
    dfa push, 5 # tos h lsl, tos push, 32 # tos move, ' move>cram execute, end
create do3palette& asm
    dfa push, 5 # tos h lsl, tos push, 48 # tos move, ' move>cram execute, end
create do4palette& asm
    dfa push, 5 # tos h lsl, tos push, 64 # tos move, ' move>cram execute, end
{ : palette ( "name" -- ) host-only
    save-input create restore-input throw \ !rompalette 
    dopalette& codefield, !rompalette 0 h, 15 0 do -1 h, loop
    does> @ compiling? if , exit endif 4 + ; }

Importing definitions {
: pal-addr ( rgb -- rgb addr )
    rompal* rompalette 2 + ?do dup i romh@ = if i unloop exit endif 2 +loop
    rompal* dup rompal] u>= abort" Palette overflow!"  dup 2+ to rompal* ;
: pal-index ( rgb -- c ) 
    dup 0< if drop 0 exit endif   pal-addr  tuck romh!  rompalette - 2/ ;
: pal-used ( -- n ) rompal* rompalette - 2/ ;
: pal-left ( -- n ) rompal] rompal* - 2/ ;
: rompalette? ( -- ) rompalette 0= abort" Palette has not been defined!" ;

: .pal-info ( -- )
    rompalette? pal-name count type ."  has " pal-left . ." slots left" ;
: .pal-data ( -- ) cr rompal* rompalette ?do i romh@ hex.3 2 +loop ;

( ---------------------------------------------------------------------------- )
( **************************************************************************** )
(       Image Conversion/Compression                                           )
( **************************************************************************** )
( ---------------------------------------------------------------------------- )
: pixel ( addr -- c ) c@ cells palette + @ pal-index ;
: pair  ( addr -- c ) dup pixel 4 lshift swap 1+ pixel + ;
: convert-image ( -- )
    image dup area bounds ?do i pair over c! 1+ 2 +loop drop
    pitch 2/ to pitch   area 2/ to area ;

( ---------------------------------------------------------------------------- )
( **************************************************************************** )
(       Asset Importer                                                         )
( **************************************************************************** )
( ---------------------------------------------------------------------------- )
-1 value sprite-size

( ---------------------------------------------------------------------------- )
: cleanup ( -- ) image free throw  -1 to sprite-size ;

: compile-tile ( addr -- ) 8 0 do dup dword@ >4< , pitch + loop drop ;

: compile-sprite ( "name" -- )
    sprite-size area sprite-picture
    image  0 + compile-tile
    image  4 + compile-tile
    image pitch 8 * + 0 + compile-tile
    image pitch 8 * + 4 + compile-tile
    ;

: compile-image ( "name" -- )
    sprite-size 0< abort" Tile imports not implemented yet."
    compile-sprite ;

: import ( addr u "name" -- )
    [ Verbose ] [IF] cr ." Import " 2dup type 4 spaces [THEN]
    rompalette?  load-image-file  convert-image  compile-image
    [ Verbose ] [IF] area . ." bytes" 4 spaces .pal-info [THEN]
    cleanup ;

( ---------------------------------------------------------------------------- )
( **************************************************************************** )
(       Public Interface                                                       )
( **************************************************************************** )
( ---------------------------------------------------------------------------- )
Forth definitions Importing {

: import: ( "filename" -- )     parse-name  import ;
: import" ( "filename" -- ) [char] " parse  import ;

: sprite ( n -- ) to sprite-size ;

: .colors ( -- ) cr .pal-info .pal-data ;

( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )



















