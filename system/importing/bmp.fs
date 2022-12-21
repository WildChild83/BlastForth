( ---------------------------------------------------------------------------- )
( **************************************************************************** )
(       BMP File Loader                                                        )
( **************************************************************************** )
( ---------------------------------------------------------------------------- )

( ---------------------------------------------------------------------------- )
(                                                                              )
(       Dependencies:                                                          )
(           - glossary.fs                                                      )
(           - import.fs                                                        )
(                                                                              )
( ---------------------------------------------------------------------------- )
Importing definitions {

( ---------------------------------------------------------------------------- )
(       BMP State Variables                                                    )
( ---------------------------------------------------------------------------- )
variable pixel-array        variable color-table
false value top-down?       0 value #colors

0 value compid  0 value rmask   0 value gmask   0 value bmask   0 value amask
                0 value roff    0 value goff    0 value boff    0 value aoff

defer pixel@+   0 value bits    0 value #bits   0 value #color3

( ---------------------------------------------------------------------------- )
(       BMP Header-Reader                                                      )
( ---------------------------------------------------------------------------- )
: bmp12header ( addr u -- addr u )
    over 22 + word@ 1 <> abort" BMP with >1 color plane is not supported."
    over 18 + word@ to width    over 20 + word@ to height
    over 24 + word@ to bpp      over 26 + color-table !
    0 to #colors   false to top-down?   0 to compid   true to #color3 ;
: bmp-bitfields ( addr u size -- addr u size )
    third 54 + dword@ to rmask
    third 58 + dword@ to gmask
    third 62 + dword@ to bmask ;
: bmp40+header ( addr u size -- addr u )
    third 26 + word@ 1 <> abort" BMP with >1 color plane is not supported."
    third 18 + sdword@ abs to width
    third 22 + sdword@ dup 0< dup to top-down? if negate endif to height
    third 28 + word@ to bpp
    third 46 + dword@ to #colors
    third 138 + color-table !
    third 30 + dword@ dup to compid case
        0 of endof
        1 of bpp 8 <> abort" Invalid BMP compression." endof
        2 of bpp 4 <> abort" Invalid BMP compression." endof
        3 of dup 56 < if 0 else third 66 + dword@ endif to amask
             bmp-bitfields endof
        6 of third 66 + dword@ to amask bmp-bitfields endof
       -1 abort" Unsupported BMP compression type."
    endcase drop   false to #color3 ;
: bmp-header ( addr u -- addr u )
    over 10 + dword@ third + pixel-array !
    over 14 + dword@ dup 12 = if drop bmp12header exit endif
    dup 40 < abort" Unsupported BMP header."
    bmp40+header ;    

( ---------------------------------------------------------------------------- )
(       BMP Mask-Applier                                                       )
( ---------------------------------------------------------------------------- )
: mshift ( n1 n2 -- )  dup 0< if abs lshift else rshift endif ;
: moff ( mask -- off ) dup 0= if exit endif
                       0 begin over 1 and 0= while 1+ >r 1 rshift r> repeat
                       begin over 255 > while 1+ >r 1 rshift r> repeat 
                       begin over 128 < while 1- >r 1 lshift r> repeat nip ;
: moffs ( -- ) rmask moff to roff   gmask moff to goff
               bmask moff to boff   amask moff to aoff ;
: &mask ( u -- rgb ) dup amask and aoff mshift 0= if drop -1 exit endif
                     dup rmask and roff mshift swap
                     dup gmask and goff mshift swap
                         bmask and boff mshift rgb ;
: rgbm2@ ( addr -- rgb )  word@ &mask ;
: rgbm4@ ( addr -- rgb ) dword@ &mask ;
: rgbm3@ ( addr -- rgb ) dup c@ swap 1+ dup c@ 8 lshift
                         swap 1+ c@ 16 lshift + + &mask ;
: rgbm2@+ ( addr -- addr' rgb ) dup 2 + swap rgbm2@ ;
: rgbm3@+ ( addr -- addr' rgb ) dup 3 + swap rgbm3@ ;
: rgbm4@+ ( addr -- addr' rgb ) dup 4 + swap rgbm4@ ;

( ---------------------------------------------------------------------------- )
(       BMP Palette-Indexer                                                    )
( ---------------------------------------------------------------------------- )
defer #color

: <#color3> ( u -- rgb )
    dup #colors >= if drop 0 exit endif 3 * color-table @ + bgr@  ;
: <#color4> ( u -- rgb )
    dup #colors >= if drop 0 exit endif 4 * color-table @ + bgra@ ;
: set#color ( -- )
    #color3 if ['] <#color3> else ['] <#color4> endif is #color ;

( ---------------------------------------------------------------------------- )
(       BMP Pixel-Loader                                                       )
( ---------------------------------------------------------------------------- )
: rgb1b@ ( addr -- rgb )
    #bits dup 0= if drop 8 >r count to bits r> endif 1- to #bits
    bits dup 2* $FE and to bits 7 rshift 1 and #color ;
: rgb2b@ ( addr -- rgb )
    #bits dup 0= if drop 4 >r count to bits r> endif 1- to #bits
    bits dup 2 lshift $FC and to bits 6 rshift 3 and #color ;
: rgb4b@ ( addr -- rgb )
    #bits dup 0= if drop 2 >r count to bits r> endif 1- to #bits
    bits dup 4 lshift $F0 and to bits 4 rshift 15 and #color ;
: rgb8b@ ( addr --rgb ) count #color ;

: rgb555b@  ( addr -- rgb ) word@ dup $7C00 and 7 rshift swap dup $03E0 and
                                     2 rshift swap $001F and 3 lshift rgb ;
: rgb555b@+ ( addr -- addr' rgb ) dup 2 + swap rgb555b@ ;    

: bmp-inner-loop ( addr u -- ) bounds ?do pixel@+ i ! cell +loop ;
: bmp-outer-loop  ( lim u -- ) ?do i pitch bmp-inner-loop pitch negate +loop ;
: load-bmp-loop ( -- )
    pixel-array @   image  top-down?
    if area bmp-inner-loop else dup area + pitch - bmp-outer-loop endif drop ;

( ---------------------------------------------------------------------------- )
: load-bmp ( addr u -- )
    bpp case
         1 of 0 to #bits set#color ['] rgb1b@ endof
         2 of 0 to #bits set#color ['] rgb2b@ endof
         4 of 0 to #bits set#color ['] rgb4b@ endof
         8 of ['] rgb8b@ endof
        16 of compid if moffs ['] rgbm2@+ else ['] rgb555b@+ endif endof
        24 of compid if moffs ['] rgbm3@+ else ['] bgr@+    endif endof
        32 of compid if moffs ['] rgbm4@+ else ['] bgra@+   endif endof
        -1 abort" Unsupported BMP bit depth."
    endcase is pixel@+
    load-bmp-loop
    drop free throw ;

( ---------------------------------------------------------------------------- )
(       External Interface                                                     )
( ---------------------------------------------------------------------------- )
: ?bmp ( addr u -- addr u flag )
    dup 14 >   third 2 + dword@ third = and   third word@ $4D42 = and ;

: init-bmp ( addr u -- addr u )
    bmp-header
    color-table @ pixel-array @ = if color-table off endif
    color-table @ #colors 0= and if 1 bpp @ lshift to #colors endif
    ['] load-bmp is load-image ;

( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )
( ---------------------------------------------------------------------------- )














