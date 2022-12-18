( ---------------------------------------------------------------------------- )
( **************************************************************************** )
(       Asset Inclusion                                                        )
( **************************************************************************** )
( ---------------------------------------------------------------------------- )

( ---------------------------------------------------------------------------- )
(                                                                              )
(       Dependencies:                                                          )
(           - forth.fs                                                         )
(           - core.fs                                                          )
(           - allocate.fs                                                      )
(                                                                              )
( ---------------------------------------------------------------------------- )
Forth definitions       Glossary Importer definitions {

( ---------------------------------------------------------------------------- )
: slurp ( addr1 u1 -- addr2 u2 )
    r/o bin open-file throw dup >r file-size throw abort" File too large."
    dup allocate throw swap 2dup r@ read-file throw
    over <> abort" File import error."  r> close-file throw ;

:   word@ ( addr -- u ) dup c@ swap 1+ c@ 8 lshift + ;
:  dword@ ( addr -- u ) dup word@ swap 2 + word@ 16 lshift + ;
:  signed    ( u -- n ) dup $80000000 and if $7FFFFFFF invert or endif ;
: sdword@ ( addr -- n ) dword@ signed ;

: bounds ( addr u -- limit index ) over + swap ;

( ---------------------------------------------------------------------------- )
create palette-values   0 hostc,  52 hostc,  87 hostc, 116 hostc,
                      144 hostc, 172 hostc, 206 hostc, 255 hostc,
create threshholds     25 hostc,  69 hostc, 101 hostc, 130 hostc,
                      158 hostc, 189 hostc, 230 hostc, 255 hostc,
: component ( c -- c' )
    0 begin 2dup threshholds + c@ > while 1+ repeat nip 2* ;
: rgb ( red green blue -- u )
    component 4 lshift swap component + 4 lshift swap component + ;

: rgb@  ( addr -- u ) dup c@ swap 1+ dup c@ swap 1+ c@ rgb ;
: bgr@  ( addr -- u ) dup 2 + c@ swap dup 1+ c@ swap c@ rgb ;
: bgra@ ( addr -- u ) dup 3 + c@ 0= if drop 0 exit endif bgr@ ;

: rgb@+  ( addr -- addr' u ) dup 3 + swap rgb@  ;
: bgr@+  ( addr -- addr' u ) dup 3 + swap bgr@  ;
: bgra@+ ( addr -- addr' u ) dup 4 + swap bgra@ ;

: rgb! ( u addr -- ) over 8 rshift over c! 1+ c! ;

( ---------------------------------------------------------------------------- )
variable width      variable height     variable bpp
defer load-image    0 value rgb-image

( ---------------------------------------------------------------------------- )
include importing/png.fs
include importing/bmp.fs

( ---------------------------------------------------------------------------- )
: init-image ( addr u -- addr u )
    ?bmp if init-bmp exit endif
    \ ?png if init-png exit endif
    true abort" Unrecognized image file format." ;

: tab ( -- ) 4 spaces ;
: dimensions? ( -- )
    width @ 3 and height @ 3 and or
    abort" Image width and height must both be multiples of 8." 
    bpp @ 32 > abort" Color depth >32 bits per pixel is not supported." ;
: allocate-rgb ( addr u -- addr u )
    width @ height @ * 2* dup allocate throw to rgb-image 
    cr tab ." rgb image: " . ." bytes" ;
: import ( addr u -- )
    cr ." Importing " 2dup type tab slurp init-image
    cr tab ." Width:  " width  @ .
    cr tab ." Height: " height @ .
    cr tab ." BPP:    " bpp    @ .
    cr tab ." Pixels: " pixel-array @ hex.
    cr tab ." Colors: " color-table @ hex. ." #colors=" #colors .
    cr tab ." Top?    " top-down? .

    dimensions?
    allocate-rgb
    load-image
    
    rgb-image  width @ height @ * 2* bounds ?do
        cr i width @ 2* bounds ?do i word@ >2< hex. 2 +loop
        height @ 2* +loop
    
    drop free throw   rgb-image free throw ;

( ---------------------------------------------------------------------------- )
Forth definitions Importer {

: import: ( "filename" -- )     parse-name  import ;
: import" ( "filename" -- ) [char] " parse  import ;

( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )



















